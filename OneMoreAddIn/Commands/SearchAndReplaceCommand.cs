//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class SearchAndReplaceCommand : Command
	{
		public SearchAndReplaceCommand () : base()
		{
		}


		public void Execute ()
		{
			try
			{
				_Execute();
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing SearchAndReplaace", exc);
			}
		}

		private void _Execute()
		{
			DialogResult result = DialogResult.None;
			string whatText;
			string withText;

			using (var dialog = new SearchAndReplaceDialog())
			{
				result = dialog.ShowDialog();
				whatText = dialog.WhatText;
				withText = dialog.WithText;
			}

			if (result == DialogResult.OK)
			{
				using (var manager = new ApplicationManager())
				{
					var page = manager.CurrentPage();
					var ns = page.GetNamespaceOfPrefix("one");

					var ranges = page.Elements(ns + "Outline")?.Descendants(ns + "T")
						.Where(e => !e.DescendantNodes().OfType<XCData>().Any(d => d.Value.Equals(string.Empty)));

					if (ranges != null)
					{
						foreach (var range in ranges)
						{
							Replace(range, whatText, withText);
						}

						//manager.UpdatePageContent(page);
					}
				}
			}
		}


		private void Replace (XElement range, string whatText, string withText)
		{
			// extract TextNodes across tags (spans) and string them together to build words

			var cdata = range.DescendantNodes().OfType<XCData>().FirstOrDefault();
			if (cdata == null)
			{
				return;
			}

			// nodes at this level should be either Text or span Element, one level

			string word = string.Empty;


			var wrap = XElement.Parse("<w>" + cdata.Value + "</w>");
			foreach (var node in wrap.Nodes())
			{
			}
		}
	}
}
