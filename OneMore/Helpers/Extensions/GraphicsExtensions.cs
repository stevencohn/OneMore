//************************************************************************************************
// Copyright © 2022 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Extensions
{
	using System.Drawing;
	using System.Drawing.Drawing2D;


	internal static class GraphicsExtensions
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="g"></param>
		/// <param name="pen"></param>
		/// <param name="bounds"></param>
		/// <param name="radius"></param>
		public static void DrawRoundedRectangle(
			this Graphics g, Pen pen, Rectangle bounds, int radius)
		{
			using var path = MakeRoundedRect(bounds, radius);
			g.DrawPath(pen, path);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="g"></param>
		/// <param name="brush"></param>
		/// <param name="bounds"></param>
		/// <param name="radius"></param>
		public static void FillRoundedRectangle(
			this Graphics g, Brush brush, Rectangle bounds, int radius)
		{
			using var path = MakeRoundedRect(bounds, radius);
			g.FillPath(brush, path);
		}


		private static GraphicsPath MakeRoundedRect(Rectangle bounds, int radius)
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
			arc.X = bounds.Right - diameter;
			path.AddArc(arc, 270, 90);

			// bottom right arc  
			arc.Y = bounds.Bottom - diameter;
			path.AddArc(arc, 0, 90);

			// bottom left arc 
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);

			path.CloseFigure();
			return path;
		}
	}
}
