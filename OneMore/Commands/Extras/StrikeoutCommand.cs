using System;
//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class StrikeoutCommand : Command
	{
		public StrikeoutCommand()
		{
		}


		public void Execute()
		{
			// built-in checkable/completeable tags
			var symbols = new List<int> {
				3,  // To do
				8,  // Client request
				12, // Schedule meeting | Callback
				28, // To do priority 1
				71, // To do priority 2
				94, // Discuss with <Person A> | B>
				95  // Discuss with manager
			};

			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				var indexes =
					page.Root.Elements(page.Namespace + "TagDef")
					.Where(e => symbols.Contains(int.Parse(e.Attribute("symbol").Value)))
					.Select(e => e.Attribute("index").Value)
					.ToList();

				if (indexes.Count == 0)
				{
					return;
				}

				var elements = page.Root.Descendants(page.Namespace + "Tag")
					.Where(e => indexes.Contains(e.Attribute("index").Value));

				if (elements == null || !elements.Any())
				{
					return;
				}

				var modified = false;

				foreach (var element in elements)
				{
					var completed = element.Attribute("completed")?.Value == "true";

					var cdatas =
						from e in element.NodesAfterSelf().OfType<XElement>()
						where e.Name.LocalName == "T"
						let c = e.Nodes().OfType<XCData>().FirstOrDefault()
						where c != null
						select c;

					foreach (var cdata in cdatas)
					{
						if (!string.IsNullOrEmpty(cdata.Value))
						{
							modified |= RestyleText(cdata, completed);
						}
					}

					//var disabled = element.Attribute("disabled");
					//if (completed && (disabled == null || disabled.Value != "true"))
					//{
					//	element.SetAttributeValue("disabled", "true");
					//}
					//else if (!completed && (disabled != null || disabled.Value == "true"))
					//{
					//	disabled.Remove();
					//}
				}

				if (modified)
				{
					manager.UpdatePageContent(page.Root);
				}
			}
		}


		private bool RestyleText(XCData cdata, bool completed)
		{
			var modified = false;
			var wrapper = cdata.GetWrapper();
			var span = wrapper.Elements("span").FirstOrDefault(e => e.Attribute("style") != null);

			if (completed)
			{
				if (span == null)
				{
					wrapper.FirstNode.ReplaceWith(
						new XElement("span",
							new XAttribute("style", "text-decoration:line-through"),
							cdata.Value
						));

					modified = true;
				}
				else
				{
					var style = new Style(span.Attribute("style").Value);
					if (!style.IsStrikethrough)
					{
						style.IsStrikethrough = true;
						var css = style.ToCss(false);
						if (string.IsNullOrEmpty(css))
						{
							wrapper.Value = span.Value;
						}
						else
						{
							span.SetAttributeValue("style", style.ToCss(false));
						}
						modified = true;
					}
				}
			}
			else
			{
				if (span != null)
				{
					var style = new Style(span.Attribute("style").Value);
					if (style.IsStrikethrough)
					{
						style.IsStrikethrough = false;
						var css = style.ToCss(false);
						if (string.IsNullOrEmpty(css))
						{
							wrapper.Value = span.Value;
						}
						else
						{
							span.SetAttributeValue("style", style.ToCss(false));
						}
						modified = true;
					}
				}
			}

			cdata.Value = wrapper.GetInnerXml();

			//logger.WriteLine($"strike{(modified ? "!" : "")}: {cdata.Value}");

			return modified;
		}
	}
}
/*
![CDATA[<span style='text-decoration:line-through'>Line OneOneOne<br>asdfasf</span>]]/
*/
