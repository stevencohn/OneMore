//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreGroupBox : GroupBox
	{
		private const int TextOffset = 10;
		private readonly ThemeManager manager;

		public MoreGroupBox()
		{
			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.CacheText,
				true);

			manager = ThemeManager.Instance;
		}



		[Category("More"), Description("Border color"), DefaultValue(typeof(Color), "ActiveBorder")]
		public Color Border { get; set; } = SystemColors.ActiveBorder;


		protected override void OnPaint(PaintEventArgs e)
		{
			var backColor = manager.GetColor(BackColor);
			e.Graphics.Clear(backColor);

			var size = e.Graphics.MeasureString(Text ?? "M", Font).ToSize();
			var clip = e.Graphics.ClipBounds;
			var half = size.Height / 2;

			using var pen = new Pen(manager.GetColor(Border), 2);
			var bounds = new Rectangle((int)clip.X, half, (int)clip.Width, (int)clip.Height - half);
			e.Graphics.DrawRoundedRectangle(pen, bounds, 4);

			if (!string.IsNullOrEmpty(Text))
			{
				var back = new SolidBrush(backColor);
				e.Graphics.FillRectangle(back,
					clip.X + TextOffset - 2, clip.Y,
					clip.X + TextOffset + size.Width, clip.Y + size.Height
					);

				using var brush = new SolidBrush(manager.GetColor(ForeColor));
				e.Graphics.DrawString(Text, Font, brush, clip.X + TextOffset, clip.Y);
			}
		}
	}
}
