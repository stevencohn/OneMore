﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class KeyboardSheet : SheetBase
	{
		private sealed class Sequence
		{
			public string Command { get; set; }
			public Hotkey Hotkey { get; set; }
		}


		private readonly IRibbonUI ribbon;
		private readonly BindingList<Sequence> keyboard;
		private bool updated = false;

		private Dictionary<string, Sequence> kbdefaults;


		public KeyboardSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "KeyboardSheet";
			Title = Resx.SnippetsSheet_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel"
				});

				cmdColumn.HeaderText = Resx.KeyboardSheet_cmdColumn_HeaderText;
				keyColumn.HeaderText = Resx.KeyboardSheet_keyColumn_HeaderText;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Command";
			gridView.Columns[1].DataPropertyName = "Hotkey";

			this.ribbon = ribbon;

			keyboard = new BindingList<Sequence>(LoadKeyboard());
			gridView.DataSource = keyboard;
		}


		private List<Sequence> LoadKeyboard()
		{
			kbdefaults = typeof(AddIn).GetMethods()
				.Select(m => m.GetCustomAttribute(typeof(CommandAttribute), false))
				.Where(a => a != null)
				.Select(a => new Sequence
				{
					Command = ((CommandAttribute)a).ResID,
					Hotkey = new Hotkey(((CommandAttribute)a).DefaultKeys)
				})
				.ToDictionary(k => k.Command, v => v);

			return kbdefaults
				.OrderBy(k => k.Value.Command)
				.Select(k => k.Value)
				.ToList();
		}


		private void AssignOnKeyDown(object sender, KeyEventArgs e)
		{
			if ( // clear assignment
				 e.KeyCode == Keys.Back ||
				 // any combination of ctrl+shift+alt+win
				(e.Modifiers != 0 &&
				 // ensure modifiers also comes with a value key
				 e.KeyCode != Keys.ControlKey &&
				 e.KeyCode != Keys.LControlKey &&
				 e.KeyCode != Keys.RControlKey &&
				 e.KeyCode != Keys.ShiftKey &&
				 e.KeyCode != Keys.LShiftKey &&
				 e.KeyCode != Keys.RShiftKey &&
				 e.KeyCode != Keys.Menu && // alt
				 e.KeyCode != Keys.LMenu &&
				 e.KeyCode != Keys.RMenu) ||
				 // F1..F24
				(e.Modifiers == 0 &&
				 e.KeyCode >= Keys.F1 &&
				 e.KeyCode <= Keys.F24))
			{
				// ignore Shift without a Fn key
				if ((e.Modifiers == Keys.Shift) && (e.KeyCode < Keys.F1 || e.KeyCode > Keys.F24))
				{
					return;
				}

				if (gridView.SelectedCells.Count > 0)
				{
					var hotkey = new Hotkey(e.KeyCode, e.Modifiers);

					var cell = gridView.SelectedCells[0];
					gridView.Rows[cell.RowIndex].Cells["keyColumn"].Value = hotkey;


				}

				e.Handled = true;
			}
		}


		public override bool CollectSettings()
		{
			if (updated)
			{
				ribbon.InvalidateControl("ribOneMoreMenu");
			}

			return false;
		}
	}
}
