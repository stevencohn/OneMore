//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
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

		private Page page;
		private XNamespace ns;


		public ArrangeContainersCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new ArrangeContainersDialog();
			if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			using var one = new OneNote(out page, out ns);

			if (dialog.Vertical)
			{
				ArrangeVertical();
			}
			else
			{
				ArrangeFlow(dialog.Columns, dialog.PageWidth);
			}

			await one.Update(page);
		}


		private void ArrangeVertical()
		{
			var containers = CollectContainers(page, ns);

			// find the topmost container position
			var yoffset = containers
				.Select(e => double.Parse(e.Element(ns + "Position").Attribute("y").Value))
				.Min();

			foreach (var container in containers)
			{
				var position = container.Element(ns + "Position");
				position.SetAttributeValue("x", LeftMargin.ToString(CultureInfo.InvariantCulture));
				position.SetAttributeValue("y", yoffset.ToString(CultureInfo.InvariantCulture));

				var height = double.Parse(container.Element(ns + "Size")
					.Attribute("height").Value, CultureInfo.InvariantCulture);

				yoffset += height + BottomMargin;
			}
		}


		private void ArrangeFlow(int columns, int pageWidth)
		{
			var containers = CollectContainers(page, ns);

			var xoffset = LeftMargin;

			// find the topmost container position
			var yoffset = containers
				.Select(e => double.Parse(e.Element(ns + "Position").Attribute("y").Value))
				.Min();

			int col = 1;
			double maxHeight = 0;
			double colwidth = (pageWidth / columns);
			double maxPageWidth = LeftMargin + pageWidth + (RightMargin * (columns - 1));

			foreach (var container in containers)
			{
				var size = container.Element(ns + "Size");
				var width = double.Parse(size.Attribute("width").Value);
				var height = double.Parse(size.Attribute("height").Value);

				if (height > maxHeight)
				{
					maxHeight = height;
				}

				// don't let containers extend too far off the page
				if ((col > columns) ||
					(col > 1 && (xoffset + width > maxPageWidth)))
				{
					xoffset = LeftMargin;
					yoffset += maxHeight + BottomMargin;
					maxHeight = height;
					col = 1;
				}

				var position = container.Element(ns + "Position");
				position.SetAttributeValue("x", xoffset.ToString(CultureInfo.InvariantCulture));
				position.SetAttributeValue("y", yoffset.ToString(CultureInfo.InvariantCulture));

				size.SetAttributeValue("width", colwidth.ToString("N3", CultureInfo.InvariantCulture));
				// must set both width and height for changes to take effect
				size.SetAttributeValue("height", (height + 0.001).ToString("N3", CultureInfo.InvariantCulture));
				size.SetAttributeValue("isSetByUser", "true");

				logger.WriteLine($"moved container to {LeftMargin} x {yoffset:N3}, size {width:N3} x {height:N3}");

				xoffset += Math.Max(width, colwidth) + RightMargin;
				col++;
			}
		}


		// Collects a list of containers that have content, filtering out those with
		// empty text runs. OneNote tends to append an empty container after Update regardless
		private IEnumerable<XElement> CollectContainers(Page page, XNamespace ns)
		{
			foreach (var container in page.Root.Elements(ns + "Outline").ToList())
			{
				var text = container
					.Descendants(ns + "T").Nodes().OfType<XCData>()
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
