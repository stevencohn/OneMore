//************************************************************************************************
// Copyright © 2026 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A borderless, non-activating, mouse-transparent popup that describes the reminders of
	/// a page. It shows full details of the most pressing reminder and a count of the rest.
	/// The popup's top-left corner is anchored at the bottom-right of the bell icon, flipping
	/// to the opposite corner where it would otherwise leave the screen.
	/// </summary>
	internal sealed class ReminderPopup : Form
	{
		private const int WS_EX_TOOLWINDOW = 0x00000080;
		private const int WS_EX_NOACTIVATE = 0x08000000;
		private const int CS_DROPSHADOW = 0x00020000;
		private const int WM_NCHITTEST = 0x0084;
		private const int HTTRANSPARENT = -1;

		private const TextFormatFlags TextFlags =
			TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
			TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding |
			TextFormatFlags.VerticalCenter;

		private Font boldFont;
		private Rectangle subjectBox;
		private Rectangle dueLabelBox, dueBox;
		private Rectangle statusLabelBox, statusBox;
		private Rectangle percentLabelBox, percentBox;
		private Rectangle barBox;
		private Rectangle footerBox;
		private int separatorY;
		private string subject, due, status, percent, footer;
		private int percentValue;


		public ReminderPopup()
		{
			AutoScaleMode = AutoScaleMode.None;
			DoubleBuffered = true;
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			TopMost = true;
		}


		protected override bool ShowWithoutActivation => true;


		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;
			}
		}


		protected override void WndProc(ref Message m)
		{
			// never take the mouse; events fall through to the calendar underneath
			if (m.Msg == WM_NCHITTEST)
			{
				m.Result = (IntPtr)HTTRANSPARENT;
				return;
			}

			base.WndProc(ref m);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				boldFont?.Dispose();
			}

			base.Dispose(disposing);
		}


		/// <summary>
		/// Shows the popup for the given page, anchored to the given bell.
		/// </summary>
		/// <param name="owner">The control hosting the bell</param>
		/// <param name="page">The page whose reminders to describe</param>
		/// <param name="bell">The bounds of the bell in the coordinates of owner</param>
		public void ShowFor(Control owner, CalendarPage page, Rectangle bell)
		{
			var reminders = page.Reminders;
			if (reminders.Count == 0)
			{
				return;
			}

			var reminder = Pick(reminders);

			var theme = ThemeProvider.Instance;
			BackColor = theme.BackColor;
			ForeColor = theme.ForeColor;
			Font = owner.Font;

			boldFont?.Dispose();
			boldFont = new Font(Font, FontStyle.Bold);

			subject = string.IsNullOrWhiteSpace(reminder.Subject) ? page.Title : reminder.Subject;
			due = reminder.Due.ToLocalTime().ToString("g");
			status = StatusName(reminder.Status);
			percentValue = Math.Max(0, Math.Min(100, reminder.Percent));
			percent = $"{percentValue}%";

			var more = reminders.Count - 1;
			footer = more == 0 ? null
				: more == 1 ? Properties.Resources.ReminderPopup_MoreOne
				: string.Format(Properties.Resources.ReminderPopup_MoreMany, more);

			var size = Arrange(owner.DeviceDpi / 96f);
			var screenBell = owner.RectangleToScreen(bell);
			var area = Screen.FromRectangle(screenBell).WorkingArea;

			// top-left corner at the bottom-right of the bell...
			var x = screenBell.Right;
			var y = screenBell.Bottom;

			// ...flipping to stay on screen
			if (x + size.Width > area.Right)
			{
				x = screenBell.Left - size.Width;
			}

			if (y + size.Height > area.Bottom)
			{
				y = screenBell.Top - size.Height;
			}

			Bounds = new Rectangle(
				Math.Max(area.Left, x), Math.Max(area.Top, y), size.Width, size.Height);

			if (Visible)
			{
				Invalidate();
			}
			else
			{
				Show(owner.FindForm());
			}
		}


		/// <summary>
		/// Choose the most pressing reminder: the unfinished one due soonest or, if all are
		/// finished, the one most recently due.
		/// </summary>
		private static Reminder Pick(List<Reminder> reminders)
		{
			Reminder open = null;
			Reminder last = null;

			foreach (var reminder in reminders)
			{
				if (reminder.Status != ReminderStatus.Completed &&
					(open is null || reminder.Due < open.Due))
				{
					open = reminder;
				}

				if (last is null || reminder.Due > last.Due)
				{
					last = reminder;
				}
			}

			return open ?? last;
		}


		private static string StatusName(ReminderStatus value)
		{
			var names = River.OneMoreAddIn.Properties.Resources.RemindDialog_statusBox_Text
				.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			var index = (int)value;
			return index >= 0 && index < names.Length ? names[index] : value.ToString();
		}


		// measures the content and lays out the boxes, returning the size of the popup
		private Size Arrange(float scale)
		{
			int S(int value) => (int)Math.Round(value * scale);

			var pad = S(8);
			var gap = S(6);
			var maxText = S(300);
			var lineGap = S(2);

			Size Measure(string text, Font font)
			{
				return TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
					TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			}

			var dueLabel = Properties.Resources.ReminderPopup_Due;
			var statusLabel = Properties.Resources.ReminderPopup_Status;
			var percentLabel = Properties.Resources.ReminderPopup_Complete;

			var subjectSize = Measure(subject, boldFont);
			var labelWidth = Math.Max(Measure(dueLabel, Font).Width,
				Math.Max(Measure(statusLabel, Font).Width, Measure(percentLabel, Font).Width));

			var lineHeight = subjectSize.Height;
			var dueSize = Measure(due, Font);
			var statusSize = Measure(status, Font);
			var percentSize = Measure(percent, Font);

			var barWidth = S(80);
			var barHeight = S(6);

			var valueWidth = Math.Max(dueSize.Width,
				Math.Max(statusSize.Width, percentSize.Width + gap + barWidth));

			var contentWidth = Math.Min(maxText,
				Math.Max(subjectSize.Width, labelWidth + gap + valueWidth));

			if (footer is not null)
			{
				contentWidth = Math.Min(maxText, Math.Max(contentWidth, Measure(footer, Font).Width));
			}

			var y = pad;
			subjectBox = new Rectangle(pad, y, contentWidth, lineHeight);
			y += lineHeight + lineGap * 2;

			var valueX = pad + labelWidth + gap;
			var valueBoxWidth = Math.Max(0, contentWidth - labelWidth - gap);

			dueLabelBox = new Rectangle(pad, y, labelWidth, lineHeight);
			dueBox = new Rectangle(valueX, y, valueBoxWidth, lineHeight);
			y += lineHeight + lineGap;

			statusLabelBox = new Rectangle(pad, y, labelWidth, lineHeight);
			statusBox = new Rectangle(valueX, y, valueBoxWidth, lineHeight);
			y += lineHeight + lineGap;

			percentLabelBox = new Rectangle(pad, y, labelWidth, lineHeight);
			percentBox = new Rectangle(valueX, y, percentSize.Width, lineHeight);
			barBox = new Rectangle(
				valueX + percentSize.Width + gap, y + (lineHeight - barHeight) / 2,
				Math.Min(barWidth, Math.Max(0, valueBoxWidth - percentSize.Width - gap)), barHeight);
			y += lineHeight;

			if (footer is not null)
			{
				y += lineGap * 2;
				separatorY = y;
				y += lineGap * 2;
				footerBox = new Rectangle(pad, y, contentWidth, lineHeight);
				y += lineHeight;
			}
			else
			{
				separatorY = -1;
			}

			return new Size(contentWidth + pad * 2, y + pad);
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (subject is null)
			{
				return;
			}

			var theme = ThemeProvider.Instance;
			var g = e.Graphics;

			using (var border = new Pen(theme.Border))
			{
				g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
			}

			var muted = Color.Gray;

			TextRenderer.DrawText(g, subject, boldFont, subjectBox, ForeColor, TextFlags);

			TextRenderer.DrawText(g, Properties.Resources.ReminderPopup_Due, Font, dueLabelBox, muted, TextFlags);
			TextRenderer.DrawText(g, due, Font, dueBox, ForeColor, TextFlags);

			TextRenderer.DrawText(g, Properties.Resources.ReminderPopup_Status, Font, statusLabelBox, muted, TextFlags);
			TextRenderer.DrawText(g, status, Font, statusBox, ForeColor, TextFlags);

			TextRenderer.DrawText(g, Properties.Resources.ReminderPopup_Complete, Font, percentLabelBox, muted, TextFlags);
			TextRenderer.DrawText(g, percent, Font, percentBox, ForeColor, TextFlags);

			if (barBox.Width > 0)
			{
				using var track = new SolidBrush(theme.MonthGrid);
				g.FillRectangle(track, barBox);

				using var fill = new SolidBrush(theme.Highlight);
				g.FillRectangle(fill, barBox.X, barBox.Y, barBox.Width * percentValue / 100, barBox.Height);
			}

			if (footer is not null)
			{
				using var line = new Pen(theme.MonthGrid);
				g.DrawLine(line, subjectBox.Left, separatorY, subjectBox.Right, separatorY);
				TextRenderer.DrawText(g, footer, Font, footerBox, muted, TextFlags);
			}
		}
	}
}
