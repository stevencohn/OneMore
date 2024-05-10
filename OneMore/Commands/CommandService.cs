﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Win32;
	using System;
	using System.IO.Pipes;
	using System.Linq;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;


	/// <summary>
	/// Listener for commands sent from OneMoreProtocolhandler through named pipe.
	/// </summary>
	internal class CommandService : Loggable
	{
		private const int MaxBytes = 512;
		private const string Protocol = "onemore://";
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

		private readonly string pipe;
		private readonly CommandFactory factory;


		public CommandService(CommandFactory factory)
			: base()
		{
			using var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
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

			logger.WriteLine("starting command service");

			var thread = new Thread(async () =>
			{
				// 'errors' allows repeated consecutive exceptions but limits that to 5 so we
				// don't fall into an infinite loop. If it somehow miraculously recovers then
				// errors is reset back to zero and normal processing continues...

				var errors = 0;
				while (errors < 5)
				{
					try
					{
						string data = null;

						using var server = CreateSecuredPipe();
						//logger.WriteLine($"command pipe started {pipe}");
						await server.WaitForConnectionAsync();

						var buffer = new byte[MaxBytes];
						await server.ReadAsync(buffer, 0, MaxBytes);
						data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
						//logger.WriteLine($"pipe received [{data}]");

						// clean up server so we can create a new one for next connection
						server.Disconnect();
						server.Close();

						if (!string.IsNullOrEmpty(data) && data.StartsWith(Protocol))
						{
							// isolate work into its own thread so any uncaught exceptions
							// won't tip over the service thread...

							var worker = new Thread(async (d) => await InvokeCommand(data))
							{
								Name = $"{nameof(CommandService)}WorkerThread"
							};

							worker.SetApartmentState(ApartmentState.STA);
							worker.IsBackground = true;
							worker.Start();

							errors = 0;
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine($"pipe exception {errors}", exc);
						errors++;
					}
				}

				logger.WriteLine("pipe no longer listening; check for exceptions above");
			})
			{
				Name = $"{nameof(CommandService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();
		}


		private NamedPipeServerStream CreateSecuredPipe()
		{
			var user = WindowsIdentity.GetCurrent().User;
			var security = new PipeSecurity();

			security.AddAccessRule(new PipeAccessRule(
				user, PipeAccessRights.FullControl, AccessControlType.Allow));

			security.SetOwner(user);
			security.SetGroup(user);

			return new NamedPipeServerStream(
				pipe, PipeDirection.In, 1,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
				MaxBytes, MaxBytes, security);
		}


		private async Task InvokeCommand(string data)
		{
			// data specifies command as onemore protocol such as
			// onemore://DoitCommand/arg1/arg2/arg2/

			var parts = data.Substring(Protocol.Length)
				.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			var action = parts[0];
			var arguments = parts.Skip(1).ToArray();
			for (int i = 0; i < arguments.Length; i++)
			{
				arguments[i] = HttpUtility.UrlDecode(arguments[i]);
			}

			//logger.WriteLine($"..invoking {action}({string.Join(", ", arguments)})");

			try
			{
				await factory.Invoke(action, arguments)
					.ContinueWith((t) =>
					{
						//logger.WriteLine($"continuation status is {t.Status}");
						if (t.IsFaulted)
						{
							logger.WriteLine("continuation fault", t.Exception);
						}
					});
			}
			catch (Exception exc)
			{
				logger.WriteLine("error invoking command", exc);
			}
		}
	}
}
