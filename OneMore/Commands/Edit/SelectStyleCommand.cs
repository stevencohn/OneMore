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
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
			using (var one = new OneNote(out var page, out ns))
			{
				analyzer = new StyleAnalyzer(page.Root);
				var style = analyzer.CollectFromSelection();

				var ok = (style != null) &&
					NormalizeTextCursor(page, analyzer);

				if (!ok)
				{
					UIHelper.ShowInfo(one.Window, Resx.Error_BodyContext);
					return;
				}

				var runs = page.Root
					.Elements(ns + "Outline")
					.Descendants(ns + "T").ToList();

				foreach (var run in runs)
				{
					AnalyzeRun(run, style);
				}

				logger.WriteLine(page.Root);
				logger.WriteLine($"Catalog depth:{analyzer.Depth}, hits:{analyzer.Hits}");

				await one.Update(page);
			}

			await Task.Yield();
		}


		// merge text cursor so we don't have to treat it as a special case
		private bool NormalizeTextCursor(Page page, StyleAnalyzer analyzer)
		{
			var cursor = page.GetTextCursor();
			if (cursor == null || page.SelectionScope != SelectionScope.Empty)
			{
				return false;
			}

			if (page.SelectionSpecial)
			{
				// positioned over a hyperlink or MathML equation
				return true;
			}

			if (cursor.PreviousNode is XElement prev &&
				cursor.NextNode is XElement next)
			{
				var prevData = prev.GetCData();
				var nextData = next.GetCData();
				var prevWrap = prevData.GetWrapper();
				var nextWrap = nextData.GetWrapper();

				if (prevWrap.Nodes().Last() is XElement prevSpan &&
					nextWrap.Nodes().First() is XElement nextSpan)
				{
					// examine the last node from Prev and the first node from Next
					// to compare their styles and if they match then combine them
					var prevStyle = analyzer.CollectStyleFrom(prevSpan);
					var nextStyle = analyzer.CollectStyleFrom(nextSpan);
					if (prevStyle.Equals(nextStyle))
					{
						// pull only the first node from Next and append it to Prev
						// then remove that first node from Next and append the remaining nodes
						// to retain their own stylings; finally remove Next altogether below...

						logger.WriteLine($"combining-0 '{prevSpan.Value}' and '{nextSpan.Value}'");
						prevSpan.Value = $"{prevSpan.Value}{nextSpan.Value}";
						nextSpan.Remove();
						prevWrap.Add(nextWrap.Nodes());
						prevData.Value = prevWrap.GetInnerXml(singleQuote: true);
					}
					else
					{
						var prevValue = MakeStylizedSpan(
							prevData, prevWrap.Nodes().Last() as XElement, analyzer.CollectStyleFrom(prev));

						var nextValue = MakeStylizedSpan(
							nextData, nextWrap.Nodes().First() as XElement, analyzer.CollectStyleFrom(next));

						logger.WriteLine($"combining-1 '{prevValue}' and '{nextValue}'");
						prevData.Value = CombineSpans(prevValue, nextValue);
					}
				}
				else
				{
					var prevValue = MakeStylizedSpan(
						prevData, prevWrap.Nodes().Last() as XElement, analyzer.CollectStyleFrom(prev));

					var nextValue = MakeStylizedSpan(
						nextData, nextWrap.Nodes().First() as XElement, analyzer.CollectStyleFrom(next));

					logger.WriteLine($"combining-2 '{prevValue}' and '{nextValue}'");					
					prevData.Value = CombineSpans(prevValue, nextValue);
				}

				next.Remove();
			}

			var parent = cursor.Parent;
			cursor.Remove();

			logger.WriteLine($"updated parent\n{parent.ToString(SaveOptions.None)}");

			return true;
		}


		private string MakeStylizedSpan(XCData data, XElement edge, Style parentStyle)
		{
			if (edge != null)
			{
				return data.Value;
			}

			var css = string.Empty;
			if (!string.IsNullOrEmpty(parentStyle.FontFamily))
			{
				css = $"font-family:{parentStyle.FontFamily}";
			}

			if (!string.IsNullOrEmpty(parentStyle.FontSize))
			{
				css = $"{css};font-size:{parentStyle.FontSize}pt";
			}

			if (!string.IsNullOrEmpty(parentStyle.Color))
			{
				css = $"{css};color:{parentStyle.Color}";
			}

			return $"<span style='{css}'>{data.Value}</span>";
		}


		private string CombineSpans(string span1, string span2)
		{
			if (span1.StartsWith("<span") && span2.StartsWith("<span"))
			{
				var wrap1 = new XCData(span1).GetWrapper();
				var wrap2 = new XCData(span2).GetWrapper();

				if (wrap1.Attribute("style") is XAttribute attr1 &&
					wrap2.Attribute("style") is XAttribute attr2)
				{
					if (attr1.Value == attr2.Value)
					{
						wrap1.Value = $"{wrap1.Value}{wrap2.Value}";
						return wrap1.ToString(SaveOptions.DisableFormatting);
					}
				}
			}

			return $"{span1}{span2}";
		}



		private void AnalyzeRun(XElement run, Style style)
		{
			var runProps = analyzer.CollectFrom(run);
			var runStyle = new Style(runProps);
			var textMatches = style.Equals(runStyle);

			if (run.GetCData() is XCData cdata && cdata.Value.Contains("<span"))
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
