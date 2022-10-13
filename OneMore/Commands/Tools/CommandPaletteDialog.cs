//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;

	internal partial class CommandPaletteDialog : UI.LocalizableForm
	{
		private readonly MoreCommandPalette palette;
		private readonly string[] commands;
		private readonly string[] recentNames;


		public CommandPaletteDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CommandPalette_Title;

				Localize(new string[]
				{
					"okButton=word_Go"
				});
			}

			VerticalOffset = 1;
		}


		public CommandPaletteDialog(string[] commands, string[] recentNames)
			: this()
		{
			palette = new MoreCommandPalette();
			palette.SetCommandPalette(cmdBox, palette);
			palette.LoadCommands(commands, recentNames);

			this.commands = commands;
			this.recentNames = recentNames;
		}


		public int Index { get; private set; } = -1;


		public bool Recent { get; private set; }


		private void ValidateCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();
			var regex = new Regex($"^{text}($|\\|.+$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			okButton.Enabled = 
				!string.IsNullOrEmpty(text) && 
				(recentNames.Any(c => regex.IsMatch(c)) || commands.Any(c => regex.IsMatch(c)));
		}


		private void InvokeCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();
			var regex = new Regex($"^{text}($|\\|.+$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			Index = commands.IndexOf(c => regex.IsMatch(c));
			if (Index < 0)
			{
				Index = recentNames.IndexOf(c => regex.IsMatch(c));
				Recent = Index >= 0;
			}

			if (Index >= 0)
			{
				logger.WriteLine($"CommandPaletteDialog.InvokeCommand({text}) index:{Index} recent:{Recent}");
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
