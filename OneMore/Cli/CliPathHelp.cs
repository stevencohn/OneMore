//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;


	/// <summary>
	/// Builds the instructive text shown when a CLI notebook/section/page path cannot be resolved.
	/// </summary>
	public static class CliPathHelp
	{
		public const string HelpUrl = "https://onemoreaddin.com/the-basics/OneMore%20CLI.htm";

		/// <summary>
		/// The start of every <see cref="PathNotFound"/> message; lets the CLI console
		/// recognize the message when it arrives as command output and show it as an error
		/// </summary>
		public const string NotFoundPrefix = "Path not found:";


		/// <summary>
		/// Returns the syntax hint and the help link, one sentence per line, for example:
		/// Use the syntax: onemore inserttoc --notebook "name" --section "group/subgroup/name"
		/// --page "name".
		/// See https://...
		/// </summary>
		/// <param name="commandName">
		/// The command name, with or without the trailing "Command" suffix
		/// </param>
		/// <param name="withPage">
		/// True to include the --page parameter in the syntax, otherwise false
		/// </param>
		public static string Hint(string commandName, bool withPage = true)
		{
			var page = withPage ? " --page \"name\"" : string.Empty;

			return "Use the syntax: onemore " + Normalize(commandName) +
				" --notebook \"name\" --section \"group/subgroup/name\"" + page + ".\n" +
				"See " + HelpUrl;
		}


		/// <summary>
		/// Returns "Path not found: [path]." followed by the hint, one sentence per line
		/// </summary>
		public static string PathNotFound(string path, string commandName, bool withPage = true)
		{
			return $"{NotFoundPrefix} {path}.\n{Hint(commandName, withPage)}";
		}


		private static string Normalize(string commandName)
		{
			if (string.IsNullOrWhiteSpace(commandName))
			{
				return "[command]";
			}

			var name = commandName.Trim();
			if (name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) &&
				name.Length > "Command".Length)
			{
				name = name.Substring(0, name.Length - "Command".Length);
			}

			return name.ToLowerInvariant();
		}
	}
}
