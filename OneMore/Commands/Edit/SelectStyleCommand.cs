//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class SelectStyleCommand : Command
	{

		public SelectStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			System.Diagnostics.Debugger.Launch();

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

				// merge text cursor so we don't have to treat it as a special case
				page.GetTextCursor(merge: true);

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
					}
				}

				logger.WriteLine(page.Root);
				
				await one.Update(page);
			}

			await Task.Yield();
		}
	}
}
