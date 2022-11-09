//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class EditTableThemesDialog : UI.LocalizableForm
	{
		private const int PreviewMargin = 15;

		private static readonly float xScaling;
		private static readonly float yScaling;

		private readonly TableThemePainter painter;
		private List<TableTheme> themes;
		private TableTheme snapshot;
		private TableTheme.ColorFont colorfont;
		private bool reorganizing;


		#region Swatch
		private sealed class SwatchClickedEventArgs : EventArgs
		{
			public SwatchClickedEventArgs(int index) { ItemIndex = index; }
			public int ItemIndex { get; set; }
			public bool Reset { get; set; }
		}

		delegate void SwatchClickedHandler(object sender, SwatchClickedEventArgs e);

		private sealed class Swatch : UserControl
		{
			private readonly PictureBox picture;
			private readonly Image image;

			public Swatch(MoreListView view)
			{
				image = new Bitmap((int)(24 * xScaling), (int)(16 * yScaling));

				picture = new PictureBox
				{
					Image = image,
					Dock = DockStyle.Left,
					Width = image.Width,
					Height = image.Height
				};

				SetColor(Color.WhiteSmoke);

				var edit = new MoreLinkLabel
				{
					Text = Resx.word_Edit,
					Font = new Font("Segoe UI", 8, FontStyle.Regular),
					Margin = new Padding(5, 2, 0, 2),
					Dock = DockStyle.Left
				};

				using var g = Graphics.FromImage(image);
				var size = g.MeasureString(edit.Text, edit.Font);
				edit.Width = (int)(size.Width + 8);

				edit.LinkClicked += new LinkLabelLinkClickedEventHandler((s, e) =>
				{
					if (((Control)s).Parent.Tag is ListViewItem host)
					{
						var index = view.Items.IndexOf(host);
						Clicked?.Invoke(host, new SwatchClickedEventArgs(index));
					}
				});

				var reset = new MoreLinkLabel
				{
					Text = Resx.word_Reset,
					Font = new Font("Segoe UI", 8, FontStyle.Regular),
					Margin = new Padding(7, 2, 0, 2),
					Dock = DockStyle.Fill
				};

				reset.LinkClicked += new LinkLabelLinkClickedEventHandler((s, e) =>
				{
					if (((Control)s).Parent.Tag is ListViewItem host)
					{
						var index = view.Items.IndexOf(host);
						Clicked?.Invoke(host,
							new SwatchClickedEventArgs(index) { Reset = true });
					}
				});

				Width = 160;
				Height = 28;
				Margin = new Padding(0, 2, 0, 2);
				Padding = new Padding(0, 2, 0, 2);

				Controls.Add(reset);
				Controls.Add(edit);
				Controls.Add(picture);
			}

			public event SwatchClickedHandler Clicked;
			public Color Color { get; private set; }

			public void SetColor(Color color)
			{
				Color = color;

				using var g = Graphics.FromImage(image);
				g.Clear(SystemColors.Window);
				var bounds = new Rectangle(0, 0, image.Size.Width - 1, image.Size.Height - 1);
				using var brush = new SolidBrush(color);
				g.FillRectangle(brush, bounds);
				g.DrawRectangle(Pens.DarkGray, new Rectangle(0, 0, bounds.Width, bounds.Height));

				picture.Invalidate();
			}
		}
		#endregion Swatch


		static EditTableThemesDialog()
		{
			(xScaling, yScaling) = UIHelper.GetScalingFactors();
		}


		public EditTableThemesDialog()
		{
			InitializeComponent();

			InitializeElementsBox();
			InitializeFontElementsBox();

			if (NeedsLocalizing())
			{
				Text = Resx.EditTableThemesDialog_Text;

				Localize(new string[]
				{
					"nameLabel=word_Name",
					"newButton",
					"renameButton=word_Rename",
					"saveButton=word_Save",
					"deleteButton=word_Delete",
					"colorsTab",
					"elementsGroup",
					"elementsBox",
					"previewGroup=word_Preview",
					"resetButton",
					"fontsTab",
					"fontElementsGroup",
					"fontsGroup=word_Font",
					"applyFontButton=word_Apply",
					"resetFontButton=word_Reset",
					"defaultFontButton=word_Default",
					"cancelButton=word_Close"
				});
			}

			toolstrip.Rescale();

			previewBox.Image = new Bitmap(previewBox.Width, previewBox.Height);

			var bounds = new Rectangle(
				PreviewMargin, PreviewMargin,
				previewBox.Width - (PreviewMargin * 2),
				previewBox.Height - (PreviewMargin * 2));

			painter = new TableThemePainter(previewBox.Image, bounds, SystemColors.Window);

			// hack to force MoreListView items to layout and repaint
			colorsTab.Paint += AdjustSizeOnce;
			colorsTab.Tag = new object();
			fontsTab.Paint += AdjustSizeOnce;
			fontsTab.Tag = new object();
		}


		public EditTableThemesDialog(List<TableTheme> themes)
			: this()
		{
			// snapshot should always be a copy of the currently selected theme,
			// not a reference, so instantiate the snapshot and copy into it
			snapshot = new TableTheme();

			if (themes.Count == 0)
			{
				themes.Insert(0, new TableTheme { Name = Resx.EditTableThemesDialog_NewStyle });
				newButton.Enabled = false;
			}
			else
			{
				themes[0].CopyTo(snapshot);
			}

			this.themes = themes;

			themes.ForEach(t => { combo.Items.Add(t); });
			combo.SelectedIndex = 0;

			painter.Paint(snapshot);

			reorganizing = true;
			familyBox.LoadFontFamilies();
			familyBox.SelectedIndex = familyBox.Items.IndexOf(StyleBase.DefaultFontFamily);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(StyleBase.DefaultFontSize.ToString("0.#", AddIn.Culture));
			colorFontsBox.Items[0].Selected = true;
			reorganizing = false;
		}


		public bool Modified { get; private set; }


		private void InitializeElementsBox()
		{
			elementsBox.StateImageList = new ImageList
			{
				ImageSize = new Size(1, (int)(16 * yScaling))
			};

			elementsBox.HighlightBackground = Color.Transparent;
			elementsBox.HighlightForeground = SystemColors.ControlText;

			var width = (int)(elementsBox.Width * 0.6);
			elementsBox.Columns.Add(new MoreColumnHeader(Resx.word_Element, width) { AutoSizeItems = true });

			width = (int)(elementsBox.Width * 0.35);
			elementsBox.Columns.Add(new MoreColumnHeader(Resx.word_Color, width));

			var names = Regex.Split(Resx.EditTableThemesDialog_elements, @"\r\n|\r|\n");
			foreach (var name in names)
			{
				var item = elementsBox.AddHostedItem(name);
				var swatch = new Swatch(elementsBox);
				swatch.Clicked += ChangeElementColor;
				var subitem = item.AddHostedSubItem(swatch);
				subitem.Alignment = ContentAlignment.MiddleCenter;
			}
		}


		private void InitializeFontElementsBox()
		{
			colorFontsBox.StateImageList = new ImageList
			{
				ImageSize = new Size(1, (int)(16 * yScaling))
			};

			var width = (int)(colorFontsBox.Width * 0.35);
			colorFontsBox.Columns.Add(new MoreColumnHeader(Resx.word_Element, width) { AutoSizeItems = true });

			width = (int)(colorFontsBox.Width * 0.55);
			colorFontsBox.Columns.Add(new MoreColumnHeader(Resx.word_Font, width));

			var names = Regex.Split(Resx.EditTableThemesDialog_fontElements, @"\r\n|\r|\n");
			foreach (var name in names)
			{
				var item = colorFontsBox.AddHostedItem(name);
				var link = new MoreLinkLabel
				{
					Text = Resx.word_Default,
					AutoSize = true
				};
				link.LinkClicked += ChangeElementFont;
				item.AddHostedSubItem(link);
			}
		}


		private void AdjustSizeOnce(object sender, PaintEventArgs e)
		{
			// playing tricks here with the Tag property, using it as a flag that something
			// needs to be done and done exactly once for each of the two tab pages...

			if (sender is TabPage page && page.Tag != null)
			{
				// by forcing the dialog to resize just one pixel, it will force the incoming
				// MoreListView to recalculate its layout and redraw itself
				Size = new Size(Size.Width + 1, Size.Height + 1);
				page.Tag = null;
			}
		}


		private void SetToolbarState()
		{
			var theme = themes[combo.SelectedIndex];
			var dirty = !theme.Equals(snapshot);
			saveButton.Enabled = dirty;
			resetButton.Enabled = dirty;
		}


		private void SortThemes()
		{
			var name = themes[combo.SelectedIndex].Name;
			themes = themes.OrderBy(t => t.Name).ToList();

			combo.Items.Clear();
			themes.ForEach(t => combo.Items.Add(t));
			combo.SelectedIndex = 0;

			combo.SelectedIndex = themes.FindIndex(t => t.Name == name);
		}


		private void ChooseTheme(object sender, EventArgs e)
		{
			if (combo.SelectedIndex < 0)
			{
				// TODO: is there a lifecycle event that causes this?
				logger.WriteLine($"ChooseTheme -1");
				return;
			}

			if (reorganizing)
			{
				return;
			}

			themes[combo.SelectedIndex].CopyTo(snapshot);

			SetSwatch(0, snapshot.WholeTable);
			SetSwatch(1, snapshot.FirstColumnStripe);
			SetSwatch(2, snapshot.SecondColumnStripe);
			SetSwatch(3, snapshot.FirstRowStripe);
			SetSwatch(4, snapshot.SecondRowStripe);
			SetSwatch(5, snapshot.FirstColumn);
			SetSwatch(6, snapshot.LastColumn);
			SetSwatch(7, snapshot.HeaderRow);
			SetSwatch(8, snapshot.TotalRow);
			SetSwatch(9, snapshot.HeaderFirstCell);
			SetSwatch(10, snapshot.HeaderLastCell);
			SetSwatch(11, snapshot.TotalFirstCell);
			SetSwatch(12, snapshot.TotalLastCell);

			painter.Paint(snapshot);
			previewBox.Invalidate();

			ShowFontLinkLabel(0, snapshot.DefaultFont);
			ShowFontLinkLabel(1, snapshot.HeaderFont);
			ShowFontLinkLabel(2, snapshot.TotalFont);
			ShowFontLinkLabel(3, snapshot.FirstColumnFont);
			ShowFontLinkLabel(4, snapshot.LastColumnFont);

			void SetSwatch(int index, Color color)
			{
				if (color.IsEmpty)
				{
					color = Color.WhiteSmoke;
				}

				GetSwatch(index).SetColor(color);
			}
		}


		private Swatch GetSwatch(int index)
		{
			return (Swatch)((MoreHostedListViewSubItem)elementsBox
				.Items[index].SubItems[1]).Control;
		}


		// Colors - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ChangeElementColor(object sender, SwatchClickedEventArgs e)
		{
			var swatch = GetSwatch(e.ItemIndex);
			Color color;

			if (e.Reset)
			{
				color = Color.Empty;
			}
			else
			{
				var location = PointToScreen(swatch.Location);
				using var dialog = new MoreColorDialog("Element Color", location.X + 75, location.Y + 60)
				{
					Color = swatch.Color
				};

				var result = dialog.ShowDialog();
				if (result == DialogResult.Cancel)
				{
					return;
				}

				color = dialog.Color;
			}

			var theme = themes[combo.SelectedIndex];

			switch (e.ItemIndex)
			{
				case 0: theme.WholeTable = color; break;
				case 1: theme.FirstColumnStripe = color; break;
				case 2: theme.SecondColumnStripe = color; break;
				case 3: theme.FirstRowStripe = color; break;
				case 4: theme.SecondRowStripe = color; break;
				case 5: theme.FirstColumn = color; break;
				case 6: theme.LastColumn = color; break;
				case 7: theme.HeaderRow = color; break;
				case 8: theme.TotalRow = color; break;
				case 9: theme.HeaderFirstCell = color; break;
				case 10: theme.HeaderLastCell = color; break;
				case 11: theme.TotalFirstCell = color; break;
				case 12: theme.TotalLastCell = color; break;
			}

			swatch.SetColor(color.IsEmpty ? Color.WhiteSmoke : color);

			painter.Paint(theme);
			previewBox.Invalidate();

			SetToolbarState();
		}


		private void ResetTheme(object sender, EventArgs e)
		{
			reorganizing = true;
			var theme = themes[combo.SelectedIndex];
			snapshot.CopyTo(theme);
			SortThemes();
			SetToolbarState();
			reorganizing = false;

			ChooseTheme(sender, e);
		}


		// Fonts - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ShowFontLinkLabel(int index, TableTheme.ColorFont font)
		{
			if (colorFontsBox.Items[index].SubItems[1] is MoreHostedListViewSubItem subitem)
			{
				if (subitem.Control is MoreLinkLabel label)
				{
					if (font != null)
					{
						label.Font = Font;
					}

					label.Text = font?.ToString() ?? Resx.word_Default;
				}
			}
		}


		private void ShowFontProperties(object sender, EventArgs e)
		{
			if (colorFontsBox.SelectedIndices.Count == 0)
			{
				return;
			}

			TableTheme.ColorFont font = null;
			var theme = themes[combo.SelectedIndex];
			var itemIndex = colorFontsBox.SelectedIndices[0];

			switch (itemIndex)
			{
				case 0: font = theme.DefaultFont; break;
				case 1: font = theme.HeaderFont; break;
				case 2: font = theme.TotalFont; break;
				case 3: font = theme.FirstColumnFont; break;
				case 4: font = theme.LastColumnFont; break;
			}

			font ??= new TableTheme.ColorFont();

			int index;

			if (familyBox.Items.Count > 0)
			{
				var name = font.Font?.FontFamily.Name ?? StyleBase.DefaultFontFamily;
				index = familyBox.Items.IndexOf(name);

				if (index < 0) index = 0;
				familyBox.SelectedIndex = index;
			}

			if (sizeBox.Items.Count > 0)
			{
				var size = font.Font?.Size ?? StyleBase.DefaultFontSize;
				index = sizeBox.Items.IndexOf(size.ToString("0.#", AddIn.Culture));

				if (index < 0) index = 0;
				sizeBox.SelectedIndex = index;
			}

			if (font.Font != null)
			{
				boldButton.Checked = font.Font.Bold;
				italicButton.Checked = font.Font.Italic;
				underlineButton.Checked = font.Font.Underline;
			}
			else
			{
				boldButton.Checked = false;
				italicButton.Checked = false;
				underlineButton.Checked = false;
			}
		}


		private void ResetSelectedFont(object sender, EventArgs e)
		{
			if (colorFontsBox.SelectedIndices.Count == 0)
			{
				return;
			}

			var theme = themes[combo.SelectedIndex];
			var itemIndex = colorFontsBox.SelectedIndices[0];
			TableTheme.ColorFont font = null;

			switch (itemIndex)
			{
				case 0: font = theme.DefaultFont = new TableTheme.ColorFont(snapshot.DefaultFont); break;
				case 1: font = theme.HeaderFont = new TableTheme.ColorFont(snapshot.HeaderFont); break;
				case 2: font = theme.TotalFont = new TableTheme.ColorFont(snapshot.TotalFont); break;
				case 3: font = theme.FirstColumnFont = new TableTheme.ColorFont(snapshot.FirstColumnFont); break;
				case 4: font = theme.LastColumnFont = new TableTheme.ColorFont(snapshot.LastColumnFont); break;
			}

			ShowFontLinkLabel(itemIndex, font);
			ShowFontProperties(sender, e);

			var dirty = !theme.Equals(snapshot);
			saveButton.Enabled = dirty;
			resetButton.Enabled = dirty;
		}

		private void DefaultSelectedFont(object sender, EventArgs e)
		{
			if (colorFontsBox.SelectedIndices.Count == 0)
			{
				return;
			}

			var theme = themes[combo.SelectedIndex];
			var itemIndex = colorFontsBox.SelectedIndices[0];

			switch (itemIndex)
			{
				case 0: { theme.DefaultFont?.Dispose(); theme.DefaultFont = null; break; }
				case 1: { theme.HeaderFont?.Dispose(); theme.HeaderFont = null; break; }
				case 2: { theme.TotalFont?.Dispose(); theme.TotalFont = null; break; }
				case 3: { theme.FirstColumnFont?.Dispose(); theme.FirstColumnFont = null; break; }
				case 4: { theme.LastColumnFont?.Dispose(); theme.LastColumnFont = null; break; }
			}

			ShowFontLinkLabel(itemIndex, null);
			ShowFontProperties(sender, e);

			var dirty = !theme.Equals(snapshot);
			saveButton.Enabled = dirty;
			resetButton.Enabled = dirty;
		}


		private void ChangeElementFont(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (((Control)sender).Tag is ListViewItem host)
			{
				colorFontsBox.SelectIf(host);
				ShowFontProperties(sender, new EventArgs());

				var theme = themes[combo.SelectedIndex];
				switch (host.Index)
				{
					case 0: colorfont = theme.DefaultFont; break;
					case 1: colorfont = theme.HeaderFont; break;
					case 2: colorfont = theme.TotalFont; break;
					case 3: colorfont = theme.FirstColumnFont; break;
					case 4: colorfont = theme.LastColumnFont; break;
				}
			}

			colorfont ??= new TableTheme.ColorFont();

			colorFontsBox.Enabled = false;
			resetFontButton.Enabled = false;
			defaultFontButton.Enabled = false;
			fontsGroup.Enabled = true;
		}


		private void ChangeFontFont(object sender, EventArgs e)
		{
			if (!reorganizing && colorfont != null)
			{
				var save = colorfont.Font;
				colorfont.Font = MakeFont();
				save?.Dispose();
			}
		}


		private void ChangeFontColor(object sender, EventArgs e)
		{
			var location = PointToScreen(fontToolstrip.Location);

			using var dialog = new MoreColorDialog("Text Color",
				location.X + colorButton.Bounds.Location.X,
				location.Y + colorButton.Bounds.Height + 4);

			dialog.Color = colorfont.Foreground;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				colorfont.Foreground = dialog.Color;
			}
		}


		private void SetFontColorDefault(object sender, EventArgs e)
		{
			colorfont.Foreground = Color.Empty;
		}


		private Font MakeFont()
		{
			var text = sizeBox.Text.Trim();
			if (!float.TryParse(text, NumberStyles.Integer | NumberStyles.AllowDecimalPoint,
				AddIn.Culture, out var size))
			{
				size = (float)StyleBase.DefaultFontSize;
			}

			var style = FontStyle.Regular;
			if (boldButton.Checked) style |= FontStyle.Bold;
			if (italicButton.Checked) style |= FontStyle.Italic;
			if (underlineButton.Checked) style |= FontStyle.Underline;

			return new Font(familyBox.Text, size, style);
		}


		private void ApplyFont(object sender, EventArgs e)
		{
			var theme = themes[combo.SelectedIndex];
			var index = colorFontsBox.SelectedIndices[0];

			switch (index)
			{
				case 0:
					theme.DefaultFont?.Dispose();
					theme.DefaultFont = colorfont;
					break;

				case 1:
					theme.HeaderFont?.Dispose();
					theme.HeaderFont = colorfont;
					break;

				case 2:
					theme.TotalFont?.Dispose();
					theme.TotalFont = colorfont;
					break;

				case 3:
					theme.FirstColumnFont?.Dispose();
					theme.FirstColumnFont = colorfont;
					break;

				case 4:
					theme.LastColumnFont?.Dispose();
					theme.LastColumnFont = colorfont;
					break;
			}

			ShowFontLinkLabel(index, colorfont);

			colorfont = null;

			colorFontsBox.Enabled = true;
			resetFontButton.Enabled = true;
			defaultFontButton.Enabled = true;
			fontsGroup.Enabled = false;

			var dirty = !theme.Equals(snapshot);
			saveButton.Enabled = dirty;
			resetButton.Enabled = dirty;
		}


		// Theme - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void CreateNewTheme(object sender, EventArgs e)
		{
			if (!themes[combo.SelectedIndex].Equals(snapshot))
			{
				if (MoreMessageBox.Show(Owner, Resx.EditTableThemesDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					return;
				}

				snapshot.CopyTo(themes[combo.SelectedIndex]);
			}

			snapshot = new TableTheme();
			themes.Add(new TableTheme { Name = Resx.EditTableThemesDialog_NewStyle });
			combo.Items.Add(Resx.EditTableThemesDialog_NewStyle);
			combo.SelectedIndex = combo.Items.Count - 1;

			newButton.Enabled = false;
		}


		private void RenameTheme(object sender, EventArgs e)
		{
			var names = themes.Select(t => t.Name).ToList();

			using var dialog = new RenameDialog(names, combo.Text) { Rename = true };
			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			themes[combo.SelectedIndex].Name = dialog.StyleName;

			reorganizing = true;
			SortThemes();
			SetToolbarState();
			reorganizing = false;
		}


		private void SaveTheme(object sender, EventArgs e)
		{
			new TableThemeProvider().SaveUserThemes(themes);
			Modified = true;

			themes[combo.SelectedIndex].CopyTo(snapshot);

			SetToolbarState();
			newButton.Enabled = true;
		}


		private void DeleteTheme(object sender, EventArgs e)
		{
			if (MoreMessageBox.Show(Owner, Resx.EditTableThemesDialog_deleteStyle,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				reorganizing = true;
				var index = combo.SelectedIndex;
				themes.RemoveAt(index);
				combo.Items.RemoveAt(index);

				new TableThemeProvider().SaveUserThemes(themes);
				Modified = true;

				if (themes.Count == 0)
				{
					themes.Add(new TableTheme { Name = Resx.EditTableThemesDialog_NewStyle });
					snapshot = new TableTheme();
					combo.Items.Add(themes[0]);
					combo.SelectedIndex = 0;
					newButton.Enabled = false;
				}
				else
				{
					if (index >= themes.Count)
					{
						index = themes.Count - 1;
					}

					themes[index].CopyTo(snapshot);
					combo.SelectedIndex = index;
				}

				SetToolbarState();
				reorganizing = false;

				ChooseTheme(sender, e);
			}
		}

		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{
			if (fontsGroup.Enabled)
			{
				colorFontsBox.Enabled = true;
				resetFontButton.Enabled = true;
				defaultFontButton.Enabled = true;
				fontsGroup.Enabled = false;
				e.Cancel = true;
			}
			else if (!themes[combo.SelectedIndex].Equals(snapshot))
			{
				if (MoreMessageBox.Show(Owner, Resx.EditTableThemesDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
