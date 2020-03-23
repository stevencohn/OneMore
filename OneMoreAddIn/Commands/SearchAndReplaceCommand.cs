//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
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
				logger.WriteLine($"Error executing {nameof(SearchAndReplaceCommand)}", exc);
			}
		}

		private void _Execute()
		{
			DialogResult result = DialogResult.None;
			string whatText;
			string withText;
			bool matchCase;

			using (var dialog = new SearchAndReplaceDialog())
			{
				result = dialog.ShowDialog();
				dialog.Focus();

				whatText = dialog.WhatText;
				withText = dialog.WithText;
				matchCase = dialog.MatchCase;
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
						var count = 0;

						foreach (var range in ranges)
						{
							count += Replace(range, whatText, withText, matchCase);
						}

						manager.UpdatePageContent(page);

						var msg = count == 1 ? "occurance was" : "occurances were";
						MessageBox.Show($"{count} {msg} replaced", "Replaced",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
		}


		public int Replace (XElement range, string whatText, string withText, bool matchCase)
		{
			// extract TextNodes across tags (spans) and string them together to build words

			var cdata = range.DescendantNodes().OfType<XCData>()?.FirstOrDefault();
			if (string.IsNullOrEmpty(cdata?.Value))
			{
				return 0;
			}

			// nodes at this level should be either Text or span Element, one level

			var comparison = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			var data = cdata.Value.Replace("<br>", "<br/>");
			var text = XElement.Parse("<w>" + data + "</w>").Value;
			var start = text.IndexOf(whatText, comparison);
			if (start < 0)
			{
				return 0;
			}

			var builder = new StringBuilder(data.Length);
			var intag = false;
			var end = start + whatText.Length;
			var di = 0;
			var ti = 0;
			var wi = 0;

			var count = 1;

			while (di < data.Length)
			{
				var c = data[di];
				if ((c == '<') && !intag)
				{
					builder.Append(c);
					intag = true;
					di++;
				}
				else if ((c == '>') && intag)
				{
					builder.Append(c);
					intag = false;
					di++;
				}
				else if (intag)
				{
					builder.Append(c);
					di++;
				}
				else if (ti < start)
				{
					builder.Append(c);
					di++;
					ti++;
				}
				else if (ti < end)
				{
					builder.Append(withText[wi++]);
					di++;
					ti++;
				}
				else if (ti < data.Length)
				{
					start = text.IndexOf(whatText, ti, comparison);
					if (start > 0)
					{
						end = start + whatText.Length;
						wi = 0;
						count++;
					}
					else
					{
						start = end = data.Length + 1;
					}
				}
			}

			cdata.Value = builder.ToString();
			return count;
		}

	}
}
