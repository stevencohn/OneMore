//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Expands collapsed elements, heading or paragraphs with collapsed indented children,
	/// and records which elements were collapsed. A subsequent collapse request will collapse
	/// only those elements previously marked as collapsed
	/// </summary>
	internal class ExpandoCommand : Command
	{
		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ExpandoCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var expand = (bool)args[0];

			using (one = new OneNote(out page, out ns))
			{
				if (expand)
				{
					await Expand();
				}
				else
				{
					await Collapse();
				}
			}
		}


		private async Task Expand()
		{
			// reset expando markers...

			page.Root.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value == "omExpando")
				.Remove();

			// expand...

			var attributes = page.Root.Descendants(ns + "OE")
				.Where(e => e.Attribute("collapsed")?.Value == "1")
				.Select(e => e.Attribute("collapsed"));

			if (attributes.Any())
			{
				foreach (var attribute in attributes)
				{
					var meta = attribute.Parent.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == "omExpando");

					if (meta == null)
					{
						// mark element as an expando
						attribute.Parent.AddFirst(new XElement(ns + "Meta",
							new XAttribute("name", "omExpando"),
							new XAttribute("content", "1")
							));
					}

					attribute.Remove();
				}

				await one.Update(page);
			}
		}


		private async Task Collapse()
		{
			var markers = page.Root.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value == "omExpando");

			if (markers.Any())
			{
				foreach (var marker in markers)
				{
					marker.Parent.SetAttributeValue("collapsed", "1");
				}

				await one.Update(page);
			}
			else
			{
				UIHelper.ShowInfo(Resx.ExpandoCommand_NoMarkers);
			}
		}
	}
}
