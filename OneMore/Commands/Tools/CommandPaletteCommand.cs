//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Displays the OneMore Command Palette dialog
	/// </summary>
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
			using var guard = EnterOnce();
			if (guard is null) { return; }

			using var dialog = new CommandPaletteDialog();
			dialog.RequestData += PopulateCommands;
			PopulateCommands(dialog, null);

			var result = dialog.ShowDialog(owner);
			var index = dialog.Index;
			var isRecent = dialog.Recent;

			// the palette dialog itself is closed at this point; release the
			// re-entry guard before invoking the chosen command, which may not
			// return until ITS OWN dialog closes (e.g. SearchCommand/
			// SearchTitleCommand when invoked with no message loop already
			// running) - otherwise the palette can't be reopened until that
			// command's dialog closes
			guard.Dispose();

			// likewise, close out our own logger block here rather than leaving it
			// open for however long the chosen command takes to run; otherwise that
			// command's own "Running command X" line inherits our stale ".." preamble
			// even though, from here on, it's running as its own independent command
			logger.End();

			if (result == DialogResult.OK && index >= 0)
			{
				var command = isRecent
					? recent[index]
					: commands[index];

				//logger.WriteLine($"invoking command[index:{index},recent:{isRecent}] 'method:{command.Method.Name}'");
				await (Task)command.Method.Invoke(AddIn.Self, new object[] { null });

				// save if not IsCancelled...

				var type = Type.GetType($"River.OneMoreAddIn.Commands.{command.Method.Name}");
				if (type != null)
				{
					var prop = type.GetProperty("IsCancelled");
					if (prop != null)
					{
						if (prop.GetValue(Activator.CreateInstance(type)) is bool isCancelled &&
							!isCancelled)
						{
							new CommandProvider().SaveToMRU(command);
						}
					}
				}
			}

			// reset focus to OneNote window - but only if the invoked command didn't
			// leave its own popup open and focused (CompleteHashtagCommand/
			// SearchCommand/SearchTitleCommand can return before their modeless
			// dialog closes, see Command.EnterOnce remarks); otherwise this would
			// steal focus right back from a freshly shown, still-open dialog
			var foreground = Native.GetForegroundWindow();
			Native.GetWindowThreadProcessId(foreground, out var pid);
			if (pid != (uint)Process.GetCurrentProcess().Id)
			{
				await using var one = new OneNote();
				Native.SwitchToThisWindow(one.WindowHandle, false);
			}
		}


		private void PopulateCommands(object sender, EventArgs e)
		{
			var dialog = sender as CommandPaletteDialog;

			var provider = new CommandProvider();

			if (e is not null)
			{
				provider.ClearMRU();
			}

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
