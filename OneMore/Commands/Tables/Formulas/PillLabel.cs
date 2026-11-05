//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// A label drawn as a rounded pill with a tinted theme highlight background and bold text.
	/// </summary>
	internal class PillLabel : Label
	{
		private readonly ThemeManager manager = ThemeManager.Instance;


		public PillLabel()
		{
			AutoSize = true;
			BackColor = Color.Transparent;
			SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint
				| ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}


		public override Size GetPreferredSize(Size proposedSize)
		{
			using var font = new Font(Font, FontStyle.Bold);
			var size = TextRenderer.MeasureText(Text, font);
			var pad = Font.Height / 2;
			return new Size(size.Width + (pad * 2), size.Height + pad);
		}


		protected override void OnTextChanged(System.EventArgs e)
		{
			base.OnTextChanged(e);
			if (AutoSize)
			{
				Size = GetPreferredSize(Size.Empty);
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var accent = manager.GetColor("HotTrack");
			var rect = new Rectangle(0, 0, Width - 1, Height - 1);

			using (var path = RoundedRect(rect))
			using (var fill = new SolidBrush(Color.FromArgb(40, accent)))
			using (var pen = new Pen(Color.FromArgb(120, accent)))
			{
				g.FillPath(fill, path);
				g.DrawPath(pen, path);
			}

			using var font = new Font(Font, FontStyle.Bold);
			TextRenderer.DrawText(g, Text, font, rect, accent,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
				| TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
		}


		private static GraphicsPath RoundedRect(Rectangle r)
		{
			var d = r.Height;
			var path = new GraphicsPath();
			path.AddArc(r.X, r.Y, d, d, 90, 180);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 180);
			path.CloseFigure();
			return path;
		}
	}
}
