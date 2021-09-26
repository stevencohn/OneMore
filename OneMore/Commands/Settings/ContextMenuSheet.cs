//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ContextMenuSheet : SheetBase
	{

		private class MenuItem
		{
			public string Key { get; set; }
			public string Text { get; set; }
			public MenuItem(string key, string text) { Key = key; Text = text; }
		}


		// keys should match existing Ribbon item IDs
		// and then add ContextMenuSheet_ID values to the Resources.resx
		public string[] keys = new string[]
		{
			"ribPasteRtfButton",			// Paste Rich Text
			"ribFootnoteButton",			// Add Footnote
			"ribCleanMenu",					// Clean Menu
			"ribBreakingButton",			// ... Change Sentence Spacing
			"ribRemoveAuthorsButton",		// ... Remove Author Information
			"ribRemoveCitationsButton",		// ... Remove Pasted Citations
			"ribRemoveEmptyButton",			// ... Remove Empty Paragraphs and Headings
			"ribRemoveSpacingButton",		// ... Remove Paragraph Spacing
			"ribTrimButton",				// ... Trim Whitespace
			"ribEditMenu",					// Edit Menu
			"ribColorizeMenu",				// ... Colorize
			"ribHighlightButton",			// ... Rotating Highlighter
			"ribNoSpellCheckButton",		// ... No Spell Check
			"ribSpellCheckButton",			// ... Spell Check
			"ribUppercaseButton",			// ... To UPPERCASE
			"ribLowercaseButton",			// ... To lowercase
			"ribTitlecaseButton",			// ... To Title Case
			"ribIncreaseFontSizeButton",	// ... Increase Text Size
			"ribDecreaseFontSizeButton",	// ... Decrease Text Size
			"ribJoinParagraphButton",		// ... Join Paragraph
			"ribCollapseContentButton",		// ... Collapse Outline
			"ribExpandContentButton",		// ... Expand Outline
			"ribSaveCollapsedButton",		// ... Save Collapsed Outline
			"ribRestoreCollapsedButton",	// ... Restore Collapsed Outline
			"ribWordCountButton",			// ... Word Count
			"ribReplaceButton",				// Search and Replace
			"ribSnippetsMenu",				// Snippets Menu
			"ribInsertSingleLineButton",	// ... Single Line
			"ribInsertDoubleLineButton",	// ... Double Line
			"ribTocButton",					// ... Table of Contents
			"ribCalendarButton",			// ... Calendar
			"ribInsertDateButton",			// ... Sortable Date
			"ribBoxButton",					// ... Block
			"ribCodeBlockButton",			// ... Code Block
			"ribInfoBlockButton",			// ... Info Block
			"ribWarnBlockButton",			// ... Warning Block
			"ribExpandButton",				// ... Expand
			"ribGrayStatusButton",			// ... Gray Status
			"ribRedStatusButton",			// ... Red Status
			"ribYellowStatusButton",		// ... Yellow Status
			"ribGreenStatusButton",			// ... Green Status
			"ribBlueStatusButton"			// ... Blue Status
		};


		public ContextMenuSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "ContextMenuSheet";
			Title = Resx.ContextMenuSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox"
				});
			}

			commandsBox.DisplayMember = "Text";

			commandsBox.Items.Clear();
			foreach (var key in keys)
			{
				var text = Resx.ResourceManager.GetString($"{Name}_{key}") ?? key;
				commandsBox.Items.Add(new MenuItem(key, text));
			}

			var settings = provider.GetCollection(Name);
			for (var i = 0; i < keys.Length; i++)
			{
				if (settings.Get<bool>(keys[i]))
				{
					commandsBox.SetItemChecked(i, true);
				}
			}
		}


		public override bool CollectSettings()
		{
			var updated = false;

			var settings = provider.GetCollection(Name);
			for (var i = 0; i < keys.Length; i++)
			{
				if (commandsBox.GetItemChecked(i))
				{
					if (settings.Add(keys[i], true))
					{
						updated = true;
					}
				}
				else
				{
					if (settings.Remove(keys[i]))
					{
						updated = true;
					}
				}
			}

			if (updated)
			{
				if (settings.Count > 0)
				{
					provider.SetCollection(settings);
				}
				else
				{
					provider.RemoveCollection(Name);
				}
			}

			return updated;
		}
	}
}
