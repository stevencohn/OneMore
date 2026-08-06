//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
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
	using Resx = Properties.Resources;


	/// <summary>
	/// Snaps root-level page objects (Outlines, InkDrawings, and Images) to the nearest
	/// background grid intersection. If one or more root-level objects are selected then
	/// only those objects are snapped; otherwise every Outline on the page is snapped.
	/// </summary>
	internal class SnapToGridCommand : Command
	{
		private static bool commandIsActive = false;


		public SnapToGridCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				await using var one = new OneNote(out var page, out var ns);

				var rootObjects = page.BodyOutlines
					.Concat(page.Root.Elements(ns + "InkDrawing"))
					.Concat(page.Root.Elements(ns + "Image"))
					.ToList();

				if (!rootObjects.Any())
				{
					ShowInfo(Resx.SnapToGridCommand_noContainers);
					return;
				}

				var ruleLines = page.Root
					.Elements(ns + "PageSettings")
					.Elements(ns + "RuleLines")
					.FirstOrDefault(e => e.Attribute("visible")?.Value == "true");

				var horizontal = ruleLines?.Element(ns + "Horizontal");
				var vertical = ruleLines?.Element(ns + "Vertical");

				if (horizontal == null && vertical == null)
				{
					ShowError(Resx.SnapToGridCommand_noGrid);
					return;
				}

				var selected = rootObjects
					.Where(e => e.Attribute("selected") is XAttribute a &&
						(a.Value == "all" || a.Value == "partial"))
					.ToList();

				var targets = selected.Any() ? selected : page.BodyOutlines.ToList();

				SnapPositions(targets, ns, horizontal, vertical);

				await one.Update(page);
			}
			finally
			{
				commandIsActive = false;
			}
		}


		/// <summary>
		/// Snaps every given element's Position to the nearest multiple of the grid
		/// spacing, X to the Vertical spacing and Y to the Horizontal spacing.
		/// </summary>
		/// <param name="elements">The root-level Outline, InkDrawing, or Image elements to snap</param>
		/// <param name="ns">The page namespace</param>
		/// <param name="horizontal">The RuleLines Horizontal element, or null</param>
		/// <param name="vertical">The RuleLines Vertical element, or null</param>
		private static void SnapPositions(
			List<XElement> elements, XNamespace ns, XElement horizontal, XElement vertical)
		{
			var ySpacing = horizontal?.GetAttributeDouble("spacing") ?? double.NaN;
			var xSpacing = vertical?.GetAttributeDouble("spacing") ?? double.NaN;

			foreach (var element in elements)
			{
				var position = element.Element(ns + "Position");
				if (position == null)
				{
					continue;
				}

				if (!double.IsNaN(xSpacing) && xSpacing > 0)
				{
					var x = position.GetAttributeDouble("x");
					position.SetAttributeValue("x",
						SnapToGrid(x, xSpacing).ToString(CultureInfo.InvariantCulture));
				}

				if (!double.IsNaN(ySpacing) && ySpacing > 0)
				{
					var y = position.GetAttributeDouble("y");
					position.SetAttributeValue("y",
						SnapToGrid(y, ySpacing).ToString(CultureInfo.InvariantCulture));
				}
			}
		}


		private static double SnapToGrid(double value, double spacing)
		{
			return Math.Round(value / spacing) * spacing;
		}
	}
}
