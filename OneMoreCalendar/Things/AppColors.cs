//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn.Helpers.Office;
	using System.Drawing;


	internal static class AppColors
	{

		public static Brush HoverBrush => new SolidBrush(ColorTranslator.FromHtml("#FFF7EDF7"));

		public static Pen HoverPen => new(ColorTranslator.FromHtml("#FFF0DAEE"));

		public static Brush RowBrush => new SolidBrush(ColorTranslator.FromHtml("#FFFDFAFE"));

		public static Brush PressedBrush => new SolidBrush(ColorTranslator.FromHtml("#FFF0DAEE"));

		public static Pen PressedPen => new(ColorTranslator.FromHtml("#FF9E5499"));


		public static readonly bool DarkMode;

		public static readonly Color BackColor;
		public static readonly Color ControlColor;
		public static readonly Color ForeColor;
		public static readonly Color HighlightForeColor;
		public static readonly Color PressedColor;
		public static readonly Color HoveredColor;
		public static readonly Color HeadBackColor;
		public static readonly Color TodayHeadColor;

		public static readonly Color MonthPrimary;
		public static readonly Color MonthSecondary;
		public static readonly Color MonthGrid;
		public static readonly Color MonthDayFore;
		public static readonly Color MonthDayBack;
		public static readonly Color MonthTodayFore;
		public static readonly Color MonthTodayBack;


		static AppColors()
		{
			DarkMode = Office.SystemDefaultDarkMode();
			if (DarkMode)
			{
				BackColor = ColorTranslator.FromHtml("#FF383838");
				ForeColor = ColorTranslator.FromHtml("#FFE6E6E6");
				ControlColor = ColorTranslator.FromHtml("#FF73356E");

				// light purple
				HighlightForeColor = ColorTranslator.FromHtml("#FFBC58B6");

				HeadBackColor = ColorTranslator.FromHtml("#FFF4E8F3");
				TodayHeadColor = ColorTranslator.FromHtml("#FFD6A6D3");

				PressedColor = ColorTranslator.FromHtml("#FFD2A1DF");
				HoveredColor = ColorTranslator.FromHtml("#FFD2A1DF");

				MonthPrimary = ColorTranslator.FromHtml("#FF1F1F1F");
				MonthSecondary = ColorTranslator.FromHtml("#FF272727");
				MonthGrid = Color.DarkGray;
				MonthDayFore = Color.Gray;
				MonthDayBack = ColorTranslator.FromHtml("#FF383838");
				MonthTodayFore = Color.LightGray;
				MonthTodayBack = ColorTranslator.FromHtml("#FFF4E8F3");
			}
			else
			{
				BackColor = Color.White;
				ForeColor = Color.Black;
				ControlColor = ColorTranslator.FromHtml("#FF73356E");

				// light purple
				HighlightForeColor = ColorTranslator.FromHtml("#FFBC58B6");

				PressedColor = ColorTranslator.FromHtml("#FFF0DAEE");
				HoveredColor = ColorTranslator.FromHtml("#FFF7EDF7");

				MonthPrimary = Color.White;
				MonthSecondary = Color.WhiteSmoke;
				MonthGrid = Color.DarkGray;
				MonthDayFore = Color.Gray;
				MonthDayBack = ColorTranslator.FromHtml("#FFF4E8F3");
				MonthTodayFore = Color.Black;
				MonthTodayBack = ColorTranslator.FromHtml("#FFD6A6D3");
			}
		}
	}
}
