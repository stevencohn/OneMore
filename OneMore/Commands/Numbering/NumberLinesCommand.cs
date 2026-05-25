//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Adds sequential line numbers to the selected paragraphs, left-justified like a code editor.
	/// </summary>
	internal class NumberLinesCommand : Command
	{
		private const int IndentWidth = 4;

		// emSpace           '\u2002'  widtht of an 'm'
		// enSpace           '\u2003'  half the width of an emSpace
		// figureSpace       '\u2007'  width of a monospace digit
		// punctuationSpace  '\u2008'  width of a period
		// narrowSpace       '\u202F'  narrower than &nbsp
		// emQuad            '\u2001'  even wider than EM space
		// enQuad            '\u2000'  wider than EM but narrower than emQuad
		private const char Space = '\u2007';
		private const char numSpace = '\u2007';

		private XNamespace ns;
		private string colorStyle;


		public NumberLinesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var color = ThemeManager.Instance.GetColor("GrayText");
			colorStyle = $"color:{ColorTranslator.ToHtml(color)};font-size:9;";

			var range = new SelectionRange(page);
			var runs = range.GetSelections();

			if (range.Scope == SelectionScope.TextCursor)
			{
				ShowInfo(Resx.NumberLinesCommand_NoSelection);
				return;
			}

			var selectedOEs = runs
				.Select(t => t.Parent)
				.Where(e => e?.Name.LocalName == "OE")
				.Distinct()
				.ToList();

			NumberSelected(selectedOEs);
			await one.Update(page);
		}


		private void NumberSelected(List<XElement> selectedOEs)
		{
			if (!selectedOEs.Any())
			{
				return;
			}

			var flatOEs = new List<XElement>();
			var processed = new HashSet<XElement>();

			foreach (var oe in selectedOEs)
			{
				if (processed.Contains(oe))
				{
					continue;
				}

				processed.Add(oe);

				var oecChildren = oe.Element(ns + "OEChildren");
				if (oecChildren is null)
				{
					flatOEs.Add(oe);
					continue;
				}

				// collect all descendants with relative depths (1 = direct child of this OE)
				var descendants = new List<(XElement desc, int depth)>();
				CollectOEs(oecChildren, 1, descendants);

				// mark all descendants so they're skipped if also in selectedOEs
				foreach (var (desc, _) in descendants)
				{
					processed.Add(desc);
				}

				// detach descendants before reinserting
				foreach (var (desc, _) in descendants)
				{
					desc.Remove();
				}

				oe.Element(ns + "OEChildren")?.Remove();
				flatOEs.Add(oe);

				// reinsert descendants as siblings after this OE, with indent spaces
				var insertAfter = oe;
				foreach (var (desc, depth) in descendants)
				{
					desc.Element(ns + "OEChildren")?.Remove();

					var t = desc.Elements(ns + "T").FirstOrDefault();
					if (t is null)
					{
						t = new XElement(ns + "T", new XCData(string.Empty));
						desc.AddFirst(t);
					}

					PrependIndent(t, depth);
					insertAfter.AddAfterSelf(desc);
					insertAfter = desc;
					flatOEs.Add(desc);
				}
			}

			var width = flatOEs.Count.ToString().Length;

			for (int i = 0; i < flatOEs.Count; i++)
			{
				var oe = flatOEs[i];

				var t = oe.Elements(ns + "T").FirstOrDefault();
				if (t is null)
				{
					t = new XElement(ns + "T", new XCData(string.Empty));
					oe.AddFirst(t);
				}

				PrependNumber(t, i + 1, width, 0);
			}
		}


		private void CollectOEs(XElement container, int depth, List<(XElement oe, int depth)> result)
		{
			foreach (var oe in container.Elements(ns + "OE"))
			{
				result.Add((oe, depth));
				var children = oe.Element(ns + "OEChildren");
				if (children is not null)
				{
					CollectOEs(children, depth + 1, result);
				}
			}
		}


		private void PrependNumber(XElement t, int num, int width, int depth)
		{
			if (t is null)
			{
				return;
			}

			var numStr = num.ToString().PadLeft(width, numSpace);
			var indent = new string(Space, depth * IndentWidth);

			var numSpan = new XElement("span",
				new XAttribute("style", colorStyle),
				$"{numStr}  {indent}");

			var cdata = t.GetCData();
			if (cdata is null)
			{
				t.Add(new XCData(string.Empty));
				cdata = t.GetCData();
			}

			if (cdata.IsEmpty())
			{
				var emptyWrapper = new XElement("cdata", numSpan);
				cdata.Value = emptyWrapper.GetInnerXml(singleQuote: true);
			}
			else
			{
				var wrapper = cdata.GetWrapper();
				var firstNode = wrapper.Nodes().FirstOrDefault();
				if (firstNode is not null)
					firstNode.AddBeforeSelf(numSpan);
				else
					wrapper.Add(numSpan);

				cdata.Value = wrapper.GetInnerXml(singleQuote: true);
			}
		}


		private static void PrependIndent(XElement t, int depth)
		{
			if (t is null || depth <= 0)
			{
				return;
			}

			var spaces = new string(Space, depth * IndentWidth);

			var cdata = t.GetCData();
			if (cdata is null)
			{
				t.Add(new XCData(spaces));
				return;
			}

			if (cdata.IsEmpty())
			{
				cdata.Value = spaces;
			}
			else
			{
				var wrapper = cdata.GetWrapper();
				var firstNode = wrapper.Nodes().FirstOrDefault();
				if (firstNode is not null)
					firstNode.AddBeforeSelf(new XText(spaces));
				else
					wrapper.Add(new XText(spaces));

				cdata.Value = wrapper.GetInnerXml(singleQuote: true);
			}
		}
	}
}
