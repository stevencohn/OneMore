﻿//************************************************************************************************
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

		static void Main(string[] args)
		{
			try
			{
				string pipeName = null;
				var key = Registry.ClassesRoot.OpenSubKey(@"River.OneMoreAddIn\CLSID", false);
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

					var buffer = Encoding.UTF8.GetBytes(args[0]);
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
	}
}
