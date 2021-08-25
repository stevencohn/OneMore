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


	/// <summary>
	/// An enhanced ComboBox that displays a selection of colors suitable for page backgrounds
	/// in both light and dard shades.
	/// </summary>
	internal class ColorsComboBox : ComboBox
	{
		private class Swatch
		{
			public readonly string Name;
			public Color Color;
			public Swatch(string name, Color color) { Name = name; Color = color; }
		}

		private const string arrow = "\u00E8"; // black right arrow

		private readonly Font itemFont;
		private readonly Font arrowFont;
		private readonly int itemHeight;
		private readonly int customIndex;
		private StringFormat stringFormat;
		private bool opened;


		public ColorsComboBox()
		{
			Items.AddRange(new[]
			{
				// light
				new Swatch(Color.White.Name, Color.White),
				new Swatch(Color.WhiteSmoke.Name, Color.WhiteSmoke),
				new Swatch(Color.AliceBlue.Name, Color.AliceBlue),
				new Swatch(Color.MintCream.Name, Color.MintCream),
				new Swatch(Color.Honeydew.Name, Color.Honeydew),
				new Swatch(Color.Ivory.Name, Color.Ivory),
				new Swatch(Color.Snow.Name, Color.Snow),
				new Swatch(Color.SeaShell.Name, Color.SeaShell),
				new Swatch(Color.Linen.Name, Color.Linen),
				// dark
				new Swatch(Color.Black.Name, Color.Black),
				new Swatch("Black Smoke", BasicColors.BlackSmoke),
				new Swatch(Color.DarkCyan.Name, Color.FromArgb(39, 81, 81)),
				new Swatch(Color.DarkOliveGreen.Name, Color.FromArgb(41, 53, 34)),
				new Swatch(Color.DarkSlateBlue.Name, Color.FromArgb(46, 46, 73)),
				new Swatch(Color.SteelBlue.Name, Color.FromArgb(33, 54, 79)),
				new Swatch(Color.MidnightBlue.Name, Color.FromArgb(39, 46, 68)),
				new Swatch(Color.SaddleBrown.Name, Color.FromArgb(76, 51, 26)),
				new Swatch(Color.Brown.Name, Color.FromArgb(84, 37, 37)),
				new Swatch(Color.Purple.Name, Color.FromArgb(61, 31, 50)),
				new Swatch("Custom", Color.Transparent)
			});

			customIndex = Items.Count - 1;
			arrowFont = new Font("Wingdings", 11.0f);
			itemFont = new Font("Calibri", 11.0f);
			itemHeight = CalculateLayout();
			stringFormat = CreateStringFormat();

			DrawMode = DrawMode.OwnerDrawVariable;
			DropDownStyle = ComboBoxStyle.DropDownList;
			DropDownHeight = itemHeight * 15;
			Height = itemHeight;
			Sorted = false;
		}


		/// <summary>
		/// Fires when the chosen color changes
		/// </summary>
		public event EventHandler ColorChanged;


		/// <summary>
		/// Gets the custom color
		/// </summary>
		public Color CustomColor => ((Swatch)Items[customIndex]).Color;


		/// <summary>
		/// Gets the selected color
		/// </summary>
		public Color Color => ((Swatch)Items[SelectedIndex]).Color;


		protected override void Dispose(bool disposing)
		{
			if (stringFormat != null)
				stringFormat.Dispose();

			if (itemFont != null)
				itemFont.Dispose();

			if (arrowFont != null)
				arrowFont.Dispose();

			base.Dispose(disposing);
		}


		/// <summary>
		/// Select the item that matches the given color, or White if no matches
		/// </summary>
		/// <param name="color">The desired color</param>
		public void SelectColor(Color color)
		{
			var index = 0;
			while (index < Items.Count)
			{
				if (color.Matches(((Swatch)Items[index]).Color))
				{
					break;
				}

				index++;
			}

			SelectedIndex = index < Items.Count ? index : 0;
		}


		/// <summary>
		/// Sets the value of the Custom Color item.
		/// </summary>
		/// <param name="color">The Color to apply</param>
		public void SetCustomColor(Color color)
		{
			((Swatch)Items[customIndex]).Color = color;
			Invalidate();

			if (SelectedIndex == customIndex)
			{
				ColorChanged?.Invoke(this, new EventArgs());
			}
			else
			{
				SelectedIndex = customIndex;
			}
		}


		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			ColorChanged?.Invoke(this, e);
		}


		private int CalculateLayout()
		{
			// account for ascenders and descenders
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
							DashPattern = new float[] { 3, 2 }
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

						if (opened && (e.State & DrawItemState.Selected) == DrawItemState.Selected)
						{
							e.Graphics.DrawString(arrow, arrowFont,
								textBrush, e.Bounds.X + 10, e.Bounds.Y);
						}
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

		protected override void OnDropDown(EventArgs e)
		{
			opened = true;
			Invalidate();
			base.OnDropDown(e);
		}

		protected override void OnDropDownClosed(EventArgs e)
		{
			opened = false;
			Invalidate();
			base.OnDropDownClosed(e);
		}
	}
}