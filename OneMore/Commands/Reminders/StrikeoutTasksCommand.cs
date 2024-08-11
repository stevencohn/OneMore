﻿//************************************************************************************************
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

			var elements = page.Root.Descendants(ns + "Tag")
				.Where(e => indexes.Contains(e.Attribute("index").Value));

			if (!elements.Any())
			{
				return;
			}

			strikeColor = Settings.ColorsSheet.GetStrikethroughForeColor() ?? Style.Automatic;

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
				await one.Update(page);
			}
		}


		private bool RestyleText(XCData cdata, bool completed)
		{
			var modified = false;
			var wrapper = cdata.GetWrapper();
			var span = wrapper.Elements("span").FirstOrDefault(e => e.Attribute("style") != null);

			if (completed)
			{
				// complete, so apply strikethrough...

				if (span == null)
				{
					// no inline styling, so rewrap text with new span...

					var style = new Style
					{
						Color = strikeColor,
						IsStrikethrough = true
					};

					span = new XElement("span", cdata.Value);
					span.SetAttributeValue("style", style.ToCss(false));
					wrapper.FirstNode.ReplaceWith(span);

					modified = true;
				}
				else
				{
					// modify existing inline styling...

					var style = new Style(span.Attribute("style").Value);
					if (!style.IsStrikethrough)
					{
						style.IsStrikethrough = true;
						style.Color = strikeColor;

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
				// not complete, so remove strikethrough...

				if (span != null)
				{
					var style = new Style(span.Attribute("style").Value);
					if (style.IsStrikethrough)
					{
						style.IsStrikethrough = false;
						if (style.Color == strikeColor)
						{
							style.Color = Style.Automatic;
						}

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

			cdata.Value = wrapper.GetInnerXml().Replace("&amp;", "&");

			return modified;
		}
	}
}
/*
![CDATA[<span style='text-decoration:line-through'>Line OneOneOne<br>asdfasf</span>]]/
*/
