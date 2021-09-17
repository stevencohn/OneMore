//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Win32;
	using System;
	using System.IO.Pipes;
	using System.Text;


	internal class CommandService : Loggable
	{
		private readonly string pipeName;


		public CommandService()
			: base()
		{
			var key = Registry.ClassesRoot.OpenSubKey(@"River.OneMoreAddIn\CLSID", false);
			if (key != null)
			{
				// get default value string
				pipeName = (string)key.GetValue(string.Empty);
			}
		}


		public void Startup()
		{
			var server = new NamedPipeServerStream(pipeName,
				PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			server.BeginWaitForConnection(new AsyncCallback(ConnectionCallBack), server);

			logger.WriteLine($"command server started on pipe {pipeName}");
		}


		private void ConnectionCallBack(IAsyncResult result)
		{
			try
			{
				var server = (NamedPipeServerStream)result.AsyncState;
				server.EndWaitForConnection(result);

				// read the incoming message
				var buffer = new byte[255];
				server.Read(buffer, 0, 255);

				var data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
				logger.WriteLine($"pipe received [{data}]");

				// Pass message back to calling form
				//PipeMessage.Invoke(data);

				// cleanup and destroy server so we can create a new one for next connection
				server.Close();
				server.Dispose();

				// create new server and wait for next connection
				server = new NamedPipeServerStream(pipeName,
					PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				server.BeginWaitForConnection(new AsyncCallback(ConnectionCallBack), server);
			}
			catch (Exception exc)
			{
				logger.WriteLine("pipe exception", exc);
			}
		}
	}
}
