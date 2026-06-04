//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Select all content with the same style treatment as the selected text. The selected
	/// text is either inferred from the insertion cursor or an explictly selected region.
	/// </summary>
	internal class SelectStyleCommand : Command
	{
		private StyleAnalyzer analyzer;
		private XNamespace ns;

		/****************************************************************************************
		 **
		 ** NOTE that OneNote does not allow multiple non-contiguous selection ranges within
		 ** a single paragraph OE element. This routine will result in selecting only the last
		 ** range in the paragraph that meets the style criteria
		 **
		 ****************************************************************************************/

		public SelectStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns);

			analyzer = new StyleAnalyzer(page.Root);
			var style = analyzer.CollectFromSelection();

			var ok = (style != null) &&
				NormalizeTextCursor(page, analyzer);

			if (!ok)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			// clear any pre-existing selection state before marking matches
			page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Attributes("selected")
				.Remove();

			var runs = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "T").ToList();

			foreach (var run in runs)
			{
				AnalyzeRun(run, style);
			}

			logger.WriteLine($"Catalog depth:{analyzer.Depth}, hits:{analyzer.Hits}");

			await one.Update(page);

			await Task.Yield();
		}


		private bool NormalizeTextCursor(Page page, StyleAnalyzer analyzer)
		{
			var range = new SelectionRange(page);

			// allowNonEmpty preserves Run scope; default false would corrupt Scope to None
			var cursor = range.GetSelection(allowNonEmpty: true);

			// valid scopes that need no cursor normalization
			if (range.Scope == SelectionScope.SpecialCursor ||
				range.Scope == SelectionScope.Run ||
				range.Scope == SelectionScope.Range)
			{
				return true;
			}

			if (range.Scope != SelectionScope.TextCursor || cursor == null)
			{
				return false;
			}

			// Rejoin the T elements that OneNote split when the cursor was placed,
			// so the full run is treated as one unit by AnalyzeRun. Only merge when the
			// styles at the join boundary actually match; otherwise leave them split.
			if (cursor.PreviousNode is XElement prev && prev.Name.LocalName == "T" &&
				cursor.NextNode is XElement next && next.Name.LocalName == "T")
			{
				var prevData = prev.GetCData();
				var nextData = next.GetCData();

				if (prevData != null && nextData != null)
				{
					var prevWrap = prevData.GetWrapper();
					var nextWrap = nextData.GetWrapper();

					if (prevWrap.Nodes().LastOrDefault() is XElement prevSpan &&
						nextWrap.Nodes().FirstOrDefault() is XElement nextSpan)
					{
						// Both boundaries are spans: merge if their styles match
						var prevSpanStyle = analyzer.CollectStyleFrom(prevSpan);
						var nextSpanStyle = analyzer.CollectStyleFrom(nextSpan);
						if (prevSpanStyle.Equals(nextSpanStyle))
						{
							prevSpan.Value = $"{prevSpan.Value}{nextSpan.Value}";
							nextSpan.Remove();
							prevWrap.Add(nextWrap.Nodes());
							prevData.Value = prevWrap.GetInnerXml(singleQuote: true);
							next.Remove();
						}
					}
					else if (prevWrap.Nodes().LastOrDefault() is XText &&
						nextWrap.Nodes().FirstOrDefault() is XText)
					{
						// Both boundaries are plain text: merge if T-level styles match
						var prevStyle = analyzer.CollectStyleFrom(prev);
						var nextStyle = analyzer.CollectStyleFrom(next);
						if (prevStyle.Equals(nextStyle))
						{
							prevData.Value = $"{prevData.Value}{nextData.Value}";
							next.Remove();
						}
					}
					// Mixed boundary (span + plain or plain + span): leave separate
				}
			}

			cursor.Remove();
			return true;
		}


		private void AnalyzeRun(XElement run, Style style)
		{
			var cdata = run.GetCData();
			if (cdata == null || cdata.IsEmpty())
			{
				return;
			}

			var runProps = analyzer.CollectFrom(run);
			var runStyle = new Style(runProps);
			var textMatches = style.Equals(runStyle);

			if (cdata.Value.Contains("<span"))
			{
				if (run.Attribute("selected") is XAttribute attr)
				{
					attr.Remove();
				}

				var candidates = new List<XElement>();
				var count = 0;

				foreach (var node in cdata.GetWrapper().Nodes())
				{
					if (node is XText text)
					{
						if (textMatches)
						{
							//logger.WriteLine($"match-text {text.Value}");

							count++;
							candidates.Add(new XElement(ns + "T",
								run.Attributes().Where(a => a.Name.LocalName != "selected"),
								new XAttribute("selected", "all"),
								new XCData(text.Value)));
						}
						else
						{
							candidates.Add(new XElement(ns + "T",
								run.Attributes(), new XCData(text.Value)));
						}
					}
					else
					{
						var span = node as XElement;
						var spanProps = analyzer.CollectFrom(span).Add(runProps);
						var spanStyle = new Style(spanProps);

						if (spanStyle.Equals(style))
						{
							//logger.WriteLine($"match-span {span.ToString(SaveOptions.DisableFormatting)}");

							count++;
							candidates.Add(new XElement(ns + "T",
								run.Attributes().Where(a => a.Name.LocalName != "selected"),
								new XAttribute("selected", "all"),
								new XCData(span.ToString(SaveOptions.DisableFormatting))));
						}
						else
						{
							candidates.Add(new XElement(ns + "T",
								run.Attributes(),
								new XCData(span.ToString(SaveOptions.DisableFormatting))));
						}
					}
				}

				if (count > 0)
				{
					if (candidates.Count == 1)
					{
						run.SetAttributeValue("selected", "all");
					}
					else
					{
						run.ReplaceWith(candidates);
					}
				}
			}
			else if (textMatches)
			{
				//logger.WriteLine($"match {run.ToString(SaveOptions.DisableFormatting)}");
				run.SetAttributeValue("selected", "all");
			}
		}
	}
}
