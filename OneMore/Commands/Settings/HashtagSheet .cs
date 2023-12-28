//************************************************************************************************
// Copyright © 2023 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtagSheet : SheetBase
	{

		public HashtagSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(HashtagSheet);
			Title = Resx.HashtagSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"intervalLabel",
					"minLabel=word_Minutes",
					"advancedGroup=phrase_AdvancedOptions",
					"styleLabel",
					"rebuildBox",
					"disabledBox"
				});
			}

			var settings = provider.GetCollection(Name);

			intervalBox.Value = settings.Get("interval", HashtagService.DefaultPollingInterval);
			disabledBox.Checked = settings.Get("disabled", false);

			var theme = new ThemeProvider().Theme;
			var styles = theme.GetStyles().Where(s => s.StyleType == StyleType.Character);
			if (styles.Any())
			{
				styleBox.Items.AddRange(styles.ToArray());
			}

			var styleIndex = settings.Get("styleIndex", 0);
			if (styleIndex < 3)
			{
				styleBox.SelectedIndex = styleIndex;
			}
			else if (styleIndex < styleBox.Items.Count - 1)
			{
				styleBox.SelectedIndex = styleIndex;
			}
			else
			{
				styleBox.SelectedIndex = 0;
			}
		}


		private void ConfirmRebuild(object sender, System.EventArgs e)
		{
			if (rebuildBox.Checked)
			{
				var result = UIHelper.ShowQuestion(
					"This will delete your hashtag database and create a new one.\n" +
					"That requires OneNote to restart and then can take quite some time.\n\n" +
					"Are you sure you want to rebuild the database?");

				if (result != DialogResult.Yes)
				{
					rebuildBox.Checked = false;
				}
			}
		}


		public override bool CollectSettings()
		{
			// general...

			var settings = provider.GetCollection(Name);

			var updated = settings.Add("interval", (int)intervalBox.Value);

			updated = rebuildBox.Checked
				? settings.Add("rebuild", true) || updated
				: settings.Remove("rebuild") || updated;

			updated = disabledBox.Checked
				? settings.Add("disabled", true) || updated
				: settings.Remove("disabled") || updated;

			if (updated)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
