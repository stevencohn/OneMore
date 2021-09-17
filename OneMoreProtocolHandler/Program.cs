//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreProtocolHandler
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO.Pipes;
	using System.Text;


	class Program
	{
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";


		static void Main(string[] args)
		{
			var arg = args[0];
			if (arg == "--register")
			{
				Register();
				return;
			}

			if (arg == "--unregister")
			{
				Unregister();
				return;
			}

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
					Debug.WriteLine("pipe client connected");

					var buffer = Encoding.UTF8.GetBytes(arg);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
			}
		}


		static void Register()
		{

		}


		static void Unregister()
		{

		}
	}
}
