//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Xml.Linq;


	internal class MapCommand : Command
	{

		private readonly Regex regex;
		private readonly Dictionary<string, string> titles;

		private OneNote one;
		private OneNote.Scope scope;
		private XNamespace ns;


		public MapCommand()
		{
			// internal hyperlinks are within CDATA similar to: <a href="onenote:#three&amp;
			//   section-id={..}&amp;page-id={..}&amp;end&amp;base-path=https://.."
			regex = new Regex(@"page-id=({[^}]+?})");

			titles = new Dictionary<string, string>();
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new MapDialog())
			{
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
			}

			var progressDialog = new UI.ProgressDialog(Execute);
			progressDialog.RunModeless();
		}


		private void Execute(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.StartClock();

			using (one = new OneNote())
			{
				var hierarchy = GetHierarchy();
				ns = one.GetNamespace(hierarchy);

				if (token.IsCancellationRequested)
					return;

				var hyperlinks = one.BuildHyperlinkCache(scope, token,
					(count) =>
					{
						progress.SetMaximum(count);
						progress.SetMessage($"Scanning {count} page references");
					},
					() =>
					{
						progress.Increment();
					});

				if (token.IsCancellationRequested)
					return;

				logger.WriteTime($"built hyperlink cache for {hyperlinks.Count} pages", true);

				var elements = hierarchy.Descendants(ns + "Page").ToList();

				progress.SetMaximum(elements.Count);
				progress.SetMessage("Building map");

				foreach (var element in elements)
				{
					progress.Increment();

					if (token.IsCancellationRequested)
						return;

					var parentId = element.Attribute("ID").Value;
					var xml = one.GetPageXml(parentId);
					var matches = regex.Matches(xml);

					if (matches.Count == 0)
					{
						element.Remove();
						continue;
					}

					var parent = new Page(XElement.Parse(xml));

					var name = element.Parent.Attribute("name").Value;
					progress.SetMessage($"Scanning {name}/{parent.Title}");

					foreach (Match match in matches)
					{
						var pid = match.Groups[1].Value;
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
									new XAttribute("ID", pageId)
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

				if (token.IsCancellationRequested)
					return;

				Prune(hierarchy);
				logger.WriteLine(hierarchy.ToString());

				BuildMapPage(hierarchy);
			}

			logger.WriteTime("map complete");
		}


		private XElement GetHierarchy()
		{
			XElement hierarchy;
			switch (scope)
			{
				case OneNote.Scope.Notebooks: // all notebooks
					hierarchy = one.GetNotebooks(OneNote.Scope.Pages);
					break;

				case OneNote.Scope.Sections: // all sectios in current notebook
					hierarchy = one.GetNotebook(OneNote.Scope.Pages);
					break;

				default: // current section
					hierarchy = one.GetSection();
					break;
			}

			// ignore Metas and the recycle bin
			hierarchy.Elements()
				.Where(e =>
					e.Name.LocalName == "Meta" ||
					e.Attributes().Any(a => a.Name == "isRecycleBin"))
				.Remove();

			return hierarchy;
		}


		/// <summary>
		/// Recursively remove any branch/node that doesn't contain a Page
		/// </summary>
		/// <param name="element">The root node</param>
		private void Prune(XElement element)
		{
			if (element.Elements().Any(e => e.Name.LocalName == "Ref"))
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


		private void BuildMapPage(XElement hierarchy)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			page.Title = "Page Map";

			var container = page.EnsureContentContainer();

			BuildMapPage(hierarchy, page, container);

			one.Update(page);
			one.NavigateTo(pageId);
		}


		private void BuildMapPage(XElement element, Page page, XElement container)
		{
			if (element.Name.LocalName == "Page")
			{
				var pname = element.Attribute("name").Value;
				var plink = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);

				container.Add(new XElement(ns + "OE",
					new XElement(ns + "T",
						new XCData($"<a href=\"{plink}\">{pname}</a>")
					)));

				foreach (var reference in element.Elements("Ref"))
				{
					pname = reference.Attribute("title").Value;
					plink = one.GetHyperlink(reference.Attribute("ID").Value, string.Empty);

					container.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XCData($". . <a href=\"{plink}\">{pname}</a>")
						)));
				}
			}
			else
			{
				int index;

				if (element.Name.LocalName == "Section")
				{
					index = MakeQuickStyle(page, StandardStyles.Heading3);
				}
				else if (element.Name.LocalName == "SectionGroup")
				{
					index = MakeQuickStyle(page, StandardStyles.Heading2);
				}
				else // notebook
				{
					index = MakeQuickStyle(page, StandardStyles.Heading1);
				}

				container.Add(new XElement(ns + "OE",
					new XAttribute("quickStyleIndex", index.ToString()),
					new XElement(ns + "T",
					new XCData(element.Attribute("name").Value)
					)));

				foreach (var child in element.Elements())
				{
					BuildMapPage(child, page, container);
				}
			}
		}


		private int MakeQuickStyle(Page page, StandardStyles standard)
		{
			var quick = standard.GetDefaults();

			var styles = page.GetQuickStyles();
			var style = styles.FirstOrDefault(s => s.Name == quick.Name);

			if (style != null)
			{
				return style.Index;
			}

			quick.Index = styles.Max(s => s.Index) + 1;

			page.AddQuickStyleDef(quick.ToElement(ns));

			return quick.Index;
		}
	}
}
/*
<one:Section xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote" name="OneMore" ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/OneMore.one" lastModifiedTime="2020-12-22T17:06:54.000Z" color="#B49EDE" isCurrentlyViewed="true">
  <one:Page ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{E1954536723328074054651949015119196467408721}" name="To avoid any issues, outstanding changes must be discarded when the lock is releasedâ€”either implicitly on â€œ session failureâ€ or explicitly by using the unlock operation." dateTime="2020-12-19T13:48:40.000Z" lastModifiedTime="2020-12-22T17:06:54.000Z" pageLevel="1">
    <one:Meta name="omHighlightIndex" content="1" />
    <Ref title="Play" pageId="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{E17811441452697988761986790585466670065101}" />
  </one:Page>
</one:Section>

<one:Notebook xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote" name="Personal" nickname="Personal" ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/" lastModifiedTime="2020-12-22T00:15:48.000Z" color="#F6B078" isCurrentlyViewed="true">
  <one:Section name="Hofstra" ID="{657DD8C1-23A4-4733-9009-BCECF5857CA5}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/Hofstra.one" lastModifiedTime="2020-12-11T11:28:03.000Z" color="#B49EDE">
    <one:Page ID="{657DD8C1-23A4-4733-9009-BCECF5857CA5}{1}{E1953424436810837733961954981927889815044431}" name="Hofstra" dateTime="2017-02-02T23:15:19.000Z" lastModifiedTime="2020-12-11T11:28:03.000Z" pageLevel="1">
      <Ref title="529 Confirmation 9/4/2020" pageId="{657DD8C1-23A4-4733-9009-BCECF5857CA5}{1}{E19518009081657497934320169331363838181454091}" />
      <Ref title="529 Confirmation 2/1/2020" pageId="{657DD8C1-23A4-4733-9009-BCECF5857CA5}{1}{E1946875086941500500191984307843079045118961}" />
    </one:Page>
  </one:Section>

<one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
  <one:Notebook name="Personal" nickname="Personal" ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/" lastModifiedTime="2020-12-22T17:06:54.000Z" color="#F6B078" isCurrentlyViewed="true">
    <one:Section name="Notes" ID="{414F807D-E5C7-041C-1513-376CEB04177B}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/Notes.one" lastModifiedTime="2020-11-14T12:03:10.000Z" color="#F6B078">
      <one:Page ID="{414F807D-E5C7-041C-1513-376CEB04177B}{1}{E19508546433742256804920172178273817917930781}" name="iDevice, Kindle, Surface" dateTime="2011-04-28T23:44:17.000Z" lastModifiedTime="2020-07-13T16:19:32.000Z" pageLevel="1" isCollapsed="true">
        <Ref title="iPhone" pageId="{19D2987D-72BD-0D29-17FD-7D30C15F1FE2}{1}{E19476019463238140933820182814637071124832001}" />
      </one:Page>
    </one:Section>
 */