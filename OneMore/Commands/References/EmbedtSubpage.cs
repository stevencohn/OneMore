//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class EmbedSubpageCommand : Command
	{
		private OneNote one;
		private Page page;
		private XNamespace ns;


		public EmbedSubpageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if ((args.Length > 0) && (args[0] is bool update) && update)
			{
				await UpdateContent();
				return;
			}

			EmbedContent();

			await Task.Yield();
		}


		//========================================================================================
		// Update...

		private async Task UpdateContent()
		{
			using (one = new OneNote(out page, out ns))
			{
				// find all embedded sections...

				var metas = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == Page.EmbeddedMetaName);

				if (!metas.Any())
				{
					UIHelper.ShowInfo(one.Window, "No embedded content found");
					return;
				}

				// updated each section...

				var updated = false;
				foreach (var meta in metas)
				{
					var tableRoot = meta.ElementsAfterSelf(ns + "Table").FirstOrDefault();
					if (tableRoot != null)
					{
						var sourceId = meta.Attribute("content").Value;
						var snippets = GetSnippets(sourceId, out var source, out var outline);
						if (!snippets.Any())
						{
							// error reading source
							return;
						}

						var table = new Table(tableRoot);
						FillCell(table[0][0], snippets, source);
						updated = true;
					}
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private IEnumerable<XElement> GetSnippets(
			string sourceId, out Page source, out Outline outline)
		{
			source = one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
			if (source == null)
			{
				UIHelper.ShowInfo(one.Window, "Source page not found");
				outline = null;
				return new List<XElement>();
			}

			var outRoot = source.Root.Elements(source.Namespace + "Outline").FirstOrDefault();
			if (outRoot == null)
			{
				UIHelper.ShowInfo(one.Window, "Source page contains no content1");
				outline = null;
				return new List<XElement>();
			}

			PageNamespace.Set(ns);
			outline = new Outline(outRoot);

			var snippets = outline.Elements(ns + "OEChildren")
				.Where(e => !e.Elements(ns + "OE")
					.Elements(ns + "Meta").Attributes(Page.EmbeddingsMetaName).Any());

			if (snippets == null || !snippets.Any())
			{
				UIHelper.ShowInfo(one.Window, "Source page contains no content2");
				return new List<XElement>();
			}

			return snippets;
		}


		private void FillCell(TableCell cell, IEnumerable<XElement> snippets, Page source)
		{
			var link = one.GetHyperlink(source.PageId, string.Empty);
			var citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			var header = new Paragraph($"<a href=\"{link}\">Embedded from {source.Title}</a>")
				.SetQuickStyle(citationIndex)
				.SetStyle("font-style:italic")
				.SetAlignment("right");

			header.AddFirst(new XElement(ns + "Meta",
				new XAttribute("name", Page.EmbedHeaderMetaName),
				new XAttribute("content", "1")));

			cell.SetContent(new XElement(ns + "OEChildren", header));

			foreach (var snippet in snippets)
			{
				cell.Root.Add(snippet);
			}
		}


		//========================================================================================
		// Embed...

		private void EmbedContent()
		{
			using (var o = new OneNote())
			{
				o.SelectLocation(
					"Select Page", "Select page to embed on this page",
					OneNote.Scope.Pages, Callback);
			}
		}


		private async Task Callback(string sourceId)
		{
			if (string.IsNullOrEmpty(sourceId))
			{
				// cancelled
				return;
			}

			using (one = new OneNote(out page, out ns))
			{
				var snippets = GetSnippets(sourceId, out var source, out var outline);
				if (!snippets.Any())
				{
					return;
				}

				page.EnsureContentContainer();

				var container = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Ancestors(ns + "OEChildren")
					.FirstOrDefault();

				if (container == null)
				{
					UIHelper.ShowInfo(one.Window, "Position cursor in body of page");
					return;
				}

				var table = new Table(ns, 1, 1)
				{
					BordersVisible = true,
				};

				var width = outline.GetWidth();
				table.SetColumnWidth(0, width == 0 ? 500 : width);

				FillCell(table[0][0], snippets, source);

				var paragraph = new Paragraph(table.Root);

				paragraph.AddFirst(new XElement(ns + "Meta",
					new XAttribute("name", Page.EmbeddedMetaName),
					new XAttribute("content", source.PageId)
					));

				container.Add(paragraph);
				await one.Update(page);
			}
		}
	}
}
