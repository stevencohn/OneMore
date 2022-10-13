//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Properties;
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;

	internal class CommandPaletteCommand : Command
	{
		private sealed class CommandInfo
		{
			public string Name { get; set; }
			public Keys Keys { get; set; }
			public MethodInfo Method { get; set; }
			public override string ToString()
			{
				return Name;
			}
		}


		private XElement recentActions;


		public CommandPaletteCommand()
		{
			// prevent replay
			IsCancelled = true;
			recentActions = null;
		}


		public override async Task Execute(params object[] args)
		{
			var commands = DiscoverCommands().OrderBy(c => c.Name);
			var recent = LoadMRU(commands).OrderBy(c => c.Name);

			logger.WriteLine($"discovered {commands.Count()} commands, {recent.Count()} mru");

			var names = commands.Select(c => c.Keys == Keys.None
				? c.Name
				: $"{c.Name}|{new Hotkey(c.Keys)}")
				.ToArray();

			var recentNames = recent.Select(c => c.Keys == Keys.None
				? c.Name
				: $"{c.Name}|{new Hotkey(c.Keys)}")
				.ToArray();

			// auto-complete feature of TextBox requires STA thread
			var index = await SingleThreaded.Invoke(() =>
			{
				using var dialog = new CommandPaletteDialog(names, recentNames);

				return dialog.ShowDialog(Owner) == DialogResult.OK
					? dialog.CommandIndex
					: -1;
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
			var commands = new List<CommandInfo>();

			// heavily relies on naming convention, suffix must be "Cmd"
			var methods = typeof(AddIn).GetMethods()
				.Where(m => m.Name.EndsWith("Cmd"));

			foreach (var method in methods)
			{
				// remove "Cmd" suffix from method name
				var nam = method.Name.Substring(0, method.Name.Length - 3);

				// translate to display name
				var name = Resources.ResourceManager.GetString($"rib{nam}Button_Label");
				if (string.IsNullOrEmpty(name))
				{
					name = Resources.ResourceManager.GetString($"om{name}Button_Label");
				}

				var keys = Keys.None;
				var att = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute));
				if (att != null)
				{
					keys = att.DefaultKeys;
				}

				if (!string.IsNullOrWhiteSpace(name))
				{
					commands.Add(new CommandInfo
					{
						Name = name,
						Keys = keys,
						Method = method
					});
				}
			}

			// load user aliases
			var settings = new SettingsProvider()
				.GetCollection(AliasSheet.CollectionName)?
				.Get<XElement>(AliasSheet.SettingsName);

			if (settings != null)
			{
				foreach (var setting in settings.Elements())
				{
					var command = commands
						.FirstOrDefault(c => c.Method.Name == setting.Attribute("methodName").Value);

					if (command != null)
					{
						commands.Add(new CommandInfo
						{
							Name = setting.Value,
							Keys = command.Keys,
							Method = command.Method
						});
					}
				}
			}

			return commands;
		}


		private IEnumerable<CommandInfo> LoadMRU(IEnumerable<CommandInfo> commands)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(CommandFactory.CollectionName);
			if (settings.Count > 0)
			{
				recentActions = settings.Get<XElement>(CommandFactory.SettingsName);
				if (recentActions != null && recentActions.HasElements)
				{
					foreach (var action in recentActions.Elements())
					{
						var command = commands
							.FirstOrDefault(c => c.Method.Name == action.Attribute("cmd")?.Value);

						if (command != null)
						{
							yield return command;
						}
					}
				}
			}
		}
	}
}
