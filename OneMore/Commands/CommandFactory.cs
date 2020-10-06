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

		public void SetLogger(ILogger value)
		{
			logger = value;
		}

		public void SetRibbon(IRibbonUI value)
		{
			ribbon = value;
		}

		public void SetOwner(IWin32Window value)
		{
			owner = value;
		}

		public void SetTrash(List<IDisposable> value)
		{
			trash = value;
		}
	}


	internal class CommandFactory
	{
		private readonly ILogger logger;
		private readonly IRibbonUI ribbon;
		private readonly IWin32Window owner;
		private readonly List<IDisposable> trash;

		public CommandFactory (ILogger logger, IRibbonUI ribbon, List<IDisposable> trash, IWin32Window owner)
		{
			this.logger = logger;
			this.ribbon = ribbon;
			this.owner = owner;
			this.trash = trash;
		}

		public T GetCommand<T> () where T : Command, new()
		{
			var command = new T();
			command.SetLogger(logger);
			command.SetRibbon(ribbon);
			command.SetOwner(owner);
			command.SetTrash(trash);

			return command;
		}
	}
}
