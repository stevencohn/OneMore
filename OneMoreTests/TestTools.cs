//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;



	[TestClass]
	public class TestTools
	{
		private CommandFactory factory;
		private List<IDisposable> trash;
		private ILogger logger;


		[TestInitialize]
		public void Initialize()
		{
			ApplicationFactory.RegisterApplication(typeof(MockApplication));

			Logger.SetDesignMode(true);
			logger = Logger.Current;
			logger.WriteLine($"logging to {logger.LogPath}");

			trash = new List<IDisposable>();

			var app = new MockApplication();

			factory = new CommandFactory(Logger.Current, null, trash,
				new Win32WindowHandle(new IntPtr((long)app.Windows.CurrentWindow.WindowHandle)))
			{
				Runtime = false
			};
		}


		[TestMethod]
		public async Task DiagnosticsCommandTest()
		{
			await factory.Run<DiagnosticsCommand>();
			Assert.IsTrue(true);
		}
	}
}
