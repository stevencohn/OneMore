//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Microsoft.Office.Core;


	// commands are injected with logger, ribbon, and the tash collector...


	public interface ICommand
	{
	}


	internal abstract class Command : ICommand
	{
		protected ILogger logger;
		protected IRibbonUI ribbon;
		protected IWin32Window owner;
		protected List<IDisposable> trash;

		public ILogger Logger { set => logger = value; }
		public IRibbonUI Ribbon { set => ribbon = value; }
		public IWin32Window Owner { set => owner = value; }
		public List<IDisposable> Trash { set => trash = value; }
	}


	internal class CommandFactory
	{
		private ILogger logger;
		private IRibbonUI ribbon;
		private IWin32Window owner;
		private List<IDisposable> trash;

		public CommandFactory (ILogger logger, IRibbonUI ribbon, List<IDisposable> trash, IWin32Window owner)
		{
			this.logger = logger;
			this.ribbon = ribbon;
			this.owner = owner;
			this.trash = trash;
		}

		public T GetCommand<T> () where T : Command, new()
		{
			var command = new T()
			{
				Logger = logger,
				Ribbon = ribbon,
				Owner = owner,
				Trash = trash
			};

			return command;
		}
	}
}
