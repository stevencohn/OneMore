//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Hosts a MoreMonthCalendar in a ToolStripDropDown so it floats below a
	/// MoreDateTimePicker, closing when the user clicks elsewhere.
	/// </summary>
	internal sealed class MoreCalendarDropDown : IDisposable
	{
		private sealed class FlatRenderer : ToolStripProfessionalRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
				// the calendar paints its own border
			}


			protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
			{
				using var brush = new SolidBrush(
					ThemeManager.Instance.GetColor("Window"));

				e.Graphics.FillRectangle(brush, e.AffectedBounds);
			}
		}


		// a click on the owner while open closes the drop-down with AppClicked; the same
		// click would then immediately reopen it, so ignore clicks that shortly follow a close
		private const int ReopenGuardMs = 250;

		private readonly ToolStripDropDown dropDown;
		private int closedTick;
		private bool closedByClick;
		private bool disposed;


		public MoreCalendarDropDown()
		{
			Calendar = new MoreMonthCalendar
			{
				// the picker selects a single date
				MaxSelectionCount = 1,
				TabStop = true
			};

			dropDown = new ToolStripDropDown
			{
				AutoClose = true,
				DropShadowEnabled = false,
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				Renderer = new FlatRenderer()
			};

			dropDown.Items.Add(new ToolStripControlHost(Calendar)
			{
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				AutoSize = false
			});

			dropDown.Closed += (s, e) =>
			{
				closedTick = Environment.TickCount;
				closedByClick = e.CloseReason == ToolStripDropDownCloseReason.AppClicked;
				Closed?.Invoke(this, e);
			};
		}


		/// <summary>
		/// Raised after the drop-down has closed, for any reason.
		/// </summary>
		public event ToolStripDropDownClosedEventHandler Closed;


		/// <summary>
		/// Gets the hosted calendar.
		/// </summary>
		public MoreMonthCalendar Calendar { get; }


		public bool Visible => dropDown.Visible;


		/// <summary>
		/// Gets a value indicating that the drop-down was just closed by a click on the
		/// application, i.e. probably the very click that is now being processed by the
		/// picker's drop button.
		/// </summary>
		public bool JustClosedByClick =>
			closedByClick && unchecked(Environment.TickCount - closedTick) < ReopenGuardMs;


		/// <summary>
		/// Shows the calendar under the owner control.
		/// </summary>
		public void Show(
			Control owner, DateTime value, DateTime min, DateTime max, LeftRightAlignment align)
		{
			if (disposed || dropDown.Visible)
			{
				return;
			}

			closedByClick = false;

			Calendar.MinDate = DateTime.MinValue;   // avoid min > current max while reassigning
			Calendar.MaxDate = DateTime.MaxValue;
			Calendar.MinDate = min;
			Calendar.MaxDate = max;
			Calendar.SeedDate(value);
			Calendar.ApplyTheme(ThemeManager.Instance);

			var size = Calendar.GetPreferredSize(Size.Empty);
			Calendar.Size = size;

			var host = dropDown.Items[0];
			host.Size = size;
			dropDown.Size = size;

			var x = align == LeftRightAlignment.Right ? owner.Width - size.Width : 0;
			var y = owner.Height;

			// show above the owner if there is no room below
			var area = Screen.FromControl(owner).WorkingArea;
			var below = owner.PointToScreen(new Point(0, owner.Height));
			if (below.Y + size.Height > area.Bottom)
			{
				var above = owner.PointToScreen(Point.Empty).Y - size.Height;
				if (above >= area.Top)
				{
					y = -size.Height;
				}
			}

			dropDown.Show(owner, new Point(x, y));
			Calendar.Focus();
		}


		public void Close()
		{
			if (!disposed && dropDown.Visible)
			{
				dropDown.Close(ToolStripDropDownCloseReason.CloseCalled);
			}
		}


		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				dropDown.Dispose();
			}
		}
	}
}
