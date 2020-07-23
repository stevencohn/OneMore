//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System.Linq;


	public partial class AddIn
	{
		private static bool bodyContext = false;


		/// <summary>
		/// Analyze the current page and determine if the text cursor is positioned in the body
		/// of the page or in the title.
		/// </summary>
		/// <param name="control">
		/// The menu item invoking the action. This should be done by one "controller" item so
		/// optimize performance. All other menu items can rely on the GetBodyContext method.
		/// </param>
		/// <returns>True</returns>
		public bool SetBodyContext(IRibbonControl control)
		{
			//Logger.Current.WriteLine($"SetBodyContext {control.Id}");

			Page page;
			using (var manager = new ApplicationManager()) { page = new Page(manager.CurrentPage()); }

			// set the context for the getters
			bodyContext = page.ConfirmBodyContext();

			// the setter always returns true; the getter will return bodyContext
			return true;
		}


		/// <summary>
		/// Gets a Boolean value indicating whether the text cursor is positioned in the body of the page.
		/// </summary>
		/// <param name="control">The menu item invoking the action.</param>
		/// <returns>True if the text cursor is position in the body.</returns>
		public bool GetBodyContext(IRibbonControl control)
		{
			//Logger.Current.WriteLine($"GetBodyContext {control.Id}");

			ribbon.Invalidate();
			return bodyContext;
		}


		/// <summary>
		/// Analyze the current page selections and determined if at least two pages are selected.
		/// </summary>
		/// <param name="control">The menu item invoking the action.</param>
		/// <returns>True if two or more pages are selected.</returns>
		public bool GetMultiPageContext(IRibbonControl control)
		{
			//Logger.Current.WriteLine($"GetMultiPageContext {control.Id}");

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
