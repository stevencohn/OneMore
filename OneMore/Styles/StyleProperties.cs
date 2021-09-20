//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System.Collections.Generic;


	/// <summary>
	/// Simple typedef and extension of a Dictionary to manage style properties
	/// </summary>
	internal class StyleProperties : Dictionary<string, string>
	{

		/// <summary>
		/// Add given properties to current collection without overriding.
		/// Used within StyleAnalyzer to aggregate styles of nested elements on the page.
		/// </summary>
		/// <param name="other">Properties to add to this collection</param>
		public StyleProperties Add(Dictionary<string, string> other)
		{
			foreach (var key in other.Keys)
			{
				if (!ContainsKey(key))
				{
					Add(key, other[key]);
				}
			}

			return this;
		}
	}
}