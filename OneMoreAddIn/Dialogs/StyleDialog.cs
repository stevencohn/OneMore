//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

#pragma warning disable IDE1006 // event handler naming convention pascal case

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// Edit a single style to create or edit multiple styles to manage.
	/// </summary>
	/// <remarks>
	/// Disposables: the List of Styles is input and managed by the consumer.
	/// All other local disposables are handled.
	/// </remarks>

	internal partial class StyleDialog : Form
	{
		private GraphicStyle selection;

		private bool updatable;


		#region Lifecycle

		/// <summary>
		/// Create a dialog to edit a single style; this is for creating new styles
		/// </summary>
		/// <param name="style"></param>

		public StyleDialog(Style style)
		{
			Initialize();

			Logger.DesignMode = DesignMode;

			updatable = true;

			Text = "New Custom Style";
			loadButton.Enabled = false;
			reorderButton.Enabled = false;
			deleteButton.Enabled = false;

			selection = new GraphicStyle(style, false);
		}


		/// <summary>
		/// Create a dialog to edit multiple styles; this is for editing existing styles.
		/// </summary>
		/// <param name="styles"></param>

		public StyleDialog(List<Style> styles)
		{
			Initialize();
			updatable = true;
			LoadStyles(styles);
		}


		private void Initialize()
		{
			InitializeComponent();

			styleTypeBox.SelectedIndex = (int)StyleType.Paragraph;
			familyBox.SelectedIndex = familyBox.Items.IndexOf(StyleBase.DefaultFontFamily);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(StyleBase.DefaultFontSize.ToString());
			spaceAfterSpinner.Value = 0;
			spaceBeforeSpinner.Value = 0;
		}


		private void LoadStyles(List<Style> styles)
		{
			// nameBox TextBox is shown when creating a new style to enter a single name
			// namesBox ComboBox is shown when editing styles to select an existing name

			namesBox.Items.Clear();

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


		private void StyleDialog_Shown(object sender, EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);

			ShowSelection();

			nameBox.Focus();
		}

		#endregion Lifecycle


		private void ShowSelection()
		{
			updatable = false;

			nameBox.Text = selection.Name;
			styleTypeBox.SelectedIndex = (int)selection.StyleType;
			familyBox.Text = selection.FontFamily;

			sizeBox.Text = selection.FontSize;

			boldButton.Checked = selection.IsBold;
			italicButton.Checked = selection.IsItalic;
			underlineButton.Checked = selection.IsUnderline;
			applyColorsBox.Checked = selection.ApplyColors;

			spaceAfterSpinner.Value = (decimal)double.Parse(selection.SpaceAfter);
			spaceBeforeSpinner.Value = (decimal)double.Parse(selection.SpaceBefore);

			updatable = true;

			previewBox.Invalidate();
		}


		/// <summary>
		/// Gets the currently selected custom style
		/// </summary>
		public Style Style => selection.GetStyle();


		public List<Style> GetStyles() =>
			namesBox.Items.Cast<GraphicStyle>().ToList().ConvertAll(e => e.GetStyle());


		private void previewBox_Paint(object sender, PaintEventArgs e)
		{
			var vcenter = previewBox.Height / 2;

			e.Graphics.Clear(Color.White); // .FillRectangle(Brushes.White, previewBox.Bounds);
			e.Graphics.DrawLine(Pens.Black, 0, vcenter, 15, vcenter);
			e.Graphics.DrawLine(Pens.Black, previewBox.Width - 15, vcenter, previewBox.Width, vcenter);

			using (var sample = MakeFont())
			{
				var sampleSize = e.Graphics.MeasureString("Sample Text", sample);
				var y = (int)(vcenter - (sampleSize.Height / 2));

				// create a clipping box so the text does not wrap
				var clip = new Rectangle(20, y, previewBox.Width - 40, (int)sampleSize.Height);

				if (selection.ApplyColors &&
					!selection.Background.IsEmpty &&
					!selection.Background.Equals(Color.Transparent))
				{
					using (var highBrush = new SolidBrush(selection.Background))
					{
						e.Graphics.FillRectangle(highBrush,
							clip.X, clip.Y, Math.Min(clip.Width, sampleSize.Width), clip.Height);
					}
				}

				Logger.Current.WriteLine(
					$"StyleDialog.preview() (family:{sample.FontFamily.Name}, size:{sample.Size}, style:{sample.Style})");

				var color = selection.ApplyColors ? selection.Foreground : Color.Black;
				using (var brush = new SolidBrush(color))
				{
					e.Graphics.DrawString("Sample Text", sample, brush, clip);
				}
			}
		}


		private Font MakeFont()
		{
			if (!float.TryParse(sizeBox.Text, out var size))
			{
				size = (float)StyleBase.DefaultFontSize;
			}

			FontStyle style = 0;
			if (boldButton.Checked) style |= FontStyle.Bold;
			if (italicButton.Checked) style |= FontStyle.Italic;
			if (underlineButton.Checked) style |= FontStyle.Underline;

			return new Font(familyBox.Text, size, style);
		}


		private void UpdateFont(object sender, EventArgs e)
		{
			if (updatable && (selection != null))
			{
				using (var oldfont = selection.Font)
				{
					selection.Font = MakeFont();
				}
			}

			previewBox.Invalidate();
		}

		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void nameBox_TextChanged(object sender, EventArgs e)
		{
			if (selection != null)
			{
				selection.Name = nameBox.Text;
			}

			okButton.Enabled = (nameBox.Text.Length > 0);
		}

		private void namesBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			selection = namesBox.SelectedItem as GraphicStyle;
			ShowSelection();
		}


		private void styleTypeBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((StyleType)styleTypeBox.SelectedIndex)
			{
				case StyleType.Character:
					spaceAfterSpinner.Enabled = false;
					spaceBeforeSpinner.Enabled = false;
					break;

				case StyleType.Paragraph:
				case StyleType.Heading:
					spaceAfterSpinner.Enabled = true;
					spaceBeforeSpinner.Enabled = true;
					break;
			}
		}


		private void colorButton_Click(object sender, EventArgs e)
		{
			var color = SelectColor("Text Color", colorButton.Bounds, selection.Foreground);
			if (!color.Equals(Color.Empty))
			{
				selection.Foreground = color;
				previewBox.Invalidate();
			}
		}

		private void defaultBlackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			selection.Foreground = Color.Black;
			previewBox.Invalidate();
		}


		private void backColorButton_ButtonClick(object sender, EventArgs e)
		{
			var color = SelectColor("Highlight Color", backColorButton.Bounds, selection.Background);
			if (!color.Equals(Color.Empty))
			{
				selection.Background = color;
				previewBox.Invalidate();
			}
		}

		private void transparentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			selection.Background = Color.Transparent;
			previewBox.Invalidate();
		}


		private Color SelectColor(string title, Rectangle bounds, Color color)
		{
			var location = PointToScreen(toolStrip.Location);

			using (var dialog = new ColorDialogEx(title,
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


		private void applyColorsBox_CheckedChanged(object sender, EventArgs e)
		{
			selection.ApplyColors
				= colorButton.Enabled
				= backColorButton.Enabled
				= applyColorsBox.Checked;

			previewBox.Invalidate();
		}


		private void spaceAfterSpinner_ValueChanged(object sender, EventArgs e)
		{
			selection.SpaceAfter = spaceAfterSpinner.Value.ToString("#0.0");

		}

		private void spaceBeforeSpinner_ValueChanged(object sender, EventArgs e)
		{
			selection.SpaceBefore = spaceBeforeSpinner.Value.ToString("#0.0");
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


		private void loadButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.DefaultExt = "xml";
				dialog.Filter = "Theme files (*.xml)|*.xml|All files (*.*)|*.*";
				dialog.Multiselect = false;
				dialog.Title = "Open Style Theme";
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang

				var path = PathFactory.GetAppDataPath();
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
					var styles = new StyleProvider().LoadTheme(dialog.FileName);
					if (styles?.Count > 0)
					{
						LoadStyles(styles);
					}
					else
					{
						MessageBox.Show(this, "Could not load this theme file?", "Error",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}


		private void reorderButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ReorderDialog(namesBox.Items))
			{
				var result = dialog.ShowDialog(this);
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

					var selected = namesBox.Items.Cast<GraphicStyle>().Where(s => s.Name.Equals(name)).FirstOrDefault();
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
		}


		private void deleteButton_Click(object sender, EventArgs e)
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
				reorderButton.Enabled = false;
				deleteButton.Enabled = false;
			}
		}
	}
}
