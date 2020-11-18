//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Globalization;
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

					var map = page.MergeQuickStyles(childPage);

					var childOutlines = childPage.Root.Elements(ns + "Outline");
					if (childOutlines == null || !childOutlines.Any())
					{
						break;
					}

					var topOffset = childOutlines.Elements(ns + "Position")
						.Min(p => double.Parse(p.Attribute("y").Value, CultureInfo.InvariantCulture));

					foreach (var childOutline in childOutlines)
					{
						// adjust position relative to new parent page outlines
						var position = childOutline.Elements(ns + "Position").FirstOrDefault();
						var y = double.Parse(position.Attribute("y").Value, CultureInfo.InvariantCulture)
							- topOffset + offset + OutlineMargin;

						position.Attribute("y").Value = y.ToString("#0.0");

						// keep track of lowest bottom
						var size = childOutline.Elements(ns + "Size").FirstOrDefault();
						var bottom = y + double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);
						if (bottom > maxOffset)
						{
							maxOffset = bottom;
						}

						position.Attribute("z").Value = z.ToString();
						z++;

						// remove its IDs so the page can apply its own
						childOutline.Attributes("objectID").Remove();
						childOutline.Descendants().Attributes("objectID").Remove();

						page.ApplyStyleMapping(map, childOutline);

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
						var bottom = double.Parse(position.Attribute("y").Value, CultureInfo.InvariantCulture)
							+ double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

						if (bottom > offset)
						{
							offset = bottom;
						}
					}
				}
			}

			return offset;
		}
	}
}
