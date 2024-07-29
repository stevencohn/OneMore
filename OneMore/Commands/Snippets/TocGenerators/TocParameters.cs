//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using System.Collections.Generic;


	/// <summary>
	/// A bag of strings containing parameters that describe how to construct a table of
	/// contents for a scope. Scopes can be Page, Section, or Notebook.
	/// </summary>
	internal sealed class TocParameters : List<string>
	{
		public TocParameters()
			: base()
		{
		}


		public TocParameters(IEnumerable<string> range)
			: this()
		{
			AddRange(range);
		}
	}
}
