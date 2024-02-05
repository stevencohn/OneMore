//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.UI;
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
					"themeLabel",
					"themeBox",
					"langLabel",
					"checkUpdatesBox",
					"advancedGroup=phrase_AdvancedOptions",
					"verboseBox",
					"experimentalBox"
				});
			}

			var settings = provider.GetCollection(Name);

			themeBox.SelectedIndex = settings.Get("theme", 0);

			var lang = settings.Get("language", "en-US");
			foreach (CultureInfo info in langBox.Items)
			{
				if (info.Name == lang)
				{
					langBox.SelectedItem = info;
					break;
				}
			}

			checkUpdatesBox.Checked = settings.Get("checkUpdates", false);
			verboseBox.Checked = settings.Get("verbose", false);
			experimentalBox.Checked = settings.Get("experimental", false);
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
				new("en-US")
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
			var save = false;

			if (settings.Add("theme", themeBox.SelectedIndex))
			{
				ThemeManager.Instance.LoadColors(themeBox.SelectedIndex);
				save = true;
			}

			var lang = ((CultureInfo)(langBox.SelectedItem)).Name;
			var updated = settings.Add("language", lang);

			// does not require a restart
			if (checkUpdatesBox.Checked)
				settings.Add("checkUpdates", true);
			else
				settings.Remove("checkUpdates");

			// requires a restart
			updated = verboseBox.Checked
				? settings.Add("verbose", true) || updated
				: settings.Remove("verbose") || updated;

			updated = experimentalBox.Checked
				? settings.Add("experimental", true) || updated
				: settings.Remove("experimental") || updated;

			if (updated || save)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
