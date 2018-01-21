//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;


	internal class ApplicationManager : IDisposable
	{

		private Application application;
		private bool disposedValue = false;


		//========================================================================================
		// Lifecycle
		//========================================================================================

		public ApplicationManager ()
		{
			application = new Application();
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

		public Application Application
		{
			get { return application; }
		}


		//========================================================================================
		// Scope methods
		//========================================================================================

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


		//========================================================================================
		// Special
		//========================================================================================

		public XElement GetPage (string pageId, PageInfo info = PageInfo.piAll)
		{
			if (pageId != null)
			{
				string xml;
				application.GetPageContent(pageId, out xml, info);

				var root = XElement.Parse(xml);
				return root;
			}

			return null;
		}


		public XElement GetHierarchy (HierarchyScope scope = HierarchyScope.hsPages)
		{
			// get our own copy
			string xml;
			application.GetHierarchy(null, scope, out xml);
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
				Logger.Current.WriteLine("Error updating page content");
				Logger.Current.WriteLine(exc);
			}
		}
	}
}
