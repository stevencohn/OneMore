//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1118 // Utility classes should not have public constructors

namespace OneMoreCli
{
	using Microsoft.Win32;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.IO;
	using System.IO.Pipes;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;


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
		private static readonly string Separator = new('─', 60);


		static async Task Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			var commands = DiscoverCommands();

			// Command-line mode - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
			if (args.Length > 0)
			{
				Environment.ExitCode = await RunCommandLine(args, commands) ? 0 : 1;
				return;
			}

			// Interactive mode - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
			Console.Title = "OneMore CLI";
			WriteBanner();

			if (commands.Count == 0)
			{
				WriteWarning("No CLI commands found.");
				WriteInfo("Implement ICliCommand in River.OneMoreAddIn.dll to add commands.");
				return;
			}

			var running = true;
			while (running)
			{
				var command = PromptForCommand(commands);
				if (command == null)
					break;

				Console.WriteLine();

				var parameters = CollectParameters(command, supplied: null);
				if (parameters == null)
				{
					WriteInfo("Cancelled.");
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine(Separator);

					try
					{
						await RunCommand(command, parameters);
						Console.WriteLine();
						WriteSuccess("Command completed successfully.");
					}
					catch (Exception exc)
					{
						Console.WriteLine();
						WriteError(exc);
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
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = 

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
		private static async Task<bool> RunCommandLine(string[] args, List<ICliCommand> commands)
		{
			var commandName = args[0];

			// --help with no command → list all commands
			if (commandName.Equals("--help", StringComparison.OrdinalIgnoreCase)
				|| commandName.Equals("-h", StringComparison.OrdinalIgnoreCase)
				|| commandName.Equals("/?", StringComparison.OrdinalIgnoreCase))
			{
				ShowHelp(commands);
				return true;
			}

			// Find the named command
			var command = commands.FirstOrDefault(c =>
				c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));

			if (command == null)
			{
				WriteError($"Unknown command: '{commandName}'");
				Console.WriteLine();
				ShowHelp(commands);
				return false;
			}

			// CommandName --help → show parameter reference for this command
			var rest = args.Skip(1).ToArray();
			if (rest.Length > 0 && IsHelpFlag(rest[0]))
			{
				ShowCommandHelp(command);
				return true;
			}

			// Parse --name value / --name=value pairs from the remaining args
			if (!TryParseArgs(rest, out var supplied, out var parseError))
			{
				WriteError(parseError);
				Console.WriteLine();
				ShowCommandHelp(command);
				return false;
			}

			// Collect parameters — uses supplied values, prompts for any missing required ones
			var parameters = CollectParameters(command, supplied);
			if (parameters == null)
			{
				WriteInfo("Cancelled.");
				return false;
			}

			try
			{
				await RunCommand(command, parameters);
				return true;
			}
			catch (Exception exc)
			{
				WriteError(exc);
				return false;
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
		private static async Task RunCommand(ICliCommand command, CliParameterSet parameters)
		{
			// Preferred path: delegate to the running add-in via the named pipe
			if (await TryRunViaAddin(command, parameters))
				return;

			// Fallback: direct COM (OneNote is not running; new Application() starts it fresh)
			if (command is ICliPageCommand)
			{
				parameters.TryGet<string>("notebook", out var notebook);
				parameters.TryGet<string>("section", out var section);
				var hasPage = parameters.TryGet<string>("page", out var page);

				if (string.IsNullOrWhiteSpace(notebook) || string.IsNullOrWhiteSpace(section))
				{
					throw new InvalidOperationException(
						$"{command.CommandName} requires 'notebook' and 'section' parameters.");
				}

				var path = string.Concat(
					notebook, "/", section, "/",
					hasPage && !string.IsNullOrWhiteSpace(page) ? page : "*");

				using var one = new OneNote();
				var pageIds = await one.FindPagesByPath(path);

				if (pageIds.Length == 0)
				{
					WriteWarning($"No pages found at path: {path}");
					return;
				}

				foreach (var pageId in pageIds)
				{
					parameters.Set("pageId", pageId);
					await CliCommandFactory.Make().Run(command.GetType(), parameters);
				}
			}
			else
			{
				await CliCommandFactory.Make().Run(command.GetType(), parameters);
			}
		}


		/// <summary>
		/// Tries to run the command by sending a <c>onemorecli://</c> request to the named pipe
		/// exposed by the running OneMore add-in. Returns <c>true</c> on success, <c>false</c>
		/// when the pipe is unavailable (add-in/OneNote not running) so the caller can fall back
		/// to direct COM activation.
		/// </summary>
		private static async Task<bool> TryRunViaAddin(ICliCommand command, CliParameterSet parameters)
		{
			var pipeName = GetPipeName();
			if (string.IsNullOrEmpty(pipeName))
				return false;

			NamedPipeClientStream pipe = null;
			try
			{
				pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

				// Short connect timeout: if the add-in isn't listening we fall back immediately
				await Task.Run(() => pipe.Connect(500));

				// Send request
				var uri = BuildCliUri($"{command.CommandName}Command", parameters);
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

				// "OK" or any non-error response → success
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
		private static string GetPipeName()
		{
			try
			{
				using var key = Registry.ClassesRoot.OpenSubKey(@"River.OneMoreAddIn\CLSID", false);
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
		private static string BuildCliUri(string commandName, CliParameterSet parameters)
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


		/// <summary>
		/// Parses <c>--name value</c>, <c>--name=value</c>, and bare <c>--flag</c>
		/// (treated as boolean <c>true</c>) from a args array.
		/// Names are matched case-insensitively.
		/// </summary>
		private static bool TryParseArgs(
			string[] args,
			out Dictionary<string, string> parsed,
			out string errorMessage)
		{
			parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			errorMessage = null;

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];

				if (!arg.StartsWith("--") && !arg.StartsWith("-"))
				{
					errorMessage = $"Unexpected argument '{arg}'. Parameters must be prefixed with '--'.";
					return false;
				}

				// Strip leading dashes
				var nameAndValue = arg.TrimStart('-');

				// --name=value form
				var eqIndex = nameAndValue.IndexOf('=');
				if (eqIndex >= 0)
				{
					var name  = nameAndValue.Substring(0, eqIndex);
					var value = StripSurroundingQuotes(nameAndValue.Substring(eqIndex + 1));
					parsed[name] = value;
					continue;
				}

				// --name value form (next token is the value, unless it looks like another flag)
				var paramName = nameAndValue;
				if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
				{
					var value = args[++i];

					// Shells that don't treat single quotes as string delimiters (e.g. cmd.exe)
					// split 'multi word value' into separate tokens. Reassemble them here.
					if (value.Length > 0 && (value[0] == '\'' || value[0] == '"'))
					{
						var quote = value[0];
						while (!(value.Length > 1 && value[value.Length - 1] == quote)
							&& i + 1 < args.Length)
						{
							value += ' ' + args[++i];
						}
					}

					parsed[paramName] = StripSurroundingQuotes(value);
				}
				else
				{
					// Bare --flag → boolean true
					parsed[paramName] = "true";
				}
			}

			return true;
		}


		/// <summary>
		/// Removes a matched pair of surrounding single or double quotes from a value token,
		/// e.g. <c>'hello world'</c> → <c>hello world</c>. Returns the original string if
		/// no surrounding quotes are present.
		/// </summary>
		private static string StripSurroundingQuotes(string value)
		{
			if (value.Length >= 2
				&& ((value[0] == '\'' && value[value.Length - 1] == '\'')
					|| (value[0] == '"' && value[value.Length - 1] == '"')))
			{
				return value.Substring(1, value.Length - 2);
			}

			return value;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Help output
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private static void ShowHelp(List<ICliCommand> commands)
		{
			Console.WriteLine("OneMore CLI");
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine("  OneMoreCli                           (interactive mode)");
			Console.WriteLine("  OneMoreCli <command> [--param value …]");
			Console.WriteLine("  OneMoreCli <command> --help");
			Console.WriteLine();

			if (commands.Count == 0)
			{
				WriteWarning("No commands are currently registered.");
				return;
			}

			Console.WriteLine("Commands:");
			Console.WriteLine();

			foreach (var cmd in commands)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write($"  {cmd.CommandName}");
				Console.ResetColor();
				Console.WriteLine($"  —  {cmd.Description}");
			}

			Console.WriteLine();
		}


		private static void ShowCommandHelp(ICliCommand command)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write($"  {command.CommandName}");
			Console.ResetColor();
			Console.WriteLine($"  —  {command.Description}");
			Console.WriteLine();

			var definition = command.DefineParameters();
			if (!definition.Any())
			{
				Console.WriteLine("  This command has no parameters.");
				Console.WriteLine();
				return;
			}

			Console.WriteLine("  Parameters:");
			Console.WriteLine();

			foreach (var p in definition)
			{
				// --name  <type>  (required | optional [default: X])
				var req = p.Required
					? "required"
					: p.DefaultValue != null ? $"optional, default: {p.DefaultValue}" : "optional";

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write($"    --{p.Name}");
				Console.ResetColor();
				Console.Write($"  <{FriendlyTypeName(p.DataType)}>  ({req})");

				// Constraint annotation
				var annotation = ConstraintAnnotation(p.Constraint);
				if (annotation != null)
					Console.Write($"  {annotation}");

				Console.WriteLine();
				Console.WriteLine($"      {p.Description}");
				Console.WriteLine();
			}

			// Usage line
			var required = definition.Where(p => p.Required)
				.Select(p => $"--{p.Name} <{FriendlyTypeName(p.DataType)}>");
			var optional = definition.Where(p => !p.Required)
				.Select(p => $"[--{p.Name} <{FriendlyTypeName(p.DataType)}>]");

			Console.WriteLine("  Usage:");
			Console.Write($"    OneMoreCli {command.CommandName}");
			foreach (var token in required.Concat(optional))
				Console.Write($" {token}");
			Console.WriteLine();
			Console.WriteLine();
		}


		/// <summary>Returns a short annotation string describing the constraint, or null.</summary>
		private static string ConstraintAnnotation(CliConstraint constraint)
		{
			return constraint switch
			{
				RangeConstraint rc  => $"[{rc.Minimum}–{rc.Maximum}]",
				EnumConstraint  ec  => $"[{string.Join("|", ec.Values)}]",
				FormatConstraint fc => fc.Hint != null ? $"[{fc.Hint}]" : null,
				_                   => null
			};
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Discovery
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Finds all non-abstract types in <c>River.OneMoreAddIn.dll</c> that implement
		/// <see cref="ICliCommand"/> and have a public default constructor.
		/// Returns them sorted by <see cref="ICliCommand.CommandName"/>.
		/// <para>
		/// Handles <see cref="ReflectionTypeLoadException"/> gracefully — the OneMore assembly
		/// has COM/Office interop dependencies that may not resolve in a standalone CLI context.
		/// Types that fail to load are skipped silently.
		/// </para>
		/// </summary>
		private static List<ICliCommand> DiscoverCommands()
		{
			var interfaceType = typeof(ICliCommand);
			var assembly = interfaceType.Assembly; // River.OneMoreAddIn.dll

			// GetTypes() throws ReflectionTypeLoadException when any type in the assembly
			// fails to load. The exception's Types array still contains the types that loaded.
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types.Where(t => t != null).ToArray();
			}

			var commands = new List<ICliCommand>();
			foreach (var type in types)
			{
				if (type.IsAbstract || type.IsInterface)
					continue;

				if (!interfaceType.IsAssignableFrom(type))
					continue;

				try
				{
					var instance = (ICliCommand)Activator.CreateInstance(type);
					commands.Add(instance);
				}
				catch
				{
					// Skip types whose constructors require arguments or throw
				}
			}

			return commands.OrderBy(c => c.CommandName).ToList();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Interactive command selection
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = 

		/// <summary>
		/// Displays the command menu and returns the user's selection,
		/// or <c>null</c> if the user chooses to quit.
		/// </summary>
		private static ICliCommand PromptForCommand(List<ICliCommand> commands)
		{
			while (true)
			{
				Console.WriteLine(Separator);
				Console.WriteLine("Available commands:");
				Console.WriteLine();

				for (var i = 0; i < commands.Count; i++)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write($"  {i + 1,2}. {commands[i].CommandName}");
					Console.ResetColor();
					Console.WriteLine($"  —  {commands[i].Description}");
				}

				Console.WriteLine();
				Console.Write("Select a command (or Q to quit): ");
				var input = Console.ReadLine()?.Trim() ?? string.Empty;

				if (string.IsNullOrEmpty(input)
					|| input.Equals("q", StringComparison.OrdinalIgnoreCase)
					|| input.Equals("quit", StringComparison.OrdinalIgnoreCase))
					return null;

				if (int.TryParse(input, out var choice) && choice >= 1 && choice <= commands.Count)
					return commands[choice - 1];

				WriteWarning("Invalid selection — enter a number from the list.");
				Console.WriteLine();
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Parameter collection  (shared by both modes)
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Builds a <see cref="CliParameterSet"/> for <paramref name="command"/>.
		/// <para>
		/// For each parameter in the command's definition:
		/// <list type="bullet">
		///   <item>If a value was pre-supplied via <paramref name="supplied"/>, it is
		///         validated and used directly; an invalid value is an error (not re-prompted)
		///         in command-line mode.</item>
		///   <item>If not supplied and required, the user is prompted interactively.</item>
		///   <item>If not supplied and optional, the parameter's default value is used.</item>
		/// </list>
		/// </para>
		/// </summary>
		/// <param name="command">The command whose parameters are being collected.</param>
		/// <param name="supplied">
		/// Pre-parsed name→raw-string values from the command line, or <c>null</c> for
		/// fully interactive mode.
		/// </param>
		/// <returns>
		/// A populated <see cref="CliParameterSet"/>, or <c>null</c> if the user cancelled
		/// or a supplied value failed validation.
		/// </returns>
		private static CliParameterSet CollectParameters(
			ICliCommand command,
			Dictionary<string, string> supplied)
		{
			var definition = command.DefineParameters();
			if (!definition.Any())
				return new CliParameterSet();

			var interactive = supplied == null;
			if (interactive)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine($"Parameters for {command.CommandName}:");
				Console.ResetColor();
				Console.WriteLine("  (type /cancel at any prompt to go back)");
				Console.WriteLine();
			}

			var set = new CliParameterSet();

			foreach (var parameter in definition)
			{
				// Value was supplied on the command line - - - - - - - - - - - - -
				if (supplied != null && supplied.TryGetValue(parameter.Name, out var raw))
				{
					if (!TryConvert(raw, parameter.DataType, out var converted, out var convertError))
					{
						WriteError($"--{parameter.Name}: {convertError}");
						return null;
					}

					if (!parameter.TryValidate(converted, out var validationError))
					{
						WriteError($"--{parameter.Name}: {validationError}");
						return null;
					}

					set.Set(parameter.Name, converted);
					continue;
				}

				// Not supplied and optional → use default - - - - - - - - - - - - - - - - - - - -
				if (!parameter.Required)
				{
					if (parameter.DefaultValue != null)
						set.Set(parameter.Name, parameter.DefaultValue);
					continue;
				}

				// Required and not supplied → prompt interactively - - - - - - - - - - - - - - -
				if (!interactive)
				{
					Console.WriteLine();
					WriteInfo($"Required parameter '--{parameter.Name}' was not provided.");
				}

				if (!PromptForParameter(parameter, out var prompted))
					return null;

				if (prompted != null)
					set.Set(parameter.Name, prompted);
			}

			return set;
		}


		/// <summary>
		/// Prompts the user for a single parameter value, repeating until a valid value
		/// is entered or the user types <c>/cancel</c>.
		/// </summary>
		/// <returns><c>false</c> if the user typed <c>/cancel</c>.</returns>
		private static bool PromptForParameter(CliParameter parameter, out object value)
		{
			value = null;

			while (true)
			{
				// Enum: render a numbered sub-list - - - - - - - - - - - - - - - - - - - - - - -

				if (parameter.Constraint is EnumConstraint enumConstraint)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"  {parameter.Name}  —  {parameter.Description}");
					Console.ResetColor();

					for (var i = 0; i < enumConstraint.Values.Count; i++)
						Console.WriteLine($"    {i + 1}. {enumConstraint.Values[i]}");

					var defaultHint = parameter.DefaultValue != null
						? $" [default: {parameter.DefaultValue}, press Enter]"
						: string.Empty;

					Console.Write($"  Choice 1–{enumConstraint.Values.Count}{defaultHint}: ");
				}
				else
				{
					// All other types - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

					var qualifier = parameter.Required ? string.Empty : ", optional";
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write($"  {parameter.Name} ({FriendlyTypeName(parameter.DataType)}{qualifier})");
					Console.ResetColor();
					Console.Write($"  —  {parameter.Description}");

					if (!parameter.Required && parameter.DefaultValue != null)
						Console.Write($" [default: {parameter.DefaultValue}, press Enter]");

					if (parameter.Constraint is FormatConstraint fc && fc.Hint != null)
						Console.Write($" [{fc.Hint}]");

					Console.Write(": ");
				}

				var raw = Console.ReadLine();

				if (raw != null && raw.Trim().Equals("/cancel", StringComparison.OrdinalIgnoreCase))
					return false;

				// Empty → default for optional, re-prompt for required
				if (string.IsNullOrWhiteSpace(raw))
				{
					if (!parameter.Required)
					{
						value = parameter.DefaultValue;
						return true;
					}

					WriteWarning("  This parameter is required — please enter a value.");
					continue;
				}

				raw = raw.Trim();

				// Resolve enum selection by ordinal
				if (parameter.Constraint is EnumConstraint ec
					&& int.TryParse(raw, out var idx)
					&& idx >= 1 && idx <= ec.Values.Count)
				{
					raw = ec.Values[idx - 1];
				}

				if (!TryConvert(raw, parameter.DataType, out var converted, out var convertError))
				{
					WriteWarning($"  {convertError}");
					continue;
				}

				if (!parameter.TryValidate(converted, out var validationError))
				{
					WriteWarning($"  {validationError}");
					continue;
				}

				value = converted;
				return true;
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Type conversion
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private static bool TryConvert(
			string raw, Type targetType, out object result, out string errorMessage)
		{
			result = null;
			errorMessage = null;

			// Boolean: accept y/n/yes/no/1/0 in addition to true/false
			if (targetType == typeof(bool))
			{
				switch (raw.ToLowerInvariant())
				{
					case "true": case "yes": case "y": case "1":
						result = true; return true;
					case "false": case "no": case "n": case "0":
						result = false; return true;
					default:
						errorMessage = "Expected true/false, yes/no, or y/n.";
						return false;
				}
			}

			// DateTime: ISO-8601 date only
			if (targetType == typeof(DateTime))
			{
				if (DateTime.TryParseExact(raw, "yyyy-MM-dd",
					CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
				{
					result = dt;
					return true;
				}

				errorMessage = "Expected a date in YYYY-MM-DD format.";
				return false;
			}

			// Everything else: TypeDescriptor handles int, double, string, …
			try
			{
				result = TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(raw);
				return true;
			}
			catch
			{
				errorMessage = $"'{raw}' is not a valid {FriendlyTypeName(targetType)}.";
				return false;
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Console helpers
		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private static void WriteBanner()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine();
			Console.WriteLine("  ╔══════════════════════════════════╗");
			Console.WriteLine("  ║       OneMore CLI Runner         ║");
			Console.WriteLine("  ╚══════════════════════════════════╝");
			Console.WriteLine();
			Console.ResetColor();
		}

		private static void WriteSuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		private static void WriteWarning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		private static void WriteError(Exception exc)
		{
			// ignore assembly load exceptions
			if (exc is System.IO.FileLoadException) { return; }
			WriteError(exc.FormatDetails());
		}

		private static void WriteError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error: {message}");
			Console.ResetColor();
		}

		private static void WriteInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		private static string FriendlyTypeName(Type type)
		{
			if (type == typeof(int))      return "integer";
			if (type == typeof(double))   return "decimal";
			if (type == typeof(bool))     return "yes/no";
			if (type == typeof(DateTime)) return "date";
			return type.Name.ToLowerInvariant();
		}

		private static bool IsHelpFlag(string arg) =>
			arg.Equals("--help", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("-h", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("/?", StringComparison.OrdinalIgnoreCase);
	}
}
