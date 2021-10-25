//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Arrange containers on a page vertically or in columns
	/// </summary>
	internal class ArrangeContainersCommand : Command
	{
		private const double LeftMargin = 36.0;
		private const double BottomMargin = 36.0;
		private const double RightMargin = 20.0;

		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ArrangeContainersCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool vertical;
			int columns;
			int pageWidth;

			using (var dialog = new ArrangeContainersDialog())
			{
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				vertical = dialog.Vertical;
				columns = dialog.Columns;
				pageWidth = dialog.PageWidth;
			}

			using (one = new OneNote(out page, out ns))
			{
				if (vertical)
				{
					ArrangeVertical();
				}
				else
				{
					ArrangeFlow(columns, pageWidth);
				}

				await one.Update(page);
			}
		}


		private void ArrangeVertical()
		{
			var containers = CollectContainers(page, ns);

			var yoffset = containers
				.Select(e => double.Parse(e.Element(ns + "Position").Attribute("y").Value))
				.Min();

			foreach (var container in containers)
			{
				var position = container.Element(ns + "Position");
				position.SetAttributeValue("x", LeftMargin);
				position.SetAttributeValue("y", yoffset);

				var size = container.Element(ns + "Size");
				yoffset += double.Parse(size.Attribute("height").Value) + BottomMargin;
			}
		}


		private void ArrangeFlow(int columns, int pageWidth)
		{
			var containers = CollectContainers(page, ns);

			var xoffset = LeftMargin;
			var yoffset = containers
				.Select(e => double.Parse(e.Element(ns + "Position").Attribute("y").Value))
				.Min();

			int col = 1;
			double maxHeight = 0;
			double width = (pageWidth / columns);

			foreach (var container in containers)
			{
				var position = container.Element(ns + "Position");
				position.SetAttributeValue("x", xoffset);
				position.SetAttributeValue("y", yoffset);

				var size = container.Element(ns + "Size");
				size.SetAttributeValue("width", width.ToString("N3"));
				size.SetAttributeValue("isSetByUser", "true");

				var height = double.Parse(size.Attribute("height").Value);
				if (height > maxHeight)
				{
					maxHeight = height;
				}

				if (col < columns)
				{
					xoffset += width + RightMargin;
					col++;
				}
				else
				{
					xoffset = LeftMargin;
					yoffset += maxHeight + BottomMargin;
					col = 1;
				}
			}
		}


		// Collects a list of containers that have content, filtering out those with
		// empty text runs. OneNote tends to append an empty container after Update regardless
		private IEnumerable<XElement> CollectContainers(Page page, XNamespace ns)
		{
			foreach (var container in page.Root.Elements(ns + "Outline").ToList())
			{
				var text = container.Descendants(ns + "T").Nodes().OfType<XCData>()
				  .Select(c => c.Value.Trim())
				  .Aggregate((a, b) => $"{a}{b}");

				if (!string.IsNullOrWhiteSpace(text))
				{
					yield return container;
				}
			}
		}
	}
}
