//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class MergeCommand : Command
	{

		private const int OutlineMargin = 30;

		private Page page;
		private XNamespace ns;
		private List<QuickStyleMapping> quickmap;


		public MergeCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var section = one.GetSection();
				ns = one.GetNamespace(section);

				// find first selected - active page

				var active =
					section.Elements(ns + "Page")
					.FirstOrDefault(e => e.Attributes("isCurrentlyViewed").Any(a => a.Value.Equals("true")));

				if (active == null)
				{
					UIHelper.ShowInfo(one.Window, "At least two pages must be selected to merge");
					return;
				}

				var selections =
					section.Elements(ns + "Page")
					.Where(e =>
						!e.Attributes("isCurrentlyViewed").Any() &&
						e.Attributes("selected").Any(a => a.Value.Equals("all")));

				if (active == null)
				{
					UIHelper.ShowInfo(one.Window, "At least two pages must be selected to merge");
					return;
				}


				// get first selected (active) page and reference its quick styles, outline, size

				page = one.GetPage(active.Attribute("ID").Value);

				quickmap = page.GetQuickStyleMap();

				var offset = GetPageBottomOffset();

				// track running bottom as we add new outlines
				var maxOffset = offset;

				// find maximum z-offset
				var z = page.Root.Elements(ns + "Outline").Elements(ns + "Position")
					.Attributes("z").Max(a => int.Parse(a.Value)) + 1;

				// merge each of the subsequently selected pages into the active page

				foreach (var selection in selections.ToList())
				{
					var childPage = one.GetPage(selection.Attribute("ID").Value);

					var styles = MergeQuickStyles(childPage.Root);

					var childOutlines = childPage.Root.Elements(ns + "Outline");
					if (childOutlines == null || !childOutlines.Any())
					{
						break;
					}

					var topOffset = childOutlines.Elements(ns + "Position")
						.Min(p => double.Parse(p.Attribute("y").Value));

					foreach (var childOutline in childOutlines)
					{
						// adjust position relative to new parent page outlines
						var position = childOutline.Elements(ns + "Position").FirstOrDefault();
						var y = double.Parse(position.Attribute("y").Value) - topOffset + offset + OutlineMargin;
						position.Attribute("y").Value = y.ToString("#0.0");

						// keep track of lowest bottom
						var size = childOutline.Elements(ns + "Size").FirstOrDefault();
						var bottom = y + double.Parse(size.Attribute("height").Value);
						if (bottom > maxOffset)
						{
							maxOffset = bottom;
						}

						position.Attribute("z").Value = z.ToString();
						z++;

						// remove its IDs so the page can apply its own
						childOutline.Attributes("objectID").Remove();
						childOutline.Descendants().Attributes("objectID").Remove();

						AdjustQuickStyles(styles, childOutline);

						page.Root.Add(childOutline);
					}

					if (maxOffset > offset)
					{
						offset = maxOffset;
					}
				}

				// update page and section hierarchy

				one.Update(page);

				foreach (var selection in selections)
				{
					one.DeleteHierarchy(selection.Attribute("ID").Value);
				}
			}
		}


		private double GetPageBottomOffset()
		{
			// find bottom of current page; bottom of lowest Outline
			double offset = 0.0;
			foreach (var outline in page.Root.Elements(ns + "Outline"))
			{
				var position = outline.Elements(ns + "Position").FirstOrDefault();
				if (position != null)
				{
					var size = outline.Elements(ns + "Size").FirstOrDefault();
					if (size != null)
					{
						var bottom = double.Parse(position.Attribute("y").Value)
							+ double.Parse(size.Attribute("height").Value);

						if (bottom > offset)
						{
							offset = bottom;
						}
					}
				}
			}

			return offset;
		}


		private List<QuickStyleMapping> MergeQuickStyles(XElement childPage)
		{
			var styleRefs = childPage.Elements(ns + "QuickStyleDef")
				.Where(p => p.Attribute("name")?.Value != "PageTitle")
				.Select(p => new QuickStyleMapping(p))
				.ToList();

			// next available index; O(n) is OK here; there should only be a few
			var index = quickmap.Max(q => q.Style.Index) + 1;

			foreach (var styleRef in styleRefs)
			{
				var quickie = quickmap.Find(q => q.Style.Equals(styleRef.Style));
				if (quickie == null)
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					styleRef.Style.Index = index++;
					styleRef.Element.Attribute("index").Value = styleRef.Style.Index.ToString();
					quickmap.Add(styleRef);

					var last = page.Root.Elements(ns + "QuickStyleDef").LastOrDefault();
					if (last != null)
					{
						last.AddAfterSelf(styleRef.Element);
					}
					else
					{
						page.Root.AddFirst(styleRef.Element);
					}
				}

				// else if found then the index may differ but keep it so it can be mapped
				// to content later...
			}

			return styleRefs;
		}


		private static void AdjustQuickStyles(List<QuickStyleMapping> styles, XElement childOutline)
		{
			// reverse sort the styles so logic doesn't overwrite subsequent index references
			foreach (var style in styles.OrderByDescending(s => s.Style.Index))
			{
				if (style.OriginalIndex != style.Style.Index.ToString())
				{
					// apply new index to child outline elements
					var elements = childOutline.Descendants()
						.Where(e => e.Attribute("quickStyleIndex")?.Value == style.OriginalIndex);

					var index = style.Style.Index.ToString();
					foreach (var element in elements)
					{
						element.Attribute("quickStyleIndex").Value = index;
					}
				}
			}
		}
	}
}
