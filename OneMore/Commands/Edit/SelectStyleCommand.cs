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

				var runs = page.Root.Descendants(ns + "T").Except(analyzer.SelectionRange);
				analyzer = new StyleAnalyzer(page.Root, nested: false);

				foreach (var run in runs)
				{
					analyzer.Clear();

					var cdata = run.GetCData();
					if (cdata.Value.Contains("<span"))
					{
						var runStyle = analyzer.CollectStyleProperties(run);
						var textMatches = style.Equals(runStyle);

						var expanded = new XElement(ns + "T", run.Attributes());
						var wrapper = cdata.GetWrapper();
						foreach (var node in wrapper.Nodes())
						{
							if (node is XText text && textMatches)
							{
								logger.WriteLine($"match-text {text.Value}");
							}
							else if (node is XElement element)
							{
								var s = analyzer.CollectStyleProperties(element);
								if (s.Equals(style))
								{
									logger.WriteLine($"match-span {element.Value}");
								}
							}
						}
					}
					else
					{
						var runStyle = new Style(analyzer.CollectStyleProperties(run));
						if (style.Equals(runStyle))
						{
							logger.WriteLine($"match {run.Value}");
						}
					}
				}

				//await one.Update(page);
			}

			await Task.Yield();
		}
	}
}
