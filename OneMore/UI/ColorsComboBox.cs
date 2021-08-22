//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Linq;
	using System.Windows.Forms;


	public class ColorsComboBox : ComboBox
	{
		private readonly Dictionary<string, Color> StandardColors;
		private int itemHeight;
		private int previewFontSize;
		private StringFormat stringFormat;


		public ColorsComboBox()
		{
			StandardColors = new Dictionary<string, Color>
			{
				// light
				{ Color.White.Name, Color.White },
				{ Color.AliceBlue.Name, Color.AliceBlue },
				{ Color.Cornsilk.Name, Color.Cornsilk },
				{ Color.FloralWhite.Name, Color.FloralWhite },
				{ Color.Honeydew.Name, Color.Honeydew },
				{ Color.Ivory.Name, Color.Ivory },
				{ Color.Linen.Name, Color.Linen },
				{ Color.MintCream.Name, Color.MintCream },
				{ Color.OldLace.Name, Color.OldLace },
				{ Color.SeaShell.Name, Color.SeaShell },
				{ Color.Snow.Name, Color.Snow },
				{ Color.WhiteSmoke.Name, Color.WhiteSmoke },
				// dark
				{ Color.Black.Name, Color.Black },
				{ "OneMore Black", Color.FromArgb(0x21, 0x21, 0x21) },
				{ Color.CornflowerBlue.Name, Color.CornflowerBlue },
				{ Color.DarkCyan.Name, Color.DarkCyan },
				{ Color.DarkOliveGreen.Name, Color.DarkOliveGreen },
				{ Color.DarkSlateBlue.Name, Color.DarkSlateBlue },
				{ Color.DimGray.Name, Color.DimGray },
				{ Color.MidnightBlue.Name, Color.MidnightBlue },
				{ Color.SaddleBrown.Name, Color.SaddleBrown },
				{ Color.SlateBlue.Name, Color.SlateBlue },
				{ Color.SteelBlue.Name, Color.SteelBlue },
				// custom
				{ "Custom", Color.WhiteSmoke }
			};

			DrawMode = DrawMode.OwnerDrawVariable;
			Sorted = false;
			PreviewFontSize = 14;

			CalculateLayout();
			CreateStringFormat();
		}


		public event EventHandler PreviewFontSizeChanged;


		protected override void Dispose(bool disposing)
		{
			ClearFontCache();

			if (stringFormat != null)
				stringFormat.Dispose();

			base.Dispose(disposing);
		}

		[Browsable(false), DesignerSerializationVisibility
		(DesignerSerializationVisibility.Hidden),
		EditorBrowsable(EditorBrowsableState.Never)]
		public new DrawMode DrawMode
		{
			get { return base.DrawMode; }
			set { base.DrawMode = value; }
		}

		[Category("Appearance"), DefaultValue(14)]
		public int PreviewFontSize
		{
			get { return previewFontSize; }
			set
			{
				previewFontSize = value;

				OnPreviewFontSizeChanged(EventArgs.Empty);
			}
		}

		[Browsable(false), DesignerSerializationVisibility
		(DesignerSerializationVisibility.Hidden),
		EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Sorted
		{
			get { return base.Sorted; }
			set { base.Sorted = value; }
		}


		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if (e.Index > -1 && e.Index < Items.Count)
			{
				e.DrawBackground();

				if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
					e.DrawFocusRectangle();

				using (SolidBrush textBrush = new SolidBrush(e.ForeColor))
				{
					string fontFamilyName;

					fontFamilyName = Items[e.Index].ToString();
					e.Graphics.DrawString(fontFamilyName, GetFont(fontFamilyName),
					textBrush, e.Bounds, stringFormat);
				}
			}
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			CalculateLayout();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			LoadFontFamilies();

			base.OnGotFocus(e);
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

			CreateStringFormat();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			if (Items.Count == 0)
			{
				int selectedIndex;

				LoadFontFamilies();

				selectedIndex = FindStringExact(Text);
				if (selectedIndex != -1)
					SelectedIndex = selectedIndex;
			}
		}


		public virtual void LoadFontFamilies()
		{
			if (Items.Count == 0)
			{
				Cursor.Current = Cursors.WaitCursor;

				var families = FontFamily.Families.ToList();

				// collecting OTF font directly is probably not necessary but just to be sure...

				var other = new PrivateFontCollection();
				var path = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
				var files = System.IO.Directory.GetFiles(path, "*.otf");
				foreach (var file in files)
				{
					other.AddFontFile(file);
				}

				if (other.Families.Length > 0)
				{
					foreach (var family in other.Families)
					{
						if (!families.Any(f => f.Name == family.Name))
						{
							families.Add(family);
						}
					}

					families = families.OrderBy(f => f.Name).ToList();
				}

				foreach (var family in families)
				{
					Items.Add(family.Name);
					family.Dispose();
				}

				families.Clear();

				Cursor.Current = Cursors.Default;
			}
		}


		private void CalculateLayout()
		{
			ClearFontCache();

			using (Font font = new Font(Font.FontFamily, PreviewFontSize))
			{
				Size textSize;

				textSize = TextRenderer.MeasureText("yY", font);
				itemHeight = textSize.Height + 2;
			}
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

		protected virtual void ClearFontCache()
		{
			if (cache != null)
			{
				foreach (string key in cache.Keys)
					cache[key].Dispose();
				cache.Clear();
			}
		}

		private void CreateStringFormat()
		{
			if (stringFormat != null)
				stringFormat.Dispose();

			stringFormat = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming = StringTrimming.EllipsisCharacter,
				HotkeyPrefix = HotkeyPrefix.None,
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};

			if (IsUsingRTL(this))
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}

		protected virtual Font GetFont(string fontFamilyName)
		{
			lock (cache)
			{
				if (!cache.ContainsKey(fontFamilyName))
				{
					Font font;

					font = GetFont(fontFamilyName, FontStyle.Regular);
					if (font == null)
						font = GetFont(fontFamilyName, FontStyle.Bold);
					if (font == null)
						font = GetFont(fontFamilyName, FontStyle.Italic);
					if (font == null)
						font = GetFont(fontFamilyName, FontStyle.Bold | FontStyle.Italic);
					if (font == null)
						font = (Font)Font.Clone();

					cache.Add(fontFamilyName, font);
				}
			}

			return cache[fontFamilyName];
		}

		protected virtual Font GetFont(string fontFamilyName, FontStyle fontStyle)
		{
			Font font;

			try
			{
				font = new Font(fontFamilyName, PreviewFontSize, fontStyle);
			}
			catch
			{
				font = null;
			}

			return font;
		}

		protected virtual void OnPreviewFontSizeChanged(EventArgs e)
		{
			PreviewFontSizeChanged?.Invoke(this, e);
			CalculateLayout();
		}
	}
}