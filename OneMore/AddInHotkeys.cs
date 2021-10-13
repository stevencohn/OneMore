//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands;
	using System.Windows.Forms;


	public partial class AddIn
	{
		private void RegisterHotkeys()
		{
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;
			logger.WriteLine($"defining hotkeys for input locale {locale}");

			HotkeyManager.Initialize();

			HotkeyManager.RegisterHotKey(async () => await AddFootnoteCmd(null),
				Keys.F, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(async () => await AddFormulaCmd(null),
				Keys.F5);

			HotkeyManager.RegisterHotKey(async () => await DecreaseFontSizeCmd(null),
				Keys.OemMinus, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(async () => await FillDownCmd(null),
				Keys.D, Hotmods.Control);

			HotkeyManager.RegisterHotKey(async () => await HighlightCmd(null),
				Keys.H, Hotmods.ControlShift);

			// this is an aweful hack to avoid a conflict with Italian keyboards that use AltGr
			// as Ctrl+Alt. This means users pressing AltGr+OemPlus to get a square bracket would
			// instead end up increasing the font size of the page when they didn't mean to!
			// So here we only register these hot keys for the US keyboard input layout.
			// This seem to apply to FIGS languages and probably UK.
			if (locale == 1033)
			{
				HotkeyManager.RegisterHotKey(async () => await IncreaseFontSizeCmd(null),
					Keys.Oemplus, Hotmods.ControlAlt);
			}

			HotkeyManager.RegisterHotKey(async () => await InsertCodeBlockCmd(null),
				Keys.F6);

			HotkeyManager.RegisterHotKey(async () => await InsertDateCmd(null),
				Keys.D, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(async () => await InsertDoubleHorizontalLineCmd(null),
				Keys.F12, Hotmods.AltShift);

			HotkeyManager.RegisterHotKey(async () => await InsertHorizontalLineCmd(null),
				Keys.F11, Hotmods.AltShift);

			HotkeyManager.RegisterHotKey(async () => await InsertTimerCmd(null),
				Keys.F2);

			HotkeyManager.RegisterHotKey(async () => await DisableSpellCheckCmd(null),
				Keys.F4);

			HotkeyManager.RegisterHotKey(async () => await PasteRtfCmd(null),
				Keys.V, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(async () => await RecalculateFormulaCmd(null),
				Keys.F5, Hotmods.Shift);

			HotkeyManager.RegisterHotKey(async () => await RemoveFootnoteCmd(null),
				Keys.F, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(async () => await ReplayCmd(null),
				Keys.R, Hotmods.AltShift);

			HotkeyManager.RegisterHotKey(async () => await SearchCmd(null),
				Keys.F, Hotmods.Alt);

			HotkeyManager.RegisterHotKey(async () => await SearchAndReplaceCmd(null),
				Keys.H, Hotmods.Control);

			HotkeyManager.RegisterHotKey(async () => await StartTimerCmd(null),
				Keys.F2, Hotmods.Alt);

			HotkeyManager.RegisterHotKey(async () => await TaggedCmd(null),
				Keys.T, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(async () => await TaggingCmd(null),
				Keys.T, Hotmods.Alt);

			HotkeyManager.RegisterHotKey(async () => await ToLowercaseCmd(null),
				Keys.U, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(async () => await ToUppercaseCmd(null),
				Keys.U, Hotmods.ControlAltShift);

			// tools

			HotkeyManager.RegisterHotKey(async () => await ShowXmlCmd(null),
				Keys.X, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<DiagnosticsCommand>(),
				Keys.F8);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ClearLogCommand>(),
				Keys.F8, Hotmods.Control);

			// custom styles, CtrlAltShift+1..9

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(0),
				Keys.D1, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(1),
				Keys.D2, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(2),
				Keys.D3, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(3),
				Keys.D4, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(4),
				Keys.D5, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(5),
				Keys.D6, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(6),
				Keys.D7, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(7),
				Keys.D8, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(async () => await factory.Run<ApplyStyleCommand>(9),
				Keys.D9, Hotmods.ControlAltShift);
		}
	}
}
