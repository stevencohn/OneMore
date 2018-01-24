//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;


	internal class AlterSizeCommand : Command
	{
		private XElement page;
		private XNamespace ns;
		private IStyleInfo defaultInfo;


		public AlterSizeCommand () : base()
		{
		}


		public void Execute (bool increase = true)
		{
			using (var manager = new ApplicationManager())
			{
				page = manager.CurrentPage();
				if (page != null)
				{
					ns = page.GetNamespaceOfPrefix("one");

					Evaluate(increase);

					manager.UpdatePageContent(page);
				}
			}
		}


		private void Evaluate (bool increase)
		{
			var templates = GetQuickStyleDefs();

			// Find all TextRanges (one:T) - each and every one!

			var ranges = page.Elements(ns + "Outline")?.Descendants(ns + "T");
			if (ranges?.Count() > 0)
			{
				foreach (var range in ranges)
				{
					var phrase = new Phrase(range);
					if (!phrase.IsEmpty)
					{
						var info = new CssInfo();

						// collect from one:T
						CollectFromSpan(range, info);

						// collect from one:OE
						CollectFromObjectElement(range.Parent, info, templates);

						if (info.FontSize == null)
						{
							info.FontSize = defaultInfo.FontSize;
						}

						var delta = increase ? 1 : -1;
						var defaultSize = ParseSize(info.FontSize) + delta;

						var data = Rebuild(phrase, defaultSize, delta);

						range.FirstNode.ReplaceWith(new XCData(data));
					}
				}
			}
		}


		private List<IStyleInfo> GetQuickStyleDefs ()
		{
			// collect Heading quick style defs (h1, h2, h3, ...)

			var templates = new List<IStyleInfo>();

			var quickdefs = page.Elements(ns + "QuickStyleDef")?.Select(e => new QuickInfo(e));
			if (quickdefs?.Count() > 0)
			{
				templates.AddRange(quickdefs);
			}

			defaultInfo = templates.FirstOrDefault(e => e.Name.Equals("P"));
			if (defaultInfo == null)
			{
				defaultInfo = new CssInfo()
				{
					Name = "P",
					FontSize = "11.0pt"
				};

				templates.Add(defaultInfo);
			}

			return templates;
		}


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
		}


		private int ParseSize (string size)
		{
			if (size.EndsWith("pt"))
			{
				size = size.Substring(0, size.Length - 2);
			}

			return (int)double.Parse(size);
		}


		public string Rebuild (Phrase phrase, int defaultSize, int delta)
		{
			string data = string.Empty;

			var wrap = phrase.GetSanitizedWrapper();
			foreach (var node in wrap.Nodes())
			{
				if (node.NodeType == XmlNodeType.Text)
				{
					data += "<span style=font-size:" + defaultSize + "pt>" + ((XText)node).Value + "</span>";
				}
				else
				{
					var info = new CssInfo();
					CollectFromSpan((XElement)node, info);
					if (info.FontSize == null)
					{
						info.FontSize = defaultSize.ToString("#.0") + "pt";
					}
					else
					{
						info.FontSize = (ParseSize(info.FontSize) + delta).ToString("#.0") + "pt";
					}

					data += "<span style=" + info.ToCss() + ">" + ((XElement)node).Value + "</span>";
				}
			}

			return data;
		}
	}
}
