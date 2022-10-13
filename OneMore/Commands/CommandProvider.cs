//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Properties;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using System.Xml.Linq;


	public sealed class CommandInfo
	{
		public string Name { get; set; }
		public Keys Keys { get; set; }
		public MethodInfo Method { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}


	internal class CommandProvider
	{
		private const string CollectionName = "mru";
		private const string SettingsName = "commands";
		private const string SettingName = "command";


		public void ClearMRU()
		{
			var provider = new SettingsProvider();
			provider.RemoveCollection(CollectionName);
			provider.Save();
		}


		public XElement LoadLastAction()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(CollectionName);

			return settings.Get<XElement>(SettingsName)?
				.Elements(SettingName)
				.LastOrDefault();
		}


		public List<CommandInfo> LoadPaletteCommands()
		{
			var commands = new List<CommandInfo>();

			// heavily relies on naming convention, suffix must be "Cmd"
			var methods = typeof(AddIn).GetMethods()
				.Where(m =>
					m.Name.EndsWith("Cmd") &&
					m.GetCustomAttribute(typeof(IgnorePaletteAttribute)) == null);

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

			// load aliases
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


		public List<CommandInfo> LoadMRU(List<CommandInfo> commands)
		{
			var mru = new List<CommandInfo>();

			var settings = new SettingsProvider().GetCollection(CollectionName);
			if (settings.Count > 0)
			{
				var recentActions = settings.Get<XElement>(SettingsName);
				if (recentActions != null && recentActions.HasElements)
				{
					foreach (var action in recentActions.Elements())
					{
						var command = commands
							.FirstOrDefault(c => c.Method.Name == action.Attribute("cmd")?.Value);

						if (command != null)
						{
							commands.Remove(command);
							mru.Add(command);
						}
					}
				}
			}

			return mru;
		}


		public void SaveToMRU(Command command, params object[] args)
		{
			// ignore commands that pass the ribbon as an argument
			if (args == null || 
				args.Any(a => a != null && a.GetType().Name.Contains("ComObject")))
			{
				return;
			}

			try
			{
				var provider = new SettingsProvider();
				var settings = provider.GetCollection(CollectionName);
				var commands = settings.Get<XElement>(SettingsName);
				if (commands == null)
				{
					commands = new XElement(SettingsName);
					settings.Add(SettingsName, commands);
				}

				// "type" records the :Command inheritor class whereas
				// "cmd" records the AddInCommands xxxCmd method name
				var trace = new System.Diagnostics.StackTrace();
				var runner = trace.GetFrames()
					.Where(f => f.GetMethod().Name.EndsWith("Cmd"))
					.Select(f => f.GetMethod().Name)
					.FirstOrDefault();

				var element = commands.Elements()
					.FirstOrDefault(e => e.Attribute("cmd").Value == runner);

				if (element != null)
				{
					element.Remove();
					commands.Add(element);
				}
				else
				{
					var arguments = new XElement("arguments");
					args.Where(a => a != null).ForEach(a =>
					{
						arguments.Add(new XElement("arg",
							new XAttribute("type", a.GetType().FullName),
							new XAttribute("value", a.ToString())
							));
					});

					var setting = new XElement(SettingName,
						new XAttribute("type", command.GetType().FullName),
						new XAttribute("cmd", runner),
						arguments
						);

					var replay = command.GetReplayArguments();
					if (replay != null)
					{
						setting.Add(new XElement("context", replay));
					}

					commands.Add(setting);
					while (commands.Elements().Count() > 5)
					{
						commands.Elements().First().Remove();
					}
				}

				provider.SetCollection(settings);
				provider.Save();
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error recording MRU", exc);
			}
		}
	}
}
