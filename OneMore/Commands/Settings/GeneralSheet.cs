//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


	internal partial class GeneralSheet : SheetBase
	{
		// theme ids in the same order as the items in themeBox
		private readonly List<string> themeIds;


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
					"langLabel=word_Language",
					"keepLocaleBox",
					"opacityLabel",
					"sequentialBox",
					"checkUpdatesBox",
					"advancedGroup=phrase_AdvancedOptions",
					"telemetryBox",
					"verboseBox",
					"experimentalBox"
				});
			}

			var settings = provider.GetCollection(Name);

			// the designer provides the localized System, Light, and Dark items; append the
			// other built-in themes and any user themes found in %APPDATA%\OneMore\Themes
			themeIds = new List<string>();
			foreach (var info in ThemeCatalog.GetThemes())
			{
				themeIds.Add(info.Id);
				if (themeIds.Count > ThemeCatalog.LocalizedCount)
				{
					themeBox.Items.Add(info.Name);
				}
			}

			var themeIndex = themeIds.FindIndex(
				id => string.Equals(id, provider.Theme, StringComparison.OrdinalIgnoreCase));

			themeBox.SelectedIndex = themeIndex >= 0 ? themeIndex : 0;

			var lang = settings.Get("language", "en-US");
			foreach (CultureInfo info in langBox.Items)
			{
				if (info.Name == lang)
				{
					langBox.SelectedItem = info;
					break;
				}
			}

			keepLocaleBox.Checked = settings.Get("keepWorkstationLocale", false);

			// shared with MoreForm so the slider and the dialogs agree on the valid range
			opacityBar.Minimum = MoreForm.MinOpacityPercent;
			opacityBar.Value = MoreForm.GetDialogOpacityPercent();
			opacityValueLabel.Text = $"{opacityBar.Value}%";

			sequentialBox.Checked = settings.Get("nonseqMatching", false);
			checkUpdatesBox.Checked = settings.Get("checkUpdates", false);
			experimentalBox.Checked = settings.Get("experimental", false);
			telemetryBox.Checked = settings.Get("telemetry", false);

			// <logging>verbose|debug</logging> is the new way
			// <verbose>true</verbose> was the old way
			var loglevel = settings.Get("logging", string.Empty).ToLower();
			verboseBox.Checked = loglevel.Length == 0
				? settings.Get("verbose", false)
				: loglevel.In("verbose", "debug");

			if (loglevel == "debug")
			{
				verboseBox.Enabled = false;
			}
		}


		private void ChangeOpacity(object sender, EventArgs e)
		{
			opacityValueLabel.Text = $"{opacityBar.Value}%";

			// preview immediately on the Settings dialog; not yet parented while loading.
			// Cancelling needs no revert because the dialog closes and the next one reads
			// the saved value
			var form = FindForm();
			if (form is not null)
			{
				form.Opacity = opacityBar.Value / 100.0;
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
					logger.WriteLine($"{file} is an unrecognized culture directory");
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

			// "themeId" replaces the legacy integer "theme" setting
			var themeId = themeIds[themeBox.SelectedIndex];
			if (settings.Add("themeId", themeId) | settings.Remove("theme"))
			{
				ThemeManager.Instance.LoadColors(themeId);
				save = true;
			}

			var lang = ((CultureInfo)(langBox.SelectedItem)).Name;
			var updated = settings.Add("language", lang);

			// requires a restart
			updated = keepLocaleBox.Checked
				? settings.Add("keepWorkstationLocale", true) || updated
				: settings.Remove("keepWorkstationLocale") || updated;

			// does not require a restart; applies to dialogs opened hereafter
			if (opacityBar.Value >= 100)
			{
				save = settings.Remove("dialogOpacity") || save;
			}
			else
			{
				save = settings.Add("dialogOpacity", opacityBar.Value) || save;
			}

			// does not require a restart
			save = sequentialBox.Checked
				? settings.Add("nonseqMatching", "true") || save
				: settings.Remove("nonseqMatching") || save;

			// does not require a restart
			save = checkUpdatesBox.Checked
				? settings.Add("checkUpdates", true) || save
				: settings.Remove("checkUpdates") || save;

			// does not require a restart
			save = telemetryBox.Checked
				? settings.Add("telemetry", true) || save
				: settings.Add("telemetry", false) || save;

			AddIn.Telemetry = telemetryBox.Checked;

			// does not require a restart; only Enabled if !debug
			if (verboseBox.Enabled)
			{
				save = verboseBox.Checked
					? settings.Add("logging", "verbose") || save
					: settings.Remove("logging") || save;

				((Logger)logger).SetLoggingLevel(verboseBox.Checked, false);
				save = settings.Remove("verbose") || save;
			}

			// requires a restart
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
