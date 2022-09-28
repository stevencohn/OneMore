//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class CommandPallettDialog : UI.LocalizableForm
	{
		private string[] commands;


		public CommandPallettDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CommandPallett_Title;

				Localize(new string[]
				{
					"okButton=word_Go"
				});
			}

			VerticalOffset = 1;
		}


		public CommandPallettDialog(string[] commands)
			: this()
		{
			var source = new AutoCompleteStringCollection();
			source.AddRange(commands);

			cmdBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			cmdBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
			cmdBox.AutoCompleteCustomSource = source;

			this.commands = commands;
		}


		public int CommandIndex { get; private set; } = -1;


		private void ValidateCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();
			okButton.Enabled = 
				!string.IsNullOrEmpty(text) &&
				commands.Any(c => c.Equals(text, StringComparison.InvariantCultureIgnoreCase));
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();
			var index = commands.IndexOf(s => s.Equals(text, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0)
			{
				CommandIndex = index;
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
