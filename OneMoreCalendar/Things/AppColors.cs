//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;


	internal static class AppColors
	{

		public static Color ControlColor => ColorTranslator.FromHtml("#FF73356E");

		public static Brush HoverBrush => new SolidBrush(ColorTranslator.FromHtml("#FFF7EDF7"));

		public static Pen HoverPen => new Pen(ColorTranslator.FromHtml("#FFF0DAEE"));

		public static Brush RowBrush => new SolidBrush(ColorTranslator.FromHtml("#FFFDFAFE"));

		public static Brush PressedBrush => new SolidBrush(ColorTranslator.FromHtml("#FFF0DAEE"));

		public static Pen PressedPen => new Pen(ColorTranslator.FromHtml("#FF9E5499"));

		public static Brush TextBrush => new SolidBrush(Color.FromArgb(115, 53, 110));
	}
}
