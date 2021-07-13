//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Instantiates and runs OneMore commands
	/// </summary>
	internal class CommandFactory
	{
		private readonly ILogger logger;
		private readonly IRibbonUI ribbon;
		private readonly IWin32Window owner;
		private readonly List<IDisposable> trash;


		/// <summary>
		/// Initialize a new factory with the given services
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <param name="ribbon">The OneNote ribbon</param>
		/// <param name="trash">A colleciton of IDisposables for cleanup on shutdown</param>
		/// <param name="owner">The owner window</param>
		public CommandFactory(
			ILogger logger, IRibbonUI ribbon, List<IDisposable> trash, IWin32Window owner)
		{
			this.logger = logger;
			this.ribbon = ribbon;
			this.owner = owner;
			this.trash = trash;
		}


		/// <summary>
		/// Instantiates and executes the specified command with optional arguments.
		/// Provides catch-all exception handling - logging and a generic message to the user
		/// </summary>
		/// <typeparam name="T">The command type</typeparam>
		/// <param name="args">The argument list</param>
		public async Task Run<T>(params object[] args) where T : Command, new()
		{
			await Run(new T(), args);
		}


		private async Task Run(Command command, params object[] args)
		{
			var type = command.GetType();
			logger.Start($"Running command {type.Name}");

			command.SetLogger(logger);
			command.SetRibbon(ribbon);
			command.SetOwner(owner);
			command.SetTrash(trash);

			try
			{
				await command.Execute(args);

				RecordLastAction(type, args);

				logger.End();
			}
			catch (Exception exc)
			{
				// catch-all exception hander

				var msg = string.Format(Resx.Command_Error, type.Name);
				logger.End();
				logger.WriteLine(msg);
				logger.WriteLine(exc);
				logger.WriteLine();

				UIHelper.ShowError(string.Format(Resx.Command_ErrorMsg, msg));
			}
		}



		private void RecordLastAction(Type type, params object[] args)
		{
			// ignore commands that pass the ribbon as an argument
			if (args.Any(a => a != null && a.GetType().Name.Contains("ComObject")))
			{
				return;
			}

			try
			{
				// SettingsProvider will only return an XElement if it has child elements so
				// wrap the argument list in an <arguments> element, which itself may be empty
				var arguments = new XElement("arguments");

				foreach (var arg in args)
				{
					if (arg != null)
					{
						arguments.Add(new XElement("arg",
							new XAttribute("type", arg.GetType().FullName),
							new XAttribute("value", arg.ToString())
							));
					}
				}

				var setting = new XElement("command",
					new XAttribute("type", type.FullName),
					arguments
					);

				var provider = new SettingsProvider();
				var settings = provider.GetCollection("lastAction");

				// setting name should equate to the XML root element name here
				// the XML is not wrapped with an extra parent, so no worries
				settings.Add("command", setting);

				provider.SetCollection(settings);
				provider.Save();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error recording last action", exc);
			}
		}


		public async Task ReplayLastAction()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("lastAction");
			var action = settings.Get<XElement>("command");
			if (action != null)
			{
				try
				{
					var command = (Command)Activator.CreateInstance(
						Type.GetType(action.Attribute("type").Value)
						);

					var args = new List<object>();
					foreach (var arg in action.Element("arguments").Elements("arg"))
					{
						var type = Type.GetType(arg.Attribute("type").Value);
						if (type.IsEnum)
						{
							args.Add(Enum.Parse(type, arg.Attribute("value").Value));
						}
						else
						{
							args.Add(Convert.ChangeType(
								arg.Attribute("value").Value,
								Type.GetType(arg.Attribute("type").Value)
								));
						}
					}

					await Run(command, args.ToArray());
				}
				catch (Exception exc)
				{
					logger.WriteLine("error parsing last action", exc);
				}
			}
			else
			{
				await Task.Yield();
			}
		}
	}
}
