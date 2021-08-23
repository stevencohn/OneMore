//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Text;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ColorsComboBox : ComboBox
	{
		private class Swatch
		{
			public readonly string Name;
			public readonly Color Color;
			public Swatch (string name, Color color) { Name = name; Color = color; }
		}

		private readonly Font itemFont;
		private readonly int itemHeight;
		private StringFormat stringFormat;


		public ColorsComboBox()
		{
			Items.AddRange(new []
			{
				// light
				new Swatch(Color.White.Name, Color.White),
				new Swatch(Color.AliceBlue.Name, Color.AliceBlue),
				new Swatch(Color.Honeydew.Name, Color.Honeydew),
				new Swatch(Color.Ivory.Name, Color.Ivory),
				new Swatch(Color.Linen.Name, Color.Linen),
				new Swatch(Color.MintCream.Name, Color.MintCream),
				new Swatch(Color.SeaShell.Name, Color.SeaShell),
				new Swatch(Color.Snow.Name, Color.Snow),
				new Swatch(Color.WhiteSmoke.Name, Color.WhiteSmoke),
				// dark
				new Swatch(Color.Black.Name, Color.Black),
				new Swatch("OneMore Black", Color.FromArgb(64, 64, 64)),
				new Swatch(Color.DarkCyan.Name, Color.DarkCyan),
				new Swatch(Color.DarkOliveGreen.Name, Color.DarkOliveGreen),
				new Swatch(Color.DarkSlateBlue.Name, Color.DarkSlateBlue),
				new Swatch(Color.DimGray.Name, Color.DimGray),
				new Swatch(Color.MidnightBlue.Name, Color.MidnightBlue),
				new Swatch(Color.SaddleBrown.Name, Color.SaddleBrown),
				new Swatch(Color.DarkSlateBlue.Name, Color.DarkSlateBlue),
				new Swatch(Color.SteelBlue.Name, Color.SteelBlue),
				new Swatch("Custom", Color.Transparent)
			});

			DrawMode = DrawMode.OwnerDrawVariable;
			DropDownStyle = ComboBoxStyle.DropDownList;
			Sorted = false;

			itemFont = new Font("Calibri", 12.0f);

			itemHeight = CalculateLayout();
			stringFormat = CreateStringFormat();

			DropDownHeight = itemHeight * 15;
			Height = itemHeight;

			SelectedIndex = 0;
		}


		protected override void Dispose(bool disposing)
		{
			if (stringFormat != null)
				stringFormat.Dispose();

			if (itemFont != null)
				itemFont.Dispose();

			base.Dispose(disposing);
		}


		private int CalculateLayout()
		{
			var textSize = TextRenderer.MeasureText("yY", itemFont);
			return textSize.Height + 4;
		}


		private StringFormat CreateStringFormat()
		{
			var format = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming = StringTrimming.EllipsisCharacter,
				HotkeyPrefix = HotkeyPrefix.None,
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};

			if (IsUsingRTL(this))
				format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

			return format;
		}


		private bool IsUsingRTL(Control control)
		{
			bool result;

			if (control.RightToLeft == RightToLeft.Yes)
				result = true;
			else if (control.RightToLeft == RightToLeft.Inherit && control.Parent != null)
				result = IsUsingRTL(control.Parent);
			else
				result = false;

			return result;
		}


		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if (e.Index > -1 && e.Index < Items.Count)
			{
				try
				{
					var swatch = (Swatch)Items[e.Index];
					var dark = swatch.Color.GetBrightness() < 0.5;

					// focus
					if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
					{
						using (var pen = new Pen(dark ? Color.Red : Color.Blue, 2)
						{
							DashStyle = DashStyle.Dash,
							DashPattern = new float[] { 4, 3 }
						})
						{
							e.Graphics.DrawRectangle(pen, e.Bounds);
						}
					}
					else
					{
						e.Graphics.DrawRectangle(new Pen(Color.White, 2), e.Bounds);
					}

					// background
					using (var backBrush = new SolidBrush(swatch.Color))
					{
						e.Graphics.FillRectangle(backBrush,
							e.Bounds.X + 1, e.Bounds.Y + 1,
							e.Bounds.Width - 2, e.Bounds.Height - 2);
					}

					// foreground
					using (var textBrush = dark
						? new SolidBrush(Color.WhiteSmoke)
						: new SolidBrush(Color.Black))
					{
						e.Graphics.DrawString(
							swatch.Name, itemFont,
							textBrush, e.Bounds, stringFormat);
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("OnDrawItem", exc);
				}
			}
		}


		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);

			if (e.Index > -1 && e.Index < Items.Count)
			{
				e.ItemHeight = itemHeight;
			}
		}


		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);

			if (stringFormat != null)
				stringFormat.Dispose();

			stringFormat = CreateStringFormat();
		}
	}
}