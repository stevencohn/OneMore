//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class EditTableThemesDialog : UI.LocalizableForm
	{
		private const int PreviewMargin = 15;

		private readonly TableThemePainter painter;
		private List<TableTheme> themes;
		private TableTheme snapshot;
		private bool reorganizing;

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

			var bounds = new Rectangle(
				PreviewMargin, PreviewMargin,
				previewBox.Width - (PreviewMargin * 2),
				previewBox.Height - (PreviewMargin * 2));

			painter = new TableThemePainter(previewBox.Image, bounds, SystemColors.Window);
		}


		public EditTableThemesDialog(List<TableTheme> themes)
			: this()
		{
			// snapshot should always be a copy of the currently selected theme,
			// not a reference, so instantiate the snapshot and copy into it
			snapshot = new TableTheme();

			if (themes.Count == 0)
			{
				themes.Insert(0, new TableTheme { Name = "New Style" });
			}
			else
			{
				themes[0].CopyTo(snapshot);
			}

			this.themes = themes;

			themes.ForEach(t => { combo.Items.Add(t); });
			combo.SelectedIndex = 0;

			painter.Paint(snapshot);
		}


		public bool Modified { get; private set; }


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
				logger.WriteLine($"ChooseTheme reorg");
				return;
			}

			logger.WriteLine($"ChooseTheme SNAPSHOT");
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

			MessageBox.Show($"items[{e.ItemIndex}] color:{swatch.Color} selectedIndex:{combo.SelectedIndex}");
			if (combo.SelectedIndex < 0 || combo.SelectedIndex >= combo.Items.Count)
			{
				return;
			}

			var theme = themes[combo.SelectedIndex];

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
		}


		private void CreateNewTheme(object sender, EventArgs e)
		{
			if (!themes[combo.SelectedIndex].Equals(snapshot))
			{
				if (MoreMessageBox.Show(Owner, "Discard unsaved changes?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					return;
				}

				snapshot.CopyTo(themes[combo.SelectedIndex]);
			}

			snapshot = new TableTheme();
			themes.Add(new TableTheme { Name = "New Style" });
			combo.Items.Add("New Style");
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
			if (MoreMessageBox.Show(Owner, "Delete this style?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				reorganizing = true;
				var index = combo.SelectedIndex;
				themes.RemoveAt(index);
				combo.Items.RemoveAt(index);

				if (themes.Count == 0)
				{
					themes.Add(new TableTheme { Name = "New Style" });
					snapshot = new TableTheme();
					combo.SelectedIndex = 0;
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

				new TableThemeProvider().SaveUserThemes(themes);
				Modified = true;

				SetToolbarState();
				reorganizing = false;
			}
		}

		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{
			if (!themes[combo.SelectedIndex].Equals(snapshot))
			{
				if (MoreMessageBox.Show(Owner, "Discard unsaved changes?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
