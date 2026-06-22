//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;


	/// <summary>
	/// Handles the interactive command menu, parameter collection, and type conversion
	/// for the OneMore CLI.
	/// </summary>
	internal static class InteractiveRunner
	{
		/// <summary>
		/// Displays the command menu and returns the user's selection — by list number or by
		/// command name — or <c>null</c> if the user chooses to quit.
		/// </summary>
		public static ICliCommand PromptForCommand(List<ICliCommand> commands)
		{
			while (true)
			{
				Console.WriteLine(CliConsole.Separator);
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
				Console.Write("Select a command by number or name (or Q to quit): ");
				var input = Console.ReadLine()?.Trim() ?? string.Empty;

				if (string.IsNullOrEmpty(input)
					|| input.Equals("q", StringComparison.OrdinalIgnoreCase)
					|| input.Equals("quit", StringComparison.OrdinalIgnoreCase))
					return null;

				if (int.TryParse(input, out var choice) && choice >= 1 && choice <= commands.Count)
					return commands[choice - 1];

				var byName = commands.FirstOrDefault(c =>
					c.CommandName.Equals(input, StringComparison.OrdinalIgnoreCase));
				if (byName != null)
					return byName;

				CliConsole.WriteWarning("Invalid selection — enter a number or name from the list.");
				Console.WriteLine();
			}
		}


		/// <summary>
		/// Builds a <see cref="CliParameterSet"/> for <paramref name="command"/>.
		/// <para>
		/// For each parameter in the command's definition:
		/// <list type="bullet">
		///   <item>If a value was pre-supplied via <paramref name="supplied"/>, it is
		///         validated and used directly; an invalid value is an error (not re-prompted).</item>
		///   <item>Otherwise, if the command needs interactive prompting at all — either because
		///         it was launched from a blank slate (<paramref name="supplied"/> is <c>null</c>)
		///         or because at least one required parameter is still missing — the user is
		///         walked through every remaining parameter, required <i>and</i> optional.</item>
		///   <item>Otherwise (all required parameters already satisfied), any remaining optional
		///         parameter silently takes its default value with no prompt.</item>
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
		public static CliParameterSet CollectParameters(
			ICliCommand command,
			Dictionary<string, string> supplied)
		{
			var definition = command.DefineParameters();
			if (!definition.Any())
				return new CliParameterSet();

			var interactiveOrigin = supplied == null;
			supplied ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var needsInteraction = interactiveOrigin
				|| definition.Any(p => p.Required && !supplied.ContainsKey(p.Name));

			if (needsInteraction)
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
				if (supplied.TryGetValue(parameter.Name, out var raw))
				{
					if (!TryConvert(raw, parameter.DataType, out var converted, out var convertError))
					{
						CliConsole.WriteError($"--{parameter.Name}: {convertError}");
						return null;
					}

					if (!parameter.TryValidate(converted, out var validationError))
					{
						CliConsole.WriteError($"--{parameter.Name}: {validationError}");
						return null;
					}

					set.Set(parameter.Name, converted);
					continue;
				}

				// Not supplied, and the command doesn't need interaction → use default - - - -
				if (!needsInteraction)
				{
					if (parameter.DefaultValue != null)
						set.Set(parameter.Name, parameter.DefaultValue);
					continue;
				}

				// Not supplied, walking the user through this command's parameters - - - - - -
				if (!interactiveOrigin && parameter.Required)
				{
					Console.WriteLine();
					CliConsole.WriteInfo($"Required parameter '--{parameter.Name}' was not provided.");
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
		public static bool PromptForParameter(CliParameter parameter, out object value)
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
				else if (parameter.DataType == typeof(bool))
				{
					// Switch: Y/N prompt with the default capitalized - - - - - - - - - - - - - -

					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write($"  {parameter.Name}");
					Console.ResetColor();
					Console.Write($"  —  {parameter.Description}");

					var hint = "y/n";
					if (!parameter.Required && parameter.DefaultValue is bool defaultBool)
						hint = defaultBool ? "Y/n" : "y/N";

					Console.Write($"  [{hint}]: ");
				}
				else
				{
					// All other types - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

					var qualifier = parameter.Required ? string.Empty : ", optional";
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write($"  {parameter.Name} ({CliConsole.FriendlyTypeName(parameter.DataType)}{qualifier})");
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

					CliConsole.WriteWarning("  This parameter is required — please enter a value.");
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
					CliConsole.WriteWarning($"  {convertError}");
					continue;
				}

				if (!parameter.TryValidate(converted, out var validationError))
				{
					CliConsole.WriteWarning($"  {validationError}");
					continue;
				}

				value = converted;
				return true;
			}
		}


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
				errorMessage = $"'{raw}' is not a valid {CliConsole.FriendlyTypeName(targetType)}.";
				return false;
			}
		}
	}
}
