//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Base class for every OneMore command, providing common services and
	/// requiring implementation of an Execute entry point
	/// </summary>
	internal abstract class Command
	{

		// commands are injected with logger, ribbon, owner window, and the tash collector...

		protected ILogger logger;
		protected IRibbonUI ribbon;
		protected IWin32Window owner;
		protected CommandFactory factory;
		protected List<IDisposable> trash;


		/// <summary>
		/// Set to true by the command if the user cancelled the action; prevents replay from
		/// recording the action as valid
		/// </summary>
		public bool IsCancelled
		{
			get;
			protected set;
		}


		/*
		 * Inheritors MUST override Execute...
		 */

		/// <summary>
		/// The entry point for the command which may take zero or more arguments
		/// </summary>
		/// <param name="args">The argument list</param>
		public virtual async Task Execute(params object[] args)
		{
			await Task.Yield();
		}


		/*
		 * Inheritors may override GetReplayArguments...
		 */

		/// <summary>
		/// Called by CommandFactory to request any contextual arguments to be used if this
		/// command is immediately replayed; they will be stored in the setting file in the
		/// mru collection.
		/// </summary>
		/// <returns>
		/// An XElement describing the replay arguments, customized for this command. There is no
		/// standard schema for this for all commands; instead, each command is responsible for
		/// generating and consuming its own replay information.
		/// </returns>
		public virtual XElement GetReplayArguments()
		{
			return null;
		}


		// Setters used by CommandFactory...

		public Command SetFactory(CommandFactory value)
		{
			factory = value;
			return this;
		}

		public Command SetLogger(ILogger value)
		{
			logger = value;
			return this;
		}


		public Command SetRibbon(IRibbonUI value)
		{
			ribbon = value;
			return this;
		}


		public Command SetOwner(IWin32Window value)
		{
			owner = value;
			return this;
		}


		public Command SetTrash(List<IDisposable> value)
		{
			trash = value;
			return this;
		}


		// MessageBox helpers...

		protected void ShowError(string message)
		{
			UI.MoreMessageBox.ShowError(owner, message);
		}


		protected void ShowInfo(string message)
		{
			UI.MoreMessageBox.Show(owner, message);
		}


		protected void ShowMessage(string message)
		{
			var box = new UI.MoreMessageBox();
			box.SetButtons(MessageBoxButtons.OK);
			box.SetIcon(MessageBoxIcon.None);

			UI.MoreMessageBox.Show(owner, message);
		}
	}
}
