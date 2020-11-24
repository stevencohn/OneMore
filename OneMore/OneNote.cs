//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

#pragma warning disable S3881 // IDisposable should be implemented correctly

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Forms = System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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

		public enum PageDetail
		{
			All = PageInfo.piAll,
			Basic = PageInfo.piBasic,
			BinaryData = PageInfo.piBinaryData,
			BinaryDataFileType = PageInfo.piBinaryDataFileType,
			BinaryDataSelection = PageInfo.piBinaryDataSelection,
			FileType = PageInfo.piFileType,
			Selection = PageInfo.piSelection,
			SelectionFileType = PageInfo.piSelectionFileType
		}

		public enum Scope
		{
			Children = HierarchyScope.hsChildren,
			Notebooks = HierarchyScope.hsNotebooks,
			Pages = HierarchyScope.hsPages,
			Sections = HierarchyScope.hsSections,
			Self = HierarchyScope.hsSelf
		}


		#region OneNoteDispatcher
		/// <summary>
		/// OneNote is an MTA application and commands run in that MTA space. Windows Forms, will
		/// also run as MTA but in a different thread and commands to the OneNote API will block
		/// when invoked from a Windows Form. So this class acts as a dispatcher back to the original
		/// command thread. It only runs when explicitly started using the one.StartDispatcher call.
		/// </summary>
		private class OneNoteDispatcher : IDisposable
		{
			private readonly BlockingCollection<Action> jobs;
			private readonly CancellationTokenSource source;
			private bool disposed = false;


			/// <summary>
			/// Initialize a new instance
			/// </summary>
			/// <param name="one"></param>
			public OneNoteDispatcher()
			{
				jobs = new BlockingCollection<Action>();
				source = new CancellationTokenSource();
			}


			protected virtual void Dispose(bool disposing)
			{
				if (!disposed)
				{
					if (disposing)
					{
						if (jobs.Any())
						{
							jobs.CompleteAdding();
							source.Cancel();
						}
					}

					disposed = true;
				}
			}

			public void Dispose()
			{
				Dispose(true);
			}


			/// <summary>
			/// Executes the given action by pushing it onto the job queue and letting the job
			/// engine take care of it.
			/// </summary>
			/// <param name="action"></param>
			public void Execute(Action action)
			{
				jobs.Add(action);
			}


			/// <summary>
			/// Called by one.StartDispatcher, this sets up the job engine to execute actions
			/// as they are added to the blocking queue.
			/// </summary>
			/// <returns></returns>
			public void Run()
			{
				var token = source.Token;

				var factory = new TaskFactory(
					TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

				factory.StartNew(() =>
				{
					try
					{
						while (!jobs.IsCompleted)
						{
							var action = jobs.Take(token);

							if (!token.IsCancellationRequested)
							{
								try
								{
									Logger.Current.WriteLine("running dispatched action");
									action();
								}
								catch (Exception exc)
								{
									Logger.Current.WriteLine("error running dispatched action", exc);
								}
							}
						}
					}
					catch (Exception exc)
					{
						Logger.Current.WriteLine("error in dispatcher", exc);
					}
				},
				token);
			}
		}
		#endregion OneNoteDispatcher


		public const string Prefix = "one";

		private Application onenote;
		private OneNoteDispatcher dispatcher = null;
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
		public OneNote(out Page page, out XNamespace ns, PageDetail info = PageDetail.Selection)
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
					if (dispatcher != null)
					{
						dispatcher.Dispose();
						dispatcher = null;
					}

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
		/// Gets the currently viewed page ID
		/// </summary>
		public string CurrentPageId => onenote.Windows.CurrentWindow?.CurrentPageId;


		/// <summary>
		/// Gets the currently viewed section ID
		/// </summary>
		public string CurrentSectionId => onenote.Windows.CurrentWindow?.CurrentSectionId;


		/// <summary>
		/// Gets the currently viewed notebook ID
		/// </summary>
		public string CurrentNotebookId => onenote.Windows.CurrentWindow?.CurrentNotebookId;


		/// <summary>
		/// Gets the Win32 Window associated with the current window's handle
		/// </summary>
		public Forms.IWin32Window Window => Forms.Control.FromHandle(WindowHandle);


		/// <summary>
		/// Gets the handle of the current window
		/// </summary>
		public IntPtr WindowHandle => (IntPtr)onenote.Windows.CurrentWindow.WindowHandle;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Create...

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
		/// Create a new section in the current notebook immediately after the open section
		/// </summary>
		/// <param name="name">The name of the new section</param>
		/// <returns>The Section element</returns>
		public XElement CreateSection(string name)
		{
			// find the current section in this notebook (may be in section group)
			var notebook = GetNotebook();
			var ns = GetNamespace(notebook);
			var current = notebook.Descendants(ns + "Section")
				.FirstOrDefault(e => e.Attribute("isCurrentlyViewed")?.Value == "true");

			XElement parent;
			var sectionIds = new List<string>();

			if (current == null)
			{
				// add first section to notebook
				notebook.Add(new XElement(ns + "Section", new XAttribute("name", name)));
				parent = notebook;
			}
			else
			{
				// get the parent of the current section (may be the notebook or a section group)
				// so we will then only update that parent rather than the entire hierarchy
				parent = current.Parent;

				// collect all section IDs for comparison
				sectionIds = parent.Elements(ns + "Section")
					.Where(e => e.Attribute("isRecycleBin") == null &&
								e.Attribute("isInRecycleBin") == null)
					.Attributes("ID").Select(a => a.Value).ToList();

				// add the new section (won't know the ID yet)
				current.AddAfterSelf(new XElement(ns + "Section", new XAttribute("name", name)));
			}

			// udpate the notebook or section group
			UpdateHierarchy(parent);

			// get the parent again so we can find the new section ID
			onenote.GetHierarchy(
				parent.Attribute("ID").Value,
				HierarchyScope.hsSections, out var xml, XMLSchema.xs2013);

			parent = XElement.Parse(xml);

			// compare the new parent with old section IDs to find the new ID
			var section = parent.Elements(ns + "Section")
				.Where(e => e.Attribute("isRecycleBin") == null &&
							e.Attribute("isInRecycleBin") == null)
				.FirstOrDefault(e => !sectionIds.Contains(e.Attribute("ID").Value));

			return section;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Get...

		/// <summary>
		/// Get the known paths used by OneNote; this is for diagnostic logging
		/// </summary>
		/// <returns></returns>
		public (string backupFolder, string defaultFolder, string unfiledFolder) GetFolders()
		{
			onenote.GetSpecialLocation(SpecialLocation.slBackUpFolder, out var backupFolder);
			onenote.GetSpecialLocation(SpecialLocation.slDefaultNotebookFolder, out var defaultFolder);
			onenote.GetSpecialLocation(SpecialLocation.slUnfiledNotesSection, out var unfiledFolder);
			return (backupFolder, defaultFolder, unfiledFolder);
		}


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
		public XElement GetNotebook(Scope scope = Scope.Sections)
		{
			return GetNotebook(CurrentNotebookId, scope);
		}


		/// <summary>
		/// Get the spcified notebook with a hierarchy of sections
		/// </summary>
		/// <param name="id">The ID of the notebook</param>
		/// <returns>A Notebook element with Section children</returns>
		public XElement GetNotebook(string id, Scope scope = Scope.Sections)
		{
			if (!string.IsNullOrEmpty(id))
			{
				onenote.GetHierarchy(id, (HierarchyScope)scope, out var xml);
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
			onenote.GetHierarchy(
				string.Empty, HierarchyScope.hsNotebooks, out var xml, XMLSchema.xs2013);

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
		public Page GetPage(PageDetail info = PageDetail.Selection)
		{
			return GetPage(CurrentPageId, info);
		}


		/// <summary>
		/// Gets the specified page.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <param name="info">The desired verbosity of the XML</param>
		/// <returns>A Page containing the root XML of the page</returns>
		public Page GetPage(string pageId, PageDetail info = PageDetail.All)
		{
			if (string.IsNullOrEmpty(pageId))
			{
				return null;
			}

			onenote.GetPageContent(pageId, out var xml, (PageInfo)info, XMLSchema.xs2013);
			if (!string.IsNullOrEmpty(xml))
			{
				return new Page(XElement.Parse(xml));
			}

			return null;
		}


		/// <summary>
		/// Gets the name, file path, and OneNote hyperlink to the current page;
		/// used to build up Favorites
		/// </summary>
		/// <returns></returns>
		public (string Name, string Path, string Link) GetPageInfo()
		{
			var page = GetPage(PageDetail.Basic);
			if (page == null)
			{
				return (null, null, null);
			}

			// name
			var name = page.Root.Attribute("name")?.Value;

			// path
			string path = null;
			var section = GetSection();
			if (section != null)
			{
				var uri = section.Attribute("path")?.Value;
				if (!string.IsNullOrEmpty(uri))
				{
					path = "/" + Path.Combine(
						Path.GetFileName(Path.GetDirectoryName(uri)),
						Path.GetFileNameWithoutExtension(uri),
						name
						).Replace("\\", "/");
				}
			}

			// link
			string link = GetHyperlink(page.PageId, string.Empty);

			return (name, path, link);
		}


		/// <summary>
		/// Gets the ID of the parent hierachy object that owns the specified object
		/// </summary>
		/// <param name="objectId">The ID of the object whose parent you want to find</param>
		/// <returns>The ID of the parent or null if not found (e.g. parent of a notebook)</returns>
		public string GetParent(string objectId)
		{
			try
			{
				onenote.GetHierarchyParent(objectId, out var parentId);
				return parentId;
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		/// Gest the current section and its child page hierarchy
		/// </summary>
		/// <returns>A Section element with Page children</returns>
		public XElement GetSection()
		{
			return GetSection(CurrentSectionId);
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
				logger.WriteLine($"error deleting page object {objectId}", exc);
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
				logger.WriteLine($"error deleting hierarchy object {objectId}", exc);
			}
		}


		/// <summary>
		/// Update the current page.
		/// </summary>
		/// <param name="page">A Page</param>
		public void Update(Page page)
		{
			Update(page.Root);
		}


		/// <summary>
		/// Updates the given content, with a unique ID, on the current page.
		/// </summary>
		/// <param name="element">A page or element within a page with a unique objectID</param>
		public void Update(XElement element)
		{
			var xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				onenote.UpdatePageContent(xml, DateTime.MinValue, XMLSchema.xs2013, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating page content", exc);
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
				logger.WriteLine("error updating hierarchy", exc);
				logger.WriteLine(element.ToString());
				logger.WriteLine();
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// QuickFiling...

		/// <summary>
		/// A callback delegate for consumers who want to know the selected item in the
		/// QuickFiling dialog.
		/// </summary>
		/// <param name="nodeId"></param>
		public delegate void SelectLocationCallback(string nodeId);


		/// <summary>
		/// Presents the QuickFiling dialog to the user and invokes the given callback
		/// when completed.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="scope"></param>
		/// <param name="callback"></param>
		public void SelectLocation(
			string title, string description, Scope scope, SelectLocationCallback callback)
		{
			var dialog = onenote.QuickFiling();
			dialog.Title = title;
			dialog.Description = description;
			dialog.ParentWindowHandle = onenote.Windows.CurrentWindow.WindowHandle;

			switch (scope)
			{
				case Scope.Notebooks:
					dialog.TreeDepth = HierarchyElement.heNotebooks;
					break;
				case Scope.Sections:
					dialog.TreeDepth = HierarchyElement.heSections;
					break;
				case Scope.Pages:
					dialog.TreeDepth = HierarchyElement.hePages;
					break;
			}

			dialog.AddButton(
				Resx.OK, HierarchyElement.heSections, HierarchyElement.heSections, false);

			dialog.Run(new FilingCallback(callback));
		}


		private class FilingCallback : IQuickFilingDialogCallback
		{
			private readonly SelectLocationCallback userCallback;

			public FilingCallback(SelectLocationCallback usercb)
			{
				userCallback = usercb;
			}

			public void OnDialogClosed(IQuickFilingDialog dialog)
			{
				userCallback(dialog.SelectedItem);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Utilities...

		/// <summary>
		/// Used to dispatch actions from a modal Forms dialog to affect OneNote.
		/// </summary>
		/// <param name="pageId"></param>
		public void Dispatch(Action action)
		{
			dispatcher.Execute(action);
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


		/// <summary>
		/// Forces OneNote to jump to the specified object, onenote Uri, or Web Uri
		/// </summary>
		/// <param name="uri">A pageId, sectionId, notebookId, onenote:URL, or Web URL</param>
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
		/// Search pages under the specified hierarchy node using the given query.
		/// </summary>
		/// <param name="nodeId"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public string Search(string nodeId, string query)
		{
			onenote.FindPages(nodeId, query, out var xml, false, false, XMLSchema.xs2013);
			return xml;
		}


		/// <summary>
		/// Search the hierarchy node for the specified meta tag
		/// </summary>
		/// <param name="nodeId">The root node: notebook, section, or page</param>
		/// <param name="name">The search string, meta key name</param>
		/// <returns>A hierarchy XML starting at the given node.</returns>
		public XElement SearchMeta(string nodeId, string name)
		{
			onenote.FindMeta(nodeId, name, out var xml, false, XMLSchema.xs2013);
			return XElement.Parse(xml);
		}


		/// <summary>
		/// Must be called from a Command thread prior to opening a modal dialog that will then
		/// dispatch actions
		/// </summary>
		public void StartDispatcher()
		{
			if (dispatcher == null)
			{
				dispatcher = new OneNoteDispatcher();
				dispatcher.Run();
			}
		}


		/// <summary>
		/// Special helper for DiagnosticsCommand
		/// </summary>
		/// <param name="builder"></param>
		public void ReportWindowDiagnostics(StringBuilder builder)
		{
			var win = onenote.Windows.CurrentWindow;

			builder.AppendLine($"CurrentNotebookId: {win.CurrentNotebookId}");
			builder.AppendLine($"CurrentPageId....: {win.CurrentPageId}");
			builder.AppendLine($"CurrentSectionId.: {win.CurrentSectionId}");
			builder.AppendLine($"CurrentSecGrpId..: {win.CurrentSectionGroupId}");
			builder.AppendLine($"DockedLocation...: {win.DockedLocation}");
			builder.AppendLine($"IsFullPageView...: {win.FullPageView}");
			builder.AppendLine($"IsSideNote.......: {win.SideNote}");
			builder.AppendLine();

			builder.AppendLine($"Windows ({onenote.Windows.Count})");

			var e = onenote.Windows.GetEnumerator();
			while (e.MoveNext())
			{
				var window = e.Current as Window;

				var threadId = Native.GetWindowThreadProcessId(
					(IntPtr)window.WindowHandle,
					out var processId);

				builder.Append($"- window [processId:{processId}, threadId:{threadId}]");
				builder.Append($" handle:{window.WindowHandle:x} active:{window.Active}");

				if (window.WindowHandle == onenote.Windows.CurrentWindow.WindowHandle)
				{
					builder.AppendLine(" (current)");
				}
			}
		}
	}
}
