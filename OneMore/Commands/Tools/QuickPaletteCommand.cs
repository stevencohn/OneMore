//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Displays the OneMore Quick Palette dialog
	/// </summary>
	internal class QuickPaletteCommand : Command
	{
		private List<string> commands;
		private int lastStyle;


		public QuickPaletteCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new CommandPaletteDialog(
				Resx.QuickPaletteCommand_Title,
				Resx.QuickPaletteCommand_Intro,
				false);

			dialog.RequestData += PopulateCommands;
			PopulateCommands(dialog, null);

			if (dialog.ShowDialog(owner) == DialogResult.OK &&
				dialog.Index >= 0)
			{
				var command = commands[dialog.Index];
				logger.WriteLine($"applying attribute {dialog.Index}: '{command}'");

				if (dialog.Index <= lastStyle)
				{
					await factory.Run<ApplyStyleCommand>(dialog.Index);
				}
				else
				{
					await InvokeBasics(dialog.Index - lastStyle - 1);
				}
			}

			// reset focus to OneNote window
			await using var one = new OneNote();
			Native.SwitchToThisWindow(one.WindowHandle, false);
		}


		private async Task InvokeBasics(int index)
		{
			var style = new Style
			{
				FontFamily = null,
				FontSize = "0",
				ApplyColors = true
			};

			switch (index)
			{
				// basic text
				case 0: style.IsBold = true; break;
				case 1: style.IsItalic = true; break;
				case 2: style.IsUnderline = true; break;
				case 3: style.IsStrikethrough = true; break;
				case 4: style.IsSubscript = true; break;
				case 5: style.IsSuperscript = true; break;

				// font colors
				case 6: style.Color = "#5B9BD5"; break;     // Blue
				case 7: style.Color = "#70AD47"; break;     // Green
				case 8: style.Color = "#ED7D31"; break;     // Orange
				case 9: style.Color = "#8064A2"; break;     // Purple
				case 10: style.Color = "#E84C22"; break;    // Red
				case 11: style.Color = "#FFD965"; break;    // Yellow

				// highlight colors
				case 12: style.Highlight = "#5B9BD5"; break;    // Blue
				case 13: style.Highlight = "#70AD47"; break;    // Green
				case 14: style.Highlight = "#ED7D31"; break;    // Orange
				case 15: style.Highlight = "#8064A2"; break;    // Purple
				case 16: style.Highlight = "#E84C22"; break;    // Red
				case 17: style.Highlight = "#FFD965"; break;    // Yellow
			}

			var cmd = (ApplyStyleCommand)await factory.Make<ApplyStyleCommand>();
			await cmd.ExecuteWithStyle(style);
		}


		private void PopulateCommands(object sender, EventArgs e)
		{
			void Add(string category, string text)
			{
				var names = text.Split(
					new string[] { Environment.NewLine },
					StringSplitOptions.RemoveEmptyEntries);

				foreach (var name in names)
				{
					commands.Add($"{category}:{name}");
				}
			}

			var dialog = sender as CommandPaletteDialog;

			commands = new List<string>();

			var styleNames = new ThemeProvider().Theme.GetStyles().Select(s => $"styles:{s.Name}");
			commands.AddRange(styleNames);
			lastStyle = commands.Count - 1;

			Add(Resx.QuickPaletteCommand_basic_category, Resx.QuickPaletteCommand_basics);
			Add(Resx.QuickPaletteCommand_color_category, Resx.QuickPaletteCommand_colors);
			Add(Resx.QuickPaletteCommand_highlight_category, Resx.QuickPaletteCommand_highlights);

			dialog.PopulateCommands(commands.ToArray(), new string[0]);
		}
	}
}
