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
					"blankBeforeHeadingsBox",
					"convertOnEnterBox"
				});
			}

			var settings = provider.GetCollection(Name);
			gfmLineBreaksBox.Checked = settings.Get("gfmLineBreaks", false);
			singleSpacingBox.Checked = settings.Get("singleSpacing", false);
			blankBeforeHeadingsBox.Checked = settings.Get("blankBeforeHeadings", false);

			// blank-before-headings only makes sense when single spacing is on
			singleSpacingBox.CheckedChanged += (s, e) => UpdateBlankBeforeHeadingsState();
			UpdateBlankBeforeHeadingsState();

			// experimental: live markdown conversion on Enter. Hidden entirely unless
			// the General sheet's experimental features flag is on, and off by default
			if (provider.GetCollection(nameof(GeneralSheet)).Get("experimental", false))
			{
				convertOnEnterBox.Checked = settings.Get("convertOnEnter", false);
			}
			else
			{
				convertOnEnterBox.Visible = false;
			}
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

			// requires a restart: the convert-on-Enter hotkey is only (un)registered at
			// startup. Only touched while the box is visible (experimental mode is on),
			// so toggling experimental mode off and back on doesn't clobber the setting
			var restart = false;
			if (convertOnEnterBox.Visible)
			{
				restart = convertOnEnterBox.Checked
					? settings.Add("convertOnEnter", true) || restart
					: settings.Remove("convertOnEnter") || restart;
			}

			if (save || restart)
			{
				provider.SetCollection(settings);
			}

			return restart;
		}
	}
}
