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


	/// <summary>
	/// Console output helpers and help-text display for the OneMore CLI.
	/// </summary>
	internal static class CliConsole
	{
		public static readonly string Separator = new('─', 60);


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Help output

		public static void ShowHelp(List<ICliCommand> commands)
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


		public static void ShowCommandHelp(ICliCommand command)
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
		// Write helpers

		public static void WriteBanner()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine();
			Console.WriteLine("  ╔══════════════════════════════════╗");
			Console.WriteLine("  ║       OneMore CLI Runner         ║");
			Console.WriteLine("  ╚══════════════════════════════════╝");
			Console.WriteLine();
			Console.ResetColor();
		}

		public static void WriteSuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void WriteWarning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void WriteError(Exception exc)
		{
			// ignore assembly load exceptions
			if (exc is System.IO.FileLoadException) { return; }
			WriteError(exc.FormatDetails());
		}

		public static void WriteError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error: {message}");
			Console.ResetColor();
		}

		public static void WriteInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(message);
			Console.ResetColor();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Type helpers

		public static string FriendlyTypeName(Type type)
		{
			if (type == typeof(int))      return "integer";
			if (type == typeof(double))   return "decimal";
			if (type == typeof(bool))     return "yes/no";
			if (type == typeof(DateTime)) return "date";
			return type.Name.ToLowerInvariant();
		}
	}
}
