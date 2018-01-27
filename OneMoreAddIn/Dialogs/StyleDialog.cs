//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

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
		private readonly string DefaultFontFamily = "Calibri";
		private const int DefaultFontSize = 11;

		private CustomStyle selection;

		private bool updatable;


		#region Lifecycle

		/// <summary>
		/// Create a dialog to edit a single style; this is for creating new styles
		/// </summary>
		/// <param name="style"></param>

		public StyleDialog (CustomStyle style)
		{
			Initialize();
			updatable = true;

			Text = "New Custom Style";
			loadButton.Enabled = false;
			reorderButton.Enabled = false;
			deleteButton.Enabled = false;

			this.selection = style;
			ShowSelection();
		}


		/// <summary>
		/// Create a dialog to edit multiple styles; this is for editing existing styles.
		/// </summary>
		/// <param name="styles"></param>

		public StyleDialog (List<CustomStyle> styles)
		{
			Initialize();
			updatable = true;
			LoadStyles(styles);
		}

		private void Initialize ()
		{
			InitializeComponent();

			spaceAfterSpinner.Value = 0;
			spaceBeforeSpinner.Value = 0;
			familyBox.SelectedIndex = familyBox.Items.IndexOf(DefaultFontFamily);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(DefaultFontSize.ToString());
		}


		private void LoadStyles (List<CustomStyle> styles)
		{
			namesBox.Items.Clear();

			if (styles.Count == 0)
			{
				styles.Add(new CustomStyle("Normal",
					new Font(DefaultFontFamily, DefaultFontSize),
					Color.Black, Color.Transparent));
			}

			namesBox.Items.AddRange(styles.ToArray());

			namesBox.Location = nameBox.Location;
			namesBox.Size = nameBox.Size;
			namesBox.Visible = true;
			nameBox.Visible = false;

			namesBox.SelectedIndex = 0;
		}


		private void StyleDialog_Shown (object sender, EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
			nameBox.Focus();
		}

		#endregion Lifecycle


		private void ShowSelection ()
		{
			updatable = false;

			nameBox.Text = selection.Name;
			familyBox.Text = selection.Font.FontFamily.Name;

			if (selection.Font.Size % 1 == 0)
				sizeBox.Text = ((int)selection.Font.Size).ToString();
			else
				sizeBox.Text = selection.Font.Size.ToString("#.0");

			boldButton.Checked = selection.Font.Bold;
			italicButton.Checked = selection.Font.Italic;
			underlineButton.Checked = selection.Font.Underline;

			spaceAfterSpinner.Value = selection.SpaceAfter;
			spaceBeforeSpinner.Value = selection.SpaceBefore;

			headingBox.Checked = selection.IsHeading;

			updatable = true;
		}


		/// <summary>
		/// Gets the currently selected custom style
		/// </summary>

		public CustomStyle CustomStyle => selection;

		public List<CustomStyle> GetStyles () => namesBox.Items.Cast<CustomStyle>().ToList();


		private void previewBox_Paint (object sender, PaintEventArgs e)
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

				if (!selection.Background.IsEmpty && !selection.Background.Equals(Color.Transparent))
				{
					using (var highBrush = new SolidBrush(selection.Background))
					{
						e.Graphics.FillRectangle(highBrush,
							clip.X, clip.Y, Math.Min(clip.Width, sampleSize.Width), clip.Height);
					}
				}

				using (var brush = new SolidBrush(selection.Color))
				{
					e.Graphics.DrawString("Sample Text", sample, brush, clip);
				}
			}
		}


		private Font MakeFont ()
		{
			if (!float.TryParse(sizeBox.Text, out var size))
			{
				size = (float)DefaultFontSize;
			}

			FontStyle style = 0;
			if (boldButton.Checked) style |= FontStyle.Bold;
			if (italicButton.Checked) style |= FontStyle.Italic;
			if (underlineButton.Checked) style |= FontStyle.Underline;

			return new Font(familyBox.Text, size, style);
		}


		private void UpdateFont (object sender, EventArgs e)
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

		private void nameBox_TextChanged (object sender, EventArgs e)
		{
			if (selection != null)
			{
				selection.Name = nameBox.Text;
			}

			okButton.Enabled = (nameBox.Text.Length > 0);
		}

		private void namesBox_SelectedIndexChanged (object sender, EventArgs e)
		{
			selection = (CustomStyle)namesBox.SelectedItem;
			ShowSelection();
		}


		private void colorButton_Click (object sender, EventArgs e)
		{
			var color = SelectColor("Text Color", colorButton.Bounds, selection.Color);
			if (!color.Equals(Color.Empty))
			{
				selection.Color = color;
				previewBox.Invalidate();
			}
		}

		private void defaultBlackToolStripMenuItem_Click (object sender, EventArgs e)
		{
			selection.Color = Color.Black;
			previewBox.Invalidate();
		}


		private void backColorButton_ButtonClick (object sender, EventArgs e)
		{
			var color = SelectColor("Highlight Color", backColorButton.Bounds, selection.Background);
			if (!color.Equals(Color.Empty))
			{
				selection.Background = color;
				previewBox.Invalidate();
			}
		}

		private void transparentToolStripMenuItem_Click (object sender, EventArgs e)
		{
			selection.Background = Color.Transparent;
			previewBox.Invalidate();
		}


		private Color SelectColor (string title, Rectangle bounds, Color color)
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

		private void spaceAfterSpinner_ValueChanged (object sender, EventArgs e)
		{
			selection.SpaceAfter = (int)spaceAfterSpinner.Value;

		}

		private void spaceBeforeSpinner_ValueChanged (object sender, EventArgs e)
		{
			selection.SpaceBefore = (int)spaceBeforeSpinner.Value;
		}

		private void headingBox_CheckedChanged (object sender, EventArgs e)
		{
			selection.IsHeading = headingBox.Checked;
		}

		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


		private void loadButton_Click (object sender, EventArgs e)
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
					var styles = new StylesProvider().LoadTheme(dialog.FileName);
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


		private void reorderButton_Click (object sender, EventArgs e)
		{
			using (var dialog = new ReorderDialog(namesBox.Items))
			{
				var result = dialog.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					string name = null;
					if (namesBox.SelectedItem != null)
					{
						name = ((CustomStyle)namesBox.SelectedItem).Name;
					}

					var items = dialog.GetItems();
					namesBox.Items.Clear();
					namesBox.Items.AddRange(items);

					var selected = namesBox.Items.Cast<CustomStyle>().Where(s => s.Name.Equals(name)).FirstOrDefault();
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


		private void deleteButton_Click (object sender, EventArgs e)
		{
			var result = MessageBox.Show(this, "Delete this custom style?", "Confirm",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

			if (result == DialogResult.Yes)
			{
				int index = namesBox.SelectedIndex;

				var style = namesBox.Items[index] as CustomStyle;
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
