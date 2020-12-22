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

		private OneNote one;
		private OneNote.Scope scope;
		private XNamespace ns;


		public MapCommand()
		{
			// internal hyperlinks are within CDATA similar to: <a href="onenote:#three&amp;
			//   section-id={..}&amp;page-id={..}&amp;end&amp;base-path=https://.."
			regex = new Regex(@"page-id=({[^}]+?})");
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

			var titles = new Dictionary<string, string>();

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

			container.Add(BuildMapPage(hierarchy, page));

			one.Update(page);
			one.NavigateTo(pageId);
		}


		private IEnumerable<XElement> BuildMapPage(XElement element, Page page)
		{
			var content = new List<XElement>();

			if (element.Name.LocalName == "Page")
			{
				var pname = element.Attribute("name").Value;
				var plink = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);

				content.Add(new XElement(ns + "OE",
					new XElement(ns + "T",
						new XCData($"<a href=\"{plink}\">{pname}</a>")
					)));

				foreach (var reference in element.Elements("Ref"))
				{
					pname = reference.Attribute("title").Value;
					plink = one.GetHyperlink(reference.Attribute("ID").Value, string.Empty);

					content.Add(new XElement(ns + "OE",
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

				var indents = new XElement(ns + "OEChildren");

				foreach (var child in element.Elements())
				{
					indents.Add(BuildMapPage(child, page));
				}

				content.Add(new XElement(ns + "OE",
					new XAttribute("quickStyleIndex", index.ToString()),
					new XElement(ns + "T", new XCData(element.Attribute("name").Value)),
					indents
					));
			}

			return content;
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
