//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreService
{
	using Microsoft.Win32;
	using River.OneMoreAddIn;
	using System;
	using System.IO.Pipes;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	internal class CommandService
	{
		private const int MaxBytes = 512;
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

		private readonly ILogger logger;
		private readonly string pipeName;


		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool ImpersonateNamedPipeClient(IntPtr hNamedPipe);


		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool RevertToSelf();


		public CommandService()
		{
			logger = Logger.Current;

			using var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
			if (key != null)
			{
				// get default value string
				pipeName = $"{(string)key.GetValue(string.Empty)}_winsvc";
			}
			else
			{
				logger.WriteLine($"error reading pipe name from {KeyPath}");
			}
		}


		public void Start()
		{
			if (string.IsNullOrEmpty(pipeName))
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
						ImpersonateNamedPipeClient(server.SafePipeHandle.DangerousGetHandle());
						//client.ImpersonationLevel = TokenImpersonationLevel.Impersonation


						logger.WriteLine($"service pipe started {pipeName}");
						await server.WaitForConnectionAsync();

						var buffer = new byte[MaxBytes];
						await server.ReadAsync(buffer, 0, MaxBytes);
						data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
						logger.WriteLine($"service pipe received [{data}]");

						// clean up server so we can create a new one for next connection
						server.Disconnect();
						server.Close();

						if (!string.IsNullOrEmpty(data)) // && data.StartsWith(Protocol))
						{
							// isolate work into its own thread so any uncaught exceptions
							// won't tip over the service thread...

							var worker = new Thread(async (d) => await InvokeCommand(data));
							worker.SetApartmentState(ApartmentState.STA);
							worker.IsBackground = true;
							worker.Start(Thread.CurrentPrincipal);

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
			});

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
				pipeName, PipeDirection.In, 1,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
				MaxBytes, MaxBytes, security);
		}


		private async Task InvokeCommand(string data)
		{
			// CommandName/arg1/arg2/arg3/
			var parts = data.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			var action = parts[0];
			var arguments = parts.Skip(1).ToArray();

			for (int i = 0; i < arguments.Length; i++)
			{
				arguments[i] = HttpUtility.UrlDecode(arguments[i]);
			}

			logger.WriteLine($"..invoking {action}({string.Join(", ", arguments)})");

			/*
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
			*/
		}
	}
}
