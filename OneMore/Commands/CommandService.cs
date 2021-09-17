//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Win32;
	using System;
	using System.IO.Pipes;
	using System.Linq;
	using System.Text;
	using System.Threading;


	/// <summary>
	/// Listener for commands sent from OneMoreProtocolhandler through named pipe.
	/// </summary>
	internal class CommandService : Loggable
	{
		private const string Protocol = "onemore://";
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

		private readonly string pipe;
		private readonly CommandFactory factory;


		public CommandService(CommandFactory factory)
			: base()
		{
			var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
			if (key != null)
			{
				// get default value string
				pipe = (string)key.GetValue(string.Empty);
			}
			else
			{
				logger.WriteLine($"error reading pipe name from {KeyPath}");
			}

			this.factory = factory;
		}



		public void Startup()
		{
			if (string.IsNullOrEmpty(pipe))
			{
				logger.WriteLine("command service not started, missing pipe name");
				return;
			}

			var thread = new Thread(async () =>
			{
				while (true)
				{
					try
					{
						string data = null;

						using (var server = new NamedPipeServerStream(pipe,
							PipeDirection.In, 1, PipeTransmissionMode.Byte,
							PipeOptions.Asynchronous))
						{
							//logger.WriteLine($"command pipe started {pipe}");
							await server.WaitForConnectionAsync();

							var buffer = new byte[255];
							await server.ReadAsync(buffer, 0, 255);
							data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
							//logger.WriteLine($"pipe received [{data}]");

							// clean up server so we can create a new one for next connection
							server.Disconnect();
							server.Close();
						}

						if (!string.IsNullOrEmpty(data) && data.StartsWith(Protocol))
						{
							// data specifies command as onemore protocol such as
							// onemore://DoitCommand/arg1/arg2/arg2/

							var parts = data.Substring(Protocol.Length)
								.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

							var action = parts[0];
							var arguments = parts.Skip(1).ToArray();

							//logger.WriteLine($"invoking {action} with {arguments.Length} arguments");
							await factory.Invoke(action, arguments);
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("pipe exception", exc);
					}
				}
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();
		}
	}
}
