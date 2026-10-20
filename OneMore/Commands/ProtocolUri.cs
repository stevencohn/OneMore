//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;


	/// <summary>
	/// Parses onemore:// protocol requests of the form onemore://DoitCommand/arg1/arg2/arg3.
	/// Shared by the named-pipe CommandService and the shell DelegateExecute handler so both
	/// activation paths apply identical validation.
	/// </summary>
	internal static class ProtocolUri
	{
		public const string Scheme = "onemore://";

		// action flows into Type.GetType, which accepts assembly-qualified names
		// ("Foo, SomeAssembly") and nested-type syntax ("Outer+Inner"); require a
		// plain identifier so neither can be smuggled in via the URL
		private static readonly Regex ActionPattern =
			new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);


		/// <summary>
		/// Determines whether the name is a plain command identifier.
		/// </summary>
		public static bool IsValidAction(string action)
		{
			return !string.IsNullOrEmpty(action) && ActionPattern.IsMatch(action);
		}


		/// <summary>
		/// Splits the request into a command name and URL-decoded arguments.
		/// </summary>
		/// <param name="uri">The full onemore:// request</param>
		/// <param name="action">The command name, if valid</param>
		/// <param name="arguments">The decoded arguments, possibly empty</param>
		/// <returns>
		/// True if the request has the onemore:// scheme and a valid command name
		/// </returns>
		public static bool TryParse(string uri, out string action, out string[] arguments)
		{
			action = null;
			arguments = Array.Empty<string>();

			if (string.IsNullOrEmpty(uri) ||
				!uri.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			var parts = uri.Substring(Scheme.Length)
				.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 0 || !IsValidAction(parts[0]))
			{
				return false;
			}

			action = parts[0];
			arguments = parts.Skip(1).Select(p => HttpUtility.UrlDecode(p)).ToArray();
			return true;
		}
	}
}
