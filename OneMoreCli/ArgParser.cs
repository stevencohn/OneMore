//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


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
		/// Scans <paramref name="args"/> for a global <c>--name value</c> or <c>--name=value</c>
		/// option, removes the matched token(s), and returns the value along with the
		/// remaining args. Unlike <see cref="TryParse"/>, this can match the option anywhere
		/// in the array, not just in a fixed position, since global options like
		/// <c>--output</c> may appear before or after the command name.
		/// </summary>
		/// <returns>
		/// <c>false</c> if the option was present but given no value; <c>true</c> otherwise
		/// (including when the option was not found at all, in which case
		/// <paramref name="value"/> is <c>null</c> and <paramref name="remaining"/> is
		/// unchanged).
		/// </returns>
		public static bool ExtractGlobalOption(
			string[] args, string name, out string value, out string[] remaining)
		{
			value = null;
			remaining = args;

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				var nameAndValue = arg.TrimStart('-');

				var eqIndex = nameAndValue.IndexOf('=');
				var candidateName = eqIndex >= 0 ? nameAndValue.Substring(0, eqIndex) : nameAndValue;

				if (!arg.StartsWith("-") || !candidateName.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (eqIndex >= 0)
				{
					value = StripSurroundingQuotes(nameAndValue.Substring(eqIndex + 1));
					remaining = args.Where((_, idx) => idx != i).ToArray();
					return true;
				}

				if (i + 1 >= args.Length)
				{
					return false;
				}

				value = StripSurroundingQuotes(args[i + 1]);
				remaining = args.Where((_, idx) => idx != i && idx != i + 1).ToArray();
				return true;
			}

			return true;
		}


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
