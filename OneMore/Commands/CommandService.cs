//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Win32;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
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

						// pipe has maxNumberOfServerInstances = 1, so a client that
						// connects but never writes would wedge the listener forever
						using var cts = new CancellationTokenSource(
							TimeSpan.FromSeconds(ReadTimeoutSeconds));
						try
						{
							_ = await server.ReadAsync(buffer, 0, MaxBytes, cts.Token);
						}
						catch (OperationCanceledException)
						{
							logger.WriteLine("pipe read timed out");
							continue;
						}

						data = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim((char)0);
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

			var cliFactory = new CommandFactory(
				logger, ribbon: null, new List<IDisposable>(), runningFromCli: true);

			Command result = null;
			string pageOutput = null;

			try
			{
				if (typeof(ICliPageCommand).IsAssignableFrom(commandType))
				{
					parameters.TryGet<string>("notebook", out var notebook);
					parameters.TryGet<string>("section", out var section);
					var hasPage = parameters.TryGet<string>("page", out var page);

					if (string.IsNullOrWhiteSpace(notebook))
					{
						await WriteCliResponse(pipe,
							$"ERR:{commandName} requires a 'notebook' parameter");
						return;
					}

					if (string.IsNullOrWhiteSpace(section))
					{
						pageOutput = await InvokeCliCommandForNotebook(cliFactory, commandType, parameters, notebook);
					}
					else
					{
						var path = string.Concat(
							notebook, "/", section, "/",
							hasPage && !string.IsNullOrWhiteSpace(page) ? page : "*");

						using var one = new OneNote();
						var pageIds = await one.FindPagesByPath(path);

						if (pageIds.Length == 0)
						{
							await WriteCliResponse(pipe, $"ERR:No pages found at path: {path}");
							return;
						}

						var outputs = new StringBuilder();
						foreach (var pageId in pageIds)
						{
							parameters.Set("pageId", pageId);
							var r = await cliFactory.Run(commandType, parameters);
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
					result = await cliFactory.Run(commandType, parameters);
				}

				var output = result?.CliOutput ?? pageOutput;
				await WriteCliResponse(pipe,
					string.IsNullOrEmpty(output) ? "OK" : "OUTPUT:" + output);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing CLI command '{commandName}'", exc);
				await WriteCliResponse(pipe, $"ERR:{exc.Message}");
			}
		}


		/// <summary>
		/// Iterates every page in every section of the named notebook and runs the command
		/// once per page. Used when section is not specified for an <see cref="ICliPageCommand"/>.
		/// Returns any non-empty CliOutput accumulated across all page runs.
		/// </summary>
		private static async Task<string> InvokeCliCommandForNotebook(
			CommandFactory cliFactory, Type commandType,
			CliParameterSet parameters, string notebookName)
		{
			using var one = new OneNote();

			var notebooks = await one.GetNotebooks();
			if (notebooks == null || !notebooks.HasElements)
			{
				return null;
			}

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null)
			{
				return null;
			}

			var notebookId = notebook.Attribute("ID").Value;
			var notebookSections = await one.GetNotebook(notebookId, OneNote.Scope.Sections);
			if (notebookSections == null)
			{
				return null;
			}

			var sectionIds = new List<string>();
			CollectSectionIds(notebookSections, sectionIds);

			var outputs = new StringBuilder();
			foreach (var sectionId in sectionIds)
			{
				var section = await one.GetSection(sectionId);
				if (section == null) continue;

				var pageNs = one.GetNamespace(section);
				var pageIds = section.Elements(pageNs + "Page")
					.Select(p => p.Attribute("ID")?.Value)
					.Where(id => id != null)
					.ToArray();

				foreach (var pageId in pageIds)
				{
					parameters.Set("pageId", pageId);
					var r = await cliFactory.Run(commandType, parameters);
					if (!string.IsNullOrEmpty(r?.CliOutput))
					{
						outputs.AppendLine(r.CliOutput);
					}
				}
			}

			return outputs.Length > 0 ? outputs.ToString() : null;
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
