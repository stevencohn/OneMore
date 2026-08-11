//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Finds all images locked to the page background and moves them into the last
	/// outline on the page, in document order, top-to-bottom then left-to-right.
	/// </summary>
	internal class RestoreBackgroundImagesCommand : Command
	{
		public RestoreBackgroundImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			var images = page.Root.Elements(ns + "Image").ToList();
			if (!images.Any())
			{
				ShowInfo(Resx.RestoreBackgroundImagesCommand_noImages);
				return;
			}

			// top-to-bottom, then left-to-right
			var ordered = images
				.OrderBy(i => i.Element(ns + "Position") is XElement p ? p.GetAttributeDouble("y") : 0)
				.ThenBy(i => i.Element(ns + "Position") is XElement p ? p.GetAttributeDouble("x") : 0)
				.ToList();

			var container = FindLastContainer(page, ns);

			foreach (var image in ordered)
			{
				var objectID = image.Attribute("objectID")?.Value;
				if (!string.IsNullOrEmpty(objectID))
				{
					// removing the top level Image from the XML tree alone isn't enough;
					// OneNote won't drop it from the page unless explicitly deleted
					one.DeleteContent(page.PageId, objectID);
				}

				image.Element(ns + "Position")?.Remove();
				image.Remove();

				container.Add(new XElement(ns + "OE", image));
			}

			await one.Update(page);
		}


		private XElement FindLastContainer(Page page, XNamespace ns)
		{
			var outline = page.BodyOutlines.LastOrDefault();
			if (outline is null)
			{
				return page.EnsureContentContainer();
			}

			var container = outline.Elements(ns + "OEChildren").LastOrDefault();
			if (container is null)
			{
				container = new XElement(ns + "OEChildren");
				outline.Add(container);
			}

			return container;
		}
	}
}
