//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;


	internal class InsertTocCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public InsertTocCommand () : base()
		{
		}


		public void Execute ()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					page = new Page(manager.CurrentPage());
					if (page.IsValid)
					{
						ns = page.Namespace;

						GenerateTableOfContents(manager);

						manager.UpdatePageContent(page.Root);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		/// <summary>
		/// Generates a table of contents at the top of the current page
		/// </summary>
		/// <param name="headings"></param>
		private void GenerateTableOfContents (ApplicationManager manager)
		{
			var headings = page.GetHeadings(manager);

			var top = page.Root.Element(ns + "Outline")?.Element(ns + "OEChildren");
			if (top == null)
			{
				return;
			}

			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var textColor = dark ? "#FFFFFF" : "#000000";

			var toc = new List<XElement>
			{
				// "Table of Contents"
				new XElement(ns + "OE",
				new XAttribute("style", $"font-size:16.0pt;color:{textColor}"),
				new XElement(ns + "T",
					new XCData("<span style='font-weight:bold'>Table of Contents</span>")
					)
				)
			};

			// use the minimum intent level
			var minlevel = headings.Min(e => e.Style.Index);

			foreach (var heading in headings)
			{
				var text = new StringBuilder();
				var count = minlevel;
				while (count < heading.Style.Index)
				{
					text.Append(". . ");
					count++;
				}

				if (!string.IsNullOrEmpty(heading.Link))
				{
					var linkColor = dark ? " style='color:#5B9BD5'" : string.Empty;
					text.Append($"<a href=\"{heading.Link}\"{linkColor}>{heading.Text}</a>");
				}
				else
				{
					text.Append(heading.Text);
				}

				toc.Add(new XElement(ns + "OE",
					new XAttribute("style", $"color:{textColor}"),
					new XElement(ns + "T", new XCData(text.ToString()))
					));
			}

			// empty line after the TOC
			toc.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));

			top.AddFirst(toc);
		}
	}
}
