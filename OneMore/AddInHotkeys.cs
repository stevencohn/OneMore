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
				factory.GetCommand<DiagnosticsCommand>().Execute(),
				Keys.F8);
		}
	}
}
