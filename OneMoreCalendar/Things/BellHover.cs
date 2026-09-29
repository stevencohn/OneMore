//************************************************************************************************
// Copyright © 2026 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Shows a ReminderPopup once the mouse has rested on a reminder bell for the Windows
	/// system hover time. Views feed it mouse movement and it takes care of the delay.
	/// </summary>
	internal sealed class BellHover : IDisposable
	{
		private readonly Control owner;
		private readonly Timer timer;
		private ReminderPopup popup;
		private CalendarPage page;
		private Rectangle bell;
		private Point anchor;


		public BellHover(Control owner)
		{
			this.owner = owner;

			timer = new Timer();
			timer.Tick += Elapsed;
		}


		/// <summary>
		/// Reports the pointer location; call on every mouse move over the owner.
		/// </summary>
		/// <param name="location">Pointer location in the coordinates of owner</param>
		/// <param name="hovered">The page whose bell is under the pointer, or null</param>
		/// <param name="bounds">The bell bounds in the coordinates of owner</param>
		public void Track(Point location, CalendarPage hovered, Rectangle bounds)
		{
			if (hovered is null)
			{
				Cancel();
				return;
			}

			if (hovered == page && bounds == bell)
			{
				// still on the same bell; a visible popup stays put, otherwise restart the
				// delay if the pointer has moved beyond the system hover tolerance
				if (popup is not null && popup.Visible)
				{
					return;
				}

				var size = SystemInformation.MouseHoverSize;
				var tolerance = new Rectangle(
					anchor.X - size.Width / 2, anchor.Y - size.Height / 2, size.Width, size.Height);

				if (!tolerance.Contains(location))
				{
					anchor = location;
					Restart();
				}

				return;
			}

			// a different bell always waits out the delay, no instant swapping
			Cancel();
			page = hovered;
			bell = bounds;
			anchor = location;
			Restart();
		}


		/// <summary>
		/// Cancels a pending popup and hides one that is showing
		/// </summary>
		public void Cancel()
		{
			timer.Stop();
			page = null;
			bell = Rectangle.Empty;
			popup?.Hide();
		}


		private void Restart()
		{
			timer.Stop();
			timer.Interval = Math.Max(1, SystemInformation.MouseHoverTime);
			timer.Start();
		}


		private void Elapsed(object sender, EventArgs e)
		{
			timer.Stop();

			if (page is null || !bell.Contains(owner.PointToClient(Cursor.Position)))
			{
				return;
			}

			popup ??= new ReminderPopup();
			popup.ShowFor(owner, page, bell);
		}


		public void Dispose()
		{
			timer.Dispose();
			popup?.Dispose();
		}
	}
}
