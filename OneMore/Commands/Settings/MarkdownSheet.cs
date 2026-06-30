//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


	internal partial class MarkdownSheet : SheetBase
	{
		public MarkdownSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(MarkdownSheet);
			Title = Resx.MarkdownSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"gfmLineBreaksBox",
					"singleSpacingBox",
					"blankBeforeHeadingsBox"
				});
			}

			var settings = provider.GetCollection(Name);
			gfmLineBreaksBox.Checked = settings.Get("gfmLineBreaks", false);
			singleSpacingBox.Checked = settings.Get("singleSpacing", false);
			blankBeforeHeadingsBox.Checked = settings.Get("blankBeforeHeadings", false);

			// blank-before-headings only makes sense when single spacing is on
			singleSpacingBox.CheckedChanged += (s, e) => UpdateBlankBeforeHeadingsState();
			UpdateBlankBeforeHeadingsState();
		}


		private void UpdateBlankBeforeHeadingsState()
		{
			blankBeforeHeadingsBox.Enabled = singleSpacingBox.Checked;
			if (!singleSpacingBox.Checked)
			{
				blankBeforeHeadingsBox.Checked = false;
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			var save = false;

			// does not require a restart
			save = gfmLineBreaksBox.Checked
				? settings.Add("gfmLineBreaks", true) || save
				: settings.Remove("gfmLineBreaks") || save;

			// does not require a restart
			save = singleSpacingBox.Checked
				? settings.Add("singleSpacing", true) || save
				: settings.Remove("singleSpacing") || save;

			// does not require a restart
			save = blankBeforeHeadingsBox.Checked
				? settings.Add("blankBeforeHeadings", true) || save
				: settings.Remove("blankBeforeHeadings") || save;

			if (save)
			{
				provider.SetCollection(settings);
			}

			// restart not required
			return false;
		}
	}
}
