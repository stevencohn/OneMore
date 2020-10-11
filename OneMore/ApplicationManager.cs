//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // "IDisposable" should be implemented correctly, but OneMore won't exit

namespace River.OneMoreAddIn
{
	using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;
	using Microsoft.Win32;
	using Forms = System.Windows.Forms;


	internal class ApplicationManager : IDisposable
	{
		private bool disposed = false;
		private readonly ILogger logger;


		// Lifecycle...

		/// <summary>
		/// Initialize a new manager, instantiating a new OneNote Application.
		/// </summary>
		public ApplicationManager ()
		{
			Application = new Application();
			logger = Logger.Current;
		}


		protected virtual void Dispose (bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Application = null;
				}

				disposed = true;
			}
		}


		public void Dispose ()
		{
			Dispose(true);
		}


		//========================================================================================
		// Properties...

		/// <summary>
		/// Gets a direct referenc to the OneNote Application; use wisely
		/// </summary>
		public Application Application { get; private set; }


		/// <summary>
		/// Gets the Win32 Window associated with the current window's handle
		/// </summary>
		public Forms.IWin32Window Window => Forms.Control.FromHandle(WindowHandle);


		/// <summary>
		/// Gets the handle of the current window
		/// </summary>
		public IntPtr WindowHandle => (IntPtr)Application.Windows.CurrentWindow.WindowHandle;


		//========================================================================================
		// Methods...
		// https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote#gethierarchy-method

		/// <summary>
		/// Gets the XML describing the current notebook's page hierarchy
		/// </summary>
		/// <returns></returns>
		public XElement CurrentNotebook ()
		{
			string id = Application.Windows.CurrentWindow?.CurrentNotebookId;

			Application.GetHierarchy(id, HierarchyScope.hsPages, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		/// <summary>
		/// Gets the XML describing the current page content
		/// </summary>
		/// <param name="info">
		/// The PageInfo scope specifying levels of detail to include in the XML
		/// </param>
		/// <returns></returns>
		public XElement CurrentPage (PageInfo info = PageInfo.piSelection)
		{
			return GetPage(Application.Windows.CurrentWindow?.CurrentPageId, info);
		}


		/// <summary>
		/// Gest the XML describing the current section's page hierarchy
		/// </summary>
		/// <returns></returns>
		public XElement CurrentSection()
		{
			string id = Application.Windows.CurrentWindow?.CurrentSectionId;

			Application.GetHierarchy(id, HierarchyScope.hsPages, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		/// <summary>
		/// Gets the name, file path, and OneNote hyperlink to the current page;
		/// used to build up Favorites
		/// </summary>
		/// <returns></returns>
		public (string Name, string Path, string Link) GetCurrentPageInfo ()
		{
			// name
			string name = null;
			var page = CurrentPage(PageInfo.piBasic);
			if (page != null)
			{
				name = page.Attribute("name")?.Value;

				//if (!string.IsNullOrEmpty(name))
				//{
				//	// printable chars only; e.g. remove title emoticon
				//	name = Regex.Replace(name, @"[^ -~]", "");
				//}
			}

			// path
			string path = null;
			var section = CurrentSection();
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
			var pageId = Application.Windows.CurrentWindow?.CurrentPageId;
			string link = null;
			if (!string.IsNullOrEmpty(pageId))
			{
				Application.GetHyperlinkToObject(pageId, "", out link);
			}

			return (name, path, link);
		}


		/// <summary>
		/// Get the XML of the specified hierarchy; used for sorting and XmlDialog
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public XElement GetHierarchy(HierarchyScope scope = HierarchyScope.hsPages)
		{
			// get our own copy
			Application.GetHierarchy(null, scope, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		/// <summary>
		/// Get the known paths used by OneNote; this is for diagnostic logging
		/// </summary>
		/// <returns></returns>
		public (string backupFolder, string defaultFolder, string unfiledFolder) GetLocations ()
		{
			Application.GetSpecialLocation(SpecialLocation.slBackUpFolder, out var backupFolder);
			Application.GetSpecialLocation(SpecialLocation.slDefaultNotebookFolder, out var defaultFolder);
			Application.GetSpecialLocation(SpecialLocation.slUnfiledNotesSection, out var unfiledFolder);
			return (backupFolder, defaultFolder, unfiledFolder);
		}


		/// <summary>
		/// Gets the XML of the specified page
		/// </summary>
		/// <param name="pageId">The ID of the page</param>
		/// <param name="info">The level of detail to include in the XML</param>
		/// <returns></returns>
		public XElement GetPage(string pageId, PageInfo info = PageInfo.piAll)
		{
			if (pageId != null)
			{
				Application.GetPageContent(pageId, out var xml, info, XMLSchema.xs2013);

				var root = XElement.Parse(xml);
				return root;
			}

			return null;
		}


		/// <summary>
		/// Forces OneNote to jump to the specified page Uri
		/// </summary>
		/// <param name="pageTag"></param>
		public void NavigateTo (string pageTag)
		{
			if (pageTag.StartsWith("onenote:"))
			{
				Application.NavigateToUrl(pageTag);
			}
			else
			{
				Application.NavigateTo(pageTag);
			}
		}



		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Editing...


		// https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote#deletehierarchy-method

		/// <summary>
		/// Deletes the given object(s) from the hierarchy; used for merging
		/// </summary>
		/// <param name="element"></param>
		public void DeleteHierarchy(string objectId)
		{
			try
			{
				Application.DeleteHierarchy(objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"MGR ERROR deleting hierarchy object {objectId}", exc);
			}
		}


		// https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote#updatehierarchy-method

		/// <summary>
		/// Update the hierarchy info with the given XML; used for sorting
		/// </summary>
		/// <param name="element"></param>
		public void UpdateHierarchy (XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				Application.UpdateHierarchy(xml);
			}
			catch (Exception exc)
			{
				logger.WriteLine("MGR ERROR updating hierarchy", exc);
				logger.WriteLine(element.ToString());
				logger.WriteLine();
			}
		}


		/// <summary>
		/// Update the current page content with the given XML
		/// </summary>
		/// <param name="element"></param>
		public void UpdatePageContent (XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				Application.UpdatePageContent(xml, DateTime.MinValue, XMLSchema.xs2013, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine("MGR ERROR updating page content", exc);
				logger.WriteLine(element.ToString());
				logger.WriteLine();
			}
		}


		/// <summary>
		/// Determines if Office is set to Black color theme.
		/// </summary>
		/// <returns>True if Black theme is set; otherwise false</returns>
		public static bool OfficeSetToBlackTheme()
		{
			var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\16.0\Common");
			if (key != null)
			{
				var theme = key.GetValue("UI Theme") as Int32?;
				if (theme == null)
				{
					theme = key.GetValue("Theme") as Int32?;
				}

				if (theme != null)
				{
					/*
					Colorful   0
					Dark Gray  3
					Black      4
					White      5
					*/

					return theme == 4;
				}
			}

			return false;
		}
	}
}
