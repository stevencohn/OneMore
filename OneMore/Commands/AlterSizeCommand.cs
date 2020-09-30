//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class AlterSizeCommand : Command
	{
		private XElement page;
		private XNamespace ns;
		private int delta;
		private bool selected;


		public AlterSizeCommand() : base()
		{
		}


		public void Execute(int delta)
		{
			this.delta = delta; // +/-1

			try
			{
				using (var manager = new ApplicationManager())
				{
					page = manager.CurrentPage();
					if (page != null)
					{
						// determine if range is selected or entire page

						ns = page.GetNamespaceOfPrefix("one");

						selected = page.Element(ns + "Outline").Descendants(ns + "T")
							.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")))
							.Any(e => e.GetCData().Value.Length > 0);

						var count = 0;

						if (selected)
						{
							count += AlterSelections();
						}
						else
						{
							count += AlterByName();
							count += AlterElementsByValue();
							count += AlterCDataByValue();
						}

						if (count > 0)
						{
							manager.UpdatePageContent(page);
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing AlterSizeCommand", exc);
			}
		}


		private int AlterSelections()
		{
			var elements = page.Element(ns + "Outline").Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value == "all"));

			var count = 0;

			if (!elements.IsNullOrEmpty())
			{
				var analyzer = new StyleAnalyzer(page, true);

				foreach (var element in elements)
				{
					analyzer.Clear();
					var style = new Style(analyzer.CollectStyleProperties(element));
					style.FontSize = (ParseFontSize(style.FontSize) + delta).ToString("#0.0") + "pt";

					var stylizer = new Stylizer(style);
					stylizer.ApplyStyle(element);
					count++;
				}
			}

			return count;
		}


		/*
		 * <one:QuickStyleDef index="1" name="p" fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="12.0" spaceBefore="0.0" spaceAfter="0.0" />
		 * <one:QuickStyleDef index="2" name="h2" fontColor="#2E75B5" highlightColor="automatic" font="Calibri" fontSize="14.0" spaceBefore="0.0" spaceAfter="0.0" />
		 *
		 * <one:OE alignment="left" quickStyleIndex="1" selected="partial" style="font-family:Calibri;font-size:11.0pt">
		 *   <one:List>
		 *     <one:Bullet bullet="2" fontSize="11.0" />
		 *   </one:List>
		 */

		private int AlterByName()
		{
			int count = 0;

			// find all elements that have an attribute named fontSize, e.g. QuickStyleDef or Bullet

			var elements = page.Descendants()
				.Where(p =>
					p.Attribute("name")?.Value != "PageTitle" &&
					p.Attribute("fontSize") != null &&
					(selected == (p.Attribute("selected")?.Value == "all")));

			if (!elements.IsNullOrEmpty())
			{
				foreach (var element in elements)
				{
					if (element != null)
					{
						var attr = element.Attribute("fontSize");
						if (attr != null)
						{
							if (double.TryParse(attr.Value, out var size))
							{
								attr.Value = (size + delta).ToString("#0.0");
								count++;
							}
						}
					}
				}
			}

			return count;
		}


		// <one:OE alignment="left" spaceBefore="14.0" quickStyleIndex="1" style="font-family:'Segoe UI';font-size:&#xA;20.0pt;color:#151515">

		private int AlterElementsByValue()
		{
			int count = 0;

			var elements = page.Descendants()
				.Where(p =>
					p.Parent.Name.LocalName != "Title" &&
					p.Attribute("style")?.Value.Contains("font-size:") == true);

			if (selected && !elements.IsNullOrEmpty())
			{
				elements = elements.Where(e => e.Attribute("selected")?.Value == "all");
			}

			if (!elements.IsNullOrEmpty())
			{
				foreach (var element in elements)
				{
					if (UpdateSpanStyle(element))
					{
						count++;
					}
				}
			}

			return count;
		}


		private int AlterCDataByValue()
		{
			int count = 0;

			var nodes = page.DescendantNodes().OfType<XCData>()
				.Where(n => n.Value.Contains("font-size:"));

			if (selected && !nodes.IsNullOrEmpty())
			{
				// parent one:T
				nodes = nodes.Where(n => n.Parent.Attribute("selected") != null);
			}

			if (!nodes.IsNullOrEmpty())
			{
				foreach (var cdata in nodes)
				{
					var wrapper = cdata.GetWrapper();

					var spans = wrapper.Elements("span")
						.Where(e => e.Attribute("style")?.Value.Contains("font-size") == true);

					if (!spans.IsNullOrEmpty())
					{
						foreach (var span in spans)
						{
							if (UpdateSpanStyle(span))
							{
								// unwrap our temporary <cdata>
								cdata.Value = wrapper.GetInnerXml();
								count++;
							}
						}
					}
				}
			}

			return count;
		}


		private bool UpdateSpanStyle(XElement span)
		{
			bool updated = false;

			var attr = span.Attribute("style");
			if (attr != null)
			{
				// remove encoded LF character (&#xA)
				var css = attr.Value.Replace("\n", string.Empty);

				//var properties = attr.Value.Split(';')
				//	.Select(p => p.Split(':'))
				//	.ToDictionary(p => p[0], p => p[1]);

				// Cannot use .ToDictionary(p => p[0], p => p[1]); here because there might
				// be duplicate properties, so overwrite duplicates in the dictionary

				var properties = new Dictionary<string, string>();
				var props = css.Split(';');
				foreach (var prop in props)
				{
					var parts = prop.Split(':');
					if (parts.Length > 1)
					{
						if (properties.ContainsKey(parts[0]))
						{
							properties[parts[0]] = parts[1];
						}
						else
						{
							properties.Add(parts[0], parts[1]);
						}
					}
				}

				if (properties.ContainsKey("font-size") == true)
				{
					properties["font-size"] =
						(ParseFontSize(properties["font-size"]) + delta).ToString("#0.0") + "pt";

					attr.Value =
						string.Join(";", properties.Select(p => p.Key + ":" + p.Value).ToArray());

					updated = true;
				}
			}

			return updated;
		}


		private double ParseFontSize(string size)
		{
			var match = Regex.Match(size, @"^([0-9]+(?:\.[0-9]+)?)(?:pt){0,1}");
			if (match.Success)
			{
				size = match.Groups[match.Groups.Count - 1].Value;
				if (!string.IsNullOrEmpty(size))
				{
					return double.Parse(size);
				}
			}

			return StyleBase.DefaultFontSize;
		}
	}
}
