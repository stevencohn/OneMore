//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125    // commented code                     
#pragma warning disable S4663   // empty commented                     

namespace OneMoreService
{
	using River.OneMoreAddIn;
	using Microsoft.Win32;
	using System;
	using System.Collections;
	using System.IO.Pipes;
	using System.Linq;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Listener for commands sent from OneMoreProtocolhandler through named pipe.
	/// </summary>
	internal class ServiceListener : River.OneMoreAddIn.Loggable
	{
		private const int MaxBytes = 512;
		public const string PipeKey = "-service";
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

		private readonly string pipeName;


		public ServiceListener()
			: base()
		{
			using var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
			if (key != null)
			{
				// get default value string
				pipeName = (string)key.GetValue(string.Empty) + PipeKey;
				logger.WriteLine($"service listener pipe name is {pipeName}");
			}
			else
			{
				logger.WriteLine($"error reading pipe name from {KeyPath}");
			}
		}



		public void Startup()
		{
			if (string.IsNullOrEmpty(pipeName))
			{
				logger.WriteLine("service listener not started, missing pipe name");
				return;
			}

			logger.WriteLine("starting service listener");

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
						logger.WriteLine($"listener pipe started {pipeName}"); //
						await server.WaitForConnectionAsync();

						var buffer = new byte[MaxBytes];
						await server.ReadAsync(buffer, 0, MaxBytes);
						data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
						logger.WriteLine($"pipe received [{data}]"); //

						if (!string.IsNullOrEmpty(data))
						{
							server.RunAsClient(async () =>
							{
								await RunAsImpersonated(data);
							});

							/*
							// isolate work into its own thread so any uncaught exceptions
							// won't tip over the service thread...

							var worker = new Thread(async (d) => await InvokeCommand(data));
							worker.SetApartmentState(ApartmentState.STA);
							worker.IsBackground = true;
							worker.Start();

							errors = 0;
							 */
						}

						// clean up server so we can create a new one for next connection
						server.Disconnect();
						server.Close();
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
			var security = new PipeSecurity();

			security.AddAccessRule(new PipeAccessRule(
				new SecurityIdentifier(WellKnownSidType.WorldSid, null),
				PipeAccessRights.ReadWrite, AccessControlType.Allow));

			var hostName = System.Net.Dns.GetHostName();
			logger.WriteLine($"starting pipe \\\\{hostName}\\pipe\\{pipeName}");

			return new NamedPipeServerStream(
				pipeName, PipeDirection.In, 1,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
				MaxBytes, MaxBytes, security);
		}


		private async Task RunAsImpersonated(string data)
		{
			if (!ServiceNative.OpenThreadToken(
				ServiceNative.GetCurrentThread(),
				ServiceNative.TOKEN_ALL_ACCESS, false, out var tokenHandle))
			{
				logger.WriteLine("!!! failed to open thread token");
				return;
			}

			IntPtr token = IntPtr.Zero;

			//if (!Native.DuplicateTokenEx(
			//	tokenHandle, Native.TOKEN_ALL_ACCESS, IntPtr.Zero,
			//	Native.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
			//	Native.TOKEN_TYPE.TokenPrimary, out IntPtr token))
			if (ServiceNative.DuplicateToken(tokenHandle, 2, ref token) == 0)
			{
				logger.WriteLine("!!! failed to duplicate impersonation token");
				return;
			}

			var user = new WindowsIdentity(token);
			using var context = user.Impersonate();
			if (context == null)
			{
				logger.WriteLine("!!! failed to get impersonation context");
				return;
			}

			logger.WriteLine($"... impersonating user {user.Name}");
			logger.WriteLine($"... env userprofile {Environment.GetEnvironmentVariable("USERPROFILE")}");
			logger.WriteLine($"... fol userprofile {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
			logger.WriteLine();
			foreach (var item in Environment.GetEnvironmentVariables()
				.Cast<DictionaryEntry>()
				.OrderBy(i => i.Key))
			{
				logger.WriteLine($"... env {item.Key} = [{item.Value}]");
			}

			logger.WriteLine();
			foreach (var item in Enum.GetValues(typeof(Environment.SpecialFolder))
				.Cast<Environment.SpecialFolder>().OrderBy(e => e.ToString()))
			{
				logger.WriteLine($"... folder {item} = [{Environment.GetFolderPath(item)}]");
			}

			logger.WriteLine();

			// user info...
			/*
			IntPtr detailPtr;
			if (!ServiceNative.NetUserGetInfo(null, user.Name, 11, out detailPtr))
			{
				Console.WriteLine($"!!! NetUserGetInfo error: {Marshal.GetLastWin32Error()}");
			}
			else if (detailPtr != IntPtr.Zero)
			{
				var detail = (ServiceNative.USER_INFO_11)Marshal.PtrToStructure(detailPtr, typeof(ServiceNative.USER_INFO_11));
				Console.WriteLine($"detail.home_dir=[{detail.usri11_home_dir}]");
			}
			else
			{
				Console.WriteLine("NetUserGetInfo empty");
			}
			*/

			// load user profile...
			/*
			var info = new ServiceNative.ProfileInfo
			{
				lpUserName = user.Name,
				lpProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				dwFlags = 1
			};

			info.dwSize = Marshal.SizeOf(info);

			if (!ServiceNative.LoadUserProfile(token, ref info))
			{
				Console.WriteLine($"!!! LoadUserProfile error code: {Marshal.GetLastWin32Error()}");
				// throw new Win32Exception(Marshal.GetLastWin32Error())
				return;
			}

			if (info.hProfile == IntPtr.Zero)
			{
				Console.WriteLine($"!!! LoadUserProfile handle not loaded, error code: {Marshal.GetLastWin32Error()}");
				// throw new Win32Exception(Marshal.GetLastWin32Error())
				return;
			}
			*/

			logger.WriteLine($"invoking as {user.Name}");
			await InvokeCommand(data);
		}


		private async Task InvokeCommand(string data)
		{
			logger.WriteLine($"invoking [{data}]");

			/*
			using var one = new OneNote();
			var root = await one.GetNotebooks();
			if (root == null)
			{
				logger.WriteLine("error found no notebooks");
				return;
			}

			var ns = one.GetNamespace(root);
			var notebooks = root.Elements(ns + "Notebook");
			if (notebooks.Any())
			{
				foreach (var notebook in notebooks)
				{
					logger.WriteLine($"notebook - {notebook.Attribute("name").Value}");
				}
			}
			*/

			new River.OneMoreAddIn.Commands.HashtagService().Startup();

			await Task.Yield();

			/*
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
			*/
		}
	}
}
