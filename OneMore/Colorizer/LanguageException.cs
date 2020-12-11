//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3871 // Exception types should be "public"

namespace River.OneMoreAddIn.Colorizer
{
	using System;


	/// <summary>
	/// Custom exception for formula evaluation errors
	/// </summary>
	internal class LanguageException : Exception
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Message that describes this exception</param>
		/// <param name="name">The name of the language</param>
		/// <param name="rule">The index of the offending rule in the ruleset</param>
		public LanguageException(string message, string name, int rule) : base(message)
		{
			Name = name;
			Rule = rule;
		}


		/// <summary>
		/// Gets or sets the name of the language in question
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the index of the suspect rule or -1 if exception is not
		/// associated with a rule
		/// </summary>
		public int Rule { get; set; }
	}
}