//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.Office.Interop.OneNote;
	using System;

	public class MockWindow : Window
	{
		public MockWindow(IApplication application)
		{
			Application = application;
			DockedLocation = DockLocation.dlDefault;
		}

		public void NavigateTo(string bstrHierarchyObjectID, string bstrObjectID = "")
		{
			throw new NotImplementedException();
		}

		public void NavigateToUrl(string bstrUrl)
		{
			throw new NotImplementedException();
		}

		public void SetDockedLocation(DockLocation DockLocation, tagPOINT ptMonitor)
		{
			DockedLocation = DockLocation;
		}

		public ulong WindowHandle => 0x00001234;

		public string CurrentPageId => Mocks.CurrentPageId;

		public string CurrentSectionId => Mocks.CurrentSectionId;

		public string CurrentSectionGroupId => Mocks.CurrentSectionGroupId;

		public string CurrentNotebookId => Mocks.CurrentNotebookId;

		public bool FullPageView { get { return false; } set { } }

		public bool Active { get { return true; } set { } }

		public DockLocation DockedLocation { get; set; }

		public IApplication Application { get; private set; }

		public bool SideNote => false;
	}
}