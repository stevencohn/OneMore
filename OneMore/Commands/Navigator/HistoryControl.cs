//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using HierarchyInfo = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// Hosted control to be used in the pinned and history MoreListViews
	/// </summary>
	internal class HistoryControl : UserControl, IChameleon, IThemedControl
	{
		private readonly PictureBox picture;
		private readonly MoreLinkLabel link;


		public HistoryControl(HierarchyInfo info)
		{
			picture = new PictureBox
			{
				Image = Resx.SectionMask.MapColor(Color.Black, ColorHelper.FromHtml(info.Color)),
				Dock = DockStyle.Left,
				Padding = new(5, 0, 0, 0),
				Width = 30
			};

			link = new MoreLinkLabel
			{
				Dock = DockStyle.Fill,
				Text = info.Name,
				Tag = info,
				Font = new("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point),
				Padding = new(0),
				Margin = new(4, 0, 0, 0)
			};

			link.LinkClicked += new LinkLabelLinkClickedEventHandler(async (s, e) =>
			{
				if (s is MoreLinkLabel label)
				{
					var info = (HierarchyInfo)label.Tag;

					// Update the view immediately; this breaks the space-time continuum,
					// not having to wait for the next update, but provides a better UX
					NavigatorWindow.SetVisited(info.PageId);

					await using var one = new OneNote();
					await one.NavigateTo(info.Link);
				}
			});

			// history items should have a Visited value but pinned items would not
			if (info.Visited > 0)
			{
				var tip = new ToolTip();
				var visited = DateTimeHelper.FromTicksSeconds(info.Visited).ToFriendlyString();
				tip.SetToolTip(link, $"{info.Path}\n{visited}");
			}

			BackColor = Color.Transparent;
			Width = 100;
			Height = 24;
			Margin = new Padding(0, 2, 0, 2);

			BackColorChanged += new EventHandler((s, e) =>
			{
				picture.BackColor = ((Control)s).BackColor;
				link.BackColor = ((Control)s).BackColor;
			});

			Controls.Add(link);
			Controls.Add(picture);
		}


		public override string Text { get => link.Text; set => link.Text = value; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		public void ApplyBackground(Color color)
		{
			BackColor = color;
			link.BackColor = color;
		}


		public void ApplyTheme(ThemeManager manager)
		{
			picture.BackColor = BackColor;
			link.BackColor = BackColor;

			var color = manager.GetColor("LinkColor");
			link.ForeColor = color;
			link.LinkColor = color;
			link.VisitedLinkColor = color;

			link.HoverColor = manager.GetColor("HoverColor");
		}


		public void ResetBackground()
		{
			BackColor = Color.Transparent;
			link.BackColor = Color.Transparent;
		}


		public void SetTitle(string title)
		{
			link.Text = title;
		}
	}
}
