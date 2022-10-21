//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class EditTableThemesDialog : UI.LocalizableForm
	{
		private const int margin = 15;

		private readonly Rectangle bounds;
		private readonly TableThemePainter painter;
		private readonly List<TableTheme> themes;
		private TableTheme snapshot;
		private bool dirty;

		#region Swatch
		private sealed class SwatchClickedEventArgs : EventArgs
		{
			public SwatchClickedEventArgs(int index) { ItemIndex = index; }
			public int ItemIndex { get; set; }
		}

		delegate void SwatchClickedHandler(object sender, SwatchClickedEventArgs e);

		private sealed class Swatch : UserControl
		{
			private readonly PictureBox picture;
			private readonly Image image;

			public Swatch(MoreListView view)
			{
				image = new Bitmap(34, 24);

				picture = new PictureBox
				{
					Image = image,
					Width = image.Width,
					Height = image.Height,
					Location = new Point(0, 2)
				};

				SetColor(Color.WhiteSmoke);

				var button = new MoreLinkLabel
				{
					Text = "Edit",
					Font = new Font("Segoe UI", 7, FontStyle.Regular),
					Margin = new Padding(5, 2, 0, 2),
					Location = new Point(picture.Width, 2)
				};

				button.LinkClicked += new LinkLabelLinkClickedEventHandler((s, e) =>
				{
					if (((Control)s).Parent.Tag is ListViewItem host)
					{
						var index = view.Items.IndexOf(host);
						Clicked?.Invoke(host, new SwatchClickedEventArgs(index));
					}
				});

				Width = 100;
				Height = 28;
				Margin = new Padding(0, 2, 0, 2);
				Padding = new Padding(0, 2, 0, 2);

				Controls.Add(button);
				Controls.Add(picture);
			}

			public event SwatchClickedHandler Clicked;
			public Color Color { get; private set; }

			public void SetColor(Color color)
			{
				Color = color;
				using var g = Graphics.FromImage(image);
				g.Clear(color);
				picture.Invalidate();
			}
		}
		#endregion Swatch


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
			// snapshot should always be a copy of the currently selected theme,
			// not a reference, so here we instantiate two new instances
			snapshot = new TableTheme { Name = "New" };
			themes.Insert(0, new TableTheme { Name = "New" });

			this.themes = themes;

			var binding = new BindingList<TableTheme>();
			namesBox.DataSource = binding;
			namesBox.DisplayMember = "Name";

			themes.ForEach(t => binding.Add(t));

			painter.Paint(snapshot);
			dirty = false;
		}


		private void InitializeForm(object sender, EventArgs e)
		{
			if (namesBox.Items.Count > 0)
			{
				namesBox.SelectedIndex = 0;
			}
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

				var swatch = new Swatch(elementsBox);
				swatch.Clicked += ChangeElementColor;
				var subitem = item.AddHostedSubItem($"{name}Sub", swatch);
				subitem.Alignment = ContentAlignment.MiddleCenter;
			}
		}


		private void ChooseTheme(object sender, EventArgs e)
		{
			if (dirty)
			{

			}

			themes[namesBox.SelectedIndex].CopyTo(snapshot);

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

			void SetSwatch(int index, Color color)
			{
				if (color == Color.Empty)
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


		private void ChangeElementColor(object sender, SwatchClickedEventArgs e)
		{
			var swatch = GetSwatch(e.ItemIndex);

			MessageBox.Show($"items[{e.ItemIndex}] color:{swatch.Color} selectedIndex:{namesBox.SelectedIndex}");
			if (namesBox.SelectedIndex < 0 || namesBox.SelectedIndex >= namesBox.Items.Count)
			{
				return;
			}

			var theme = themes[namesBox.SelectedIndex];

			switch (e.ItemIndex)
			{
				case 0: theme.WholeTable = Color.Blue; break;
				case 1: theme.FirstColumnStripe = Color.Red; break;
				case 2: theme.SecondColumnStripe = Color.White; break;
				case 3: theme.FirstRowStripe = Color.Orange; break;
				case 4: theme.SecondRowStripe = Color.White; break;
				case 5: theme.FirstColumn = Color.Green; break;
				case 6: theme.LastColumn = Color.LightGreen; break;
				case 7: theme.HeaderRow = Color.Gray; break;
				case 8: theme.TotalRow = Color.LightGray; break;
				case 9: theme.HeaderFirstCell = Color.Black; break;
				case 10: theme.HeaderLastCell = Color.Black; break;
				case 11: theme.TotalFirstCell = Color.Pink; break;
				case 12: theme.TotalLastCell = Color.Pink; break;
			}

			swatch.SetColor(Color.Red);

			painter.Paint(theme);
			previewBox.Invalidate();

			dirty = !theme.Equals(snapshot);
			saveButton.Enabled = dirty;
			resetButton.Enabled = dirty;
		}


		private void ResetTheme(object sender, EventArgs e)
		{
			var theme = themes[namesBox.SelectedIndex];
			snapshot.CopyTo(theme);
			ChooseTheme(sender, e);
			dirty = false;
			saveButton.Enabled = false;
			resetButton.Enabled = false;
		}

		private void SaveTheme(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(namesBox.Text))
			{
				MoreMessageBox.ShowError(Owner, "Enter a name");
				return;
			}

			if (namesBox.SelectedIndex == 0)
			{

			}

			new TableThemeProvider().SaveUserThemes(themes);
		}


		private void DeleteTheme(object sender, EventArgs e)
		{

		}

		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{

		}
	}
}
