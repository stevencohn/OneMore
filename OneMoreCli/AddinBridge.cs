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
		public static async Task<bool> TryRun(
			ICliCommand command, CliParameterSet parameters, CancellationToken cancellationToken,
			string cliSessionId = null)
		{
			var pipeName = GetPipeName();
			if (string.IsNullOrEmpty(pipeName))
			{
				return false;
			}

			NamedPipeClientStream pipe = null;
			var connected = false;
			try
			{
				pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

				// Short connect timeout: if the add-in isn't listening we fall back immediately
				await Task.Run(() => pipe.Connect(500));
				connected = true;

				var commandName = command.CommandName.EndsWith("Command", StringComparison.OrdinalIgnoreCase)
					? command.CommandName
					: $"{command.CommandName}Command";

				// Send request
				var uri = BuildUri(commandName, parameters, cliSessionId);
				var requestBytes = Encoding.UTF8.GetBytes(uri);
				await pipe.WriteAsync(requestBytes, 0, requestBytes.Length);
				await pipe.FlushAsync();

				// Read response — server writes then disconnects, so read until EOF / IOException.
				// Before the final OK/ERR/OUTPUT/CANCELLED payload, the server may interleave
				// "PROGRESS:<name>\n" lines as each section starts; print those immediately
				// instead of waiting for EOF.
				var sb = new StringBuilder();
				var pending = new StringBuilder();
				var inFinal = false;
				var cancelledByUser = false;
				var buffer = new byte[512];

				// Idle timeout, not a total-time cap: a large notebook export can legitimately
				// run for many minutes, but the add-in streams PROGRESS lines as it goes, so we
				// only give up if nothing at all arrives for this long. The deadline is reset
				// after every read below. (A single fixed cap here would expire mid-export and,
				// via the COM fallback, re-run the entire notebook - duplicating exported files.)
				var idleTimeout = TimeSpan.FromMinutes(5);
				using var timeoutCts = new CancellationTokenSource(idleTimeout);
				using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
					timeoutCts.Token, cancellationToken);
				try
				{
					int n;
					while ((n = await pipe.ReadAsync(buffer, 0, buffer.Length, linkedCts.Token)) > 0)
					{
						// data arrived - the add-in is alive and working; reset the idle deadline
						timeoutCts.CancelAfter(idleTimeout);

						var text = Encoding.UTF8.GetString(buffer, 0, n);
						if (inFinal)
						{
							sb.Append(text);
							continue;
						}

						pending.Append(text);
						int idx;
						while (!inFinal && (idx = pending.ToString().IndexOf('\n')) >= 0)
						{
							var line = pending.ToString(0, idx);

							if (line.StartsWith("PROGRESS:", StringComparison.OrdinalIgnoreCase))
							{
								pending.Remove(0, idx + 1);
								CliConsole.WriteInfo($"section: {line.Substring("PROGRESS:".Length)}");
							}
							else if (line.StartsWith("HEARTBEAT", StringComparison.OrdinalIgnoreCase))
							{
								pending.Remove(0, idx + 1);
								// idle timer already reset above on read; nothing to print
							}
							else
							{
								// final payload starts here; flush everything buffered so far
								// (not just this first line) before switching to passthrough
								sb.Append(pending.ToString());
								pending.Clear();
								inFinal = true;
							}
						}
					}

					if (!inFinal && pending.Length > 0)
					{
						sb.Append(pending.ToString());
					}
				}
				catch (IOException)
				{
					// Server disconnected after writing response — normal end of stream
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// user pressed Ctrl+C — tell the server, then give it a short grace
					// period to stop the in-flight batch and report back before we move on
					cancelledByUser = true;
					await NotifyServerCancelled(pipe, sb);
				}
				catch (OperationCanceledException)
				{
					throw new TimeoutException(
						"The add-in stopped responding (no progress for 5 minutes).");
				}

				var response = sb.ToString().Trim('\0').Trim();

				if (response.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
					throw new Exception(response.Substring(4));

				if (response.StartsWith("CANCELLED", StringComparison.OrdinalIgnoreCase))
				{
					var idx = response.IndexOf(':');
					if (idx >= 0 && idx + 1 < response.Length)
					{
						CliConsole.WriteOutput(response.Substring(idx + 1));
					}

					throw new OperationCanceledException("Cancelled by user.");
				}

				if (response.StartsWith("OUTPUT:", StringComparison.OrdinalIgnoreCase))
				{
					CliConsole.WriteOutput(response.Substring(7));
				}
				else if (cancelledByUser && !response.Equals("OK", StringComparison.OrdinalIgnoreCase))
				{
					// cancelled, and the server never clearly acknowledged completion within
					// the grace period — report cancellation rather than claiming success
					throw new OperationCanceledException("Cancelled by user.");
				}

				// "OK" or "OUTPUT:..." → success
				return true;
			}
			catch (TimeoutException) when (!connected)
			{
				// pipe.Connect timed out → add-in not running
				return false;
			}
			catch (IOException) when (!connected)
			{
				// pipe not found or broken before we could connect → add-in not running
				return false;
			}
			// Any failure AFTER a successful connection (IOException, the idle-timeout
			// TimeoutException, etc.) propagates rather than returning false: the add-in
			// already accepted and may have partly completed the command, so falling back to
			// direct COM here would re-run it from scratch and duplicate its work.
			finally
			{
				pipe?.Dispose();
			}
		}


		/// <summary>
		/// Best-effort notifies the server that the user wants to cancel by writing a short
		/// marker to the still-open pipe (the server treats any inbound byte as a cancel
		/// signal while a command is running), then waits up to 10 seconds for the server's
		/// terminal response — likely a <c>CANCELLED:</c> payload once it stops the in-flight
		/// batch — appending whatever arrives to <paramref name="sb"/>. Swallows all failures;
		/// the caller proceeds either way once this returns.
		/// </summary>
		private static async Task NotifyServerCancelled(NamedPipeClientStream pipe, StringBuilder sb)
		{
			try
			{
				var marker = Encoding.UTF8.GetBytes("CANCEL");
				await pipe.WriteAsync(marker, 0, marker.Length);
				await pipe.FlushAsync();
			}
			catch
			{
				return;
			}

			try
			{
				using var graceCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
				var buffer = new byte[512];
				int n;
				while ((n = await pipe.ReadAsync(buffer, 0, buffer.Length, graceCts.Token)) > 0)
				{
					sb.Append(Encoding.UTF8.GetString(buffer, 0, n));
				}
			}
			catch
			{
				// give up waiting for the server's acknowledgement; caller proceeds anyway
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
		/// </summary>
		public static string BuildUri(
			string commandName, CliParameterSet parameters, string cliSessionId = null)
		{
			var sb = new StringBuilder("onemorecli://");
			sb.Append(commandName);

			var first = true;
			foreach (var key in parameters.Keys)
			{
				if (parameters.TryGet<object>(key, out var value))
				{
					sb.Append(first ? '?' : '&');
					sb.Append(Uri.EscapeDataString(key));
					sb.Append('=');
					sb.Append(Uri.EscapeDataString(value?.ToString() ?? string.Empty));
					first = false;
				}
			}

			if (!string.IsNullOrEmpty(cliSessionId))
			{
				sb.Append(first ? '?' : '&');
				sb.Append("cliSessionId=");
				sb.Append(Uri.EscapeDataString(cliSessionId));
			}

			return sb.ToString();
		}
	}
}
