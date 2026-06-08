//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using Microsoft.Win32;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.IO;
	using System.IO.Pipes;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Named-pipe client that delegates CLI command execution to the running OneMore add-in.
	/// When OneNote is open the add-in has a live, properly initialised COM connection;
	/// routing through it avoids the blank-second-instance problem caused by calling
	/// <c>new Application()</c> from an external process against Microsoft 365 (C2R) OneNote.
	/// </summary>
	internal static class AddinBridge
	{
		private const string PipeRegistryPath = @"River.OneMoreAddIn\CLSID";


		/// <summary>
		/// Tries to run the command by sending a <c>onemorecli://</c> request to the named pipe
		/// exposed by the running OneMore add-in. Returns <c>true</c> on success, <c>false</c>
		/// when the pipe is unavailable (add-in / OneNote not running) so the caller can fall back
		/// to direct COM activation.
		/// </summary>
		public static async Task<bool> TryRun(ICliCommand command, CliParameterSet parameters)
		{
			var pipeName = GetPipeName();
			if (string.IsNullOrEmpty(pipeName))
			{
				return false;
			}

			NamedPipeClientStream pipe = null;
			try
			{
				pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

				// Short connect timeout: if the add-in isn't listening we fall back immediately
				await Task.Run(() => pipe.Connect(500));

				var commandName = command.CommandName.EndsWith("Command", StringComparison.OrdinalIgnoreCase)
					? command.CommandName
					: $"{command.CommandName}Command";

				// Send request
				var uri = BuildUri(commandName, parameters);
				var requestBytes = Encoding.UTF8.GetBytes(uri);
				await pipe.WriteAsync(requestBytes, 0, requestBytes.Length);
				await pipe.FlushAsync();

				// Read response — server writes then disconnects, so read until EOF / IOException
				var sb = new StringBuilder();
				var buffer = new byte[512];
				using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
				try
				{
					int n;
					while ((n = await pipe.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
					{
						sb.Append(Encoding.UTF8.GetString(buffer, 0, n));
					}
				}
				catch (IOException)
				{
					// Server disconnected after writing response — normal end of stream
				}
				catch (OperationCanceledException)
				{
					throw new TimeoutException("The add-in did not respond within 5 minutes.");
				}

				var response = sb.ToString().Trim('\0').Trim();

				if (response.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
					throw new Exception(response.Substring(4));

				if (response.StartsWith("OUTPUT:", StringComparison.OrdinalIgnoreCase))
					Console.Write(response.Substring(7));

				// "OK" or "OUTPUT:..." → success
				return true;
			}
			catch (TimeoutException)
			{
				// pipe.Connect timed out → add-in not running
				return false;
			}
			catch (IOException)
			{
				// pipe not found or broken before we could use it → add-in not running
				return false;
			}
			finally
			{
				pipe?.Dispose();
			}
		}


		/// <summary>
		/// Reads the named pipe name from the registry key written by the OneMore installer.
		/// Returns <c>null</c> if the key is absent (add-in not installed or registry inaccessible).
		/// </summary>
		public static string GetPipeName()
		{
			try
			{
				using var key = Registry.ClassesRoot.OpenSubKey(PipeRegistryPath, false);
				return key?.GetValue(string.Empty) as string;
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		/// Serialises <paramref name="commandName"/> and <paramref name="parameters"/> into a
		/// <c>onemorecli://CommandName?key=value&amp;…</c> URI string for the add-in pipe.
		/// Values are percent-encoded with <see cref="Uri.EscapeDataString"/>.
		/// The injected <c>pageId</c> key is excluded — the server side resolves pages itself.
		/// </summary>
		public static string BuildUri(string commandName, CliParameterSet parameters)
		{
			var sb = new StringBuilder("onemorecli://");
			sb.Append(commandName);

			var first = true;
			foreach (var key in parameters.Keys)
			{
				// pageId is resolved server-side; don't forward it
				if (key.Equals("pageId", StringComparison.OrdinalIgnoreCase))
					continue;

				if (parameters.TryGet<object>(key, out var value))
				{
					sb.Append(first ? '?' : '&');
					sb.Append(Uri.EscapeDataString(key));
					sb.Append('=');
					sb.Append(Uri.EscapeDataString(value?.ToString() ?? string.Empty));
					first = false;
				}
			}

			return sb.ToString();
		}
	}
}
