//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Linq;


	internal class CollapsePagesCommand : Command
	{
		public CollapsePagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var section = one.GetSection();
				var ns = one.GetNamespace(section);

				// find all level 1 pages not collapsed and immediately followed by level 2 page

				var pages =
					from e in section.Elements(ns + "Page")
					let n = e.NextNode
					where n != null
					&& e.Attribute("pageLevel").Value.Equals("1")
					&& !e.Attributes("isCollapsed").Any(x => x.Value.Equals("true"))
					&& n.NodeType == XmlNodeType.Element && ((XElement)n).Attribute("pageLevel").Value.Equals("2")
					select e;

				if (pages?.Count() > 0)
				{
					logger.WriteLine($"found {pages.Count()} expanded pages");

					foreach (var page in pages)
					{
						page.Add(new XAttribute("isCollapsed", "true"));
					}

					one.UpdateHierarchy(section);
				}
				else
				{
					logger.WriteLine($"found 0 expanded pages");
				}
			}

			await Task.Yield();
		}
	}
}
