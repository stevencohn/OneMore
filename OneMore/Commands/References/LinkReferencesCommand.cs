﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class LinkReferencesCommand : Command
	{
		private OneNote one;
		private OneNote.Scope scope;
		private XNamespace ns;


		public LinkReferencesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new LinkDialog())
			{
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
			}

			var progressDialog = new UI.ProgressDialog(Execute);
			await progressDialog.RunModeless();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			using (one = new OneNote(out var page, out ns))
			{
				var title = page.Title;

				string startId = string.Empty;
				switch (scope)
				{
					case OneNote.Scope.Sections: startId = one.CurrentNotebookId; break;
					case OneNote.Scope.Pages: startId = one.CurrentSectionId; break;
				}

				var results = one.Search(startId, title);

				if (token.IsCancellationRequested)
				{
					logger.WriteLine("cancelled");
					return;
				}



				var elements = hierarchy.Descendants(ns + "Page").ToList();
				var titles = new Dictionary<string, string>();

				logger.WriteLine($"building map for {elements.Count} pages");
				progress.SetMaximum(elements.Count);
				progress.SetMessage($"Building map for {elements.Count} pages");

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

					// prevent duplicates
					var refs = new List<string>();

					foreach (Match match in matches)
					{
						var pid = match.Groups[1].Value;
						if (refs.Contains(pid))
						{
							// already captured this pid
							continue;
						}

						var hyperlink = hyperlinks[pid];
						if (hyperlink.PageID != parentId)
						{
							string title;
							if (titles.ContainsKey(hyperlink.PageID))
							{
								title = titles[hyperlink.PageID];
							}
							else
							{
								var p = one.GetPage(hyperlink.PageID, OneNote.PageDetail.Basic);
								title = p.Title;
								titles.Add(hyperlink.PageID, title);
							}

							element.Add(new XElement("Ref",
								new XAttribute("title", title),
								new XAttribute("ID", hyperlink.PageID)
								));

							//logger.WriteLine($" - {title}");

							refs.Add(pid);
						}
					}
				}

				if (titles.Count == 0)
				{
					UIHelper.ShowMessage("No linked pages were found");
					return;
				}

				if (token.IsCancellationRequested)
					return;

				Prune(hierarchy);
				await BuildMapPage(hierarchy);
			}

			logger.WriteTime("map complete");
			logger.End();
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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async Task BuildMapPage(XElement hierarchy)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = one.GetPage(pageId);
			page.SetMeta(Page.PageMapMetaName, "true");

			switch (scope)
			{
				case OneNote.Scope.Notebooks: page.Title = "Page Map of All Notebooks"; break;
				case OneNote.Scope.Sections: page.Title = "Page Map of This Notebook"; break;
				default: page.Title = "Page Map of This Section"; break;
			}

			var container = page.EnsureContentContainer();

			container.Add(
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(
					"<span style=\"font-weight:bold\">Key</span><br>\n" +
					"Heading 1 = Notebooks<br>\n" +
					"Heading 2 = Section<br>\n" +
					"Heading 3 = Section Groups (italic)")
				)),
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty)))
				);

			if (hierarchy.Name.LocalName == "Notebooks")
			{
				hierarchy.Elements().ToList().ForEach((node) =>
				{
					container.Add(BuildMapPage(node, page));
				});
			}
			else
			{
				container.Add(BuildMapPage(hierarchy, page));
			}

			await one.Update(page);
			await one.NavigateTo(pageId);
		}


		private IEnumerable<XElement> BuildMapPage(XElement element, Page page)
		{
			var content = new List<XElement>();

			if (element.Name.LocalName == "Page")
			{
				var pname = element.Attribute("name").Value;
				var plink = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);

				var children = new XElement(ns + "OEChildren");

				foreach (var reference in element.Elements("Ref"))
				{
					var rname = reference.Attribute("title").Value;
					var rlink = one.GetHyperlink(reference.Attribute("ID").Value, string.Empty);

					children.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XCData($"{RightArrow} <a href=\"{rlink}\">{rname}</a>")
						)));
				}

				content.Add(new XElement(ns + "OE",
					new XElement(ns + "T",
						new XCData($"<a href=\"{plink}\">{pname}</a>")),
					children
					));
			}
			else
			{
				var text = element.Attribute("name").Value;

				int index;
				if (element.Name.LocalName == "Section")
				{
					index = MakeQuickStyle(page, StandardStyles.Heading2);
				}
				else if (element.Name.LocalName == "SectionGroup")
				{
					index = MakeQuickStyle(page, StandardStyles.Heading3);
					text = $"<span style=\"font-style:'italic'\">{text}</span>";
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
					new XElement(ns + "T", new XCData(text)),
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
