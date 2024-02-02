//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using Colorizer = Colorizer.Colorizer;
	using Resx = Properties.Resources;


	internal partial class ColorizeDialog : UI.MoreForm
	{
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

			var languages = Colorizer.LoadLanguageNames();
			var imageList = MakeImageList(languages);

			view.SmallImageList = imageList;

			var index = 0;
			foreach (var key in languages.Keys)
			{
				view.Items.Add(new ListViewItem(key, index++)
				{
					Tag = languages[key]
				});
			}

			if (view.Items.Count > 0)
			{
				view.Items[0].Selected = true;
			}
		}


		public string LanguageKey => view.SelectedItems.Count == 0
			? null
			: view.SelectedItems[0].Tag as string;


		private ImageList MakeImageList(IDictionary<string, string> languages)
		{
			var list = new ImageList();

			foreach (var key in languages.Keys)
			{
				try
				{
					var path = Path.Combine(
						Colorizer.GetColorizerDirectory(), $@"Languages\{languages[key]}.png");

					if (File.Exists(path))
					{
						list.Images.Add((Bitmap)Image.FromFile(path));
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);
				}
			}

			return list;
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
