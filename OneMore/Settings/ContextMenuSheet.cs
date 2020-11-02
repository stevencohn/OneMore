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
		public string[] keys = new string[]
		{
			"ribNoSpellCheckButton",		// No Spell Check
			"ribPasteRtfButton",			// Paste Rich Text
			"ribFootnoteButton",			// Add Footnote
			"ribEditMenu",					// Edit Menu
			"ribHighlightButton",			// ... Rotating Highlighter
			"ribUppercaseButton",			// ... To UPPERCASE
			"ribLowercaseButton",			// ... To lowercase
			"ribIncreaseFontSizeButton",	// ... Increase Text Size
			"ribDecreaseFontSizeButton",	// ... Decrease Text Size
			"ribCleanMenu",					// Clean Menu
			"ribRemoveAuthorsButton",		// ... Remove Author Information
			"ribRemoveCitationsButton",		// ... Remove Pasted Citations
			"ribRemoveEmptyButton",			// ... Remove Empty Paragraphs and Headings
			"ribRemoveSpacingButton",		// ... Remove Paragraph Spacing
			"ribTrimButton",				// ... Trim Whitespace
			"ribSnippetsMenu",				// Snippets Menu
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


		public override void CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			for (var i = 0; i < keys.Length; i++)
			{
				if (commandsBox.GetItemChecked(i))
				{
					settings.Add(keys[i], true);
				}
				else
				{
					settings.Remove(keys[i]);
				}
			}

			provider.SetCollection(settings);
		}
	}
}
