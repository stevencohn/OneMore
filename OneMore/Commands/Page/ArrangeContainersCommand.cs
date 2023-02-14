//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Interop.PowerPoint;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using System.Xml.Linq;


	/// <summary>
	/// Arrange containers on a page vertically or in columns
	/// </summary>
	internal class ArrangeContainersCommand : Command
	{
		private const double LeftMargin = 36.0;
		private const double BottomMargin = 36.0;
		private const double RightMargin = 20.0;
		private const double TopMargin = 86.0;

		private Page page;
		private XNamespace ns;
		private double topMargin;


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

			FindTopMargin();

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


		private void FindTopMargin()
		{
			// consider tagging bank
			var bank = page.Root.Elements(ns + "Outline").Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value.Equals(MetaNames.TaggingBank))
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (bank == null)
			{
				topMargin = TopMargin;
			}
			else
			{

				System.Diagnostics.Debugger.Launch();
				bank.Element(ns + "Position").GetAttributeValue("y", out var y, 0.0);
				bank.Element(ns + "Size").GetAttributeValue("height", out var h, 0.0);
				topMargin = Math.Max(y + h + 10, TopMargin);
			}
		}


		private void ArrangeVertical()
		{
			var containers = CollectContainers(page, ns);

			// find the topmost container position
			var yoffset = Math.Min(
				topMargin,
				containers.Select(e => double.Parse(e.Element(ns + "Position")
					.Attribute("y").Value, CultureInfo.InvariantCulture)).Min()
				);

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
			var yoffset = Math.Min(
				topMargin,
				containers.Select(e => double.Parse(e.Element(ns + "Position")
					.Attribute("y").Value, CultureInfo.InvariantCulture)).Min()
				);

			int col = 1;
			double maxHeight = 0;
			double colwidth = (pageWidth / columns);
			double maxPageWidth = LeftMargin + pageWidth + (RightMargin * (columns - 1));

			foreach (var container in containers)
			{
				var size = container.Element(ns + "Size");
				var width = double.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);
				var height = double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

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
			var containers = page.Root.Elements(ns + "Outline")
				.Where(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank))
				.ToList();

			foreach (var container in containers)
			{
				var runs = container.Descendants(ns + "T");
				if (runs.Any())
				{
					var text = runs.Nodes().OfType<XCData>()
						.Select(c => c.Value.Trim())
						.Aggregate((a, b) => $"{a}{b}");

					if (text.Length > 0)
					{
						yield return container;
					}
				}
				else
				{
					// likely contains an InsertedFile or image without text runs
					yield return container;
				}
			}
		}
	}
}
