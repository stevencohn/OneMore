//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class SelectStyleCommand : Command
	{

		public SelectStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var analyzer = new StyleAnalyzer2(page.Root);
				var style = analyzer.CollectFromSelection();
				if (style == null)
				{
					// TODO: error dialog?
					return;
				}

				logger.WriteLine($"style {style.ToCss()}");

				NormalizeTextCursor(page, analyzer);

				var runs = page.Root.Descendants(ns + "T").ToList();
				foreach (var run in runs)
				{
					var runProps = analyzer.CollectFrom(run);
					var runStyle = new Style(runProps);
					var textMatches = style.Equals(runStyle);

					if (run.GetCData() is XCData cdata && cdata.Value.Contains("<span"))
					{
						var candidates = new List<XElement>();
						var count = 0;

						var wrapper = cdata.GetWrapper();
						foreach (var node in wrapper.Nodes())
						{
							if (node is XText text)
							{
								if (textMatches)
								{
									logger.WriteLine($"match-text {text.Value}");

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
									logger.WriteLine($"match-span {span.ToString(SaveOptions.DisableFormatting)}");

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
						logger.WriteLine($"match {run.ToString(SaveOptions.DisableFormatting)}");
						run.SetAttributeValue("selected", "all");
					}
				}

				logger.WriteLine(page.Root);

				await one.Update(page);
			}

			await Task.Yield();
		}


		// merge text cursor so we don't have to treat it as a special case
		private void NormalizeTextCursor(Page page, StyleAnalyzer2 analyzer)
		{
			var cursor = page.GetTextCursor();
			if (cursor == null || page.SelectionScope != SelectionScope.Empty)
			{
				return;
			}

			if (cursor.PreviousNode is XElement prev &&
				cursor.NextNode is XElement next)
			{
				var prevData = prev.GetCData();
				var nextData = next.GetCData();
				var prevWrap = prevData.GetWrapper();
				var nextWrap = nextData.GetWrapper();

				if (prevWrap.Elements().Last() is XElement prevSpan &&
					nextWrap.Elements().First() is XElement nextSpan)
				{
					var prevStyle = analyzer.CollectStyleFrom(prevSpan);
					var nextStyle = analyzer.CollectStyleFrom(nextSpan);
					if (prevStyle.Equals(nextStyle))
					{
						prevSpan.Value = $"{prevSpan.Value}{nextSpan.Value}";
						nextSpan.Remove();
						prevWrap.Add(nextWrap.Nodes());
						prevData.Value = prevWrap.ToString(SaveOptions.DisableFormatting);
					}
					else
					{
						prevData.Value = $"{prevData.Value}{nextData.Value}";
					}
				}
				else
				{
					prevData.Value = $"{prevData.Value}{nextData.Value}";
				}

				next.Remove();
			}

			cursor.Remove();
		}
	}
}
