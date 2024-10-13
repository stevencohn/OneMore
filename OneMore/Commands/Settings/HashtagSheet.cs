//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtagSheet : SheetBase
	{

		public HashtagSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(HashtagSheet);
			Title = Resx.word_Hashtags;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"intervalLabel",
					"minLabel=word_Minutes",
					"advancedGroup=phrase_AdvancedOptions",
					"styleLabel",
					"styleBox",
					"filterBox",
					"scheduleLink",
					"warningBox",
					"disabledBox"
				});
			}

			var settings = provider.GetCollection(Name);

			intervalBox.Value = settings.Get("interval", HashtagService.DefaultPollingInterval);
			disabledBox.Checked = settings.Get("disabled", false);

			var theme = new ThemeProvider().Theme;
			var styles = theme.GetStyles().Where(s => s.StyleType == StyleType.Character).ToList();
			if (styles.Any())
			{
				styleBox.Items.AddRange(styles.ToArray());
			}

			var styleIndex = settings.Get("styleIndex", 0);
			if (styleIndex < 3)
			{
				// None, Red FG, Yellow BG
				styleBox.SelectedIndex = styleIndex;
			}
			else
			{
				var styleName = settings.Get("styleName", string.Empty);
				var index = styles.FindIndex(s => s.Name == styleName);
				if (index >= 0)
				{
					// custom character styles
					styleBox.SelectedIndex = 3 + index;
				}
				else
				{
					// default to None
					styleBox.SelectedIndex = 0;
				}
			}

			filterBox.Checked = settings.Get<bool>("unfiltered");

			if (provider.GetCollection("GeneralSheet").Get("experimental", false))
			{
				delayBox.Value = settings.Get("delay", HashtagScanner.DefaultThrottle);
			}
			else
			{
				delayLabel.Visible = false;
				delayBox.Visible = false;
				msLabel.Visible = false;
			}
		}


		private async void ScheduleRebuild(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var cmd = new HashtagScanCommand();
			cmd.SetLogger(logger);
			cmd.SetOwner(this);
			await cmd.Execute();
		}


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			var updated = settings.Add("interval", (int)intervalBox.Value);

			updated = settings.Add("styleIndex", styleBox.SelectedIndex) || updated;
			updated = settings.Add("styleName", styleBox.Text) || updated;

			updated = filterBox.Checked
				? settings.Add("unfiltered", true) || updated
				: settings.Remove("unfiltered") || updated;

			updated = disabledBox.Checked
				? settings.Add("disabled", true) || updated
				: settings.Remove("disabled") || updated;

			updated = settings.Add("delay", (int)delayBox.Value) || updated;

			if (updated)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
