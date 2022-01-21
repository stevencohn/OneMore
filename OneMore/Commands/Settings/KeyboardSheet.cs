//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class KeyboardSheet : SheetBase
	{
		private sealed class Sequence
		{
			public string Command { get; set; }
			public string Keys { get; set; }
		}


		private readonly IRibbonUI ribbon;
		private readonly BindingList<Sequence> keyboard;
		private bool updated = false;


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
			gridView.Columns[1].DataPropertyName = "Keys";

			this.ribbon = ribbon;

			keyboard = new BindingList<Sequence>(LoadKeyboard());
			gridView.DataSource = keyboard;
		}


		private List<Sequence> LoadKeyboard()
		{
			return new List<Sequence>
			{
				new Sequence { Command = "Foo", Keys = "Ctrl+A" },
				new Sequence { Command = "Bar", Keys = "Ctrl+B" }
			};
		}


		private void gridView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Back ||
				(e.Modifiers != 0 &&
				 e.KeyCode != Keys.ControlKey &&
				 e.KeyCode != Keys.ShiftKey &&
				 e.KeyCode != Keys.LShiftKey &&
				 e.KeyCode != Keys.RShiftKey &&
				 e.KeyCode != Keys.Menu &&
				 e.KeyCode != Keys.LMenu &&
				 e.KeyCode != Keys.RMenu))
			{
				if ((e.Modifiers == Keys.Shift) && (e.KeyCode < Keys.F1 || e.KeyCode > Keys.F24))
				{
					return;
				}

				if (e.Modifiers == (Keys)((int)Keys.Control + (int)Keys.Shift) && e.KeyCode == Keys.C)
				{
					e.Handled = true;
					return;
				}

				var sequence = string.Empty;

				if (e.KeyCode != Keys.Back)
				{
					if (e.Control) sequence = "Ctrl+";
					if (e.Shift) sequence = $"{sequence}Shift+";
					if (e.Alt) sequence = $"{sequence}Alt+";
					sequence = $"{sequence}{e.KeyCode}";
				}

				Logger.Current.WriteLine($"keys=[{sequence}]");

				if (gridView.SelectedCells.Count > 0)
				{
					var cell = gridView.SelectedCells[0];
					gridView.Rows[cell.RowIndex].Cells["keyColumn"].Value = sequence;
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
