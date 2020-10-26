//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
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
		public void Run<T>(params object[] args) where T : Command, new()
		{
			try
			{
				logger.Start($"Running command {typeof(T).Name}");

				var command = new T();
				command.SetLogger(logger);
				command.SetRibbon(ribbon);
				command.SetOwner(owner);
				command.SetTrash(trash);

				command.Execute(args);

				logger.End();
			}
			catch (Exception exc)
			{
				// catch-all exception hander

				var msg = string.Format(Resx.Command_Error, typeof(T).Name);
				logger.End();
				logger.WriteLine(msg);
				logger.WriteLine(exc);
				logger.WriteLine();

				UIHelper.ShowError(string.Format(Resx.Command_ErrorMsg, msg));
			}
		}
	}
}
