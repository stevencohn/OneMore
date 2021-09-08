//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class InsertBreadcrumbCommand : Command
	{
		private const string RightArrow = "\u2192";

		public InsertBreadcrumbCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var (_, path, _) = one.GetPageInfo();

				// GetDirectoryName will revert forward-slashes to backward-slashes
				path = "<span style='font-style:italic'>"
					+ Path.GetDirectoryName(path).Substring(1).Replace("\\", $" {RightArrow} ")
					+ "</span>";

				// <one:Meta name="omBreadcrumb" content="1" />
				var paragraph = page.Root
					.Elements(ns + "Outline")
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "Meta")
					.Where(e => 
						e.Attributes().Any(a => a.Value.Equals(Page.BreadcrumbMetaName)))
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					var index = page.GetQuickStyle(Styles.StandardStyles.Quote).Index;
					paragraph = new Paragraph(ns,
						new XElement(ns + "Meta",
							new XAttribute("name", Page.BreadcrumbMetaName),
							new XAttribute("content", "1")),
						new XElement(ns + "T",
							new XCData(path))
						).SetQuickStyle(index);

					var container = page.EnsureContentContainer();
					container.AddFirst(paragraph, new Paragraph(ns));
				}
				else
				{
					var run = paragraph.Elements(ns + "T").FirstOrDefault();
					if (run == null)
					{
						paragraph.Add(new XElement(ns + "T",
							new XCData(path))
							);
					}
					else
					{
						run.GetCData().Value = path;
					}
				}

				await one.Update(page);
			}
		}
	}
}
