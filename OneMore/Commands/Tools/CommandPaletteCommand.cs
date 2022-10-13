//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class CommandPaletteCommand : Command
	{
		private List<CommandInfo> commands;
		private List<CommandInfo> recent;


		public CommandPaletteCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new CommandPaletteDialog();
			dialog.RequestData += PopulateCommands;
			PopulateCommands(dialog, EventArgs.Empty);

			if (dialog.ShowDialog(Owner) == DialogResult.OK &&
				dialog.Index >= 0)
			{
				var command = dialog.Recent
					? recent.ElementAt(dialog.Index)
					: commands.ElementAt(dialog.Index);

				logger.WriteLine($"invoking command[{dialog.Index},{dialog.Recent}] '{command.Method.Name}'");
				await (Task)command.Method.Invoke(AddIn.Self, new object[] { null });
			}
		}

		private void PopulateCommands(object sender, EventArgs e)
		{
			var dialog = sender as CommandPaletteDialog;

			var provider = new CommandProvider();
			commands = provider.LoadPaletteCommands().OrderBy(c => c.Name).ToList();
			recent = provider.LoadMRU(commands).OrderBy(c => c.Name).ToList();
			logger.WriteLine($"discovered {commands.Count} commands, {recent.Count} mru");

			var names = commands.Select(c => c.Keys == Keys.None
				? c.Name
				: $"{c.Name}|{new Hotkey(c.Keys)}")
				.ToArray();

			var recentNames = recent.Select(c => c.Keys == Keys.None
				? c.Name
				: $"{c.Name}|{new Hotkey(c.Keys)}")
				.ToArray();

			dialog.PopulateCommands(names, recentNames);
		}
	}
}
