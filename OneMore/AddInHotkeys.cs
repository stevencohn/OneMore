//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Windows.Forms;


	public partial class AddIn
	{
		private void RegisterHotkeys()
		{
			HotkeyManager.RegisterHotKey(() =>
				AddFootnoteCmd(null),
				Keys.F, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() =>
				RemoveFootnoteCmd(null),
				Keys.F, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() =>
				InsertHorizontalLineCmd(null),
				Keys.OemMinus, Hotmods.AltShift);

			HotkeyManager.RegisterHotKey(() =>
				InsertDoubleHorizontalLineCmd(null),
				Keys.Oemplus, Hotmods.AltShift);

			HotkeyManager.RegisterHotKey(() =>
				NoSpellCheckCmd(null),
				Keys.F4);

			HotkeyManager.RegisterHotKey(() =>
				PasteRtfCmd(null),
				Keys.V, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() =>
				SearchAndReplaceCmd(null),
				Keys.H, Hotmods.Control);

			HotkeyManager.RegisterHotKey(() =>
				ToLowercaseCmd(null),
				Keys.U, Hotmods.ControlShift);

			HotkeyManager.RegisterHotKey(() =>
				ToUppercaseCmd(null),
				Keys.U, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				IncreaseFontSizeCmd(null),
				Keys.Oemplus, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() =>
				DecreaseFontSizeCmd(null),
				Keys.OemMinus, Hotmods.ControlAlt);

			HotkeyManager.RegisterHotKey(() =>
				ShowXmlCmd(null),
				Keys.X, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.Run<DiagnosticsCommand>(),
				Keys.F8);

			HotkeyManager.RegisterHotKey(() =>
				factory.Run<ClearLogCommand>(),
				Keys.F8, Hotmods.Control);

			// custom styles, CtrlAltShift+1..9

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(0),
				Keys.D1, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(1),
				Keys.D2, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(2),
				Keys.D3, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(3),
				Keys.D4, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(4),
				Keys.D5, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(5),
				Keys.D6, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(6),
				Keys.D7, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(7),
				Keys.D8, Hotmods.ControlAltShift);

			HotkeyManager.RegisterHotKey(() =>
				factory.GetCommand<ApplyStyleCommand>().Execute(8),
				Keys.D9, Hotmods.ControlAltShift);
		}
	}
}
