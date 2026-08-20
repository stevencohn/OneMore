//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;
	using HierarchyInfo = OneNote.HierarchyInfo;


	/// <summary>
	/// Hosted control to be used in the pinned and history MoreListViews
	/// </summary>
	internal class HistoryControl : UserControl, IChameleon, IThemedControl
	{
		/// <summary>
		/// A thin self-drawn vertical bar indicating the section color, matching the
		/// treatment used by SearchResultsCardView instead of a color-shifted PNG mask.
		/// For paragraph references, draws diagonal stripes instead of solid color.
		/// </summary>
		private sealed class ColorBar : Panel
		{
			public Color BarColor { get; set; }
			public bool IsStriped { get; set; }

			protected override void OnPaint(PaintEventArgs e)
			{
				if (BarColor == Color.Empty || BarColor == Color.Transparent)
				{
					return;
				}

				if (IsStriped)
				{
					using var brush = new HatchBrush(HatchStyle.BackwardDiagonal, BarColor, Color.Transparent);
					e.Graphics.FillRectangle(brush, ClientRectangle);
				}
				else
				{
					using var brush = new SolidBrush(BarColor);
					e.Graphics.FillRectangle(brush, ClientRectangle);
				}
			}
		}


		private readonly ColorBar bar;
		private readonly MoreLinkLabel link;
		private readonly Font linkFont;
		private EventHandler backColorChangedHandler;
		private ToolTip tip;


		public HistoryControl(HierarchyInfo info)
		{
			bar = new ColorBar
			{
				Dock = DockStyle.Left,
				Width = 8,
				BarColor = ColorHelper.FromHtml(info.Color),
				IsStriped = !string.IsNullOrEmpty(info.ObjectId)
			};

			linkFont = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);

			link = new MoreLinkLabel
			{
				Dock = DockStyle.Fill,
				Text = info.Name,
				Tag = info,
				Font = linkFont,
				Padding = new(4, 0, 0, 0),
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
				tip = new ToolTip();
				var visited = DateTimeHelper.FromTicksSeconds(info.Visited).ToFriendlyString();
				tip.SetToolTip(link, $"{info.Path}\n{visited}");
			}

			BackColor = Color.Transparent;
			Width = 100;
			Height = 24;
			Margin = new Padding(0, 2, 0, 2);

			backColorChangedHandler = (s, e) =>
			{
				link.BackColor = ((Control)s).BackColor;
			};
			BackColorChanged += backColorChangedHandler;

			Controls.Add(link);
			Controls.Add(bar);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				tip?.Dispose();
				bar?.Dispose();
				link?.Dispose();
				linkFont?.Dispose();
				BackColorChanged -= backColorChangedHandler;
			}

			base.Dispose(disposing);
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
			((ILoadControl)link).OnLoad();
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
