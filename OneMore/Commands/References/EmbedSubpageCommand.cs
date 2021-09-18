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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class EmbedSubpageCommand : Command
	{
		// OE meta indicating content embedded content from another page
		private const string EmbeddedMetaName = "omEmbedded";
		// OE meta indicating content embedded content header paragraph
		private const string EmbedHeaderMetaName = "omEmbedHeader";
		// OE meta indicating content lists other pages where this page is embedded
		private const string EmbeddingsMetaName = "omEmbeddings";

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
				// not sure why logger is null here so we have to set it
				logger = Logger.Current;

				var sourceId = args.Length > 1 ? args[1] as string : null;
				await UpdateContent(sourceId);
				return;
			}

			EmbedContent();
		}


		//========================================================================================
		// Update...

		private async Task UpdateContent(string sourceId)
		{
			using (one = new OneNote(out page, out ns))
			{
				// find all embedded sections...

				var metas = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == EmbeddedMetaName);

				if (!string.IsNullOrEmpty(sourceId))
				{
					// refine filter by source pageId
					metas = metas.Where(e => e.Attribute("content").Value == sourceId);
				}

				if (!metas.Any())
				{
					UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoEmbedded);
					return;
				}

				// updated each section...

				var updated = false;
				foreach (var meta in metas)
				{
					var tableRoot = meta.ElementsAfterSelf(ns + "Table").FirstOrDefault();
					if (tableRoot != null)
					{
						sourceId = meta.Attribute("content").Value;
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
				UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoSource);
				outline = null;
				return new List<XElement>();
			}

			var outRoot = source.Root.Elements(source.Namespace + "Outline").FirstOrDefault();
			if (outRoot == null)
			{
				UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoContent);
				outline = null;
				return new List<XElement>();
			}

			PageNamespace.Set(ns);
			outline = new Outline(outRoot);

			var snippets = outline.Elements(ns + "OEChildren")
				.Where(e => !e.Elements(ns + "OE")
					.Elements(ns + "Meta").Attributes(EmbeddingsMetaName).Any());

			if (snippets == null || !snippets.Any())
			{
				UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoContent);
				return new List<XElement>();
			}

			return snippets;
		}


		private void FillCell(TableCell cell, IEnumerable<XElement> snippets, Page source)
		{
			var link = one.GetHyperlink(source.PageId, string.Empty);
			var citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			var text = $"<a href=\"{link}\">Embedded from {source.Title}</a> | <a " +
				$"href=\"onemore://EmbedSubpageProxy/true/{source.PageId}\">{Resx.EmbedSubpageCommand_Refresh}</a>";

			var header = new Paragraph(text)
				.SetQuickStyle(citationIndex)
				.SetStyle("font-style:italic")
				.SetAlignment("right");

			header.AddFirst(new Meta(EmbedHeaderMetaName, "1"));

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
					Resx.EmbedSubpageCommand_Select,
					Resx.EmbedSubpageCommand_SelectIntro,
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

				var container = page.Root.Elements(ns + "Outline")
					.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Ancestors(ns + "OEChildren")
					.FirstOrDefault();

				if (container == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.Error_BodyContext);
					return;
				}

				var table = new Table(ns, 1, 1)
				{
					BordersVisible = true,
				};

				// is cursor positioned in an existing table cell?
				XElement hostCell = container.Parent;
				while (hostCell != null && hostCell.Name.LocalName != "Cell")
				{
					hostCell = hostCell.Parent;
				}

				if (hostCell == null)
				{
					// set width to width of source page outline
					var width = outline.GetWidth();
					table.SetColumnWidth(0, width == 0 ? 500 : width);
				}

				FillCell(table[0][0], snippets, source);

				page.AddNextParagraph(new Paragraph(
					new Meta(EmbeddedMetaName, source.PageId),
					table.Root
					));

				await one.Update(page);
			}
		}
	}
}
