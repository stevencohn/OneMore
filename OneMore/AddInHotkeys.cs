//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;

	public partial class AddIn
	{
		private void RegisterHotkeys()
		{
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;
			logger.WriteLine($"defining hotkeys for input locale {locale}");

			var s = new SettingsProvider()
				.GetCollection("KeyboardSheet")?.Get<XElement>("commands");

			HotkeyManager.Initialize();

			RegisterHotKey(s, nameof(AddFootnoteCmd), async () => await AddFootnoteCmd(null));
			RegisterHotKey(s, nameof(AddFormulaCmd), async () => await AddFormulaCmd(null));
			RegisterHotKey(s, nameof(ApplyStyle1Cmd), async () => await ApplyStyle1Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle2Cmd), async () => await ApplyStyle2Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle3Cmd), async () => await ApplyStyle3Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle4Cmd), async () => await ApplyStyle4Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle5Cmd), async () => await ApplyStyle5Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle6Cmd), async () => await ApplyStyle6Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle7Cmd), async () => await ApplyStyle7Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle8Cmd), async () => await ApplyStyle8Cmd(null));
			RegisterHotKey(s, nameof(ApplyStyle9Cmd), async () => await ApplyStyle9Cmd(null));
			RegisterHotKey(s, nameof(CalendarCmd), async () => await CalendarCmd(null));
			RegisterHotKey(s, nameof(ClearLogCmd), async () => await ClearLogCmd(null));
			RegisterHotKey(s, nameof(CopyLinkToPageCmd), async () => await CopyLinkToPageCmd(null));
			RegisterHotKey(s, nameof(CopyLinkToParagraphCmd), async () => await CopyLinkToParagraphCmd(null));
			RegisterHotKey(s, nameof(DecreaseFontSizeCmd), async () => await DecreaseFontSizeCmd(null));
			RegisterHotKey(s, nameof(DiagnosticsCmd), async () => await DiagnosticsCmd(null));
			RegisterHotKey(s, nameof(FillDownCmd), async () => await FillDownCmd(null));
			RegisterHotKey(s, nameof(HighlightCmd), async () => await HighlightCmd(null));
			RegisterHotKey(s, nameof(InsertBoxCmd), async () => await InsertBoxCmd(null));
			RegisterHotKey(s, nameof(InsertCodeBlockCmd), async () => await InsertCodeBlockCmd(null));
			RegisterHotKey(s, nameof(InsertDateCmd), async () => await InsertDateCmd(null));
			RegisterHotKey(s, nameof(InsertDateTimeCmd), async () => await InsertDateTimeCmd(null));
			RegisterHotKey(s, nameof(InsertDoubleHorizontalLineCmd), async () => await InsertDoubleHorizontalLineCmd(null));
			RegisterHotKey(s, nameof(InsertHorizontalLineCmd), async () => await InsertHorizontalLineCmd(null));
			RegisterHotKey(s, nameof(InsertTimerCmd), async () => await InsertTimerCmd(null));
			RegisterHotKey(s, nameof(DisableSpellCheckCmd), async () => await DisableSpellCheckCmd(null));
			RegisterHotKey(s, nameof(PasteRtfCmd), async () => await PasteRtfCmd(null));
			RegisterHotKey(s, nameof(PasteTextCmd), async () => await PasteTextCmd(null));
			RegisterHotKey(s, nameof(RecalculateFormulaCmd), async () => await RecalculateFormulaCmd(null));
			RegisterHotKey(s, nameof(RemoveFootnoteCmd), async () => await RemoveFootnoteCmd(null));
			RegisterHotKey(s, nameof(ReplayCmd), async () => await ReplayCmd(null));
			RegisterHotKey(s, nameof(RemindCmd), async () => await RemindCmd(null));
			RegisterHotKey(s, nameof(SearchCmd), async () => await SearchCmd(null));
			RegisterHotKey(s, nameof(SearchAndReplaceCmd), async () => await SearchAndReplaceCmd(null));
			RegisterHotKey(s, nameof(ShowXmlCmd), async () => await ShowXmlCmd(null));
			RegisterHotKey(s, nameof(StartTimerCmd), async () => await StartTimerCmd(null));
			RegisterHotKey(s, nameof(TaggedCmd), async () => await TaggedCmd(null));
			RegisterHotKey(s, nameof(TaggingCmd), async () => await TaggingCmd(null));
			RegisterHotKey(s, nameof(ToLowercaseCmd), async () => await ToLowercaseCmd(null));
			RegisterHotKey(s, nameof(ToUppercaseCmd), async () => await ToUppercaseCmd(null));

			// an awful hack to avoid a conflict with Italian keyboard (FIGS and likely UK) that
			// use AltGr as Ctrl+Alt. This means users pressing AltGr+OemPlus to get a square
			// bracket would instead end up increasing the font size of the page when they didn't
			// mean to! So here we only register these hot keys for the US keyboard input layout.
			if (locale == 1033)
			{
				RegisterHotKey(s, nameof(IncreaseFontSizeCmd), async () => await IncreaseFontSizeCmd(null));
			}
		}


		private void RegisterHotKey(XElement settings, string methodName, Action action)
		{			
			if (typeof(AddIn).GetMethod(methodName)
				.GetCustomAttributes(typeof(CommandAttribute), false)
				.FirstOrDefault() is CommandAttribute cmd)
			{
				var hotkey = new Hotkey(cmd.DefaultKeys);

				var setting = settings?.Elements("command")
					.FirstOrDefault(e => e.Attribute("command")?.Value == methodName);

				if (setting != null)
				{
					if (Enum.TryParse<Keys>(setting.Attribute("keys")?.Value, true, out var keys))
					{
						// has user deliberately cleared command binding (?HasFlag doesn't work?)
						if ((keys & Keys.KeyCode) == Keys.Back)
						{
							hotkey = null;
						}
						// has user overridden default binding
						else if ((hotkey.Keys & hotkey.Modifiers) != keys)
						{
							hotkey = new Hotkey(keys);
						}
					}
				}

				if (hotkey != null)
				{
					HotkeyManager.RegisterHotKey(action, hotkey);
				}
			}
		}
	}
}
