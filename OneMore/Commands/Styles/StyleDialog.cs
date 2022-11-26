//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Edit a single style to create or edit multiple styles to manage.
	/// </summary>
	/// <remarks>
	/// Disposables: the List of Styles is input and managed by the consumer.
	/// All other local disposables are handled.
	/// </remarks>

	internal partial class StyleDialog : UI.LocalizableForm
	{
		private GraphicStyle selection;
		private readonly Color pageColor;
		private bool allowEvents;
		private Theme theme;
		private Control activeFocus;


		#region Lifecycle

		/// <summary>
		/// Create a dialog to edit a single style; this is for creating new styles
		/// </summary>
		/// <param name="style"></param>

		public StyleDialog(Style style, Color pageColor)
		{
			Initialize();
			Logger.SetDesignMode(DesignMode);

			allowEvents = false;
			this.pageColor = pageColor;

			mainTools.Visible = false;
			loadButton.Enabled = false;
			saveButton.Enabled = false;
			newStyleButton.Enabled = false;
			reorderButton.Enabled = false;
			deleteButton.Enabled = false;

			selection = new GraphicStyle(style, false);

			Text = Resx.StyleDialog_NewText;
		}


		/// <summary>
		/// Create a dialog to edit multiple styles; this is for editing existing styles.
		/// </summary>
		/// <param name="styles"></param>

		public StyleDialog(Theme theme, Color pageColor)
		{
			Initialize();

			allowEvents = false;

			var styles = theme.GetStyles();
			LoadStyles(styles);

			if (styles.Count > 0)
			{
				selection = new GraphicStyle(styles[0], false);
			}

			this.pageColor = pageColor;

			Text = string.Format(Resx.StyleDialog_ThemeText, theme.Name);
			this.theme = theme;
		}


		private void Initialize()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					// menu
					"FileMenu",
					"loadButton",
					"saveButton",
					"newStyleButton",
					"renameButton=word_Rename",
					"deleteButton=word_Delete",
					"reorderButton",
					// toolstrip
					"boldButton",
					"italicButton",
					"underlineButton",
					"strikeButton",
					"superButton",
					"subButton",
					"colorButton.ToolTipText",
					"backColorButton.ToolTipText",
					"defaultBlackToolStripMenuItem",
					"transparentToolStripMenuItem",
					// labels
					"beforeLabel",
					"afterLabel",
					"spacingLabel",
					"nameLabel",
					"fontLabel",
					"styleTypeLabel",
					"applyColorsBox",
					"okButton",
					"cancelButton=word_Cancel"
				});

				styleTypeBox.Items.Clear();
				styleTypeBox.Items.AddRange(Resx.StyleDialog_styleTypeBox_Items.Split(new char[] { '\n' }));
			}

			mainTools.Rescale();
			toolStrip.Rescale();

			if (AddIn.Culture.NumberFormat.NumberDecimalSeparator != ".")
			{
				for (int i = 0; i < sizeBox.Items.Count; i++)
				{
					sizeBox.Items[i] = sizeBox.Items[i].ToString()
						.Replace(".", AddIn.Culture.NumberFormat.NumberDecimalSeparator);
				}
			}

			styleTypeBox.SelectedIndex = (int)StyleType.Paragraph;
			familyBox.SelectedIndex = familyBox.Items.IndexOf(StyleBase.DefaultFontFamily);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(StyleBase.DefaultFontSize.ToString());
			spaceAfterSpinner.Value = 0;
			spaceBeforeSpinner.Value = 0;
			spacingSpinner.Value = 0;
		}


		private void LoadStyles(List<Style> styles)
		{
			// nameBox TextBox is shown when creating a new style to enter a single name
			// namesBox ComboBox is shown when editing styles to select an existing name

			// dispose and clear existing items...

			var ie = namesBox.Items.GetEnumerator();
			while (ie.MoveNext())
			{
				var item = ie.Current as GraphicStyle;
				item.Dispose();
			}

			namesBox.Items.Clear();

			// load new items...

			if (styles.Count == 0)
			{
				styles.Add(new Style
				{
					Name = "Style-" + new Random().Next(1000, 9999).ToString()
				});
			}

			var items = styles.ConvertAll(e => new GraphicStyle(e, false));

			namesBox.Items.AddRange(items.ToArray());

			namesBox.Location = nameBox.Location;
			namesBox.Size = nameBox.Size;
			namesBox.Visible = true;
			nameBox.Visible = false;

			namesBox.SelectedIndex = 0;
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			allowEvents = true;

			if (selection != null)
			{
				ShowSelection();
			}

			nameBox.Focus();
		}

		#endregion Lifecycle


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Gets the currently selected custom style. Used when creating a single new style
		/// </summary>
		public Style Style
		{
			get
			{
				var style = selection.GetStyle();

				style.Name = nameBox.Text;

				if (subButton.Checked) style.IsSubscript = true;
				if (superButton.Checked) style.IsSuperscript = true;

				return style;
			}
		}


		/// <summary>
		/// Get the modified theme. Used when editing an entire theme.
		/// </summary>
		public Theme Theme => new(MakeStyles(), theme.Key, theme.Name, theme.Dark);


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private List<Style> MakeStyles()
		{
			return namesBox.Items.Cast<GraphicStyle>().ToList().ConvertAll(e => e.GetStyle());
		}


		private void ShowSelection()
		{
			allowEvents = false;

			// nameBox may not be visible but oh well
			nameBox.Text = selection.Name;

			styleTypeBox.SelectedIndex = (int)selection.StyleType;
			familyBox.Text = selection.FontFamily;

			// normalize number to remove ".0"
			sizeBox.Text = double.Parse(selection.FontSize, CultureInfo.InvariantCulture)
				.ToString("0.#", AddIn.Culture);

			boldButton.Checked = selection.IsBold;
			italicButton.Checked = selection.IsItalic;
			underlineButton.Checked = selection.IsUnderline;
			strikeButton.Checked = selection.IsStrikethrough;
			superButton.Checked = selection.IsSuperscript;
			subButton.Checked = selection.IsSubscript;

			applyColorsBox.Checked = selection.ApplyColors;

			if (double.TryParse(selection.SpaceAfter, NumberStyles.Any, CultureInfo.InvariantCulture, out var sa))
			{
				var dsa = (decimal)sa;
				if (dsa >= spaceAfterSpinner.Minimum && dsa <= spaceAfterSpinner.Maximum)
					spaceAfterSpinner.Value = dsa;
			}

			if (double.TryParse(selection.SpaceBefore, NumberStyles.Any, CultureInfo.InvariantCulture, out var sb))
			{
				var dsb = (decimal)sb;
				if (dsb >= spaceBeforeSpinner.Minimum && dsb <= spaceBeforeSpinner.Maximum)
					spaceBeforeSpinner.Value = dsb;
			}

			if (double.TryParse(selection.Spacing, NumberStyles.Any, CultureInfo.InvariantCulture, out var ss))
			{
				var dss = (decimal)ss;
				if (dss >= spacingSpinner.Minimum && dss <= spacingSpinner.Maximum)
					spacingSpinner.Value = dss;
			}

			allowEvents = true;

			previewBox.Invalidate();
		}


		private void RepaintSample(object sender, PaintEventArgs e)
		{
			var vcenter = previewBox.Height / 2;

			var contrastPen = pageColor.GetBrightness() <= 0.5 ? Pens.White : Pens.Black;

			e.Graphics.Clear(pageColor);
			e.Graphics.DrawLine(contrastPen, 0, vcenter, 15, vcenter);
			e.Graphics.DrawLine(contrastPen, previewBox.Width - 15, vcenter, previewBox.Width, vcenter);

			var offset = superButton.Checked || subButton.Checked;

			if (!float.TryParse(sizeBox.Text, NumberStyles.Any, AddIn.Culture, out var sampleFontSize))
			{
				sampleFontSize = (float)StyleBase.DefaultFontSize;
			}

			var sampleFont = offset
				? new Font(familyBox.Text, sampleFontSize)
				: MakeFont(sampleFontSize); // dispose

			var sampleSize = e.Graphics.MeasureString(offset ? "Sample" : "Sample ", sampleFont);

			// top Y, -1/2 height offset from centerline
			var y = (int)(vcenter - (sampleSize.Height / 2));

			var textFontSize = offset
				? (float)Math.Round(sampleFontSize * 0.5)
				: sampleFontSize;

			var textFont = MakeFont(Math.Max(textFontSize, 4)); // dispose

			var textSize = e.Graphics.MeasureString("Text", textFont);
			var allWidth = sampleSize.Width + textSize.Width;

			// clipping box and background bounding box
			var sampleClip = new Rectangle(20, y, previewBox.Width - 40, (int)sampleSize.Height);

			var textClip = new Rectangle(
				20 + (int)sampleSize.Width, y,
				offset ? (int)textSize.Width : previewBox.Width - 40,
				(int)sampleSize.Height);

			if (selection?.ApplyColors == true &&
				!selection.Background.IsEmpty &&
				!selection.Background.Equals(Color.Transparent))
			{
				using var highBrush = new SolidBrush(selection.Background);
				if (offset)
				{
					e.Graphics.FillRectangle(highBrush,
						textClip.X, textClip.Y, textClip.Width, textClip.Height);
				}
				else
				{
					e.Graphics.FillRectangle(highBrush,
						sampleClip.X, sampleClip.Y,
						Math.Min(sampleClip.Width, allWidth), sampleClip.Height);
				}
			}

			var format = new StringFormat(StringFormatFlags.NoWrap);

			var textColor = selection?.ApplyColors == true ? selection.Foreground : contrastPen.Color;
			var textBrush = new SolidBrush(textColor); // dispose

			var sampleColor = offset ? Color.Gray : textColor;
			var sampleBrush = new SolidBrush(sampleColor); // dispose

			e.Graphics.DrawString("Sample ", sampleFont, sampleBrush, sampleClip, format);

			if (subButton.Checked) textClip.Y += (int)textSize.Height;
			e.Graphics.DrawString("Text", textFont, textBrush, textClip, format);

			textBrush.Dispose();
			sampleBrush.Dispose();
			textFont.Dispose();
			sampleFont.Dispose();
		}


		private Font MakeFont(float size)
		{
			FontStyle style = 0;
			if (boldButton.Checked) style |= FontStyle.Bold;
			if (italicButton.Checked) style |= FontStyle.Italic;
			if (underlineButton.Checked) style |= FontStyle.Underline;
			if (strikeButton.Checked) style |= FontStyle.Strikeout;

			return new Font(familyBox.Text, size, style);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void SetActiveFocus(object sender, EventArgs e)
		{
			activeFocus = (Control)sender;
		}


		private void ChangeStyleName(object sender, EventArgs e)
		{
			if (allowEvents && nameBox.Visible)
			{
				if (selection != null)
				{
					selection.Name = nameBox.Text;
				}

				okButton.Enabled = (nameBox.Text.Length > 0);
			}
		}

		private void ChangeStyleListSelection(object sender, EventArgs e)
		{
			if (allowEvents && namesBox.Visible)
			{
				selection = namesBox.SelectedItem as GraphicStyle;
				ShowSelection();
			}
		}


		private void ChangeStyleType(object sender, EventArgs e)
		{
			if (namesBox.Visible)
			{
				selection = namesBox.SelectedItem as GraphicStyle;
			}

			if (selection != null)
			{
				if (allowEvents)
				{
					selection.StyleType = (StyleType)styleTypeBox.SelectedIndex;
				}

				switch (selection.StyleType)
				{
					case StyleType.Character:
						spaceAfterSpinner.Enabled = false;
						spaceBeforeSpinner.Enabled = false;
						spacingSpinner.Enabled = false;
						break;

					case StyleType.Paragraph:
					case StyleType.Heading:
						spaceAfterSpinner.Enabled = true;
						spaceBeforeSpinner.Enabled = true;
						spacingSpinner.Enabled = true;
						break;
				}
			}
		}

		private void ChangeFontFamily(object sender, EventArgs e)
		{
			if (allowEvents && (selection != null))
			{
				var save = selection.Font;

				if (!float.TryParse(sizeBox.Text, NumberStyles.Any, AddIn.Culture, out var size))
				{
					size = (float)StyleBase.DefaultFontSize;
				}

				selection.FontFamily = familyBox.Text;
				selection.Font = MakeFont(size);

				save.Dispose();

				previewBox.Invalidate();
			}
		}


		private void ChangeFontSize(object sender, EventArgs e)
		{
			if (allowEvents && (selection != null))
			{
				var save = selection.Font;

				if (!float.TryParse(sizeBox.Text, NumberStyles.Any, AddIn.Culture, out var size))
				{
					size = (float)StyleBase.DefaultFontSize;
				}

				selection.FontSize = size.ToString("0.0#", CultureInfo.InvariantCulture);
				selection.Font = MakeFont(size);

				save.Dispose();

				previewBox.Invalidate();
			}
		}


		private void ChangeFontStyle(object sender, EventArgs e)
		{
			if (allowEvents && (selection != null))
			{
				var save = selection.Font;

				if (!float.TryParse(sizeBox.Text, NumberStyles.Any, AddIn.Culture, out var size))
				{
					size = (float)StyleBase.DefaultFontSize;
				}

				selection.IsBold = boldButton.Checked;
				selection.IsItalic = italicButton.Checked;
				selection.IsUnderline = underlineButton.Checked;
				selection.IsStrikethrough = strikeButton.Checked;
				selection.IsSuperscript = superButton.Checked;
				selection.IsSubscript = subButton.Checked;

				selection.Font = MakeFont(size);

				save.Dispose();

				previewBox.Invalidate();
			}
		}


		private void ToggleSuperSub(object sender, EventArgs e)
		{
			if (sender == superButton)
			{
				subButton.Checked = false;
			}
			else if (sender == subButton)
			{
				superButton.Checked = false;
			}

			ChangeFontStyle(sender, e);
		}


		private void ChangeColor(object sender, EventArgs e)
		{
			var color = SelectColor("Text Color", colorButton.Bounds, selection.Foreground);
			if (!color.Equals(Color.Empty))
			{
				selection.Foreground = color;
				previewBox.Invalidate();
			}
		}

		private void ChangeColorToDefault(object sender, EventArgs e)
		{
			selection.Foreground = Color.Black;
			previewBox.Invalidate();
		}


		private void ChangeHighlightColor(object sender, EventArgs e)
		{
			var color = SelectColor("Highlight Color", backColorButton.Bounds, selection.Background);
			if (!color.Equals(Color.Empty))
			{
				selection.Background = color;
				previewBox.Invalidate();
			}
		}

		private void ChangeHighlightToDefault(object sender, EventArgs e)
		{
			selection.Background = Color.Transparent;
			previewBox.Invalidate();
		}


		private Color SelectColor(string title, Rectangle bounds, Color color)
		{
			var location = PointToScreen(toolStrip.Location);

			using (var dialog = new UI.MoreColorDialog(title,
				location.X + bounds.Location.X,
				location.Y + bounds.Height + 4))
			{
				dialog.Color = color;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					return dialog.Color;
				}
			}

			return Color.Empty;
		}


		private void ChangeApplyColorsOption(object sender, EventArgs e)
		{
			selection.ApplyColors
				= colorButton.Enabled
				= backColorButton.Enabled
				= applyColorsBox.Checked;

			previewBox.Invalidate();
		}


		private void ChangeSpaceAfter(object sender, EventArgs e)
		{
			selection.SpaceAfter = spaceAfterSpinner.Value.ToString("#0.0", CultureInfo.InvariantCulture);

		}


		private void ChangeSpaceBefore(object sender, EventArgs e)
		{
			selection.SpaceBefore = spaceBeforeSpinner.Value.ToString("#0.0", CultureInfo.InvariantCulture);
		}


		private void ChangeSpacing(object sender, EventArgs e)
		{
			selection.Spacing = spacingSpinner.Value.ToString("#0.0", CultureInfo.InvariantCulture);
		}


		private void SaveStyle(object sender, EventArgs e)
		{
			// handles case where cursor is left in a text box without losing focus before
			// the Save button is pressed; will force the appropriate model update before saving

			if (activeFocus == nameBox) { ChangeStyleName(nameBox, e); }
			else if (activeFocus == styleTypeBox) { ChangeStyleType(styleTypeBox, e); }
			else if (activeFocus == familyBox) { ChangeFontFamily(familyBox, e); }
			else if (activeFocus == sizeBox) { ChangeFontSize(sizeBox, e); }
			else if (activeFocus == spaceAfterSpinner) { ChangeSpaceAfter(spaceAfterSpinner, e); }
			else if (activeFocus == spaceBeforeSpinner) { ChangeSpaceBefore(spaceBeforeSpinner, e); }
			else if (activeFocus == spacingSpinner) { ChangeSpacing(spacingSpinner, e); }
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Following are only active when editing a theme, not creating a new style


		private void AddStyle(object sender, EventArgs e)
		{
			var index = 0;
			var names = new List<string>();
			foreach (GraphicStyle style in namesBox.Items)
			{
				names.Add(style.Name);
				if (index <= style.Index)
					index = style.Index + 1;
			}

			var name = "Style-" + new Random().Next(1000, 9999).ToString();

			using (var dialog = new RenameDialog(names, name))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				name = dialog.StyleName;
			}

			namesBox.Items.Add(new GraphicStyle(new Style
			{
				Name = name,
				Index = index
			},
			false));

			saveButton.Enabled = true;
			reorderButton.Enabled = true;
			renameButton.Enabled = true;
			deleteButton.Enabled = true;

			namesBox.SelectedIndex = namesBox.Items.Count - 1;
		}


		private void RenameStyle(object sender, EventArgs e)
		{
			var index = 0;
			var names = new List<string>();
			foreach (GraphicStyle styleItem in namesBox.Items)
			{
				if (index != namesBox.SelectedIndex)
				{
					names.Add(styleItem.Name);
				}

				index++;
			}

			if (!names.Any())
			{
				return;
			}

			var style = (GraphicStyle)namesBox.Items[namesBox.SelectedIndex];
			var name = style.Name;

			using (var dialog = new RenameDialog(names, name) { Rename = true })
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				name = dialog.StyleName;
			}

			style.Name = name;
			index = namesBox.SelectedIndex;
			namesBox.Items.RemoveAt(index);
			namesBox.Items.Insert(index, style);
			namesBox.SelectedIndex = index;
		}



		private void DeleteStyle(object sender, EventArgs e)
		{
			var result = MessageBox.Show(this, "Delete this custom style?", "Confirm",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

			if (result == DialogResult.Yes)
			{
				int index = namesBox.SelectedIndex;

				var style = namesBox.Items[index] as GraphicStyle;
				style.Name = string.Empty;

				namesBox.Items.RemoveAt(index);

				if (namesBox.Items.Count > 0)
				{
					if (index > namesBox.Items.Count - 1)
					{
						namesBox.SelectedIndex = namesBox.Items.Count - 1;
					}
					else
					{
						namesBox.SelectedIndex = index;
					}
				}
			}

			if (namesBox.Items.Count == 0)
			{
				saveButton.Enabled = false;
				reorderButton.Enabled = false;
				renameButton.Enabled = false;
				deleteButton.Enabled = false;
			}
		}


		private void ReorderStyles(object sender, EventArgs e)
		{
			using var dialog = new ReorderDialog(namesBox.Items);
			dialog.ManualLocation = true;
			var point = PointToScreen(mainTools.Location);
			dialog.Location = new Point(point.X + dialog.Width, point.Y);

			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				string name = null;
				if (namesBox.SelectedItem != null)
				{
					name = ((GraphicStyle)namesBox.SelectedItem).Name;
				}

				var items = dialog.GetItems();
				namesBox.Items.Clear();
				namesBox.Items.AddRange(items);

				var selected = namesBox.Items.Cast<GraphicStyle>()
					.FirstOrDefault(s => s.Name.Equals(name));

				if (selected != null)
				{
					namesBox.SelectedItem = selected;
				}
				else
				{
					namesBox.SelectedIndex = 0;
				}
			}
		}


		private void LoadTheme(object sender, EventArgs e)
		{
			using var dialog = new OpenFileDialog();
			dialog.DefaultExt = "xml";
			dialog.Filter = "Theme files (*.xml)|*.xml|All files (*.*)|*.*";
			dialog.Multiselect = false;
			dialog.Title = "Open Style Theme";
			dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
			dialog.AutoUpgradeEnabled = true; // simpler UI, faster

			var path = Path.Combine(PathHelper.GetAppDataPath(), Resx.ThemesFolder);
			if (Directory.Exists(path))
			{
				dialog.InitialDirectory = path;
			}
			else
			{
				dialog.InitialDirectory =
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				theme = new ThemeProvider(dialog.FileName).Theme;
				var styles = theme?.GetStyles();
				if (styles?.Count > 0)
				{
					LoadStyles(styles);

					// update dialog title
					Text = string.Format(Resx.StyleDialog_ThemeText, theme.Name);
				}
				else
				{
					MessageBox.Show(this, "Could not load this theme file?", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}


		private void SaveTheme(object sender, EventArgs e)
		{
			using var dialog = new SaveFileDialog();
			dialog.DefaultExt = "xml";
			dialog.Filter = "Theme files (*.xml)|*.xml|All files (*.*)|*.*";
			dialog.Title = "Save Style Theme";
			dialog.ShowHelp = true; // stupid, but this is needed to avoid hang

			var path = PathHelper.GetAppDataPath();
			if (Directory.Exists(path))
			{
				dialog.InitialDirectory = path;
			}
			else
			{
				dialog.InitialDirectory =
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var key = Path.GetFileNameWithoutExtension(dialog.FileName);
				theme = new Theme(MakeStyles(), key, key, theme.Dark);
				ThemeProvider.Save(theme, dialog.FileName);

				Text = string.Format(
					Resx.StyleDialog_ThemeText,
					Path.GetFileNameWithoutExtension(dialog.FileName));
			}
		}
	}
}
