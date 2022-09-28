//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class CommandPallettCommand : Command
	{
		private sealed class CommandInfo
		{
			public string Name { get; set; }
			public MethodInfo Method { get; set; }
			public override string ToString()
			{
				return Name;
			}
		}


		public CommandPallettCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			var commands = DiscoverCommands().OrderBy(c => c.Name);
			logger.WriteLine($"discovered {commands.Count()} commands");

			// auto-complete feature of TextBox requires STA thread
			var index = await SingleThreaded.Invoke(() =>
			{
				var names = commands.Select(c => c.Name).ToArray();

				using (var dialog = new CommandPallettDialog(names))
				{
					return (dialog.ShowDialog(Owner) == DialogResult.OK)
						? dialog.CommandIndex
						: -1;
				}
			});

			if (index >= 0 && index < commands.Count())
			{
				var command = commands.ElementAt(index);
				logger.WriteLine($"invoking command[{index}] '{command.Method.Name}'");

				await (Task)command.Method.Invoke(AddIn.Self, new object[] { null });
			}
		}


		private IEnumerable<CommandInfo> DiscoverCommands()
		{
			// heavily relies on naming convention, suffix must be "Cmd"
			var methods = typeof(AddIn).GetMethods()
				.Where(m => m.Name.EndsWith("Cmd"));

			foreach (var method in methods)
			{
				// remove "Cmd" suffix from method name
				var nam = method.Name.Substring(0, method.Name.Length - 3);

				// translate to display name
				var name = Properties.Resources.ResourceManager.GetString($"rib{nam}Button_Label");
				if (string.IsNullOrEmpty(name))
				{
					name = Properties.Resources.ResourceManager.GetString($"om{name}Button_Label");
				}

				if (!string.IsNullOrWhiteSpace(name))
				{
					yield return new CommandInfo
					{
						Name = name,
						Method = method
					};
				}
			}
		}
	}
}
