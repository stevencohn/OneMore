//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using Resx = Properties.Resources;


	internal partial class PageSheet : SheetBase
	{
		/// <summary>
		/// Default minimum page size, in KB, for the TOC up-to-date warning
		/// </summary>
		public const int DefaultTocWarnSize = 250;


		public PageSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(PageSheet);
			Title = Resx.PageSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"duplicateGroup",
					"insertNoteBox",
					"insertBacklinksBox",
					"refreshTocBox",
					"tocGroup",
					"tocWarnBox",
					"tocSizeLabel"
				});
			}

			var settings = provider.GetCollection(Name);
			insertNoteBox.Checked = settings.Get<bool>("insertNote");
			insertBacklinksBox.Checked = settings.Get<bool>("insertBacklinks");
			refreshTocBox.Checked = settings.Get<bool>("refreshToc");

			tocWarnBox.Checked = settings.Get<bool>("tocWarnUpToDate");
			tocSizeBox.Value = Math.Max(tocSizeBox.Minimum, Math.Min(tocSizeBox.Maximum,
				settings.Get("tocWarnSize", DefaultTocWarnSize)));

			tocSizeLabel.Enabled = tocSizeBox.Enabled = tocWarnBox.Checked;
			tocWarnBox.CheckedChanged += (s, e) =>
				tocSizeLabel.Enabled = tocSizeBox.Enabled = tocWarnBox.Checked;
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			var updated = false;

			updated = insertNoteBox.Checked
				? settings.Add("insertNote", true) || updated
				: settings.Remove("insertNote") || updated;

			updated = insertBacklinksBox.Checked
				? settings.Add("insertBacklinks", true) || updated
				: settings.Remove("insertBacklinks") || updated;

			updated = refreshTocBox.Checked
				? settings.Add("refreshToc", true) || updated
				: settings.Remove("refreshToc") || updated;

			updated = tocWarnBox.Checked
				? settings.Add("tocWarnUpToDate", true) || updated
				: settings.Remove("tocWarnUpToDate") || updated;

			var size = (int)tocSizeBox.Value;
			updated = size != DefaultTocWarnSize
				? settings.Add("tocWarnSize", size) || updated
				: settings.Remove("tocWarnSize") || updated;

			if (updated)
			{
				provider.SetCollection(settings);
			}

			// none of these settings require restarting the add-in to take effect
			return false;
		}
	}
}
