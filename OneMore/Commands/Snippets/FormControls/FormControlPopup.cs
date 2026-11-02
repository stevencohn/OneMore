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
	/// Base for the frameless popups shown in place when the user clicks a form control
	/// hyperlink. Provides a thin border, a close "X" row across the top, Esc to cancel,
	/// dismissal when focus is lost, and placement below a screen point kept on screen.
	/// </summary>
	internal class FormControlPopup : MoreForm
	{
		private const int CloseRowHeight = 24;
		private const int Gap = 4;

		private readonly Point origin;
		private readonly float scale;
		private readonly Label closeLabel;
		private bool closing;
		private int shownTick;


		/// <summary>
		/// Initializes a new popup
		/// </summary>
		/// <param name="origin">
		/// Screen point of the hyperlink start; the popup appears just below it
		/// </param>
		protected FormControlPopup(Point origin)
		{
			this.origin = origin;

			AutoScaleMode = AutoScaleMode.None;
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ManualLocation = true;
			ShowInTaskbar = false;
			ApplyDialogOpacity = false;
			KeyPreview = true;
			TopMost = true;
			Padding = new Padding(1);

			scale = Scaling.GetDpi(this) / 96f;

			var theme = ThemeManager.Instance;
			BackColor = theme.GetColor("Window");
			ForeColor = theme.GetColor("WindowText");

			closeLabel = new Label
			{
				AutoSize = false,
				Text = "✕",
				Font = new Font("Segoe UI Symbol", 9F),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand,
				BackColor = BackColor,
				ForeColor = ForeColor,
				Size = new Size(S(CloseRowHeight), S(CloseRowHeight)),
				Dock = DockStyle.None
			};

			closeLabel.Click += (s, e) => Dismiss();
			closeLabel.MouseEnter += (s, e) => closeLabel.BackColor = theme.GetColor("HoverColor");
			closeLabel.MouseLeave += (s, e) => closeLabel.BackColor = BackColor;
			Controls.Add(closeLabel);
		}


		/// <summary>
		/// Gets the height of the close row; content is placed below it.
		/// </summary>
		protected int CloseRow => S(CloseRowHeight);


		protected int S(int value) => (int)(value * scale);


		/// <summary>
		/// Sizes the popup to its content, positions it and keeps it on screen. Call once the
		/// content control has been added and sized.
		/// </summary>
		/// <param name="contentSize">The size of the content below the close row</param>
		protected void Place(Size contentSize)
		{
			var size = new Size(
				contentSize.Width + Padding.Horizontal,
				contentSize.Height + CloseRow + Padding.Vertical);

			ClientSize = size;
			closeLabel.Location = new Point(size.Width - Padding.Right - closeLabel.Width, Padding.Top);

			var area = Screen.FromPoint(origin).WorkingArea;

			var x = origin.X;
			var y = origin.Y + S(Gap);

			if (x + size.Width > area.Right) { x = area.Right - size.Width; }
			if (x < area.Left) { x = area.Left; }

			// no room below? show above the hyperlink instead
			if (y + size.Height > area.Bottom)
			{
				var above = origin.Y - size.Height - S(Gap * 3);
				y = above >= area.Top ? above : area.Bottom - size.Height;
			}

			Location = new Point(x, y);
		}


		protected override CreateParams CreateParams
		{
			get
			{
				// force a drop-shadow on the borderless window
				const int CS_DROPSHADOW = 0x20000;
				var cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;
			}
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Dismiss();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		protected override void OnShown(System.EventArgs e)
		{
			base.OnShown(e);
			shownTick = Environment.TickCount;
			Native.SetForegroundWindow(Handle);
			Activate();
		}


		protected override void OnDeactivate(System.EventArgs e)
		{
			base.OnDeactivate(e);

			// clicking elsewhere dismisses the popup
			// ignore the focus churn while the popup is still taking foreground from OneNote
			if (!closing && Visible && unchecked(Environment.TickCount - shownTick) > 400)
			{
				Dismiss();
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			using var pen = new Pen(ThemeManager.Instance.GetColor("Border"));
			e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
		}


		/// <summary>
		/// Closes the popup, with OK if it chose a value or Cancel otherwise
		/// </summary>
		protected void Dismiss(bool chosen = false)
		{
			if (closing)
			{
				return;
			}

			closing = true;
			DialogResult = chosen ? DialogResult.OK : DialogResult.Cancel;
			Close();
		}
	}
}
