//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Removes all ink drawings and annotations from the current page.
	/// </summary>
	internal class RemoveInkCommand : Command
	{
		private readonly PageSchema schema;
		private Page page;
		private XNamespace ns;


		public RemoveInkCommand()
		{
			schema = new PageSchema();
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out page, out ns);

			PageNamespace.Set(ns);

			var ink = page.Root.Descendants(ns + "InkDrawing");
			if (ink.Any())
			{
				ink.ForEach(e => one.DeleteContent(page.PageId, e.Attribute("objectID").Value));
			}

			var updated = false;

			ink = page.Root.Descendants(ns + "InkWord").Reverse().ToList();
			if (ink is not null && ink.Any())
			{
				foreach (var word in ink)
				{
					var parent = word.Parent;
					word.Remove();
					updated = true;

					RemoveEmptyContainers(one, parent);
				}
			}

			ink = page.Root.Descendants(ns + "InkParagraph").Reverse().ToList();
			if (ink is not null && ink.Any())
			{
				foreach (var paragraph in ink)
				{
					var parent = paragraph.Parent;
					paragraph.Remove();
					updated = true;

					RemoveEmptyContainers(one, parent);
				}
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		// InkParagraph can only exist inside of an OE
		// InkWord can existing in an InkParagraph or an OE
		private void RemoveEmptyContainers(OneNote one, XElement node)
		{
			var parent = node.Parent;

			while (parent is not null)
			{
				// OEChildren can only be inside a Cell and an Outline
				if (node.Name.LocalName == "Cell")
				{
					if (node.Element(ns + "OEChildren") is null)
					{
						// don't leave the cell empty
						node.Add(new Paragraph(string.Empty));
					}

					break;
				}

				// remove empty OE.. OEChildren.. Outline..

				var names = schema.GetContentNames(node.Name.LocalName);

				if (node.Elements().Any(e => e.Name.LocalName.In(names)))
				{
					break;
				}
				else
				{
					node.Remove();

					if (node.Name.LocalName == "Outline")
					{
						one.DeleteContent(page.PageId, node.Attribute("objectID").Value);
						break;
					}

					node = parent;
					parent = node.Parent;
				}
			}
		}
	}
}
