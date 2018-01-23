//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	internal class InsertTocCommand : Command
	{
		private class Heading
		{
			public string Text;
			public string Link;
			public int Level;
		}


		private XElement page;
		private XNamespace ns;


		public InsertTocCommand () : base()
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
				logger.WriteLine("Error executing InsertTocCommand", exc);
			}
		}


		private void _Execute ()
		{
			using (var manager = new ApplicationManager())
			{
				page = manager.CurrentPage();
				if (page != null)
				{
					ns = page.GetNamespaceOfPrefix("one");

					Evaluate(manager);

					manager.UpdatePageContent(page);
				}
			}
		}


		private void Evaluate (ApplicationManager manager)
		{
			System.Diagnostics.Debugger.Launch();


			var headings = new List<Heading>();
			var templates = GetHeadingTemplates();

			// get page.ObjectId for hyperlinks
			var pid = page.Attribute("ID")?.Value;

			// Find all OEs that might be headings: must contain one T block; if there are more
			// than one T block then only allow the OE if those Ts represent current selection

			var candidates =
				from e in page.Element(ns + "Outline").Element(ns + "OEChildren").Elements(ns + "OE")
				let n = e.Elements(ns + "T").Count()
				where n == 1 || (n <= 3 && e.Elements(ns + "T").Attributes("selected").Any(a => a.Value.Equals("all")))
				select e;

			if (candidates?.Count() > 0)
			{
				foreach (var candidate in candidates)
				{
					CssInfo info = null;

					var phrases = candidate.Elements(ns + "T").Select(e => new Phrase(e));
					foreach (var phrase in phrases)
					{
						if (!phrase.IsEmpty)
						{
							info = phrase.GetStyleInfo();
							if (info != null)
							{
								// collect from one:T
								CollectFromSpan(phrase.CData.Parent, info);
							}
						}
					}

					if (info != null)
					{
						// collect from one:OE
						CollectFromObjectElement(candidate, info, templates);

						var template = templates.Where(t => t.Matches(info)).FirstOrDefault();
						if (template != null)
						{
							string link = null;
							if (pid != null)
							{
								var oid = candidate.Attribute("objectID")?.Value;
								if (oid != null)
								{
									manager.Application.GetHyperlinkToObject(pid, oid, out link);
								}
							}

							var heading = new Heading()
							{
								Text = ClearFormatting(candidate.Value),
								Link = link,
								Level = template.Level
							};

							logger.WriteLine($"Heading => {heading.Text} ({heading.Level})");
							headings.Add(heading);
						}
					}
				}
			}

			InsertToc(headings);
		}


		#region GetHeadingTemplates
		private List<IStyleInfo> GetHeadingTemplates ()
		{
			var templates = new List<IStyleInfo>();

			// collect Heading quick style defs (h1, h2, h3, ...)

			var quickdefs =
				from e in page.Elements(ns + "QuickStyleDef")
				let n = e.Attribute("name")
				where (n != null) && (Regex.IsMatch(n.Value, @"h\d")) // || n.Value.Equals("PageTitle"))
				select new QuickInfo(e);

			if (quickdefs?.Count() > 0)
			{
				int level = 0;
				foreach (var def in quickdefs)
				{
					def.Level = level++;
					templates.Add(def);
				}
			}

			// collect custom heading styles

			var customs = new StylesProvider()
				.Filter(e => e.Attributes("isHeading").Any(a => a.Value.ToLower().Equals("true")));

			if (customs?.Count() > 0)
			{
				var level = 0;
				foreach (var custom in customs)
				{
					templates.Add(new CustomInfo(custom) { Level = level++ });
				}
			}

			return templates;
		}
		#endregion GetHeadingTemplates


		private void CollectFromSpan (XElement element, CssInfo info)
		{
			var spanstyle = element.Attributes("style").Select(a => a.Value).FirstOrDefault();
			if (spanstyle != null)
			{
				info.Collect(spanstyle);
			}
		}


		private void CollectFromObjectElement (XElement element, CssInfo info, List<IStyleInfo> templates)
		{
			CollectFromSpan(element, info);

			var a = element.Attribute("quickStyleIndex");
			if (a != null)
			{
				var template = templates.Where(e => a.Value.Equals(e.Index)).FirstOrDefault();
				if (template != null)
				{
					info.Collect(template);
				}
			}

			a = element.Attribute("spaceBefore");
			if (a != null)
			{
				info.SpaceBefore = a.Value;
			}

			a = element.Attribute("spaceAfter");
			if (a != null)
			{
				info.SpaceAfter = a.Value;
			}
		}


		public string ClearFormatting (string text)
		{
			var clear = string.Empty;

			var ctext = text.Replace("<br>", "<br/>");
			var wrap = XElement.Parse("<w>" + ctext + "</w>");
			foreach (var node in wrap.Nodes())
			{
				if (node.NodeType == XmlNodeType.Text)
					clear += node.ToString();
				else
					clear += ((XElement)node).Value;
			}

			return clear;
		}


		private void InsertToc (List<Heading> headings)
		{
			var top = page.Element(ns + "Outline")?.Element(ns + "OEChildren");
			if (top == null)
			{
				return;
			}

			var title = new XElement(ns + "OE",
				new XAttribute("style", "font-size:16.0pt"),
				new XElement(ns + "T",
					new XCData("<span style='font-weight:bold'>Table of Contents</span>")
					)
				);

			// create a temporary root node, to be thrown away
			var content = new XElement(ns + "OEChildren");

			if (headings.Count > 0)
			{
				// build the hierarchical heading list
				BuildContent(content, headings, 0, 0);

				logger.WriteLine(content.ToString(SaveOptions.None));
			}

			// empty line after the TOC
			content.Add(new XElement(ns + "OE",
				new XElement(ns + "T", new XCData(string.Empty))
				));

			top.AddFirst(title, content.Nodes());
		}


		private int BuildContent (XElement content, List<Heading> headings, int index, int level)
		{
			while ((index < headings.Count) && (headings[index].Level >= level))
			{
				var heading = headings[index];

				if (heading.Level == level)
				{
					string text;
					if (heading.Link == null)
					{
						text = heading.Text;
					}
					else
					{
						text = $"<a href=\"{heading.Link}\">{heading.Text}</a>";
					}

					content.Add(new XElement(ns + "OE",
						new XElement(ns + "T", new XCData(text)))
						);

					index++;
				}
				else if (heading.Level > level)
				{
					var children = new XElement(ns + "OEChildren");
					index = BuildContent(children, headings, index, heading.Level);

					var elements = content.Elements(ns + "OE");
					if (elements == null)
					{
						content.Add(children);
					}
					else
					{
						content.Elements(ns + "OE").Last().Add(children);
					}
				}
			}

			return index;
		}
	}
}
