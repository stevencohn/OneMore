//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


	internal partial class PageSheet : SheetBase
	{

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
					"refreshTocBox"
				});
			}

			var settings = provider.GetCollection(Name);
			insertNoteBox.Checked = settings.Get<bool>("insertNote");
			insertBacklinksBox.Checked = settings.Get<bool>("insertBacklinks");
			refreshTocBox.Checked = settings.Get<bool>("refreshToc");
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

			if (updated)
			{
				provider.SetCollection(settings);
			}

			// none of these settings require restarting the add-in to take effect
			return false;
		}
	}
}
