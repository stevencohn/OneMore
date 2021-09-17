//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO.Pipes;
	using System.Text;


	internal class CommandService : Loggable
	{
		private const string PipeName = "River.OneMore";

		public CommandService()
			: base()
		{
			//Computer\HKEY_CLASSES_ROOT\River.OneMoreAddIn\CLSID
		}


		public void Startup()
		{
			var server = new NamedPipeServerStream(PipeName,
				PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			server.BeginWaitForConnection(new AsyncCallback(ConnectionCallBack), server);

			logger.WriteLine("command server started");
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
				var data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim();
				logger.WriteLine($"pipe received [{data}]");

				// Pass message back to calling form
				//PipeMessage.Invoke(data);

				// Kill original sever and create new wait server
				server.Close();
				server.Dispose();

				server = new NamedPipeServerStream(PipeName,
					PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				// Recursively wait for the connection again and again....
				server.BeginWaitForConnection(new AsyncCallback(ConnectionCallBack), server);
			}
			catch (Exception exc)
			{
				logger.WriteLine("pipe exception", exc);
			}
		}
	}
}
