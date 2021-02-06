//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class MergeCommand : Command
	{

		private const int OutlineMargin = 30;

		private OneNote one;
		private Page page;
		private XNamespace ns;


		public MergeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
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
						e.Attributes("selected").Any(a => a.Value.Equals("all")))
					.ToList();

				// get first selected (active) page and reference its quick styles, outline, size

				page = one.GetPage(active.Attribute("ID").Value);

				if (page.Root.Elements(ns + "Outline").Count() == 1 && SimplePages(selections))
				{
					// result in a single outline with all content
					ConcatenateContent(page, selections);
				}
				else
				{
					// result in multiple outlines preserving relative positioning
					AppendOutlines(page, selections);
				}

				// update page and section hierarchy

				await one.Update(page);

				foreach (var selection in selections)
				{
					one.DeleteHierarchy(selection.Attribute("ID").Value);
				}
			}
		}


		private bool SimplePages(List<XElement> selections)
		{
			// accrues cost of loading every page twice, but limits memory by avoiding
			// having all pages in memory at once

			foreach (var selection in selections)
			{
				var child = one.GetPage(selection.Attribute("ID").Value, OneNote.PageDetail.Basic);
				if (child.Root.Elements(ns + "Outline").Count() > 1)
				{
					return false;
				}
			}

			return true;
		}


		private void ConcatenateContent(Page page, List<XElement> selections)
		{
			logger.WriteLine("concatenating content into single outline");

			var pageOutline = page.Root.Elements(ns + "Outline").Last();
			var quickIndex = page.GetQuickStyle(Styles.StandardStyles.PageTitle);

			foreach (var selection in selections)
			{
				var child = one.GetPage(
					selection.Attribute("ID").Value, OneNote.PageDetail.BinaryData);

				var outline = child.Root.Elements(ns + "Outline").FirstOrDefault();
				if (outline == null)
				{
					continue;
				}

				// copy child's Title to page...

				var oec = pageOutline.Elements(ns + "OEChildren").Last();

				oec.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));

				InsertLineCommand.AppendLine(one, page, '─'); // long dash

				oec.Add(new XElement(ns + "OE",
					new XAttribute("quickStyleIndex", quickIndex.Index),
					new XElement(ns + "T", new XCData(child.Title))
					));

				// copy child content to page...

				var map = page.MergeQuickStyles(child);

				// remove IDs so the page can apply its own; OneNote does that for us automatically
				outline.Attributes("objectID").Remove();
				outline.Descendants().Attributes("objectID").Remove();

				page.ApplyStyleMapping(map, outline);

				oec.Add(outline.Elements(ns + "OEChildren").Elements());
			}
		}


		private void AppendOutlines(Page page, List<XElement> selections)
		{
			logger.WriteLine("appending outlines with relative positioning");

			var offset = GetPageBottomOffset();

			// track running bottom as we add new outlines
			var maxOffset = offset;

			// find maximum z-offset
			var z = page.Root.Elements(ns + "Outline").Elements(ns + "Position")
				.Attributes("z").Max(a => int.Parse(a.Value)) + 1;

			// merge each of the subsequently selected pages into the active page

			foreach (var selection in selections)
			{
				var child = one.GetPage(
					selection.Attribute("ID").Value, OneNote.PageDetail.BinaryData);

				var outlines = child.Root.Elements(ns + "Outline");
				if (outlines == null || !outlines.Any())
				{
					continue;
				}

				var map = page.MergeQuickStyles(child);

				var topOffset = outlines.Elements(ns + "Position")
					.Min(p => double.Parse(p.Attribute("y").Value, CultureInfo.InvariantCulture));

				foreach (var outline in outlines)
				{
					// adjust position relative to new parent page outlines
					var position = outline.Elements(ns + "Position").FirstOrDefault();
					var y = double.Parse(position.Attribute("y").Value, CultureInfo.InvariantCulture)
						- topOffset + offset + OutlineMargin;

					position.Attribute("y").Value = y.ToString("#0.0", CultureInfo.InvariantCulture);

					// keep track of lowest bottom
					var size = outline.Elements(ns + "Size").FirstOrDefault();
					var bottom = y + double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);
					if (bottom > maxOffset)
					{
						maxOffset = bottom;
					}

					position.Attribute("z").Value = z.ToString();
					z++;

					// remove its IDs so the page can apply its own
					outline.Attributes("objectID").Remove();
					outline.Descendants().Attributes("objectID").Remove();

					page.ApplyStyleMapping(map, outline);

					page.Root.Add(outline);
				}

				if (maxOffset > offset)
				{
					offset = maxOffset;
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
