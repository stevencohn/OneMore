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
					"doubledBox",
					"notifyBox",
					"selectLink",
					"scheduleLink",
					"warningLabel",
					"disabledBox"
				});
			}

			var settings = provider.GetCollection(Name);

			intervalBox.Value = settings.Get("interval", HashtagStage.DefaultPollingInterval);

			var disabled = settings.Get("disabled", false);
			disabledBox.Checked = disabled;
			scheduleLink.Enabled = !disabled;

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
			doubledBox.Checked = settings.Get<bool>("doubled");
			notifyBox.Checked = settings.Get<bool>("notify");

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


		private void SelectNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using var dialog = new NotebooksDialog();
			dialog.ShowDialog(this);
		}


		private async void ScheduleRebuild(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var cmd = new HashtagScanCommand();
			cmd.SetLogger(logger);
			cmd.SetOwner(this);
			await cmd.Execute();
		}


		private void ToggleDIsabled(object sender, EventArgs e)
		{
			scheduleLink.Enabled = !disabledBox.Checked;
		}


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			// requires a restart; the interval is read once when the hashtag stage is created
			var updated = settings.Add("interval", (int)intervalBox.Value);

			// the rest do not require a restart; the scanner reads them again for each scan
			var save = settings.Add("styleIndex", styleBox.SelectedIndex);
			save = settings.Add("styleName", styleBox.Text) || save;

			save = filterBox.Checked
				? settings.Add("unfiltered", true) || save
				: settings.Remove("unfiltered") || save;

			save = doubledBox.Checked
				? settings.Add("doubled", true) || save
				: settings.Remove("doubled") || save;

			save = notifyBox.Checked
				? settings.Add("notify", true) || save
				: settings.Remove("notify") || save;

			// does not require a restart; the pipeline reads this every cycle
			save = disabledBox.Checked
				? settings.Add("disabled", true) || save
				: settings.Remove("disabled") || save;

			save = settings.Add("delay", (int)delayBox.Value) || save;

			if (updated || save)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
