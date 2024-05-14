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


	/// <summary>
	/// Displays the OneMore Quick Palette dialog
	/// </summary>
	internal class QuickPaletteCommand : Command
	{
		private List<string> commands;
		private int lastStyle;
		private int lastBasic;
		private int lastColor;


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
				dialog.Index >= 0)
			{
				var command = commands[dialog.Index];
				logger.WriteLine($"command[{dialog.Index}], name:'{command}'");

				if (dialog.Index <= lastStyle)
				{
					await AddIn.Self.ApplyStyleCmd(null, null, dialog.Index);
				}
				else if (dialog.Index <= lastBasic)
				{
				}
				else if (dialog.Index <= lastColor)
				{
				}
				else
				{
				}
			}

			// reset focus to OneNote window
			await using var one = new OneNote();
			Native.SwitchToThisWindow(one.WindowHandle, false);
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
			lastBasic = commands.Count - 1;

			commands.Add("font colors:Blue");
			commands.Add("font colors:Green");
			commands.Add("font colors:Orange");
			commands.Add("font colors:Purple");
			commands.Add("font colors:Red");
			commands.Add("font colors:Yellow");
			lastColor = commands.Count - 1;

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
