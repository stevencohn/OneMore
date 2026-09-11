//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// One row of the Fonts tab's role list: a role label plus a one-line summary of its
	/// current setting. Clicking the row raises Selected so the owning dialog can load the
	/// role into the detail panel; IsSelected drives a left accent bar + tinted background.
	/// </summary>
	internal sealed class ThemeFontRoleRow : Panel
	{
		private const int AccentWidth = 3;

		private readonly MoreLabel titleLabel;
		private readonly MoreLabel summaryLabel;
		private bool selected;


		public ThemeFontRoleRow(string roleName)
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);

			Height = 44;
			Cursor = Cursors.Hand;
			Padding = new Padding(AccentWidth + 9, 6, 8, 6);

			titleLabel = new MoreLabel
			{
				AutoSize = false,
				Dock = DockStyle.Top,
				Height = 20,
				Cursor = Cursors.Hand,
				Font = new Font(Font, FontStyle.Bold),
				Text = roleName
			};
			Controls.Add(titleLabel);

			summaryLabel = new MoreLabel
			{
				AutoSize = false,
				Dock = DockStyle.Top,
				Height = 18,
				Cursor = Cursors.Hand,
				ThemedFore = "GrayText",
				Text = string.Empty
			};
			Controls.Add(summaryLabel);

			titleLabel.Click += (s, e) => OnClick(EventArgs.Empty);
			summaryLabel.Click += (s, e) => OnClick(EventArgs.Empty);

			ApplyColors();
		}


		/// <summary>
		/// Raised when this row is clicked (directly, or via one of its child labels).
		/// </summary>
		public event EventHandler Selected;


		public string Summary
		{
			get => summaryLabel.Text;
			set => summaryLabel.Text = value;
		}


		public bool IsSelected
		{
			get => selected;
			set
			{
				if (selected != value)
				{
					selected = value;
					ApplyColors();
					Invalidate();
				}
			}
		}


		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			Selected?.Invoke(this, EventArgs.Empty);
		}


		private void ApplyColors()
		{
			var manager = ThemeManager.Instance;

			BackColor = selected
				? manager.GetColor("LinkHighlight")
				: manager.GetColor("Window");

			titleLabel.BackColor = BackColor;
			titleLabel.ForeColor = manager.GetColor("ControlText");

			summaryLabel.BackColor = BackColor;
			summaryLabel.ForeColor = manager.GetColor("GrayText");
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (selected)
			{
				using var brush = new SolidBrush(ThemeManager.Instance.GetColor("HotTrack"));
				e.Graphics.FillRectangle(brush, 0, 0, AccentWidth, Height);
			}
		}
	}
}
