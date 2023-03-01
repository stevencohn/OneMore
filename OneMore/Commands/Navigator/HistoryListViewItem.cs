//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;
	using HierarchyInfo = OneNote.HierarchyInfo;


	/// <summary>
	/// Hosted control to be used in the pinned and history MoreListViews
	/// </summary>
	internal class HistoryListViewItem : UserControl, IChameleon
	{
		private readonly MoreLinkLabel link;


		public HistoryListViewItem(HierarchyInfo info)
		{
			var picture = new PictureBox
			{
				Image = new Bitmap(24, 24),
				Dock = DockStyle.Left,
				Width = 34
			};

			using var g = Graphics.FromImage(picture.Image);
			g.Clear(SystemColors.Window);

			using var image = Resx.SectionMask;
			image.MapColor(Color.Black, ColorHelper.FromHtml(info.Color));
			g.DrawImage(image, 0, 0, 24, 24);

			link = new MoreLinkLabel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
				ForeColor = SystemColors.WindowText,
				LinkColor = SystemColors.WindowText,
				VisitedLinkColor = SystemColors.WindowText,
				Text = info.Name,
				Tag = info,
				Font = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point),
				Padding = new Padding(0),
				Margin = new Padding(4, 0, 0, 0)
			};

			link.LinkClicked += new LinkLabelLinkClickedEventHandler(async (s, e) =>
			{
				if (s is MoreLinkLabel label)
				{
					var info = (HierarchyInfo)label.Tag;

					// TODO: this breaks the space-time continuum
					NavigatorWindow.SetVisited(info.PageId);

					using var one = new OneNote();
					await one.NavigateTo(info.Link);
					Native.SwitchToThisWindow(one.WindowHandle, false);
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
				link.BackColor = ((Control)s).BackColor;
			});

			Controls.Add(link);
			Controls.Add(picture);
		}


		public void ApplyBackground(Color color)
		{
			BackColor = color;
			link.BackColor = color;
		}


		public void ResetBackground()
		{
			BackColor = Color.Transparent;
			link.BackColor = Color.Transparent;
		}
	}
}
