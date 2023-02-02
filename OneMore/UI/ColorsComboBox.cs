//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Helpers.Office;
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
		private sealed class Swatch
		{
			public readonly string Name;
			public Color Color;
			public Color Paint;
			public Swatch(string name)
			{
				Name = name;
				Color = Color.Transparent;
				Paint = Color.Empty;
			}
			public Swatch(string name, uint color)
				: this(name, color, color)
			{
			}
			public Swatch(string name, uint color, uint paint)
			{
				Name = name;
				Color = Color.FromArgb((int)color);
				Paint = Color.FromArgb((int)paint);
			}
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
			if (Office.IsBlackThemeEnabled())
			{
				// dark colors - the color:param is applied to the page but inverted by OneNote
				// so the rendered color follows a conversion algorithm. The paint:param is used
				// to render the swatch in the ComboBox dropdown showing how it will appear
				// when applied to a page in dark mode...

				Items.AddRange(new[]
				{
					// dark
					new Swatch("Black", 0xFFEEEEEE, 0xFF131313),
					new Swatch("Gray", 0xFFCFCFCF, 0xFF303030),
					new Swatch("Dark Teal", 0xFFB6DCDB, 0xFF1F4140),
					new Swatch("Dark Olive Green", 0xFFD0E2C4, 0xFF2F4321),
					new Swatch("Dark Slate Blue", 0xFFB6B6D1, 0xFF232338),
					new Swatch("Midnight Blue", 0xFFBBC2D8, 0xFF23293D),
					new Swatch("Saddle Brown", 0xFFE5CCB3, 0xFF473018),
					new Swatch("Brown", 0xFFDAABAB, 0xFF401C1C),
					new Swatch("Dark Purple", 0xFFE0C2D5, 0xFF412135),
					// light
					new Swatch("White", 0xFF010101, 0xFFF3F3F3),
					new Swatch("White Smoke", 0xFF111111, 0xFFF2F2F2),
					new Swatch("Alice Blue", 0xFF003E75, 0xFFECF5FF),
					new Swatch("Mint Cream", 0xFF00753A, 0xFFECFFF5),
					new Swatch("Honeydew", 0xFF007500, 0xFFECFFEC),
					new Swatch("Ivory", 0xFF757500, 0xFFFFFFEC),
					new Swatch("Snow", 0xFF225151, 0xFFF0F9F9),
					new Swatch("Seashell", 0xFF220E00, 0xFFFFF0E6),
					new Swatch("Linen", 0xFF1B1005, 0xFFFCF3EB),
					// custom
					new Swatch("Custom")
				});
			}
			else
			{
				// light colors - explicit, exact color representations from ligth to dark
				// are applied to the page and rendered as specified...

				Items.AddRange(new[]
				{
					// light
					new Swatch("White", 0xFFF3F3F3),
					new Swatch("White Smoke", 0xFFF2F2F2),
					new Swatch("Alice Blue", 0xFFECF5FF),
					new Swatch("Mint Cream", 0xFFECFFF5),
					new Swatch("Honeydew", 0xFFECFFEC),
					new Swatch("Ivory", 0xFFFFFFEC),
					new Swatch("Snow", 0xFFF0F9F9),
					new Swatch("Seashell", 0xFFFFF0E6),
					new Swatch("Linen", 0xFFFCF3EB),
					// dark
					new Swatch("Black", 0xFF131313),
					new Swatch("Gray", 0xFF303030),
					new Swatch("Dark Teal", 0xFF1F4140),
					new Swatch("Dark Olive Green", 0xFF2F4321),
					new Swatch("Dark Slate Blue", 0xFF232338),
					new Swatch("Midnight Blue", 0xFF23293D),
					new Swatch("Saddle Brown", 0xFF473018),
					new Swatch("Brown", 0xFF401C1C),
					new Swatch("Dark Purple", 0xFF412135),
					// custom
					new Swatch("Custom")
				});
			}

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
			stringFormat?.Dispose();
			itemFont?.Dispose();
			arrowFont?.Dispose();
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
					var color = swatch.Paint == Color.Empty ? swatch.Color : swatch.Paint;
					var dark = color.GetBrightness() < 0.5;

					// focus
					if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
					{
						using var pen = new Pen(dark ? Color.Red : Color.Blue, 2)
						{
							DashStyle = DashStyle.Dash,
							DashPattern = new float[] { 3, 2 }
						};
						e.Graphics.DrawRectangle(pen, e.Bounds);
					}
					else
					{
						e.Graphics.DrawRectangle(new Pen(Color.White, 2), e.Bounds);
					}

					// background
					using var backBrush = new SolidBrush(color);
					e.Graphics.FillRectangle(backBrush,
						e.Bounds.X + 1, e.Bounds.Y + 1,
						e.Bounds.Width - 2, e.Bounds.Height - 2);

					// foreground
					using var textBrush = dark
						? new SolidBrush(Color.WhiteSmoke)
						: new SolidBrush(Color.Black);

					e.Graphics.DrawString(
						swatch.Name, itemFont,
						textBrush, e.Bounds, stringFormat);

					if (opened && (e.State & DrawItemState.Selected) == DrawItemState.Selected)
					{
						e.Graphics.DrawString(arrow, arrowFont,
							textBrush, e.Bounds.X + 10, e.Bounds.Y);
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
			stringFormat?.Dispose();
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