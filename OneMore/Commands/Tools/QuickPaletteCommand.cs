﻿//************************************************************************************************
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
			using var dialog = new CommandPaletteDialog("Quick Palette", "do things...", false);

			dialog.RequestData += PopulateCommands;
			PopulateCommands(dialog, null);

			if (dialog.ShowDialog(owner) == DialogResult.OK &&
				dialog.Index >= 0 &&
				args[0] is CommandFactory factory)
			{
				var command = commands[dialog.Index];
				logger.WriteLine($"command[{dialog.Index}], name:'{command}'");

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
				case 6: style.Color = "#5B9BD5"; break;		// Blue
				case 7: style.Color = "#70AD47"; break;		// Green
				case 8: style.Color = "#ED7D31"; break;		// Orange
				case 9: style.Color = "#8064A2"; break;		// Purple
				case 10: style.Color = "#E84C22"; break;	// Red
				case 11: style.Color = "#FFC000"; break;	// Yellow

				// highlight colors
				case 12: style.Highlight = "#5B9BD5"; break;	// Blue
				case 13: style.Highlight = "#70AD47"; break;	// Green
				case 14: style.Highlight = "#ED7D31"; break;	// Orange
				case 15: style.Highlight = "#8064A2"; break;	// Purple
				case 16: style.Highlight = "#E84C22"; break;	// Red
				case 17: style.Highlight = "#FFC000"; break;	// Yellow
			}

			var cmd = (ApplyStyleCommand)await factory.Make<ApplyStyleCommand>();
			await cmd.ExecuteWithStyle(style);
		}


		private void PopulateCommands(object sender, EventArgs e)
		{
			var dialog = sender as CommandPaletteDialog;

			commands = new List<string>();

			var styleNames = new ThemeProvider().Theme.GetStyles().Select(s => $"styles:{s.Name}");
			commands.AddRange(styleNames);
			lastStyle = commands.Count - 1;

			commands.Add("basic text:Bold");
			commands.Add("basic text:Italic");
			commands.Add("basic text:Underline");
			commands.Add("basic text:Strikethrough");
			commands.Add("basic text:Subscript");
			commands.Add("basic text:Superscript");

			commands.Add("font colors:Blue");
			commands.Add("font colors:Green");
			commands.Add("font colors:Orange");
			commands.Add("font colors:Purple");
			commands.Add("font colors:Red");
			commands.Add("font colors:Yellow");

			commands.Add("highlight colors:Blue");
			commands.Add("highlight colors:Green");
			commands.Add("highlight colors:Orange");
			commands.Add("highlight colors:Purple");
			commands.Add("highlight colors:Red");
			commands.Add("highlight colors:Yellow");

			dialog.PopulateCommands(commands.ToArray(), new string[0]);
		}
	}
}
