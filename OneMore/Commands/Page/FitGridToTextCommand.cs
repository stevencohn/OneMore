//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Auto-adjusts the size of the background grid, either lines or squares, to the size of
	/// the most commonly used font on the page
	/// </summary>
	/// <remarks>
	/// There is an option to override the auto calculation and enter a custom value as well. 
	/// Grids and outlines on the page are not linked in any way in OneNote so grid may not 
	/// align perfectly with text but the lines should be consistent in relation to the text 
	/// content. Note this works well for pages that are mostly text; complicated pages with 
	/// tables and headings will throw off the alignments since grid cannot dynamically change 
	/// throughout a page.
	/// </remarks>
	internal class FitGridToTextCommand : Command
	{
		public FitGridToTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic);
			var ruleLines = page.Root
				.Elements(ns + "PageSettings")
				.Elements(ns + "RuleLines")
				.FirstOrDefault(e => e.Attribute("visible")?.Value == "true");

			var horizontal = ruleLines?.Element(ns + "Horizontal");
			var vertical = ruleLines?.Element(ns + "Vertical");

			if (horizontal == null && vertical == null)
			{
				ShowError(Resx.FitGridToTextCommand_noGrid);
				return;
			}

			var quickStyles = page.GetQuickStyles().Where(s => s.Name == "p");
			if (!quickStyles.Any())
			{
				ShowError(Resx.FitGridToTextCommand_noText);
				return;
			}

			var pindexes = quickStyles.Select(s => s.Index.ToString());

			var common = page.Root.Descendants(ns + "OE")
				// find all "p" paragraphs
				.Where(e => pindexes.Contains(e.Attribute("quickStyleIndex")?.Value))
				.Select(e => new
				{
					Element = e,
					Index = e.Attribute("quickStyleIndex").Value,
					Css = e.Attribute("style")?.Value
				})
				// count instances of distinct combinations
				.GroupBy(o => new { o.Index, o.Css })
				.Select(group => new
				{
					group.Key.Index,
					group.First().Element,
					Count = group.Count()
				})
				// grab the most commonly used; if there are two equally
				// used styles then this is arbitrary but OK
				.OrderByDescending(g => g.Count)
				.FirstOrDefault();

			if (common != null)
			{
				var analyzer = new StyleAnalyzer(page.Root);
				var style = analyzer.CollectStyleFrom(common.Element);
				var height = CalculateLineHeight(style);

				using var dialog = new FitGridToTextDialog(style.FontSize, height);
				if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
				{
					var spacing = dialog.Spacing.ToString(CultureInfo.InvariantCulture);

					horizontal?.SetAttributeValue("spacing", spacing);
					vertical?.SetAttributeValue("spacing", spacing);

					if (dialog.SnapOutlines)
					{
						AdjustOutlinePositions(page, ns, horizontal, vertical);
					}

					AdjustOutlinePositions(page, ns, horizontal, vertical);

					await one.Update(page);
				}
			}
		}


		private static double CalculateLineHeight(StyleBase style)
		{
			using var image = new Bitmap(1, 1);
			using var g = Graphics.FromImage(image);

			var fontSize = float.Parse(style.FontSize, NumberStyles.Any, CultureInfo.InvariantCulture);
			using var font = new Font(style.FontFamily, fontSize, FontStyle.Regular);

			// the height of a single line is apparently greater than
			// half of two lines! so use difference...
			var size1 = g.MeasureString("A", font);
			var size2 = g.MeasureString("A\nA", font);
			var linespace = (size1.Height * 2) - size2.Height;

			// (g.DpiY / 144) means this will work for 100% desktop scaling
			// and for %150 desktop scaling...

			return (size1.Height - linespace) / (g.DpiY / 144) / 2;
		}


		/// <summary>
		/// Snaps every body outline's Position to the nearest multiple of the grid
		/// spacing, X to the Vertical spacing and Y to the Horizontal spacing.
		/// </summary>
		/// <param name="page">The page whose outlines will be adjusted</param>
		/// <param name="ns">The page namespace</param>
		/// <param name="horizontal">The RuleLines Horizontal element, or null</param>
		/// <param name="vertical">The RuleLines Vertical element, or null</param>
		private static void AdjustOutlinePositions(
			Page page, XNamespace ns, XElement horizontal, XElement vertical)
		{
			var ySpacing = horizontal?.GetAttributeDouble("spacing") ?? double.NaN;
			var xSpacing = vertical?.GetAttributeDouble("spacing") ?? double.NaN;

			foreach (var outline in page.BodyOutlines)
			{
				var position = outline.Element(ns + "Position");
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
