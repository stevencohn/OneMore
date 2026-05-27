//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// Parses command-line arguments into a name→value dictionary.
	/// </summary>
	internal static class ArgParser
	{
		/// <summary>
		/// Returns <c>true</c> if <paramref name="arg"/> is one of the recognised
		/// help flags: <c>--help</c>, <c>-h</c>, or <c>/?</c>.
		/// </summary>
		public static bool IsHelpFlag(string arg) =>
			arg.Equals("--help", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("-h", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("/?", StringComparison.OrdinalIgnoreCase);


		/// <summary>
		/// Parses <c>--name value</c>, <c>--name=value</c>, and bare <c>--flag</c>
		/// (treated as boolean <c>true</c>) from an args array.
		/// Names are matched case-insensitively.
		/// </summary>
		public static bool TryParse(
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
	}
}
