//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Helpers.Office;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ContextMenuSheet : SheetBase
	{

		private sealed class MenuItem
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
			"ribAddFootnoteButton",			// Add Footnote
			"ribCleanMenu",					// Clean Menu
			"ribBreakingButton",			// ... Change Sentence Spacing
			"ribRemoveAuthorsButton",		// ... Remove Author Information
			"ribRemoveCitationsButton",		// ... Remove Pasted Citations
			"ribRemoveEmptyButton",			// ... Remove Empty Paragraphs and Headings
			"ribRemoveSpacingButton",		// ... Remove Paragraph Spacing
			"ribRestoreAutosizeButton",		// ... Restore Auto-size Container Widths
			"ribTrimButton",				// ... Trim Whitespace
			"ribEditMenu",					// Edit Menu
			"ribColorizeMenu",				// ... Colorize
			"ribCopyAsMarkdownButton",		// ... Copy as Markdown
			"ribProofingMenu",				// ... Proofing Language
			"ribHighlightButton",			// ... Rotating Highlighter
			"ribDisableSpellCheckButton",	// ... Disable Spell Check
			"ribEnableSpellCheckButton",	// ... Enable Spell Check
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
			"ribCopyLinkToPageButton",		// Copy Linnk To Page
			"ribCopyLinkToParagraphButton",	// Copy Linnk To Paragraph
			"ribRemindersMenu",				// Reminders
			"ribRemindButton",				// ... Add or update reminder
			"ribCompleteReminderButton",	// ... Complete reminder
			"ribDeleteReminderButton",		// ... Delete reminder
			"ribSearchAndReplaceButton",	// Search and Replace
			"ribSnippetsMenu",				// Snippets Menu
			"ribInsertSingleLineButton",	// ... Single Line
			"ribInsertDoubleLineButton",	// ... Double Line
			"ribInsertTocButton",			// ... Table of Contents
			"ribInsertCalendarButton",		// ... Calendar
			"ribInsertDateButton",			// ... Sortable Date
			"ribInsertBoxButton",			// ... Block
			"ribInsertCodeBlockButton",		// ... Code Block
			"ribInsertInfoBlockButton",		// ... Info Block
			"ribInsertWarnBlockButton",		// ... Warning Block
			"ribInsertExpandButton",		// ... Expand/Collapse
			"ribInsertGrayStatusButton",	// ... Gray Status
			"ribInsertRedStatusButton",		// ... Red Status
			"ribInsertYellowStatusButton",	// ... Yellow Status
			"ribInsertGreenStatusButton",	// ... Green Status
			"ribInsertBlueStatusButton"		// ... Blue Status
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

			var langs = Office.GetEditingLanguages();
			var noproof = langs == null || langs.Length < 2;

			commandsBox.DisplayMember = "Text";

			commandsBox.Items.Clear();
			foreach (var key in keys)
			{
				if (key == "ribProofingMenu" && noproof)
				{
					continue;
				}

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
				// index might be greater than count-1 only if the settings collection contains
				// more items than now are defined in the commandsBox; this is unlikely but...?

				if (i < commandsBox.Items.Count)
				{
					if (commandsBox.GetItemChecked(i))
					{
						if (!settings.Contains(keys[i]) && settings.Add(keys[i], true))
						{
							updated = true;
						}
					}
					else
					{
						if (settings.Contains(keys[i]) && settings.Remove(keys[i]))
						{
							updated = true;
						}
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



		/// <summary>
		/// Temporary upgrade to rename resource IDs in the settings file
		/// Created in v5.1.0. To be removed a few versions after that.
		/// </summary>
		/// <param name="provider"></param>
		public static void UpgradESettings(SettingsProvider provider)
		{
			var exchange = new System.Collections.Generic.Dictionary<string, string>
			{
				{ "ribFootnoteButton", "ribAddFootnoteButton" },
				{ "ribNoSpellCheckButton", "ribDisableSpellCheckButton" },
				{ "ribSpellCheckButton", "ribEnableSpellCheckButton" },
				{ "ribReplaceButton", "ribSearchAndReplaceButton" },
				{ "ribTocButton", "ribInsertTocButton" },
				{ "ribCalendarButton", "ribInsertCalendarButton" },
				{ "ribBoxButton", "ribInsertBoxButton" },
				{ "ribCodeBlockButton", "ribInsertCodeBlockButton" },
				{ "ribInfoBlockButton", "ribInsertInfoBlockButton" },
				{ "ribExpandButton", "ribInsertExpandButton" },
				{ "ribGrayStatusButton", "ribInsertGrayStatusButton" },
				{ "ribRedStatusButton", "ribInsertRedStatusButton" },
				{ "ribYellowStatusButton", "ribInsertYellowStatusButton" },
				{ "ribGreenStatusButton", "ribInsertGreenStatusButton" },
				{ "ribBlueStatusButton", "ribInsertBlueStatusButton" }
			};

			var collection = provider.GetCollection("ContextMenuSheet");
			var updated = false;

			exchange.ForEach(item =>
			{
				if (collection.Contains(item.Key))
				{
					collection.Remove(item.Key);
					collection.Add(item.Value, true);
					updated = true;
				}
			});

			if (updated)
			{
				provider.SetCollection(collection);
				provider.Save();
			}
		}
	}
}
