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


	#region Wrappers
	internal class ResetTasksCommand : EditTasksCommand
	{
		public ResetTasksCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(ResetCmd);
		}
	}

	internal class StrikeoutTasksCommand : EditTasksCommand
	{
		public StrikeoutTasksCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StrikeoutCmd);
		}
	}
	#endregion


	/// <summary>
	/// Resets completed tasks or toggles strikethrough text next to all
	/// completed/incompleted tags
	/// </summary>
	internal class EditTasksCommand : Command
	{
		protected int ResetCmd = 0;
		protected int StrikeoutCmd = 1;

		private readonly List<int> symbols;
		private readonly string strikeColor;

		private string fontColor;


		public EditTasksCommand()
		{
			// built-in checkable/completeable tags
			symbols = new List<int> {
				3,  // To do
				8,  // Client request
				12, // Schedule meeting | Callback
				28, // To do priority 1
				71, // To do priority 2
				94, // Discuss with <Person A> | B>
				95  // Discuss with manager
			};

			strikeColor = Settings.ColorsSheet.GetStrikethroughForeColor();
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length != 1)
			{
				return;
			}

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

			var modified = (int)args[0] == ResetCmd
				? ResetAllTasks(tags)
				: StrikethroughAllTasks(tags);

			if (modified)
			{
				await one.Update(page);
			}
		}


		private bool ResetAllTasks(IEnumerable<XElement> tags)
		{
			var modified = false;
			fontColor = Style.Automatic;

			foreach (var tag in tags)
			{
				tag.GetAttributeValue<bool>("completed", out var completed);
				if (completed)
				{
					tag.SetAttributeValue("completed", "false");

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
							modified |= RestyleWithoutStrikethrough(cdata);
						}
					}
				}
			}

			return modified;
		}


		private bool StrikethroughAllTasks(IEnumerable<XElement> tags)
		{
			var modified = false;
			fontColor = strikeColor ?? Style.Automatic;

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

			return modified;
		}


		private bool RestyleWithStrikethrough(XCData cdata)
		{
			var modified = false;
			var wrapper = cdata.GetWrapper();

			var basicCss = new Style
			{
				Color = fontColor,
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
						style.Color = fontColor;
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
					style.Color = fontColor;
					style.ApplyColors = true;
					attribute.Value = style.ToCss(withColor: true);
				}

				cdata.Value = wrapper.GetInnerXml().Replace("&amp;", "&");
			}

			return modified;
		}


		private bool RestyleWithoutStrikethrough(XCData cdata)
		{
			bool Unstrike(XElement span)
			{
				// span always has exactly one style attribute
				if (span.Attribute("style") is XAttribute attribute)
				{
					var style = new Style(attribute.Value);
					if (style.IsStrikethrough || (strikeColor is not null && style.Color == strikeColor))
					{
						style.IsStrikethrough = false;
						style.Color = Style.Automatic;
						span.SetAttributeValue("style", style.ToCss(withColor: true));
						return true;
					}
				}
				return false;
			}

			var modified = false;
			var wrapper = cdata.GetWrapper();

			// target only XElement/span nodes, ignore XText nodes
			foreach (var span in wrapper
				.Nodes().OfType<XElement>()
				.Where(n => n.Name.LocalName == "span").ToList())
			{
				modified |= Unstrike(span);
			}

			modified |= Unstrike(cdata.Parent.Parent); // cdata->T->OE

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
