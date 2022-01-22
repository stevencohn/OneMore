//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;


	public partial class AddIn
	{
		private void RegisterHotkeys()
		{
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;
			logger.WriteLine($"defining hotkeys for input locale {locale}");

			HotkeyManager.Initialize();

			RegisterHotKey(nameof(AddFootnoteCmd), async () => await AddFootnoteCmd(null));
			RegisterHotKey(nameof(AddFormulaCmd), async () => await AddFormulaCmd(null));
			RegisterHotKey(nameof(ApplyStyle1Cmd), async () => await ApplyStyle1Cmd(null));
			RegisterHotKey(nameof(ApplyStyle2Cmd), async () => await ApplyStyle2Cmd(null));
			RegisterHotKey(nameof(ApplyStyle3Cmd), async () => await ApplyStyle3Cmd(null));
			RegisterHotKey(nameof(ApplyStyle4Cmd), async () => await ApplyStyle4Cmd(null));
			RegisterHotKey(nameof(ApplyStyle5Cmd), async () => await ApplyStyle5Cmd(null));
			RegisterHotKey(nameof(ApplyStyle6Cmd), async () => await ApplyStyle6Cmd(null));
			RegisterHotKey(nameof(ApplyStyle7Cmd), async () => await ApplyStyle7Cmd(null));
			RegisterHotKey(nameof(ApplyStyle8Cmd), async () => await ApplyStyle8Cmd(null));
			RegisterHotKey(nameof(ApplyStyle9Cmd), async () => await ApplyStyle9Cmd(null));
			RegisterHotKey(nameof(ClearLogCmd), async () => await ClearLogCmd(null));
			RegisterHotKey(nameof(DecreaseFontSizeCmd), async () => await DecreaseFontSizeCmd(null));
			RegisterHotKey(nameof(DiagnosticsCmd), async () => await DiagnosticsCmd(null));
			RegisterHotKey(nameof(FillDownCmd), async () => await FillDownCmd(null));
			RegisterHotKey(nameof(HighlightCmd), async () => await HighlightCmd(null));
			RegisterHotKey(nameof(InsertCodeBlockCmd), async () => await InsertCodeBlockCmd(null));
			RegisterHotKey(nameof(InsertDateCmd), async () => await InsertDateCmd(null));
			RegisterHotKey(nameof(InsertDoubleHorizontalLineCmd), async () => await InsertDoubleHorizontalLineCmd(null));
			RegisterHotKey(nameof(InsertHorizontalLineCmd), async () => await InsertHorizontalLineCmd(null));
			RegisterHotKey(nameof(InsertTimerCmd), async () => await InsertTimerCmd(null));
			RegisterHotKey(nameof(DisableSpellCheckCmd), async () => await DisableSpellCheckCmd(null));
			RegisterHotKey(nameof(PasteRtfCmd), async () => await PasteRtfCmd(null));
			RegisterHotKey(nameof(RecalculateFormulaCmd), async () => await RecalculateFormulaCmd(null));
			RegisterHotKey(nameof(RemoveFootnoteCmd), async () => await RemoveFootnoteCmd(null));
			RegisterHotKey(nameof(ReplayCmd), async () => await ReplayCmd(null));
			RegisterHotKey(nameof(RemindCmd), async () => await RemindCmd(null));
			RegisterHotKey(nameof(SearchCmd), async () => await SearchCmd(null));
			RegisterHotKey(nameof(SearchAndReplaceCmd), async () => await SearchAndReplaceCmd(null));
			RegisterHotKey(nameof(ShowXmlCmd), async () => await ShowXmlCmd(null));
			RegisterHotKey(nameof(StartTimerCmd), async () => await StartTimerCmd(null));
			RegisterHotKey(nameof(TaggedCmd), async () => await TaggedCmd(null));
			RegisterHotKey(nameof(TaggingCmd), async () => await TaggingCmd(null));
			RegisterHotKey(nameof(ToLowercaseCmd), async () => await ToLowercaseCmd(null));
			RegisterHotKey(nameof(ToUppercaseCmd), async () => await ToUppercaseCmd(null));

			// an awful hack to avoid a conflict with Italian keyboard (FIGS and likely UK) that
			// use AltGr as Ctrl+Alt. This means users pressing AltGr+OemPlus to get a square
			// bracket would instead end up increasing the font size of the page when they didn't
			// mean to! So here we only register these hot keys for the US keyboard input layout.
			if (locale == 1033)
			{
				RegisterHotKey(nameof(IncreaseFontSizeCmd), async () => await IncreaseFontSizeCmd(null));
			}
		}


		private void RegisterHotKey(string methodName, Action action)
		{			
			if (typeof(AddIn).GetMethod(methodName)
				.GetCustomAttributes(typeof(CommandAttribute), false)
				.FirstOrDefault() is CommandAttribute cmd)
			{
				var hotkey = new Hotkey(cmd.DefaultKeys);
				HotkeyManager.RegisterHotKey(action, hotkey);
			}
		}
	}
}
