//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
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
			using (var one = new OneNote(out var page, out var ns))
			{
				var analyzer = new StyleAnalyzer(page.Root);
				var style = analyzer.CollectFromSelection();

				if (style == null)
				{
					// TODO: error dialog?
					return;
				}

				logger.WriteLine($"style {style.ToCss()}");

				var runs = page.Root.Descendants(ns + "T").Except(analyzer.SelectionRange);
				analyzer.SetNested(false);

				foreach (var run in runs)
				{
					analyzer.Clear();
					var runprops = analyzer.CollectStyleProperties(run);
					var runstyle = new Style(runprops);
					var textMatches = style.Equals(runstyle);

					var cdata = run.GetCData();
					if (cdata.Value.Contains("<span"))
					{
						//var expanded = new XElement(ns + "T", run.Attributes());

						var wrapper = cdata.GetWrapper();
						foreach (var node in wrapper.Nodes())
						{
							if (node is XText text && textMatches)
							{
								logger.WriteLine($"match-text {text.Value}");
							}
							else if (node is XElement element)
							{
								analyzer.Clear();
								runprops = analyzer.CollectStyleProperties(run);
								var s = new Style(analyzer.CollectStyleProperties(element));
								s.Merge(runprops);
								if (s.Equals(style))
								{
									logger.WriteLine($"match-span {element.ToString(SaveOptions.DisableFormatting)}");
									logger.WriteLine($"... style {s.ToCss()}");
								}
								//else
								//{
								//	logger.WriteLine($"miss-span {element.ToString(SaveOptions.DisableFormatting)}");
								//	logger.WriteLine($"... miss style {s.ToCss()}");
								//}
							}
						}
					}
					else
					{
						if (style.Equals(runstyle))
						{
							logger.WriteLine($"match {run.ToString(SaveOptions.DisableFormatting)}");
						}
					}
				}

				//await one.Update(page);
			}

			await Task.Yield();
		}
	}
}
