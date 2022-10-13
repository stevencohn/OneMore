//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;

	internal partial class CommandPaletteDialog : UI.LocalizableForm
	{
		private readonly MoreAutoCompleteList palette;
		private string[] commands;
		private string[] recentNames;
		private bool escaping;


		public CommandPaletteDialog()
		{
			InitializeComponent();

			palette = new MoreAutoCompleteList();
			palette.SetAutoCompleteList(cmdBox, palette);

			if (NeedsLocalizing())
			{
				Text = Resx.CommandPalette_Title;

				Localize(new string[]
				{
					"introLabel",
					"clearLink"
				});
			}

			VerticalOffset = 1;
		}


		public event EventHandler RequestData;


		public int Index { get; private set; } = -1;


		public bool Recent { get; private set; }


		public void PopulateCommands(string[] commands, string[] recentNames)
		{
			palette.LoadCommands(commands, recentNames);
			this.commands = commands;
			this.recentNames = recentNames;
		}


		private void ClearRecentCommands(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (MoreMessageBox.Show(this,
				Resx.CommandPalette_clear,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				new CommandProvider().ClearMRU();
				RequestData?.Invoke(this, EventArgs.Empty);
			}
		}


		private void ValidateCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();

			errorProvider.SetError(cmdBox, 
				string.IsNullOrWhiteSpace(text) || palette.HasMatches 
					? String.Empty
					: Resx.CommandPalette_unrecognized);
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
				//logger.WriteLine($"CommandPaletteDialog.InvokeCommand({text}) index:{Index} recent:{Recent}");
				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape && escaping)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				InvokeCommand(sender, e);
			}

			escaping = false;
		}

		private void DoPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				escaping = !palette.Visible;
			}
		}
	}
}
