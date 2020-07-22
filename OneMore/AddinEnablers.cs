//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System.Linq;
	using System.Xml.Linq;


	public partial class AddIn
	{

		/// <summary>
		/// Check if focus is currently on the content of the current page.
		/// </summary>
		/// <param name="control"></param>
		/// <returns>True if the context is correct; false otherwise</returns>
		public bool EnsureBodyContext(IRibbonControl control)
		{
			if (clockSpeed < ReasonableClockSpeed)
			{
				// short-circuit the tests and always enable menu items.
				// command handlers must do their own error checking!
				return true;
			}

			XElement page;
			using (var manager = new ApplicationManager()) { page = manager.CurrentPage(); }

			var ns = page.GetNamespaceOfPrefix("one");

			var found = page.Elements(ns + "Outline")?
				.Descendants(ns + "T")?
				.Attributes("selected").Any(a => a.Value.Equals("all"));

			ribbon.Invalidate();

			return found ?? true;
		}


		/// <summary>
		/// Check if there are more than one pages selected in the current section
		/// </summary>
		/// <param name="control"></param>
		/// <returns>True if more than one page is selected; false otherwise</returns>
		public bool EnsureMultiPageSelections(IRibbonControl control)
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				var ns = section.GetNamespaceOfPrefix("one");

				var count =
					section.Elements(ns + "Page")
					.Count(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

				return count > 1;
			}
		}
	}
}
