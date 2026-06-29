//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Colorizer = Colorizer.Colorizer;
	using Resx = Properties.Resources;


	internal partial class ColorizeDialog : UI.MoreForm
	{
		private readonly Dictionary<string, Bitmap> languageImages = new();


		public ColorizeDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ColorizeDialog_Title;

				Localize(new string[]
				{
					"okButton=word_OK"
				});
			}

			var languages = LoadFilteredLanguages();
			LoadLanguageImages(languages);

			view.SmallImageList = new ImageList { ImageSize = new Size(1, 18) };
			view.GetCellImage = GetLanguageCellImage;

			foreach (var key in languages.Keys)
			{
				view.Items.Add(new ListViewItem(key) { Tag = languages[key] });
			}

			view.SetColumnProportions(1f);

			if (view.Items.Count > 0)
			{
				view.Items[0].Selected = true;
			}
		}


		public static IDictionary<string, string> LoadFilteredLanguages()
		{
			var languages = Colorizer.LoadLanguageNames();

			// load hidden languages from Settings
			var hidden = new SettingsProvider()
				.GetCollection(nameof(ColorizerSheet))
				.Get(ColorizerSheet.HiddenKey, new XElement(ColorizerSheet.HiddenKey));

			// remove hidden languages
			var keys = languages.Keys.ToList();
			foreach (var key in keys.Where(key => hidden.Element(languages[key]) is not null))
			{
				languages.Remove(key);
			}

			return languages;
		}


		public string LanguageKey => view.SelectedItems.Count == 0
			? null
			: view.SelectedItems[0].Tag as string;


		private void LoadLanguageImages(IDictionary<string, string> languages)
		{
			foreach (var key in languages.Keys)
			{
				try
				{
					var path = Path.Combine(
						Colorizer.GetColorizerDirectory(), $@"Languages\{languages[key]}.png");

					if (File.Exists(path))
					{
						using var original = Image.FromFile(path);
						var scaled = new Bitmap(16, 16);
						using (var g = Graphics.FromImage(scaled))
						{
							g.InterpolationMode =
								System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
							g.DrawImage(original, 0, 0, 16, 16);
						}
						languageImages[key] = scaled;
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);
				}
			}
		}


		private Image GetLanguageCellImage(ListViewItem item, int columnIndex) =>
			languageImages.TryGetValue(item.Text, out var image) ? image : null;


		protected override void OnClosed(EventArgs e)
		{
			foreach (var image in languageImages.Values)
			{
				image.Dispose();
			}
			languageImages.Clear();
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			view.Focus();
		}


		private void SelectOnDoubleClick(object sender, EventArgs e)
		{
			if (view.SelectedItems.Count > 0)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}



		private void CancelOnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
