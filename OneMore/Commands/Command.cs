//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;


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
		protected List<IDisposable> trash;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Settings used by CommandFactory

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


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Must override by inheritors...

		/// <summary>
		/// The entry point for the command which may take zero or more arguments
		/// </summary>
		/// <param name="args">The argument list</param>
		public virtual void Execute(params object[] args)
		{
		}
	}
}
