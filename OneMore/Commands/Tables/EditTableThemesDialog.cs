//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class EditTableThemesDialog : UI.LocalizableForm
	{
		private const int margin = 15;

		private readonly Rectangle bounds;
		private readonly TableThemePainter painter;
		private readonly List<TableTheme> themes;
		private bool dirty;


		public EditTableThemesDialog()
		{
			InitializeComponent();
			InitializeElementsBox();

			previewBox.Image = new Bitmap(previewBox.Width, previewBox.Height);

			bounds = new Rectangle(
				margin, margin,
				previewBox.Width - (margin * 2), previewBox.Height - (margin * 2));

			painter = new TableThemePainter(previewBox.Image, bounds, SystemColors.Window);
		}


		public EditTableThemesDialog(List<TableTheme> themes)
			: this()
		{
			this.themes = themes;

			var binding = new BindingList<TableTheme>();
			namesBox.DataSource = binding;
			namesBox.DisplayMember = "Name";

			foreach (var theme in themes)
			{
				binding.Add(theme);
			}

			dirty = false;

			painter.Paint(new TableTheme());
		}


		private void InitializeElementsBox()
		{
			elementsBox.HighlightBackground = Color.Transparent;
			elementsBox.Columns.Add(new MoreColumnHeader("Element", 280) { AutoSizeItems = true });
			elementsBox.Columns.Add(new MoreColumnHeader("Color", 120));

			var names = Regex.Split(Resx.EditTableThemesDialog_elements, @"\r\n|\r|\n");
			foreach (var name in names)
			{
				var item = elementsBox.AddHostedItem(name);

				var subitem = item.AddHostedSubItem($"{name}Sub", MakeButton(elementsBox));
				subitem.Alignment = ContentAlignment.MiddleCenter;
			}
		}


		private Control MakeButton(MoreListView view)
		{
			var picture = new PictureBox
			{
				Image = new Bitmap(34, 24),
				Dock = DockStyle.Left
			};

			using var g = Graphics.FromImage(picture.Image);
			g.Clear(Color.WhiteSmoke);

			var button = new Button
			{
				Dock = DockStyle.Right,
				Text = "edit",
				Font = new Font("Segoe UI", 6, FontStyle.Regular),
				Padding = new Padding(0),
				Margin = new Padding(4, 2, 0, 2),
				FlatStyle = FlatStyle.Flat,
				Width = 60,
				Height = 30
			};

			button.MouseClick += new MouseEventHandler((s, e) =>
			{
				if (((Control)s).Tag is ListViewItem host)
				{
					//view.SelectIf(host);
					var index = view.Items.IndexOf(host);
					MessageBox.Show($"items[{index}]");
				}
			});

			var control = new UserControl
			{
				Width = 100,
				Height = 30,
				Margin = new Padding(0, 2, 0, 2),
				Padding = new Padding(0, 2, 0, 2)
			};

			control.Controls.Add(button);
			control.Controls.Add(picture);

			return control;
		}

		private void ChooseTheme(object sender, System.EventArgs e)
		{

		}

		private void ResetTheme(object sender, System.EventArgs e)
		{

		}

		private void RenameTheme(object sender, System.EventArgs e)
		{

		}

		private void DeleteTheme(object sender, System.EventArgs e)
		{

		}
	}
}
