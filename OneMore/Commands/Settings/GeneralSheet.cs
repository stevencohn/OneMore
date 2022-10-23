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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class GeneralSheet : SheetBase
	{
		public GeneralSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			LoadLanguages();

			Name = "GeneralSheet";
			Title = Resx.GeneralSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"enablersBox",
					"checkUpdatesBox",
					"langLabel"
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
			dialog.Title = "Image Viewer";
			dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
			dialog.InitialDirectory = GetValidPath(imageViewerBox.Text);

			var result = dialog.ShowDialog();
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
			if (!string.IsNullOrEmpty(path) && !File.Exists(path))
			{
				errorProvider1.SetError(imageViewerButton, "Path not found");
			}
			else
			{
				errorProvider1.SetError(imageViewerButton, String.Empty);
			}
		}


		public override bool CollectSettings()
		{

			// general...

			var genup = false;
			var settings = provider.GetCollection(Name);

			if (settings.Add("enablers", enablersBox.Checked)) genup = true;
			if (settings.Add("checkUpdates", checkUpdatesBox.Checked)) genup = true;

			var lang = ((CultureInfo)(langBox.SelectedItem)).Name;
			if (settings.Add("language", lang)) genup = true;

			if (genup)
			{
				provider.SetCollection(settings);
				AddIn.EnablersEnabled = enablersBox.Checked;
			}

			// image viewer...

			var imgup = false;
			settings = provider.GetCollection("images");
			var viewer = imageViewerBox.Text.Trim();
			if (string.IsNullOrEmpty(viewer))
			{
				viewer = "mspaint";
			}
			if (settings.Add("viewer", viewer)) imgup = true;
			if (imgup)
			{
				provider.SetCollection(settings);
			}

			return genup || imgup;
		}
	}
}
