//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System.Collections.Generic;
	using System.Text.RegularExpressions;


	// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
	// Rule...

	/// <summary>
	/// Defines a single rule for the language such as comments, string, or keywords.
	/// </summary>
	internal interface IRule
	{
		/// <summary>
		/// Gets a regular expression that defines what the rule matches and captures
		/// </summary>
		string Pattern { get; }


		/// <summary>
		/// Gets the current scope override, used for multi-line patterns such as multi-line
		/// comments. Default scope is string.Empty
		/// </summary>
		string Scope { get; }


		/// <summary>
		/// Gets the scope of each capture in the regular expression
		/// </summary>
		IList<string> Captures { get; }
	}


	/// <summary>
	/// Used only for deserialization; the interface is used thereafter
	/// </summary>
	internal class Rule : IRule
	{
		private string scope;
		private List<string> captures;


		public Rule()
		{
		}

		public string Pattern { get; set; }

		public string Scope
		{
			get => scope;
			set => scope = value.ToLower();
		}


		public IList<string> Captures
		{
			get => captures;

			// standardize on lowercase names because users are stupid
			set => captures = ((List<string>)value).ConvertAll(s => s.ToLower());
		}
	}


	// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
	// Language...

	/// <summary>
	/// Defines how to parse the source code of a given language
	/// </summary>
	internal interface ILanguage
	{
		/// <summary>
		/// Gets the friendly name of the language
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Gets the pattern of the first line of a source file. Can be null.
		/// Useful for languages such as XML or ASP.NET with a declarative first line
		/// </summary>
		string PreamblePattern { get; }


		/// <summary>
		/// Get the list of rules that define the language
		/// </summary>
		List<IRule> Rules { get; }
	}


	/// <summary>
	/// Defines the extended properties for a compiled language including its compiled
	/// regular expression and the ordered list of scopes expected in that expression.
	/// </summary>
	internal interface ICompiledLanguage : ILanguage
	{
		/// <summary>
		/// The compiled pattern matching expression, combined from all language rules
		/// </summary>
		Regex Regex { get; }


		/// <summary>
		/// The ordered list of scopes in the compiled regular expression
		/// </summary>
		IList<string> Scopes { get; }
	}


	/// <summary>
	/// Used only for deserialization (and a little sneaky use in the compiler).
	/// The interface is used thereafter.
	/// </summary>
	internal class Language : ICompiledLanguage
	{
		public Language()
		{
		}


		// Definition...

		public string Name { get; set; }

		public string PreamblePattern { get; set; }

		public List<IRule> Rules { get; set; }

		// Compilation...

		public Regex Regex { get; set; }

		public IList<string> Scopes { get; set; }
	}
}
