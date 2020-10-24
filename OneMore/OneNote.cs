//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // IDisposable should be implemented correctly

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Xml.Linq;
	using Forms = System.Windows.Forms;
	using ON = Microsoft.Office.Interop.OneNote;


	/// <summary>
	/// Wraps the OneNote interop API
	/// </summary>
	/// <see cref="https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote"/>
	internal class OneNote : IDisposable
	{
		public enum ExportFormat
		{
			EMF = PublishFormat.pfEMF,
			HTML = PublishFormat.pfHTML,
			MHTML = PublishFormat.pfMHTML,
			OneNote = PublishFormat.pfOneNote,
			OneNote2007 = PublishFormat.pfOneNote2007,
			OneNotePackage = PublishFormat.pfOneNotePackage,
			PDF = PublishFormat.pfPDF,
			Word = PublishFormat.pfWord,
			XPS = PublishFormat.pfXPS,
			XML = 1000
		}

		public enum PageInfo
		{
			All = ON.PageInfo.piAll,
			Basic = ON.PageInfo.piBasic,
			BinaryData = ON.PageInfo.piBinaryData,
			BinaryDataFileType = ON.PageInfo.piBinaryDataFileType,
			BinaryDataSelection = ON.PageInfo.piBinaryDataSelection,
			FileType = ON.PageInfo.piFileType,
			Selection = ON.PageInfo.piSelection,
			SelectionFileType = ON.PageInfo.piSelectionFileType
		}

		public enum Scope
		{
			Children = ON.HierarchyScope.hsChildren,
			Notebooks = ON.HierarchyScope.hsNotebooks,
			Pages = ON.HierarchyScope.hsPages,
			Sections = ON.HierarchyScope.hsSections,
			Self = ON.HierarchyScope.hsSelf
		}


		public const string Prefix = "one";

		private Application onenote;
		private readonly ILogger logger;
		private bool disposed;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Constructors...

		/// <summary>
		/// Initialize a new wrapper
		/// </summary>
		public OneNote()
		{
			onenote = new Application();
			logger = Logger.Current;
		}


		/// <summary>
		/// Initialize a new wrapper and return the current page
		/// </summary>
		/// <param name="page">The current Page</param>
		/// <param name="ns">The namespace of the current page</param>
		/// <param name="info">The desired verbosity of the XML</param>
		public OneNote(out Page page, out XNamespace ns, PageInfo info = PageInfo.Selection)
			: this()
		{
			page = GetPage(info);
			ns = page.Namespace;
		}


		#region Lifecycle
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					onenote = null;
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			// DO NOT call this otherwise OneNote will not shutdown properly
			//GC.SuppressFinalize(this);
		}
		#endregion Lifecycle


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Properties

		/// <summary>
		/// Gets the Win32 Window associated with the current window's handle
		/// </summary>
		public Forms.IWin32Window Window => Forms.Control.FromHandle(WindowHandle);



		/// <summary>
		/// Gets the handle of the current window
		/// </summary>
		public IntPtr WindowHandle => (IntPtr)onenote.Windows.CurrentWindow.WindowHandle;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Get...

		/// <summary>
		/// Gets a onenote:hyperline to an object on the specified page
		/// </summary>
		/// <param name="pageId">The ID of a page</param>
		/// <param name="objectId">
		/// The ID of an object on the page or string.Empty to link to the page itself
		/// </param>
		/// <returns></returns>
		public string GetHyperlink(string pageId, string objectId)
		{
			onenote.GetHyperlinkToObject(pageId, objectId, out var hyperlink);
			return hyperlink;
		}


		/// <summary>
		/// Gets the OneNote namespace for the given element
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public XNamespace GetNamespace(XElement element)
		{
			return element.GetNamespaceOfPrefix(Prefix);
		}


		/// <summary>
		/// Gets the current notebook with a hierarchy of sections
		/// </summary>
		/// <returns>A Notebook element with Section children</returns>
		public XElement GetNotebook()
		{
			return GetNotebook(onenote.Windows.CurrentWindow?.CurrentNotebookId);
		}


		/// <summary>
		/// Get the spcified notebook with a hierarchy of sections
		/// </summary>
		/// <param name="id">The ID of the notebook</param>
		/// <returns>A Notebook element with Section children</returns>
		public XElement GetNotebook(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				onenote.GetHierarchy(id, HierarchyScope.hsSections, out var xml);
				if (!string.IsNullOrEmpty(xml))
				{
					return XElement.Parse(xml);
				}
			}

			return null;
		}


		/// <summary>
		/// Gets a root note containing Notebook elements
		/// </summary>
		/// <returns>A Notebooks element with Notebook children</returns>
		public XElement GetNotebooks()
		{
			// find the ID of the current notebook
			onenote.GetHierarchy(string.Empty, HierarchyScope.hsNotebooks, out var xml, XMLSchema.xs2013);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		/// <summary>
		/// Gets the current page.
		/// </summary>
		/// <param name="info">The desired verbosity of the XML</param>
		/// <returns>A Page containing the root XML of the page</returns>
		public Page GetPage(PageInfo info = PageInfo.Selection)
		{
			return GetPage(onenote.Windows.CurrentWindow?.CurrentPageId, info);
		}


		/// <summary>
		/// Gets the specified page.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <param name="info">The desired verbosity of the XML</param>
		/// <returns>A Page containing the root XML of the page</returns>
		public Page GetPage(string pageId, PageInfo info = PageInfo.All)
		{
			if (string.IsNullOrEmpty(pageId))
			{
				return null;
			}

			onenote.GetPageContent(pageId, out var xml, (ON.PageInfo)info, XMLSchema.xs2013);
			if (!string.IsNullOrEmpty(xml))
			{
				return new Page(XElement.Parse(xml));
			}

			return null;
		}


		/// <summary>
		/// Gest the current section and its child page hierarchy
		/// </summary>
		/// <returns>A Section element with Page children</returns>
		public XElement GetSection()
		{
			return GetSection(onenote.Windows.CurrentWindow?.CurrentSectionId);
		}


		/// <summary>
		/// Gest the specified section and its child page hierarchy
		/// </summary>
		/// <returns>A Section element with Page children</returns>
		public XElement GetSection(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				onenote.GetHierarchy(id, HierarchyScope.hsPages, out var xml);
				if (!string.IsNullOrEmpty(xml))
				{
					return XElement.Parse(xml);
				}
			}

			return null;
		}



		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Update...

		/// <summary>
		/// Deletes the given object from the specified page
		/// </summary>
		/// <param name="pageId">The ID of the page to modify</param>
		/// <param name="objectId">The ID of the object to remove from the page</param>
		public void DeleteContent(string pageId, string objectId)
		{
			try
			{
				onenote.DeletePageContent(pageId, objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"ERROR deleting page object {objectId}", exc);
			}
		}


		/// <summary>
		/// Deletes the given object(s) from the hierarchy; used for merging
		/// </summary>
		/// <param name="element"></param>
		public void DeleteHierarchy(string objectId)
		{
			try
			{
				onenote.DeleteHierarchy(objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"ERROR deleting hierarchy object {objectId}", exc);
			}
		}


		/// <summary>
		/// Update the current page.
		/// </summary>
		/// <param name="page">A Page</param>
		public void Update(Page page)
		{
			UpdateContent(page.Root);
		}


		/// <summary>
		/// Updates the current page with the given content.
		/// </summary>
		/// <param name="element">A page or element within a page with a unique objectID</param>
		public void UpdateContent(XElement element)
		{
			var xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				onenote.UpdatePageContent(xml, DateTime.MinValue, XMLSchema.xs2013, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine("ERROR updating page content", exc);
				logger.WriteLine(element.ToString());
				logger.WriteLine();
			}
		}


		/// <summary>
		/// Update the hierarchy info with the given XML; used for sorting
		/// </summary>
		/// <param name="element"></param>
		public void UpdateHierarchy(XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				onenote.UpdateHierarchy(xml);
			}
			catch (Exception exc)
			{
				logger.WriteLine("ERROR updating hierarchy", exc);
				logger.WriteLine(element.ToString());
				logger.WriteLine();
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Utilities...

		/// <summary>
		/// Creates a new page in the specified section
		/// </summary>
		/// <param name="sectionId">The section to contain the new page</param>
		/// <param name="pageId">The ID of the new page</param>
		public void CreatePage(string sectionId, out string pageId)
		{
			onenote.CreateNewPage(sectionId, out pageId);
		}


		/// <summary>
		/// Forces OneNote to jump to the specified page or Web Uri
		/// </summary>
		/// <param name="uri"></param>
		public void NavigateTo(string uri)
		{
			if (uri.StartsWith("onenote:"))
			{
				onenote.NavigateToUrl(uri);
			}
			else
			{
				onenote.NavigateTo(uri);
			}
		}


		/// <summary>
		/// Exports the specified page to a file using the given format
		/// </summary>
		/// <param name="pageId">The page ID</param>
		/// <param name="path">The output file path</param>
		/// <param name="format">The format</param>
		public void Export(string pageId, string path, ExportFormat format)
		{
			onenote.Publish(pageId, path, (PublishFormat)format, string.Empty);
		}
	}
}
