//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Toggles strikethrough text next to all completed/incompleted tags
	/// </summary>
	internal class StrikeoutTasksCommand : Command
	{
		private string strikeColor;


		public StrikeoutTasksCommand()
		{
		}


		public override async Task Execute(params object[] args)
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

			await using var one = new OneNote(out var page, out var ns);

			var indexes =
				page.Root.Elements(ns + "TagDef")
				.Where(e => symbols.Contains(int.Parse(e.Attribute("symbol").Value)))
				.Select(e => e.Attribute("index").Value)
				.ToList();

			if (indexes.Count == 0)
			{
				return;
			}

			var tags = page.Root.Descendants(ns + "Tag")
				.Where(e => indexes.Contains(e.Attribute("index").Value));

			if (!tags.Any())
			{
				return;
			}

			strikeColor = Settings.ColorsSheet.GetStrikethroughForeColor() ?? Style.Automatic;

			var modified = false;

			foreach (var tag in tags)
			{
				tag.GetAttributeValue<bool>("completed", out var completed);

				// each OE can only have 0..1 cdata but there can be more than one run after
				// the tag if the text caret is position on the paragraph so must enumerate

				var cdatas = tag.NodesAfterSelf().OfType<XElement>()
					.Where(e => e.Name.LocalName == "T")
					.Nodes().OfType<XCData>()
					.ToList();

				foreach (var cdata in cdatas)
				{
					if (!string.IsNullOrEmpty(cdata.Value)) // use Empty! not whitespace
					{
						modified |= completed
							? RestyleWithStrikethrough(cdata)
							: RestyleWithoutStrikethrough(cdata);
					}
				}
			}

			if (modified)
			{
				await one.Update(page);
			}
		}


		private bool RestyleWithStrikethrough(XCData cdata)
		{
			var modified = false;
			var wrapper = cdata.GetWrapper();

			var basicCss = new Style
			{
				Color = strikeColor,
				IsStrikethrough = true,
				ApplyColors = true
			}
			.ToCss(withColor: true);

			foreach (var node in wrapper.Nodes().ToList())
			{
				if (node is XText text)
				{
					text.ReplaceWith(new XElement("span",
						new XAttribute("style", basicCss),
						text));

					modified = true;
				}
				else if (node is XElement span && span.Name.LocalName == "span")
				{
					// span always and only have a style attribute
					var style = new Style(span.Attribute("style").Value);
					if (!style.IsStrikethrough)
					{
						style.IsStrikethrough = true;
						style.Color = strikeColor;
						style.ApplyColors = true;
						span.SetAttributeValue("style", style.ToCss(withColor: true));

						modified = true;
					}
				}
			}

			if (modified)
			{
				if (cdata.Parent.Attribute("style") is XAttribute attribute)
				{
					var style = new Style(attribute.Value);
					style.Color = strikeColor;
					style.ApplyColors = true;
					attribute.Value = style.ToCss(withColor: true);
				}

				cdata.Value = wrapper.GetInnerXml().Replace("&amp;", "&");
			}

			return modified;
		}


		private bool RestyleWithoutStrikethrough(XCData cdata)
		{
			var modified = false;
			var wrapper = cdata.GetWrapper();

			var emptyStyle = new Style();

			// target only XElement/span nodes, ignore XText nodes
			foreach (var span in wrapper
				.Nodes().OfType<XElement>()
				.Where(n => n.Name.LocalName == "span").ToList())
			{
				// span always and only have a style attribute
				var style = new Style(span.Attribute("style").Value);
				if (style.IsStrikethrough)
				{
					style.IsStrikethrough = false;

					// this will override any inline span styling with the CData
					style.Color = Style.Automatic;

					if (style.Equals(emptyStyle))
					{
						span.ReplaceWith(new XText(span.Value));
					}
					else
					{
						span.SetAttributeValue("style", style.ToCss(withColor: true));
					}
				}

				modified = true;
			}

			if (modified)
			{
				cdata.Value = wrapper.GetInnerXml().Replace("&amp;", "&");
			}

			return modified;
		}
	}
}
/*
![CDATA[<span style='text-decoration:line-through'>Line OneOneOne<br>asdfasf</span>]]/
*/
