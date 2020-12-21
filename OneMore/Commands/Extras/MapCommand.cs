//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class MapCommand : Command
	{

		private readonly Regex regex;
		private readonly Dictionary<string, string> titles;
		private OneNote one;


		public MapCommand()
		{
			// internal hyperlinks are within CDATA similar to: <a href="onenote:#three&amp;
			//   section-id={..}&amp;page-id={..}&amp;end&amp;base-path=https://.."
			regex = new Regex(@"section-id=(?<sid>{.*?}).*?page-id=(?<pid>{.*?})");

			titles = new Dictionary<string, string>();
		}


		public override void Execute(params object[] args)
		{
			OneNote.Scope scope;
			using (var dialog = new MapDialog())
			{
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
			}

			logger.StartClock();

			using (one = new OneNote())
			{
				XElement container;
				switch (scope)
				{
					case OneNote.Scope.Notebooks: // all notebooks
						container = one.GetNotebooks(OneNote.Scope.Pages);
						break;

					case OneNote.Scope.Sections: // all sectios in current notebook
						container = one.GetNotebook(OneNote.Scope.Pages);
						break;

					default: // current section
						container = one.GetSection();
						break;
				}

				// ignore the recycle bin
				container.Elements()
					.Where(e => e.Attributes().Any(a => a.Name == "isRecycleBin"))
					.Remove();

				var ns = one.GetNamespace(container);


				//System.Diagnostics.Debugger.Launch();


				var hyperlinks = one.BuildHyperlinkCache(scope);
				logger.WriteTime($"built hyperlink cache for {hyperlinks.Count} pages", true);

				var elements = container.Descendants(ns + "Page").ToList();
				foreach (var element in elements)
				{
					var parentId = element.Attribute("ID").Value;
					var xml = one.GetPageXml(parentId);
					var matches = regex.Matches(xml);

					if (matches.Count == 0)
					{
						element.Remove();
						continue;
					}

					var parent = new Page(XElement.Parse(xml));

					var n = element.Parent.Attribute("name").Value;
					logger.WriteLine($"parent {n}/{parent.Title}");

					foreach (Match match in matches)
					{
						var pid = match.Groups["pid"].Value;
						if (hyperlinks.ContainsKey(pid))
						{
							var pageId = hyperlinks[pid];
							if (pageId != parentId)
							{
								string title;
								if (titles.ContainsKey(pageId))
								{
									title = titles[pageId];
								}
								else
								{
									var p = one.GetPage(pageId, OneNote.PageDetail.Basic);
									title = p.Title;
									titles.Add(pageId, title);
								}

								element.Add(new XElement("Ref",
									new XAttribute("title", title),
									new XAttribute("pageId", pageId)
									));

								logger.WriteLine($" - {title}");
							}
						}
						else
						{
							logger.WriteLine($"not found {pid}");
						}
					}
				}

				Prune(container);
				logger.WriteLine(container.ToString());
			}

			logger.WriteTime("completed map");
		}


		/// <summary>
		/// Recursively remove any branch/node that doesn't contain a Page
		/// </summary>
		/// <param name="element">The root node</param>
		private void Prune(XElement element)
		{
			if (element.Elements().Any(e => e.Name.LocalName == "Page"))
			{
				return;
			}

			if (element.HasElements)
			{
				foreach (var item in element.Elements().ToList())
				{
					Prune(item);
				}

				if (!element.HasElements)
					element.Remove();
			}
			else
			{
				element.Remove();
			}
		}
	}
}
