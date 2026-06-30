//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Win32;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Pipes;
	using System.Linq;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;


	/// <summary>
	/// Listener for commands sent from OneMoreProtocolhandler through named pipe.
	/// Also handles CLI commands sent from OneMoreCli.exe via the same pipe using
	/// the <c>onemorecli://</c> protocol prefix, returning a response to the caller.
	/// </summary>
	internal class CommandService : Loggable
	{
		private const int MaxBytes = 4096;
		private const int ReadTimeoutSeconds = 5;
		private const string Protocol = "onemore://";
		private const string CliProtocol = "onemorecli://";
		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

		// action flows into Type.GetType, which accepts assembly-qualified names
		// ("Foo, SomeAssembly") and nested-type syntax ("Outer+Inner"); require a
		// plain identifier so neither can be smuggled in via the URL
		private static readonly Regex ActionPattern =
			new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

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
						var bytesRead = 0;

						// A client that connects but never writes would wedge the listener
						// forever; the read timeout unblocks it and lets the loop continue.
						using var cts = new CancellationTokenSource(
							TimeSpan.FromSeconds(ReadTimeoutSeconds));
						try
						{
							bytesRead = await server.ReadAsync(buffer, 0, MaxBytes, cts.Token);
						}
						catch (OperationCanceledException)
						{
							logger.WriteLine("pipe read timed out");
							continue;
						}

						data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim((char)0);
						//logger.WriteLine($"pipe received [{data}]");

						if (!string.IsNullOrEmpty(data) && data.StartsWith(CliProtocol))
						{
							// CLI request: process synchronously so we can write the response
							// back before disconnecting. The server pipe stays open until the
							// response is flushed and drained.
							errors = 0;
							try
							{
								await InvokeCliCommand(data, server);
							}
							catch (Exception exc)
							{
								logger.WriteLine("error handling CLI command", exc);
								try { await WriteCliResponse(server, $"ERR:{exc.Message}"); }
								catch { /* best effort */ }
							}

							server.Disconnect();
							server.Close();
						}
						else
						{
							// Existing onemore:// protocol: disconnect immediately then fire
							// a worker thread (fire-and-forget, no response).
							server.Disconnect();
							server.Close();

							if (!string.IsNullOrEmpty(data) && data.StartsWith(Protocol))
							{
								// isolate work into its own thread so any uncaught exceptions
								// won't tip over the service thread...

								var worker = new Thread(async () => await InvokeCommand(data))
								{
									Name = $"{nameof(CommandService)}WorkerThread"
								};

								worker.SetApartmentState(ApartmentState.STA);
								worker.IsBackground = true;
								worker.Start();

								errors = 0;
							}
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

			// InOut so CLI callers (PipeDirection.InOut) can read the response.
			// The existing OneMoreProtocolHandler client uses PipeDirection.Out and
			// never reads, so the direction change is backward-compatible.
			return new NamedPipeServerStream(
				pipe, PipeDirection.InOut, 1,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
				MaxBytes, MaxBytes, security);
		}


		private async Task InvokeCommand(string data)
		{
			// data specifies command as onemore protocol such as
			// onemore://DoitCommand/arg1/arg2/arg2/

			var parts = data.Substring(Protocol.Length)
				.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 0)
			{
				return;
			}

			var action = parts[0];
			if (!ActionPattern.IsMatch(action))
			{
				logger.WriteLine($"rejected invalid command name '{action}'");
				return;
			}

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


		/// <summary>
		/// Handles a <c>onemorecli://CommandName?param1=value1&amp;param2=value2</c> request
		/// from OneMoreCli.exe. Resolves pages via <see cref="OneNote.FindPagesByPath"/> for
		/// <see cref="ICliPageCommand"/> implementations, then runs the command once per page.
		/// Writes <c>OK</c> or <c>ERR:message</c> back through <paramref name="pipe"/> before
		/// returning.
		/// </summary>
		private async Task InvokeCliCommand(string data, NamedPipeServerStream pipe)
		{
			// Strip protocol prefix and split on '?'
			var body = data.Substring(CliProtocol.Length);
			var qIndex = body.IndexOf('?');
			var commandName = qIndex >= 0 ? body.Substring(0, qIndex) : body;
			var queryString = qIndex >= 0 ? body.Substring(qIndex + 1) : string.Empty;

			if (!ActionPattern.IsMatch(commandName))
			{
				await WriteCliResponse(pipe, $"ERR:Invalid command name '{commandName}'");
				return;
			}

			var typeName = $"River.OneMoreAddIn.Commands.{commandName}";
			var commandType = Type.GetType(typeName, false);
			if (commandType == null)
			{
				await WriteCliResponse(pipe, $"ERR:Unknown command '{commandName}'");
				return;
			}

			// Build CliParameterSet from URL query string.
			// Values are URL-encoded strings; the command reads them via TryGet<T>() which
			// uses Convert.ChangeType, so "true"→bool, "42"→int, etc. all work correctly.
			var parameters = new CliParameterSet();
			if (!string.IsNullOrEmpty(queryString))
			{
				foreach (var pair in queryString.Split('&'))
				{
					var eqIdx = pair.IndexOf('=');
					if (eqIdx > 0)
					{
						var name = HttpUtility.UrlDecode(pair.Substring(0, eqIdx));
						var value = HttpUtility.UrlDecode(pair.Substring(eqIdx + 1));
						parameters.Set(name, value);
					}
				}
			}

			// activate per-invocation telemetry dedup if the CLI sent a session ID
			if (parameters.TryGet<string>("cliSessionId", out var cliSessionId)
				&& !string.IsNullOrEmpty(cliSessionId))
			{
				TelemetryClient.BeginCliSession(cliSessionId);
			}

			// resolve notebook nickname → canonical name before dispatching
			await Cli.CliNotebookResolver.ResolveNotebookName(parameters);

			var cliFactory = new CommandFactory(
				logger, ribbon: null, new List<IDisposable>(), runningFromCli: true);

			Command result = null;
			string pageOutput = null;
			var cancelled = false;

			// While the command runs, watch the pipe for any inbound activity; the client
			// never sends anything after its initial request during normal operation, so
			// any byte received (or a broken connection) means it wants to cancel or has
			// gone away. cts.Token is checked between pages/sections below so a long batch
			// can stop early instead of running to completion against an absent client.
			using var cts = new CancellationTokenSource();

			// fire-and-forget: runs concurrently for the life of this command and self-handles
			// all exceptions; we never await it (see StopWatcher) so we don't keep a reference
			_ = WatchForClientCancellation(pipe, cts);

			void StopWatcher()
			{
				// Signal the watcher to stop, but do NOT block awaiting its pending read.
				// On .NET Framework, cancelling the token does not unblock an in-flight
				// overlapped NamedPipeServerStream.ReadAsync, so awaiting the watcher here
				// would hang until the client disconnects - which is exactly the multi-second
				// CLI hang this avoids. The watcher's read is inbound and independent of our
				// outbound response write; it faults and unwinds harmlessly when the listener
				// loop disconnects/closes the pipe immediately after this command returns.
				cts.Cancel();
			}

			try
			{
				if (typeof(ICliPageCommand).IsAssignableFrom(commandType))
				{
					parameters.TryGet<string>("notebook", out var notebook);
					parameters.TryGet<string>("section", out var section);
					var hasPage = parameters.TryGet<string>("page", out var page);

					if (string.IsNullOrWhiteSpace(notebook))
					{
						StopWatcher();
						await WriteCliResponse(pipe,
							$"ERR:{commandName} requires a 'notebook' parameter");
						return;
					}

					parameters.TryGet<bool>("backup", out var isBackup);
					if (isBackup && !hasPage)
					{
						(pageOutput, cancelled) = await InvokeCliBackupExport(
							parameters, notebook, section, pipe, cts.Token);
					}
					else if (string.IsNullOrWhiteSpace(section))
					{
						(pageOutput, cancelled) = await InvokeCliCommandForNotebook(
							cliFactory, commandType, parameters, notebook, pipe, cts.Token);
					}
					else
					{
						var effectivePage = hasPage && !string.IsNullOrWhiteSpace(page) ? page : "*";

						using var one = new OneNote();
						var pageIds = await one.FindPagesByPath(notebook, section, effectivePage);

						if (pageIds.Length == 0)
						{
							StopWatcher();
							await WriteCliResponse(pipe,
								$"ERR:No pages found at path: {notebook}/{section}/{effectivePage}");
							return;
						}

						var outputs = new StringBuilder();
						foreach (var pageId in pageIds)
						{
							if (cts.Token.IsCancellationRequested)
							{
								cancelled = true;
								break;
							}

							parameters.Set("pageId", pageId);
							var r = await cliFactory.Run(commandType, cts.Token, parameters, one);
							if (!string.IsNullOrEmpty(r?.CliOutput))
							{
								outputs.AppendLine(r.CliOutput);
							}
						}
						pageOutput = outputs.Length > 0 ? outputs.ToString() : null;
					}
				}
				else
				{
					// single-shot command; no iteration to checkpoint so cancellation
					// can't be honored mid-call, only ever reported for the page loops above
					result = await cliFactory.Run(commandType, parameters);
				}
			}
			catch (Exception exc)
			{
				StopWatcher();
				logger.WriteLine($"error executing CLI command '{commandName}'", exc);
				await WriteCliResponse(pipe, $"ERR:{exc.Message}");
				return;
			}
			finally
			{
				TelemetryClient.EndCliSession();
			}

			// stop watching for client cancellation; the response write below is outbound
			// and safe to issue while the watcher's inbound read is still pending
			StopWatcher();

			var output = result?.CliOutput ?? pageOutput;
			await WriteCliResponse(pipe, cancelled
				? (string.IsNullOrEmpty(output) ? "CANCELLED" : $"CANCELLED:{output}")
				: (string.IsNullOrEmpty(output) ? "OK" : $"OUTPUT:{output}"));
		}


		/// <summary>
		/// Iterates every page in every section of the named notebook and runs the command
		/// once per page. Used when section is not specified for an <see cref="ICliPageCommand"/>.
		/// Writes a <c>PROGRESS:</c> line through <paramref name="pipe"/> as each section starts
		/// so the CLI client can report progress while the notebook is still being processed.
		/// Checks <paramref name="token"/> before each section and page and stops early if set.
		/// Returns any non-empty CliOutput accumulated across all page runs, plus whether the
		/// iteration stopped early due to cancellation.
		/// </summary>
		private static async Task<(string output, bool cancelled)> InvokeCliCommandForNotebook(
			CommandFactory cliFactory, Type commandType,
			CliParameterSet parameters, string notebookName, NamedPipeServerStream pipe,
			CancellationToken token)
		{
			using var one = new OneNote();

			var notebooks = await one.GetNotebooks();
			for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebooks = await one.GetNotebooks();
			}

			if (notebooks == null || !notebooks.HasElements)
			{
				return (null, false);
			}

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null)
			{
				return (null, false);
			}

			var notebookId = notebook.Attribute("ID").Value;
			var notebookSections = await one.GetNotebook(notebookId, OneNote.Scope.Sections);
			for (int attempt = 1; attempt < 4 && (notebookSections == null || !notebookSections.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebookSections = await one.GetNotebook(notebookId, OneNote.Scope.Sections);
			}

			if (notebookSections == null || !notebookSections.HasElements)
			{
				return (null, false);
			}

			var sectionIds = new List<string>();
			CollectSectionIds(notebookSections, sectionIds);

			var outputs = new StringBuilder();
			var cancelled = false;

			foreach (var sectionId in sectionIds)
			{
				if (token.IsCancellationRequested)
				{
					cancelled = true;
					break;
				}

				var section = await one.GetSection(sectionId);
				if (section == null) continue;

				var pageNs = one.GetNamespace(section);
				var pageIds = section.Elements(pageNs + "Page")
					.Select(p => p.Attribute("ID")?.Value)
					.Where(id => id != null)
					.Distinct()
					.ToArray();

				if (pageIds.Length == 0) continue;

				var sectionName = section.Attribute("name")?.Value;
				if (!string.IsNullOrEmpty(sectionName))
				{
					await WriteCliResponse(pipe, $"PROGRESS:{sectionName}\n");
				}

				foreach (var pageId in pageIds)
				{
					if (token.IsCancellationRequested)
					{
						cancelled = true;
						break;
					}

					parameters.Set("pageId", pageId);
					var r = await cliFactory.Run(commandType, token, parameters, one);
					if (!string.IsNullOrEmpty(r?.CliOutput))
					{
						outputs.AppendLine(r.CliOutput);
					}
				}

				if (cancelled) break;
			}

			return (outputs.Length > 0 ? outputs.ToString() : null, cancelled);
		}


		/// <summary>
		/// Exports each section in the specified scope to its own <c>.one</c> file.
		/// Used when the CLI <c>export</c> command is invoked with <c>--backup</c> and no
		/// <c>--page</c>. When <paramref name="sectionPath"/> is non-empty, only that section
		/// is exported; otherwise every section in the notebook is exported.
		/// Section group ancestry is preserved as <c>[GroupName]</c> subdirectories under
		/// <paramref name="parameters"/><c>.outpath</c>.
		/// </summary>
		private static async Task<(string output, bool cancelled)> InvokeCliBackupExport(
			CliParameterSet parameters, string notebookName, string sectionPath,
			NamedPipeServerStream pipe, CancellationToken token)
		{
			parameters.TryGet<string>("outpath", out var outpath);
			if (string.IsNullOrWhiteSpace(outpath))
			{
				await WriteCliResponse(pipe, "ERR:export --backup requires an 'outpath' parameter");
				return (null, false);
			}

			using var one = new OneNote();

			var notebooks = await one.GetNotebooks();
			if (notebooks == null)
			{
				await WriteCliResponse(pipe, "ERR:No notebooks found");
				return (null, false);
			}

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null)
			{
				await WriteCliResponse(pipe, $"ERR:Notebook not found: '{notebookName}'");
				return (null, false);
			}

			var notebookId = notebook.Attribute("ID").Value;
			var notebookSections = await one.GetNotebook(notebookId, OneNote.Scope.Sections);
			if (notebookSections == null)
			{
				await WriteCliResponse(pipe, $"ERR:Could not load sections for notebook: '{notebookName}'");
				return (null, false);
			}

			var cancelled = false;

			if (!string.IsNullOrWhiteSpace(sectionPath))
			{
				// Single-section backup
				var sectionParts = sectionPath.Trim().Trim('/').Split('/');
				XElement node = notebookSections;
				foreach (var part in sectionParts)
				{
					node = node.Elements().FirstOrDefault(e =>
						(e.Name.LocalName == "Section" || e.Name.LocalName == "SectionGroup") &&
						string.Equals(e.Attribute("name")?.Value, part.Trim(),
							StringComparison.InvariantCultureIgnoreCase));

					if (node == null)
					{
						await WriteCliResponse(pipe, $"ERR:Section not found: '{sectionPath}'");
						return (null, false);
					}
				}

				if (node.Name.LocalName != "Section")
				{
					await WriteCliResponse(pipe,
						$"ERR:'{sectionPath}' is a section group — specify a section within it");
					return (null, false);
				}

				var sectionId = node.Attribute("ID")?.Value;
				var sectInfo = await one.GetSectionInfo(sectionId);
				if (sectInfo == null)
				{
					await WriteCliResponse(pipe, $"ERR:Could not read section info for '{sectionPath}'");
					return (null, false);
				}

				PathHelper.EnsurePathExists(outpath);
				var outFile = Path.Combine(outpath, PathHelper.CleanFileName(sectInfo.Name) + ".one");
				if (File.Exists(outFile)) { File.Delete(outFile); }

				await WriteCliResponse(pipe, $"PROGRESS:{sectInfo.Name}\n");
				one.Export(sectionId, outFile, OneNote.ExportFormat.OneNote);
			}
			else
			{
				// All-sections backup
				var sectionIds = new List<string>();
				CollectSectionIds(notebookSections, sectionIds);

				if (sectionIds.Count == 0)
				{
					await WriteCliResponse(pipe, $"ERR:No sections found in notebook: '{notebookName}'");
					return (null, false);
				}

				foreach (var sectionId in sectionIds)
				{
					if (token.IsCancellationRequested)
					{
						cancelled = true;
						break;
					}

					var sectInfo = await one.GetSectionInfo(sectionId);
					if (sectInfo == null) { continue; }

					var groupPath = outpath;
					foreach (var group in sectInfo.SectionGroups)
					{
						groupPath = Path.Combine(groupPath, $"[{PathHelper.CleanFileName(group)}]");
					}

					PathHelper.EnsurePathExists(groupPath);
					var outFile = Path.Combine(groupPath, PathHelper.CleanFileName(sectInfo.Name) + ".one");
					if (File.Exists(outFile)) { File.Delete(outFile); }

					await WriteCliResponse(pipe, $"PROGRESS:{sectInfo.Name}\n");
					one.Export(sectionId, outFile, OneNote.ExportFormat.OneNote);
				}
			}

			return (null, cancelled);
		}


		/// <summary>
		/// While a CLI command is executing, watches the pipe for any inbound activity from
		/// the client. The client never sends anything after its initial request during
		/// normal operation, so any completed read here — bytes received, EOF (0-length
		/// read), or a fault because the connection broke — means the client wants to cancel
		/// or has gone away; either way, cancels <paramref name="cts"/>. Exits quietly once
		/// the caller cancels <paramref name="cts"/> itself to signal the command has finished.
		/// </summary>
		private static async Task WatchForClientCancellation(
			NamedPipeServerStream pipe, CancellationTokenSource cts)
		{
			try
			{
				var buffer = new byte[16];
				await pipe.ReadAsync(buffer, 0, buffer.Length, cts.Token);
				cts.Cancel();
			}
			catch (OperationCanceledException)
			{
				// normal shutdown: the command finished and the caller already cancelled cts
			}
			catch
			{
				// pipe broke for some other reason (client vanished) - treat as a cancel request
				try { cts.Cancel(); } catch { /* already cancelled/disposed */ }
			}
		}


		private static void CollectSectionIds(XElement node, List<string> ids)
		{
			foreach (var element in node.Elements())
			{
				var localName = element.Name.LocalName;
				if (localName == "Section"
					&& element.Attribute("isRecycleBin")?.Value != "true")
				{
					var id = element.Attribute("ID")?.Value;
					if (id != null) ids.Add(id);
				}
				else if (localName == "SectionGroup"
					&& element.Attribute("isRecycleBin")?.Value != "true")
				{
					CollectSectionIds(element, ids);
				}
			}
		}


		private static async Task WriteCliResponse(NamedPipeServerStream pipe, string response)
		{
			var bytes = Encoding.UTF8.GetBytes(response);
			await pipe.WriteAsync(bytes, 0, bytes.Length);
			await pipe.FlushAsync();
			pipe.WaitForPipeDrain();
		}
	}
}
