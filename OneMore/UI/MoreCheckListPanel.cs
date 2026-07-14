//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A bordered composite of a "Select all | Select none" link pair, a MoreCheckList
	/// body, and a caption label below - the repeated shape used by each step of a
	/// checklist-driven wizard (e.g. the Import Outlook Contacts wizard's Folders,
	/// Categories, and Contacts steps). The owner configures the hosted List's columns,
	/// cell styling/images, and row population; this panel only wires up bulk
	/// select-all/none and bubbles row-level changes.
	/// </summary>
	internal sealed class MoreCheckListPanel : Panel
	{
		private readonly MoreCheckList list;
		private readonly MoreLabel caption;


		public MoreCheckListPanel()
		{
			BorderStyle = BorderStyle.None;

			caption = new MoreLabel
			{
				Dock = DockStyle.Bottom,
				AutoSize = false,
				Height = 20,
				TextAlign = ContentAlignment.MiddleLeft,
				Visible = false
			};

			list = new MoreCheckList
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false
			};
			list.CheckChanged += (s, e) => SelectionChanged?.Invoke(this, EventArgs.Empty);

			var selectAllLink = new MoreLinkLabel
			{
				AutoSize = true,
				Margin = new Padding(4),
				Text = Resx.phrase_SelectAll
			};
			selectAllLink.LinkClicked += (s, e) => SetAllChecked(true);

			var selectNoneLink = new MoreLinkLabel
			{
				AutoSize = true,
				Margin = new Padding(4),
				Text = Resx.phrase_SelectNone
			};
			selectNoneLink.LinkClicked += (s, e) => SetAllChecked(false);

			var separator = new MoreLabel
			{
				AutoSize = true,
				Margin = new Padding(4),
				Text = "|"
			};

			var headerPanel = new MoreFlowLayoutPanel
			{
				Dock = DockStyle.Top,
				FlowDirection = FlowDirection.RightToLeft,
				WrapContents = false,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};

			// RightToLeft flow places the first-added control at the right edge, so add in
			// reverse of the desired "Select all | Select none" reading order
			headerPanel.Controls.Add(selectNoneLink);
			headerPanel.Controls.Add(separator);
			headerPanel.Controls.Add(selectAllLink);

			Controls.Add(list);
			Controls.Add(headerPanel);
			Controls.Add(caption);
		}


		/// <summary>
		/// Gets the list of checkable rows hosted by this panel. The owner configures its
		/// columns, cell styling/images, and row population directly on this list.
		/// </summary>
		public MoreCheckList List => list;


		/// <summary>
		/// Gets or sets the caption text shown below the list, e.g. "3 of 5 folders selected".
		/// </summary>
		public string Caption
		{
			get => caption.Text;
			set
			{
				caption.Text = value;
				caption.Visible = !string.IsNullOrEmpty(value);
			}
		}


		/// <summary>
		/// Raised whenever "Select all"/"Select none" or an individual row changes a
		/// row's checked state.
		/// </summary>
		public event EventHandler SelectionChanged;


		private void SetAllChecked(bool check)
		{
			list.BeginUpdate();
			foreach (ListViewItem item in list.Items)
			{
				item.Checked = check;
			}
			list.EndUpdate();

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
