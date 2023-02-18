//************************************************************************************************
// Copyright © 2021 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


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
					"advancedGroup",
					"verboseBox"
				});
			}

			var settings = provider.GetCollection(Name);

			enablersBox.Checked = settings.Get("enablers", true);
			checkUpdatesBox.Checked = settings.Get("checkUpdates", false);
			verboseBox.Checked = settings.Get("verbose", false);

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


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			var updated = enablersBox.Checked
				? settings.Add("enablers", true)
				: settings.Remove("enablers");

			// does not require a restart
			if (checkUpdatesBox.Checked)
				settings.Add("checkUpdates", true);
			else
				settings.Remove("checkUpdates");

			var lang = ((CultureInfo)(langBox.SelectedItem)).Name;
			updated = settings.Add("language", lang) || updated;

			updated = verboseBox.Checked
				? settings.Add("verbose", true) || updated
				: settings.Remove("verbose") || updated;

			// deprecated
			updated = settings.Remove("imageViewer") || updated;

			if (updated)
			{
				provider.SetCollection(settings);
				AddIn.EnablersEnabled = enablersBox.Checked;
			}

			return updated;
		}
	}
}
