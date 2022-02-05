//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class EmbedSubpageCommand : Command
	{
		private sealed class SourceInfo
		{
			public IEnumerable<XElement> Snippets;
			public string SourceId;
			public Page Page;
			public Outline Outline;
		}


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
				var linkId = args.Length > 2 ? args[2] as string : null;
				await UpdateContent(sourceId, linkId);
				return;
			}

			EmbedContent();
		}


		//========================================================================================
		// Update...

		private async Task UpdateContent(string sourceId, string linkId)
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
						var source = await GetSource(sourceId, linkId);
						if (source == null || !source.Snippets.Any())
						{
							// error reading source
							UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoEmbedded);
							return;
						}

						var table = new Table(tableRoot);
						FillCell(table[0][0], source.Snippets, source.Page);
						updated = true;
					}
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private async Task<SourceInfo> GetSource(string sourceId, string linkId)
		{
			var source = one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
			if (source == null)
			{
				if (linkId == null)
				{
					// linkId should only be null from EmbedContent because it's using the local
					// reference to the page, but GetPage still couldn't find it so give up!
					return null;
				}

				logger.WriteLine("recoverying from GetPage by mapping to hyperlink");

				var token = new CancellationTokenSource();
				var map = await one.BuildHyperlinkMap(OneNote.Scope.Sections, token.Token,
					async (count) => { await Task.Yield(); },
					async () => { await Task.Yield(); });

				if (map.ContainsKey(linkId))
				{
					sourceId = map[linkId].PageID;
					source = one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
				}

				if (source == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoSource);
					return null;
				}
			}

			var outRoot = source.Root.Elements(source.Namespace + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank));

			if (outRoot == null)
			{
				UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoContent);
				return null;
			}

			PageNamespace.Set(ns);
			var outline = new Outline(outRoot);

			var snippets = outline.Elements(ns + "OEChildren")
				.Where(e => !e.Elements(ns + "OE")
					.Elements(ns + "Meta").Attributes(EmbeddingsMetaName).Any());

			if (snippets == null || !snippets.Any())
			{
				UIHelper.ShowInfo(one.Window, Resx.EmbedSubpageCommand_NoContent);
				return null;
			}

			return new SourceInfo
			{
				Snippets = snippets,
				SourceId = sourceId,
				Page = source,
				Outline = outline
			};
		}


		private void FillCell(TableCell cell, IEnumerable<XElement> snippets, Page source)
		{
			var link = one.GetHyperlink(source.PageId, string.Empty);
			var match = Regex.Match(link, @"page-id=({[^}]+?})");
			var linkId = match.Success ? match.Groups[1].Value : string.Empty;

			var tagmap = page.MergeTagDefs(source);
			var quickmap = page.MergeQuickStyles(source);
			var citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			var text = $"<a href=\"{link}\">Embedded from {source.Title}</a> | <a " +
				$"href=\"onemore://EmbedSubpageProxy/true/{source.PageId}/{linkId}\">{Resx.EmbedSubpageCommand_Refresh}</a>";

			var header = new Paragraph(text)
				.SetQuickStyle(citationIndex)
				.SetStyle("font-style:italic")
				.SetAlignment("right");

			header.AddFirst(new Meta(EmbedHeaderMetaName, "1"));

			cell.SetContent(new XElement(ns + "OEChildren", header));

			foreach (var snippet in snippets)
			{
				page.ApplyStyleMapping(quickmap, snippet);
				page.ApplyTagDefMapping(tagmap, snippet);

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
				var source = await GetSource(sourceId, null);
				if (source == null || !source.Snippets.Any())
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
					var width = source.Outline.GetWidth();
					table.SetColumnWidth(0, width == 0 ? 500 : width);
				}

				FillCell(table[0][0], source.Snippets, source.Page);

				page.AddNextParagraph(new Paragraph(
					new Meta(EmbeddedMetaName, source.Page.PageId),
					table.Root
					));

				await one.Update(page);
			}
		}
	}
}
