//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Windows.Forms;


	public partial class AddIn
	{
		private void RegisterHotkeys()
		{
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;
			logger.WriteLine($"defining hotkeys for input locale {locale}");

			var map = GetKeyboardDefaults();

			HotkeyManager.Initialize();

			HotkeyManager.RegisterHotKey(async () => await AddFootnoteCmd(null),
				map[nameof(AddFootnoteCmd)]);

			HotkeyManager.RegisterHotKey(async () => await AddFormulaCmd(null),
				map[nameof(AddFormulaCmd)]);

			HotkeyManager.RegisterHotKey(async () => await DecreaseFontSizeCmd(null),
				map[nameof(DecreaseFontSizeCmd)]);

			HotkeyManager.RegisterHotKey(async () => await FillDownCmd(null),
				map[nameof(FillDownCmd)]);

			HotkeyManager.RegisterHotKey(async () => await HighlightCmd(null),
				map[nameof(HighlightCmd)]);

			// this is an awful hack to avoid a conflict with Italian keyboards that use AltGr
			// as Ctrl+Alt. This means users pressing AltGr+OemPlus to get a square bracket would
			// instead end up increasing the font size of the page when they didn't mean to!
			// So here we only register these hot keys for the US keyboard input layout.
			// This seem to apply to FIGS languages and probably UK.
			if (locale == 1033)
			{
				HotkeyManager.RegisterHotKey(async () => await IncreaseFontSizeCmd(null),
					map[nameof(IncreaseFontSizeCmd)]);
			}

			HotkeyManager.RegisterHotKey(async () => await InsertCodeBlockCmd(null),
				map[nameof(InsertCodeBlockCmd)]);

			HotkeyManager.RegisterHotKey(async () => await InsertDateCmd(null),
				map[nameof(InsertDateCmd)]);

			HotkeyManager.RegisterHotKey(async () => await InsertDoubleHorizontalLineCmd(null),
				map[nameof(InsertDoubleHorizontalLineCmd)]);

			HotkeyManager.RegisterHotKey(async () => await InsertHorizontalLineCmd(null),
				map[nameof(InsertHorizontalLineCmd)]);

			HotkeyManager.RegisterHotKey(async () => await InsertTimerCmd(null),
				map[nameof(InsertTimerCmd)]);

			HotkeyManager.RegisterHotKey(async () => await DisableSpellCheckCmd(null),
				map[nameof(DisableSpellCheckCmd)]);

			HotkeyManager.RegisterHotKey(async () => await PasteRtfCmd(null),
				map[nameof(PasteRtfCmd)]);

			HotkeyManager.RegisterHotKey(async () => await RecalculateFormulaCmd(null),
				map[nameof(RecalculateFormulaCmd)]);

			HotkeyManager.RegisterHotKey(async () => await RemoveFootnoteCmd(null),
				map[nameof(RemoveFootnoteCmd)]);

			HotkeyManager.RegisterHotKey(async () => await ReplayCmd(null),
				map[nameof(ReplayCmd)]);

			HotkeyManager.RegisterHotKey(async () => await RemindCmd(null),
				map[nameof(RemindCmd)]);

			HotkeyManager.RegisterHotKey(async () => await SearchCmd(null),
				map[nameof(SearchCmd)]);

			HotkeyManager.RegisterHotKey(async () => await SearchAndReplaceCmd(null),
				map[nameof(SearchAndReplaceCmd)]);

			HotkeyManager.RegisterHotKey(async () => await StartTimerCmd(null),
				map[nameof(StartTimerCmd)]);

			HotkeyManager.RegisterHotKey(async () => await TaggedCmd(null),
				map[nameof(TaggedCmd)]);

			HotkeyManager.RegisterHotKey(async () => await TaggingCmd(null),
				map[nameof(TaggingCmd)]);

			HotkeyManager.RegisterHotKey(async () => await ToLowercaseCmd(null),
				map[nameof(ToLowercaseCmd)]);

			HotkeyManager.RegisterHotKey(async () => await ToUppercaseCmd(null),
				map[nameof(ToUppercaseCmd)]);

			// tools

			HotkeyManager.RegisterHotKey(async () => await ShowXmlCmd(null),
				map[nameof(ShowXmlCmd)]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<DiagnosticsCommand>(),
				map[nameof(DiagnosticsCommand)]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ClearLogCommand>(),
				map[nameof(ClearLogCommand)]);

			// custom styles, CtrlAltShift+1..9

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(0),
				map["ApplyStyle1"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(1),
				map["ApplyStyle2"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(2),
				map["ApplyStyle3"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(3),
				map["ApplyStyle4"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(4),
				map["ApplyStyle5"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(5),
				map["ApplyStyle6"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(6),
				map["ApplyStyle7"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(7),
				map["ApplyStyle8"]);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(8),
				map["ApplyStyle9"]);
		}


		private Dictionary<string, Hotkey> GetKeyboardDefaults()
		{
			var map = new Dictionary<string, Hotkey>
			{
				{ nameof(AddFootnoteCmd), new Hotkey(Keys.F, Hotmods.ControlAlt) },
				{ nameof(AddFormulaCmd), new Hotkey(Keys.F5) },
				{ nameof(DecreaseFontSizeCmd), new Hotkey(Keys.OemMinus, Hotmods.ControlAlt) },
				{ nameof(FillDownCmd), new Hotkey(Keys.D, Hotmods.Control) },
				{ nameof(HighlightCmd), new Hotkey(Keys.H, Hotmods.ControlShift) },
				{ nameof(IncreaseFontSizeCmd), new Hotkey(Keys.Oemplus, Hotmods.ControlAlt) },
				{ nameof(InsertCodeBlockCmd), new Hotkey(Keys.F6) },
				{ nameof(InsertDateCmd), new Hotkey(Keys.D, Hotmods.ControlShift) },
				{ nameof(InsertDoubleHorizontalLineCmd), new Hotkey(Keys.F12, Hotmods.AltShift) },
				{ nameof(InsertHorizontalLineCmd), new Hotkey(Keys.F11, Hotmods.AltShift) },
				{ nameof(InsertTimerCmd), new Hotkey(Keys.F2) },
				{ nameof(DisableSpellCheckCmd), new Hotkey(Keys.F4) },
				{ nameof(PasteRtfCmd), new Hotkey(Keys.V, Hotmods.ControlAlt) },
				{ nameof(RecalculateFormulaCmd), new Hotkey(Keys.F5, Hotmods.Shift) },
				{ nameof(RemoveFootnoteCmd), new Hotkey(Keys.F, Hotmods.ControlShift) },
				{ nameof(ReplayCmd), new Hotkey(Keys.R, Hotmods.AltShift) },
				{ nameof(RemindCmd), new Hotkey(Keys.F8) },
				{ nameof(SearchCmd), new Hotkey(Keys.F, Hotmods.Alt) },
				{ nameof(SearchAndReplaceCmd), new Hotkey(Keys.H, Hotmods.Control) },
				{ nameof(StartTimerCmd), new Hotkey(Keys.F2, Hotmods.Alt) },
				{ nameof(TaggedCmd), new Hotkey(Keys.T, Hotmods.ControlAlt) },
				{ nameof(TaggingCmd), new Hotkey(Keys.T, Hotmods.Alt) },
				{ nameof(ToLowercaseCmd), new Hotkey(Keys.U, Hotmods.ControlShift) },
				{ nameof(ToUppercaseCmd), new Hotkey(Keys.U, Hotmods.ControlAltShift) },

				// tools

				{ nameof(ShowXmlCmd), new Hotkey(Keys.X, Hotmods.ControlAltShift) },
				{ nameof(DiagnosticsCommand), new Hotkey(Keys.F8, Hotmods.Shift) },
				{ nameof(ClearLogCommand), new Hotkey(Keys.F8, Hotmods.Control) },

				// custom styles

				{ "ApplyStyle1", new Hotkey(Keys.D1, Hotmods.ControlAltShift) },
				{ "ApplyStyle2", new Hotkey(Keys.D2, Hotmods.ControlAltShift) },
				{ "ApplyStyle3", new Hotkey(Keys.D3, Hotmods.ControlAltShift) },
				{ "ApplyStyle4", new Hotkey(Keys.D4, Hotmods.ControlAltShift) },
				{ "ApplyStyle5", new Hotkey(Keys.D5, Hotmods.ControlAltShift) },
				{ "ApplyStyle6", new Hotkey(Keys.D6, Hotmods.ControlAltShift) },
				{ "ApplyStyle7", new Hotkey(Keys.D7, Hotmods.ControlAltShift) },
				{ "ApplyStyle8", new Hotkey(Keys.D8, Hotmods.ControlAltShift) },
				{ "ApplyStyle9", new Hotkey(Keys.D9, Hotmods.ControlAltShift) }
			};

			return map;
		}
	}
}
