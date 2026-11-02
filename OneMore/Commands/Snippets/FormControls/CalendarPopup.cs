//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;


	/// <summary>
	/// A frameless popup hosting just the OneMore month calendar, with a close row on top.
	/// </summary>
	internal sealed class CalendarPopup : FormControlPopup
	{
		private readonly MoreMonthCalendar calendar;


		public CalendarPopup(Point origin, DateTime? initial = null)
			: base(origin)
		{
			calendar = new MoreMonthCalendar
			{
				MaxSelectionCount = 1,
				TabStop = true
			};

			calendar.ApplyTheme(ThemeManager.Instance);
			calendar.SeedDate(initial ?? DateTime.Today);

			var size = calendar.GetPreferredSize(Size.Empty);
			calendar.Bounds = new Rectangle(Padding.Left, Padding.Top + CloseRow, size.Width, size.Height);
			calendar.DateSelected += (s, e) =>
			{
				SelectedDate = e.Start;
				Dismiss(true);
			};

			Controls.Add(calendar);
			Place(size);
		}


		/// <summary>
		/// Gets the date chosen by the user; valid when the dialog result is OK.
		/// </summary>
		public DateTime SelectedDate { get; private set; }
	}
}
