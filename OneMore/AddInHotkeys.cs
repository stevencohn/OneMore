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
			logger.WriteLine("defining hotkeys");
				
			HotkeyManager.Initialize();

			HotkeyManager.RegisterHotKey(() => AddFootnoteCmd(null),
				Keys.F, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() => AddFormulaCmd(null),
				Keys.F5);

			HotkeyManager.RegisterHotKey(() => DecreaseFontSizeCmd(null),
				Keys.OemMinus, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() => HighlightCmd(null),
				Keys.H, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => IncreaseFontSizeCmd(null),
				Keys.Oemplus, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() => InsertCodeBlockCmd(null),
				Keys.F6);

			HotkeyManager.RegisterHotKey(() => InsertDateCmd(null),
				Keys.D, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => InsertDoubleHorizontalLineCmd(null),
				Keys.Oemplus, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => InsertHorizontalLineCmd(null),
				Keys.OemMinus, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => NoSpellCheckCmd(null),
				Keys.F4);

			HotkeyManager.RegisterHotKey(() => PasteRtfCmd(null),
				Keys.V, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() => RecalculateFormulaCmd(null),
				Keys.F5, Hotmods.Shift);

			HotkeyManager.RegisterHotKey(() => RemoveFootnoteCmd(null),
				Keys.F, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => SearchCmd(null),
				Keys.F, Hotmods.Alt);

			HotkeyManager.RegisterHotKey(() => SearchAndReplaceCmd(null),
				Keys.H, Hotmods.Control);

			HotkeyManager.RegisterHotKey(() => TaggedCmd(null),
				Keys.T, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() => TaggingCmd(null),
				Keys.T, Hotmods.Alt);

			HotkeyManager.RegisterHotKey(() => ToLowercaseCmd(null),
				Keys.U, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() => ToUppercaseCmd(null),
				Keys.U, Hotmods.ControlAltShift);

			// tools

			HotkeyManager.RegisterHotKey(() => ShowXmlCmd(null),
				Keys.X, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<DiagnosticsCommand>(),
				Keys.F8);

			HotkeyManager.RegisterHotKey(() => factory.Run<ClearLogCommand>(),
				Keys.F8, Hotmods.Control);

			// custom styles, CtrlAltShift+1..9

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(0),
				Keys.D1, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(1),
				Keys.D2, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(2),
				Keys.D3, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(3),
				Keys.D4, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(4),
				Keys.D5, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(5),
				Keys.D6, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(6),
				Keys.D7, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(7),
				Keys.D8, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() => factory.Run<ApplyStyleCommand>(9),
				Keys.D9, Hotmods.ControlAltShift);
		}
	}
}
