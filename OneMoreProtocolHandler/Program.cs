//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreProtocolHandler
{
	using Microsoft.Win32;
	using System;
	using System.IO.Pipes;
	using System.Text;


	class Program
	{
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";
		private static Logger logger;


		static void Main(string[] args)
		{
			if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				// nothing to do
				return;
			}

			logger = new Logger("OneMoreProtocolHandler");

			SendCommand(args[0]);
		}


		static void SendCommand(string arg)
		{
			try
			{
				string pipeName = null;
				var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
				if (key != null)
				{
					// get default value string
					pipeName = (string)key.GetValue(string.Empty);
				}

				if (string.IsNullOrEmpty(pipeName))
				{
					// problem!
					return;
				}

				using (var client = new NamedPipeClientStream(".",
					pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
				{
					// being an event-driven program to bounce messages back to OneMore
					// set timeout because we shouldn't need to wait for server ever!
					client.Connect(500);
					logger.WriteLine("pipe client connected");
					logger.WriteLine($"writing '{arg}'");

					var buffer = Encoding.UTF8.GetBytes(arg);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error writing '{arg}'");
				logger.WriteLine(exc);
			}
		}
	}
}
