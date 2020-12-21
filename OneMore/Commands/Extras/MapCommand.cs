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

				logger.WriteLine(container.ToString());
			}

			logger.WriteTime("completed map");
		}
	}
}
/*
<one:Section xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote" name="Bozo" ID="{58361066-90D5-083C-1BFF-E864E3829D3C}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/Queens/Jesters/Bozo.one" lastModifiedTime="2020-12-21T12:53:15.000Z" color="#D5A4BB" isCurrentlyViewed="true">
  <one:Page ID="{58361066-90D5-083C-1BFF-E864E3829D3C}{1}{E1952585663932227640441976408449536156760461}" name="Table of Contents - Notebook Flux" dateTime="2020-12-21T12:53:13.000Z" lastModifiedTime="2020-12-21T12:53:13.000Z" pageLevel="1" isCurrentlyViewed="true" />
  <one:Page ID="{58361066-90D5-083C-1BFF-E864E3829D3C}{1}{E1953698246395506670281930038931012073898261}" name="Table of Contents - Section Bozo" dateTime="2020-12-21T12:52:39.000Z" lastModifiedTime="2020-12-21T12:52:39.000Z" pageLevel="1" />
  <one:Page ID="{58361066-90D5-083C-1BFF-E864E3829D3C}{1}{E1856951129749826737220152322277752777303331}" name="bottom" dateTime="2020-10-27T16:16:50.000Z" lastModifiedTime="2020-11-14T14:28:36.000Z" pageLevel="1" />
</one:Section>

<one:T selected="all"><![CDATA[<a 
	href="onenote:#bottom&amp;section-id={33AAF2F3-FDD9-44AE-8F98-7D956E8F2A13}&amp;page-id={0CD0DE2E-C3D2-4CF2-BA28-021CAFB62639}&amp;end&amp;base-path=https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/Queens/Jesters/Bozo.one">bottom</a>]]></one:T>
*/