//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class MergeCommand : Command
	{

		private const int OutlineMargin = 30;


		private class QuickRef
		{
			public XElement Element { get; private set; }
			public QuickStyleDef Style { get; private set; }
			public string OriginalIndex { get; private set; }

			public QuickRef(XElement element)
			{
				Element = element;
				Style = new QuickStyleDef(element);
				OriginalIndex = Style.Index.ToString();
			}
		}


		private XElement page;
		private XNamespace ns;
		private List<QuickRef> quickies;


		public MergeCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				ns = section.GetNamespaceOfPrefix("one");

				// find first selected - active page

				var active =
					section.Elements(ns + "Page")
					.FirstOrDefault(e => e.Attributes("isCurrentlyViewed").Any(a => a.Value.Equals("true")));

				if (active == null)
				{
					UIHelper.ShowMessage(manager.Window, "At least two pages must be selected to merge");
					return;
				}

				var selections =
					section.Elements(ns + "Page")
					.Where(e =>
						!e.Attributes("isCurrentlyViewed").Any() &&
						e.Attributes("selected").Any(a => a.Value.Equals("all")));

				if (active == null)
				{
					UIHelper.ShowMessage(manager.Window, "At least two pages must be selected to merge");
					return;
				}


				// get first selected (active) page and reference its quick styles, outline, size

				page = manager.GetPage(active.Attribute("ID").Value);

				quickies = page.Elements(ns + "QuickStyleDef")
					.Select(p => new QuickRef(p))
					.ToList();

				var offset = GetPageBottomOffset();

				// track running bottom as we add new outlines
				var maxOffset = offset;

				// find maximum z-offset
				var z = page.Elements(ns + "Outline").Elements(ns + "Position")
					.Attributes("z").Max(a => int.Parse(a.Value)) + 1;

				// merge each of the subsequently selected pages into the active page

				foreach (var selection in selections.ToList())
				{
					var childPage = manager.GetPage(selection.Attribute("ID").Value);

					var styles = MergeQuickStyles(childPage);

					var childOutlines = childPage.Elements(ns + "Outline");
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

						page.Add(childOutline);
					}

					if (maxOffset > offset)
					{
						offset = maxOffset;
					}
				}

				// update page and section hierarchy

				manager.UpdatePageContent(page);

				foreach (var selection in selections)
				{
					manager.DeleteHierarchy(selection.Attribute("ID").Value);
				}
			}
		}


		private double GetPageBottomOffset()
		{
			// find bottom of current page; bottom of lowest Outline
			double offset = 0.0;
			foreach (var outline in page.Elements(ns + "Outline"))
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


		private List<QuickRef> MergeQuickStyles(XElement childPage)
		{
			var styleRefs = childPage.Elements(ns + "QuickStyleDef")
				.Where(p => p.Attribute("name")?.Value != "PageTitle")
				.Select(p => new QuickRef(p))
				.ToList();

			// next available index; O(n) is OK here; there should only be a few
			var index = quickies.Max(q => q.Style.Index) + 1;

			foreach (var styleRef in styleRefs)
			{
				var quickie = quickies.Find(q => q.Style.Equals(styleRef.Style));
				var same = false;
				if (quickie != null)
				{
					// found match but is it the same index?
					same = quickie.Style.Index == styleRef.Style.Index;
				}
				else
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					styleRef.Style.Index = index++;
					styleRef.Element.Attribute("index").Value = styleRef.Style.Index.ToString();
					quickies.Add(styleRef);

					var last = page.Elements(ns + "QuickStyleDef").LastOrDefault();
					if (last != null)
					{
						last.AddAfterSelf(styleRef.Element);
					}
					else
					{
						page.AddFirst(styleRef.Element);
					}
				}
			}

			return styleRefs;
		}


		private void AdjustQuickStyles(List<QuickRef> styles, XElement childOutline)
		{
			// need to reverse sort the styles so the logic doesn't continually overwrite
			// subsequent index references

			foreach (var style in styles.OrderByDescending(s => s.Style.Index))
			{
				if (style.OriginalIndex != style.Style.Index.ToString())
				{
					// apply new index to child outline elements

					var elements = childOutline.Descendants()
						.Where(e => e.Attribute("quickStyleIndex")?.Value == style.OriginalIndex);

					foreach (var element in elements)
					{
						element.Attribute("quickStyleIndex").Value = style.Style.Index.ToString();
					}
				}
			}
		}
	}
}
