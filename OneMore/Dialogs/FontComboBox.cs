namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Windows.Forms;


	public class FontComboBox : ComboBox
	{
		private Dictionary<string, Font> _fontCache;
		private int _itemHeight;
		private int _previewFontSize;
		private StringFormat _stringFormat;


		public FontComboBox ()
		{
			_fontCache = new Dictionary<string, Font>();

			this.DrawMode = DrawMode.OwnerDrawVariable;
			this.Sorted = true;
			this.PreviewFontSize = 14;

			this.CalculateLayout();
			this.CreateStringFormat();
		}


		public event EventHandler PreviewFontSizeChanged;


		protected override void Dispose (bool disposing)
		{
			this.ClearFontCache();

			if (_stringFormat != null)
				_stringFormat.Dispose();

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
			get { return _previewFontSize; }
			set
			{
				_previewFontSize = value;

				this.OnPreviewFontSizeChanged(EventArgs.Empty);
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


		protected override void OnDrawItem (DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if (e.Index > -1 && e.Index < this.Items.Count)
			{
				e.DrawBackground();

				if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
					e.DrawFocusRectangle();

				using (SolidBrush textBrush = new SolidBrush(e.ForeColor))
				{
					string fontFamilyName;

					fontFamilyName = this.Items[e.Index].ToString();
					e.Graphics.DrawString(fontFamilyName, this.GetFont(fontFamilyName),
					textBrush, e.Bounds, _stringFormat);
				}
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged(e);

			this.CalculateLayout();
		}

		protected override void OnGotFocus (EventArgs e)
		{
			this.LoadFontFamilies();

			base.OnGotFocus(e);
		}

		protected override void OnMeasureItem (MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);

			if (e.Index > -1 && e.Index < this.Items.Count)
			{
				e.ItemHeight = _itemHeight;
			}
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged(e);

			this.CreateStringFormat();
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged(e);

			if (this.Items.Count == 0)
			{
				int selectedIndex;

				this.LoadFontFamilies();

				selectedIndex = this.FindStringExact(this.Text);
				if (selectedIndex != -1)
					this.SelectedIndex = selectedIndex;
			}
		}


		public virtual void LoadFontFamilies ()
		{
			if (this.Items.Count == 0)
			{
				Cursor.Current = Cursors.WaitCursor;

				foreach (FontFamily fontFamily in FontFamily.Families)
					this.Items.Add(fontFamily.Name);

				Cursor.Current = Cursors.Default;
			}
		}


		private void CalculateLayout ()
		{
			this.ClearFontCache();

			using (Font font = new Font(this.Font.FontFamily, (float)this.PreviewFontSize))
			{
				Size textSize;

				textSize = TextRenderer.MeasureText("yY", font);
				_itemHeight = textSize.Height + 2;
			}
		}

		private bool IsUsingRTL (Control control)
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

		protected virtual void ClearFontCache ()
		{
			if (_fontCache != null)
			{
				foreach (string key in _fontCache.Keys)
					_fontCache[key].Dispose();
				_fontCache.Clear();
			}
		}

		protected virtual void CreateStringFormat ()
		{
			if (_stringFormat != null)
				_stringFormat.Dispose();

			_stringFormat = new StringFormat(StringFormatFlags.NoWrap);
			_stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			_stringFormat.HotkeyPrefix = HotkeyPrefix.None;
			_stringFormat.Alignment = StringAlignment.Near;
			_stringFormat.LineAlignment = StringAlignment.Center;

			if (this.IsUsingRTL(this))
				_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}

		protected virtual Font GetFont (string fontFamilyName)
		{
			lock (_fontCache)
			{
				if (!_fontCache.ContainsKey(fontFamilyName))
				{
					Font font;

					font = this.GetFont(fontFamilyName, FontStyle.Regular);
					if (font == null)
						font = this.GetFont(fontFamilyName, FontStyle.Bold);
					if (font == null)
						font = this.GetFont(fontFamilyName, FontStyle.Italic);
					if (font == null)
						font = this.GetFont(fontFamilyName, FontStyle.Bold | FontStyle.Italic);
					if (font == null)
						font = (Font)this.Font.Clone();

					_fontCache.Add(fontFamilyName, font);
				}
			}

			return _fontCache[fontFamilyName];
		}

		protected virtual Font GetFont (string fontFamilyName, FontStyle fontStyle)
		{
			Font font;

			try
			{
				font = new Font(fontFamilyName, this.PreviewFontSize, fontStyle);
			}
			catch
			{
				font = null;
			}

			return font;
		}

		protected virtual void OnPreviewFontSizeChanged (EventArgs e)
		{
			if (PreviewFontSizeChanged != null)
				PreviewFontSizeChanged(this, e);

			this.CalculateLayout();
		}
	}
}