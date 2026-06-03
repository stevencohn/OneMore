//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	internal class BoundsInfo
	{
		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }
	}


	internal class WindowInfo
	{
		public bool IsCurrent { get; set; }
		// populated only when IsCurrent is true
		public string CurrentNotebookId { get; set; }
		public string CurrentPageId { get; set; }
		public string CurrentSectionId { get; set; }
		public string CurrentSectionGroupId { get; set; }
		public string CurrentPage { get; set; }
		public string DockedLocation { get; set; }
		public bool? IsFullPageView { get; set; }
		public bool? IsSideNote { get; set; }
		// populated for every window
		public bool Active { get; set; }
		public uint ProcessId { get; set; }
		public uint ThreadId { get; set; }
		public string WindowHandle { get; set; }
		public BoundsInfo Bounds { get; set; }
	}
}
