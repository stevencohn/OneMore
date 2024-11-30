//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class CommandPaletteDialog : MoreForm
	{
		private readonly MoreAutoCompleteList palette;
		private string[] commands;
		private string[] recentNames;
		private bool escaping;


		public CommandPaletteDialog()
			: this(Resx.CommandPalette_Title, Resx.CommandPaletteDialog_introLabel_Text, true)
		{
		}


		public CommandPaletteDialog(string title, string intro, bool showClearOption)
		{
			InitializeComponent();

			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			var nonseq = settings.Get("nonseqMatching", false);

			palette = new MoreAutoCompleteList
			{
				// allow nonsequential character matching
				NonsequentialMatching = nonseq,
				// keeping this as false will eliminate flicker on startup
				ShowPopupOnStartup = false
			};

			palette.SetAutoCompleteList(cmdBox);

			Text = title;
			introLabel.Text = intro;

			if (!showClearOption)
			{
				clearLink.Visible = false;
				cmdBox.Top -= clearLink.Height;
				Height -= clearLink.Height;
			}
			else if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"clearLink"
				});
			}
		}


		public event EventHandler RequestData;


		public int Index { get; private set; } = -1;


		public bool Recent { get; private set; }


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			//Native.SwitchToThisWindow(Handle, false);
		}


		public void PopulateCommands(string[] commands, string[] recentNames)
		{
			palette.LoadCommands(commands, recentNames);
			this.commands = commands;
			this.recentNames = recentNames;

			clearLink.Enabled = recentNames?.Length > 0;
		}


		private void ClearRecentCommands(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (MoreMessageBox.Show(this,
				Resx.CommandPalette_clear,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				RequestData?.Invoke(this, e);
			}
		}


		private void ValidateCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();

			errorProvider.SetError(cmdBox,
				string.IsNullOrWhiteSpace(text) || palette.HasMatches
					? string.Empty
					: Resx.CommandPalette_unrecognized);
		}


		private void InvokeCommand(object sender, EventArgs e)
		{
			var text = cmdBox.Text.Trim();

			var pattern = new Regex(
				@$"(?:(?<cat>[^{palette.CategoryDivider}]+){palette.CategoryDivider})?" +
				text +
				$@"(?:\{palette.KeyDivider}(?<seq>.*))?",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			Index = commands.IndexOf(c => pattern.IsMatch(c));
			if (Index < 0)
			{
				Index = recentNames.IndexOf(c => pattern.IsMatch(c));
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
				escaping = !palette.IsPopupVisible;
			}
		}
	}
}
