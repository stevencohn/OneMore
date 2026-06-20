//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using System.Collections.Generic;


	/// <summary>
	/// A named collection windows describing the layout.
	/// </summary>
	internal sealed class Layout
	{
		public Layout()
		{
			Windows = new();
		}

		/// <summary>
		/// The layout ID, auto-assigned by the DB
		/// </summary>
		public int LayoutID { get; set; }

		/// <summary>
		/// The user-supplied name of the layout.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The list of windows within this layout.
		/// </summary>
		public List<LayoutWindow> Windows { get; set; }
	}


	/// <summary>
	/// A single window within a layout, targeting a single page, section, or section group.
	/// </summary>
	internal sealed class LayoutWindow : ITargetReference
	{
		/// <summary>
		/// Database ID of this layout, used for updates and deletes.
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Database ID of the layout that contains this window.
		/// </summary>
		public int LayoutID { get; set; }

		/// <summary>
		/// The target page name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// User supplied friendly name or alias of the page, or null if the user has
		/// not overridden the name. Callers that display this value should fall back to
		/// Name, e.g. "window.Alias ?? window.Name".
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// The full hierarchy path of the page.
		/// </summary>
		public string Location { get; set; }

		/// <summary>
		/// The onenote:// URL of the page, which may be used as a hyperlink or for 
		/// verification. This is the raw URL as stored in the database and may not be valid 
		/// if the page is no longer reachable at that URL.
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// The ID of the notebook that contains the page.
		/// </summary>
		public string NotebookID { get; set; }

		/// <summary>
		/// The ID of the section or section group that contains the page.
		/// </summary>
		public string SectionID { get; set; }

		/// <summary>
		/// The ID of the page to show in this window.
		/// </summary>
		public string PageID { get; set; }

		/// <summary>
		/// The custom z-order of the window.
		/// </summary>
		public int ZOrder { get; set; }

		/// <summary>
		/// The name of the display device (monitor) the window was on when saved, e.g.
		/// "\\.\DISPLAY1", or null if not captured.
		/// </summary>
		public string Device { get; set; }

		/// <summary>
		/// The saved left edge of the window, in absolute desktop coordinates, or null if
		/// window geometry was not captured.
		/// </summary>
		public int? WinLeft { get; set; }

		/// <summary>
		/// The saved top edge of the window, in absolute desktop coordinates, or null if
		/// window geometry was not captured.
		/// </summary>
		public int? WinTop { get; set; }

		/// <summary>
		/// The saved right edge of the window, in absolute desktop coordinates, or null if
		/// window geometry was not captured.
		/// </summary>
		public int? WinRight { get; set; }

		/// <summary>
		/// The saved bottom edge of the window, in absolute desktop coordinates, or null if
		/// window geometry was not captured.
		/// </summary>
		public int? WinBottom { get; set; }

		// runtime only properties

		/// <summary>
		/// The Verified status of the window's target.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public TargetStatus Status { get; set; }
	}


	/// <summary>
	/// The Layouts ribbon drop-down menu with windows layout.
	/// </summary>
	internal sealed class LayoutsCollection
	{
		public LayoutsCollection()
		{
			Layouts = new();
		}

		/// <summary>
		/// Version of this data model schema. Used for upgrade paths in the future.
		/// </summary>
		public int SchemaVersion { get; set; } = 1;

		/// <summary>
		/// The layouts.
		/// </summary>
		public List<Layout> Layouts { get; set; }
	}
}
