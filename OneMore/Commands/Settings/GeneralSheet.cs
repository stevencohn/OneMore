//************************************************************************************************
// Copyright © 2021 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Windows.UI.Xaml.Controls;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class GeneralSheet : SheetBase
	{
		public GeneralSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			LoadLanguages();

			Name = nameof(GeneralSheet);
			Title = Resx.GeneralSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"enablersBox",
					"checkUpdatesBox",
					"langLabel",
					"imageViewerLabel"
				});
			}

			var settings = provider.GetCollection(Name);

			enablersBox.Checked = settings.Get("enablers", true);
			checkUpdatesBox.Checked = settings.Get("checkUpdates", false);

			var lang = settings.Get("language", "en-US");
			foreach (CultureInfo info in langBox.Items)
			{
				if (info.Name == lang)
				{
					langBox.SelectedItem = info;
					break;
				}
			}

			// reader-makes-right...
			var viewer = settings.Get<string>("imageViewer")
				?? provider.GetCollection("images").Get("viewer", "mspaint");

			imageViewerBox.Text = viewer;
		}


		private void LoadLanguages()
		{
			if (langBox.Items.Count > 0)
			{
				return;
			}

			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var regex = new Regex(@"[a-z]{2}\-[A-Z]{2}");

			var files = Directory.GetDirectories(path)
				.Select(f => Path.GetFileName(f))
				.Where(f => regex.IsMatch(f));

			var languages = new List<CultureInfo>
			{
				new CultureInfo("en-US")
			};

			foreach (var file in files)
			{
				try
				{
					var info = CultureInfo.GetCultureInfo(file);

					// given a proper form zz-ZZ but an unknown culture, dotnet will succeed
					// but will populate the language name as ZZZ
					if (info != null && info.ThreeLetterWindowsLanguageName != "ZZZ")
					{
						languages.Add(info);
					}
				}
				catch
				{
					// an exception is thrown if the culture name is a bad form
					Logger.Current.WriteLine($"{file} is an unrecognized culture directory");
				}
			}

			langBox.DisplayMember = "DisplayName";
			langBox.DataSource = languages.OrderBy(l => l.DisplayName).ToArray();
		}


		private void BrowseImageViewer(object sender, System.EventArgs e)
		{
			// both cmdBox and argsBox use this handler
			using var dialog = new OpenFileDialog();
			dialog.Filter = "All files (*.*)|*.*";
			dialog.CheckFileExists = true;
			dialog.Multiselect = false;
			dialog.Title = Resx.GeneralSheet_imageBrowser;
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


		private void ValidateImageViewer(object sender, EventArgs e)
		{
			var path = imageViewerBox.Text.Trim();
			if (!string.IsNullOrEmpty(path) && (path != "mspaint") && !File.Exists(path))
			{
				errorProvider1.SetError(imageViewerButton, Resx.GeneralSheet_pathNotFound);
			}
			else
			{
				errorProvider1.SetError(imageViewerButton, String.Empty);
			}
		}


		public override bool CollectSettings()
		{
			// set to true to recommend OneNote restart
			var updated = false;

			// general...

			var settings = provider.GetCollection(Name);

			if (settings.Add("enablers", enablersBox.Checked)) updated = true;
			if (settings.Add("checkUpdates", checkUpdatesBox.Checked)) updated = true;

			var lang = ((CultureInfo)(langBox.SelectedItem)).Name;
			if (settings.Add("language", lang)) updated = true;

			var viewer = imageViewerBox.Text.Trim();
			if (string.IsNullOrEmpty(viewer))
			{
				viewer = "mspaint";
			}
			if (settings.Add("imageViewer", viewer)) updated = true;

			if (updated)
			{
				provider.SetCollection(settings);
				AddIn.EnablersEnabled = enablersBox.Checked;
			}

			// remove old images/viewer entry
			settings = provider.GetCollection("images");
			if (settings != null)
			{
				provider.RemoveCollection("images");
			}

			return updated;
		}
	}
}
