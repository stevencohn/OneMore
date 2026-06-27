//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// CLI host for OneMore commands.  Supports two usage modes:
	/// <list type="bullet">
	///   <item>
	///     <b>Interactive</b> — run with no arguments; presents a menu, prompts for each
	///     parameter value, then executes the chosen command.
	///   </item>
	///   <item>
	///     <b>Command-line</b> — pass the command name and zero or more
	///     <c>--param value</c> arguments; any required parameters not supplied on the
	///     command line are prompted for interactively before execution.
	///   </item>
	/// </list>
	/// <para>
	/// Commands are discovered automatically: any class in <c>River.OneMoreAddIn.dll</c>
	/// that implements <see cref="ICliCommand"/> and has a public default constructor is
	/// included.
	/// </para>
	/// </summary>
	internal class Program
	{
		// the CancellationTokenSource for whichever command is currently running, if any;
		// recreated fresh before each command run so a single CancelKeyPress handler can
		// serve every command in an interactive session without leaking cancellation state
		// from one run into the next
		private static CancellationTokenSource currentCts;


		static async Task Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			Console.CancelKeyPress += (s, e) =>
			{
				var cts = currentCts;
				if (cts == null || cts.IsCancellationRequested)
				{
					// no command running, or cancellation was already requested - let the
					// default behavior terminate the process immediately (force-quit escape
					// hatch for a second Ctrl+C, or a no-op kill if nothing is running)
					return;
				}

				e.Cancel = true;
				CliConsole.WriteWarning("Cancelling… (press Ctrl+C again to force quit)");
				cts.Cancel();
			};

			var commands = CommandDiscovery.Discover();

			// Command-line mode - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
			if (args.Length > 0)
			{
				Environment.ExitCode = await RunCommandLine(args, commands) ? 0 : 1;
				return;
			}

			// Interactive mode - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
			Console.Title = "OneMore CLI";
			CliConsole.WriteBanner();

			if (commands.Count == 0)
			{
				CliConsole.WriteWarning("No CLI commands found.");
				CliConsole.WriteInfo("Implement ICliCommand in River.OneMoreAddIn.dll to add commands.");
				return;
			}

			var running = true;
			while (running)
			{
				var command = InteractiveRunner.PromptForCommand(commands);
				if (command == null)
					break;

				Console.WriteLine();

				var parameters = InteractiveRunner.CollectParameters(command, supplied: null);
				if (parameters == null)
				{
					CliConsole.WriteInfo("Cancelled.");
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine(CliConsole.Separator);

					currentCts = new CancellationTokenSource();
					try
					{
						await RunCommand(command, parameters, currentCts.Token);
						Console.WriteLine();
						CliConsole.WriteSuccess("Command completed successfully.");
					}
					catch (OperationCanceledException)
					{
						Console.WriteLine();
						CliConsole.WriteInfo("Command cancelled.");
					}
					catch (Exception exc)
					{
						Console.WriteLine();
						CliConsole.WriteError(exc);
					}
					finally
					{
						currentCts.Dispose();
						currentCts = null;
					}
				}

				Console.WriteLine();
				Console.Write("Run another command? [Y/n] ");
				var again = Console.ReadLine()?.Trim() ?? string.Empty;
				running = string.IsNullOrEmpty(again)
					|| again.Equals("y", StringComparison.OrdinalIgnoreCase)
					|| again.Equals("yes", StringComparison.OrdinalIgnoreCase);
			}

			Console.WriteLine();
			Console.WriteLine("Goodbye.");
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Command-line mode

		/// <summary>
		/// Handles non-interactive invocation:
		/// <c>OneMoreCli &lt;CommandName&gt; [--param value …]</c>
		/// <para>
		/// Special forms:
		/// <list type="bullet">
		///   <item><c>OneMoreCli --help</c> — lists all commands.</item>
		///   <item><c>OneMoreCli &lt;CommandName&gt; --help</c> — shows parameters for one command.</item>
		/// </list>
		/// </para>
		/// </summary>
		/// <returns><c>true</c> on success; <c>false</c> on error.</returns>
		private static async Task<bool> RunCommandLine(string[] args, System.Collections.Generic.List<ICliCommand> commands)
		{
			var commandName = args[0];

			// --help with no command → list all commands
			if (ArgParser.IsHelpFlag(commandName))
			{
				CliConsole.ShowHelp(commands);
				return true;
			}

			// Find the named command
			var command = commands.FirstOrDefault(c =>
				c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));

			if (command == null)
			{
				CliConsole.WriteError($"Unknown command: '{commandName}'");
				Console.WriteLine();
				CliConsole.ShowHelp(commands);
				return false;
			}

			// CommandName --help → show parameter reference for this command
			var rest = args.Skip(1).ToArray();
			if (rest.Length > 0 && ArgParser.IsHelpFlag(rest[0]))
			{
				CliConsole.ShowCommandHelp(command);
				return true;
			}

			// Parse --name value / --name=value pairs from the remaining args
			if (!ArgParser.TryParse(rest, out var supplied, out var parseError))
			{
				CliConsole.WriteError(parseError);
				Console.WriteLine();
				CliConsole.ShowCommandHelp(command);
				return false;
			}

			// Collect parameters — uses supplied values, prompts for any missing required ones
			var parameters = InteractiveRunner.CollectParameters(command, supplied);
			if (parameters == null)
			{
				CliConsole.WriteInfo("Cancelled.");
				return false;
			}

			currentCts = new CancellationTokenSource();
			try
			{
				await RunCommand(command, parameters, currentCts.Token);
				return true;
			}
			catch (OperationCanceledException)
			{
				CliConsole.WriteInfo("Command cancelled.");
				return false;
			}
			catch (Exception exc)
			{
				CliConsole.WriteError(exc);
				return false;
			}
			finally
			{
				currentCts.Dispose();
				currentCts = null;
			}
		}


		/// <summary>
		/// Dispatches a command with its collected parameters.
		/// <para>
		/// First attempts to delegate to the running OneMore add-in via its named pipe
		/// (<c>onemorecli://</c> protocol). This is the preferred path when OneNote is already
		/// open because the add-in has a live, properly initialised COM connection.
		/// </para>
		/// <para>
		/// Falls back to direct COM activation when the add-in pipe is unavailable (i.e. OneNote
		/// is not running). For <see cref="ICliPageCommand"/> implementations the path is then
		/// resolved locally and the command is invoked once per page.
		/// </para>
		/// </summary>
		private static async Task RunCommand(
			ICliCommand command, CliParameterSet parameters, CancellationToken token)
		{
			var cliSessionId = Guid.NewGuid().ToString("N");

			// Preferred path: delegate to the running add-in via the named pipe
			if (await AddinBridge.TryRun(command, parameters, token, cliSessionId))
			{
				return;
			}

			// ICliInteractiveCommand signals that the command needs OneNote running as a
			// full UI process. Some APIs (e.g. Publish) are unavailable against the
			// headless COM server that `new Application()` would otherwise activate.
			if (command is ICliInteractiveCommand)
			{
				if (!await OneNoteLauncher.EnsureRunning())
				{
					CliConsole.WriteError(
						$"{command.CommandName} requires interactive OneNote but it could not be launched");
					return;
				}
			}

			// Fallback: direct COM (OneNote is not running; new Application() starts it fresh)
			TelemetryClient.BeginCliSession(cliSessionId);
			try
			{
				if (command is ICliPageCommand)
				{
					parameters.TryGet<string>("notebook", out var notebook);
					parameters.TryGet<string>("section", out var section);
					var hasPage = parameters.TryGet<string>("page", out var page);

					if (string.IsNullOrWhiteSpace(notebook))
					{
						throw new InvalidOperationException(
							$"{command.CommandName} requires a 'notebook' parameter.");
					}

					if (string.IsNullOrWhiteSpace(section))
					{
						await RunForNotebook(command, parameters, notebook, token);
						return;
					}

					var effectivePage = hasPage && !string.IsNullOrWhiteSpace(page) ? page : "*";

					using var one = new OneNote();
					var pageIds = await one.FindPagesByPath(notebook, section, effectivePage);

					if (pageIds.Length == 0)
					{
						CliConsole.WriteWarning($"No pages found at path: {notebook}/{section}/{effectivePage}");
						return;
					}

					foreach (var pageId in pageIds)
					{
						token.ThrowIfCancellationRequested();

						parameters.Set("pageId", pageId);
						var r = await CliCommandFactory.Make().Run(command.GetType(), token, parameters);
						if (!string.IsNullOrEmpty(r?.CliOutput))
							Console.Write(r.CliOutput);
					}
				}
				else
				{
					var result = await CliCommandFactory.Make().Run(command.GetType(), token, parameters);
					var output = result?.CliOutput;
					if (!string.IsNullOrEmpty(output))
					{
						Console.Write(output);
					}
				}
			}
			finally
			{
				TelemetryClient.EndCliSession();
			}
		}


		/// <summary>
		/// Iterates every page in every section of the named notebook and runs the command
		/// once per page. Used when section is not specified for an <see cref="ICliPageCommand"/>.
		/// </summary>
		private static async Task RunForNotebook(
			ICliCommand command, CliParameterSet parameters, string notebookName,
			CancellationToken token)
		{
			using var one = new OneNote();

			var notebooks = await one.GetNotebooks();
			if (notebooks == null || !notebooks.HasElements)
			{
				CliConsole.WriteWarning("No notebooks found.");
				return;
			}

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null)
			{
				CliConsole.WriteWarning($"Notebook not found: '{notebookName}'");
				return;
			}

			var notebookId = notebook.Attribute("ID").Value;
			var notebookSections = await one.GetNotebook(notebookId, OneNote.Scope.Sections);
			if (notebookSections == null)
			{
				CliConsole.WriteWarning($"Could not load sections for notebook: '{notebookName}'");
				return;
			}

			var sectionIds = new List<string>();
			CollectSectionIds(notebookSections, sectionIds);

			if (sectionIds.Count == 0)
			{
				CliConsole.WriteWarning($"No sections found in notebook: '{notebookName}'");
				return;
			}

			foreach (var sectionId in sectionIds)
			{
				token.ThrowIfCancellationRequested();

				var section = await one.GetSection(sectionId);
				if (section == null) continue;

				var pageNs = one.GetNamespace(section);
				var pageIds = section.Elements(pageNs + "Page")
					.Select(p => p.Attribute("ID")?.Value)
					.Where(id => id != null)
					.ToArray();

				if (pageIds.Length == 0) continue;

				var sectionName = section.Attribute("name")?.Value;
				if (!string.IsNullOrEmpty(sectionName))
				{
					CliConsole.WriteDetail($"section: {sectionName}");
				}

				foreach (var pageId in pageIds)
				{
					token.ThrowIfCancellationRequested();

					parameters.Set("pageId", pageId);
					var r = await CliCommandFactory.Make().Run(command.GetType(), token, parameters);
					if (!string.IsNullOrEmpty(r?.CliOutput))
						Console.Write(r.CliOutput);
				}
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
	}
}
