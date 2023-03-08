//************************************************************************************************
// Copyright © 2023 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ImagesSheet : SheetBase
	{

		public ImagesSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "ImagesSheet";
			Title = Resx.ImagesSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"generalGroup=word_General",
					"imageViewerLabel",
					"resizeGroup",
					"widthLabel",
					"plantGroup",
					"plantAfterBox",
					"plantCollapseBox"
				});
			}

			var settings = provider.GetCollection(Name);

			// general

			// reader-makes-right...
			var viewer = settings.Get<string>("imageViewer")
				?? provider.GetCollection("GeneralSheet").Get<string>("imageViewer")
				?? provider.GetCollection("images").Get("viewer", "mspaint");

			imageViewerBox.Text = viewer;

			// resizeGroup

			widthBox.Value = settings.Get("presetWidth", 500);

			// plantGroup

			plantAfterBox.Checked = settings.Get("plantAfter", false);
			plantCollapseBox.Checked = settings.Get("plantCollapsed", false);
		}


		private void ValidateImageViewer(object sender, EventArgs e)
		{
			var path = imageViewerBox.Text.Trim();
			if (!string.IsNullOrEmpty(path) && (path != "mspaint") && !File.Exists(path))
			{
				errorProvider.SetError(imageViewerButton, Resx.phrase_PathNotFound);
			}
			else
			{
				errorProvider.SetError(imageViewerButton, string.Empty);
			}
		}


		private void BrowseImageViewer(object sender, System.EventArgs e)
		{
			// both cmdBox and argsBox use this handler
			using var dialog = new OpenFileDialog();
			dialog.Filter = "All files (*.*)|*.*";
			dialog.CheckFileExists = true;
			dialog.Multiselect = false;
			dialog.Title = Resx.ImagesSheet_imageBrowser;
			dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
			dialog.InitialDirectory = GetValidPath(imageViewerBox.Text);

			var result = dialog.ShowDialog(/* leave empty */);
			if (result == DialogResult.OK)
			{
				imageViewerBox.Text = dialog.FileName;
			}
		}


		private string GetValidPath(string path)
		{
			path = path.Trim();
			if (path == string.Empty)
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			}

			if (File.Exists(path))
			{
				return Path.GetDirectoryName(path);
			}
			else if (Directory.Exists(path))
			{
				return path;
			}

			path = Path.GetDirectoryName(path);
			if (Directory.Exists(path))
			{
				return path;
			}

			return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		}




		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			// generalGroup

			var viewer = imageViewerBox.Text.Trim();
			if (string.IsNullOrEmpty(viewer))
			{
				viewer = "mspaint";
			}
			settings.Add("imageViewer", viewer);

			// resizeGroup

			settings.Add("presetWidth", (int)widthBox.Value);

			// plantGroup

			settings.Add("plantAfter", plantAfterBox.Checked.ToString());
			settings.Add("plantCollapsed", plantCollapseBox.Checked.ToString());

			provider.SetCollection(settings);

			// restart not required
			return false;
		}
	}
}
