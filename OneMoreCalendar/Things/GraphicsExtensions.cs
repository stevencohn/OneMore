//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;


	internal static class GraphicsExtensions
	{
		/// <summary>
		/// Draws a rounded rectangle specified by a bounding Rectangle and a common corner
		/// radius value for each corners.
		/// </summary>
		/// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
		/// <param name="pen">The <see cref="Pen"/> instance to be used for the drawing.</param>
		/// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
		/// <param name="cornerRadius">Size of the corner radius for each corners.</param>
		public static void DrawRoundedRectangle(
			this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
		{
			if (graphics == null)
				throw new ArgumentNullException(nameof(graphics), "cannot be null");
			if (pen == null)
				throw new ArgumentNullException(nameof(pen), "cannot be null");

			using var path = CreateRoundedRectangle(bounds, cornerRadius);
			graphics.DrawPath(pen, path);
		}


		/// <summary>
		/// Fills a rounded rectangle specified by a bounding Rectangle and a common corner
		/// radius value for each corners.
		/// </summary>
		/// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
		/// <param name="brush">The <see cref="Brush"/> instance to be used for the drawing.</param>
		/// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
		/// <param name="cornerRadius">Size of the corner radius for each corners.</param>
		public static void FillRoundedRectangle(
			this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
		{
			if (graphics == null)
				throw new ArgumentNullException(nameof(graphics), "cannot be null");
			if (brush == null)
				throw new ArgumentNullException(nameof(brush), "cannot be null");

			using var path = CreateRoundedRectangle(bounds, cornerRadius);
			graphics.FillPath(brush, path);
		}


		/// <summary>
		/// Returns the path for a rounded rectangle specified by a bounding 
		/// Rectangle structure and a common corner radius value for each corners.
		/// </summary>
		/// <param name="bounds">A Rectangle structure that bounds the rounded rectangle.</param>
		/// <param name="radius">Size of the corner radius for each corners.</param>
		private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
		{
			var path = new GraphicsPath();
			if (radius == 0)
			{
				path.AddRectangle(bounds);
				return path;
			}

			var diameter = radius * 2;
			var size = new Size(diameter, diameter);
			var arc = new Rectangle(bounds.Location, size);

			// top left arc
			path.AddArc(arc, 180, 90);

			// top right arc
			arc.X = bounds.Right - diameter - 1;
			path.AddArc(arc, 270, 90);

			// bottom right arc
			arc.Y = bounds.Bottom - diameter - 1;
			path.AddArc(arc, 0, 90);

			// bottom left arc
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);

			path.CloseFigure();
			return path;
		}
	}
}
