//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;
	using Forms = System.Windows.Forms;


	internal class ApplicationManager : IDisposable
	{

		private Application application;
		private bool disposedValue = false;
		private readonly ILogger logger;


		//========================================================================================
		// Lifecycle
		//========================================================================================

		public ApplicationManager ()
		{
			application = new Application();
			logger = Logger.Current;
		}


		protected virtual void Dispose (bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					application = null;
				}

				disposedValue = true;
			}
		}


		public void Dispose ()
		{
			Dispose(true);
		}


		//========================================================================================
		// Properties
		//========================================================================================

		public Application Application => application;


		public Forms.IWin32Window Window => Forms.Control.FromHandle(WindowHandle);


		public IntPtr WindowHandle => (IntPtr)application.Windows.CurrentWindow.WindowHandle;


		//========================================================================================
		// Scope methods
		//========================================================================================

		// https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote#gethierarchy-method

		public XElement CurrentNotebook ()
		{
			string id = application.Windows.CurrentWindow?.CurrentNotebookId;

			application.GetHierarchy(id, HierarchyScope.hsPages, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		public XElement CurrentSection ()
		{
			string id = application.Windows.CurrentWindow?.CurrentSectionId;

			application.GetHierarchy(id, HierarchyScope.hsPages, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		public XElement CurrentPage (PageInfo info = PageInfo.piSelection)
		{
			return GetPage(application.Windows.CurrentWindow?.CurrentPageId, info);
		}


		public (string Name, string Path, string Link) GetCurrentPageInfo ()
		{
			// name
			string name = null;
			var page = CurrentPage(PageInfo.piBasic);
			if (page != null)
			{
				name = page.Attribute("name")?.Value;

				if (!string.IsNullOrEmpty(name))
				{
					// printable chars only; e.g. remove title emoticon
					name = Regex.Replace(name, @"[^ -~]", "");
				}
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
			var pageId = application.Windows.CurrentWindow?.CurrentPageId;
			string link = null;
			if (!string.IsNullOrEmpty(pageId))
			{
				application.GetHyperlinkToObject(pageId, "", out link);
			}

			return (name, path, link);
		}


		//========================================================================================
		// Special
		//========================================================================================

		public XElement GetPage (string pageId, PageInfo info = PageInfo.piAll)
		{
			if (pageId != null)
			{
				application.GetPageContent(pageId, out var xml, info);

				var root = XElement.Parse(xml);
				return root;
			}

			return null;
		}


		public XElement GetHierarchy (HierarchyScope scope = HierarchyScope.hsPages)
		{
			// get our own copy
			application.GetHierarchy(null, scope, out var xml);
			if (!string.IsNullOrEmpty(xml))
			{
				return XElement.Parse(xml);
			}

			return null;
		}


		public (string backupFolder, string defaultFolder, string unfiledFolder) GetLocations ()
		{
			application.GetSpecialLocation(SpecialLocation.slBackUpFolder, out var backupFolder);
			application.GetSpecialLocation(SpecialLocation.slDefaultNotebookFolder, out var defaultFolder);
			application.GetSpecialLocation(SpecialLocation.slUnfiledNotesSection, out var unfiledFolder);
			return (backupFolder, defaultFolder, unfiledFolder);
		}


		public void NavigateTo (string pageTag)
		{
			if (pageTag.StartsWith("onenote:"))
			{
				application.NavigateToUrl(pageTag);
			}
			else
			{
				application.NavigateTo(pageTag);
			}
		}


		// https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote#updatehierarchy-method

		public void UpdateHierarchy (XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);
			application.UpdateHierarchy(xml);
		}


		public void UpdatePageContent (XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				application.UpdatePageContent(xml);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error updating page content");
				logger.WriteLine(exc);
			}
		}
	}
}
