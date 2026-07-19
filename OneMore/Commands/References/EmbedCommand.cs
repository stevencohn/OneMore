//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S107 // more than 7 params

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Embeds the body of a page, or a delimited section of a page, into the current page
	/// with a Refresh link to resynchronize the content from the source.
	/// </summary>
	/// <remarks>
	/// The embedded content is wrapped in a single-cell table with a visible border.
	/// A Refresh link in the upper right of the cell allows the user to resync changes.
	/// The source page (and optional objectId for paragraph linking) is identified either
	/// from a OneNote URI on the clipboard or via the SelectLocation dialog.
	/// Begin/end tag strings may be used to embed only a slice of the source page.
	/// Tags are matched independently within each Outline on the source page; an outline
	/// may contain multiple begin/end blocks, all of which are embedded, and a begin tag
	/// with no matching end tag in the same outline runs to the end of that outline.
	/// </remarks>
	[CommandService]
	internal class EmbedCommand : Command, ICliPageCommand
	{
		private const string EmbedMetaName = "omEmbed";
		private const string EmbedHeaderMeta = "omEmbedHeader";

		// Placeholder for empty optional URL path segments in the refresh link.
		// InvokeCommand splits on '/' with RemoveEmptyEntries, so a truly empty segment
		// would collapse and shift every subsequent positional arg by one. Using this
		// sentinel keeps the slot present; Execute decodes it back to empty string.
		private const string EmptySegment = "\x01";


		private sealed class SourceInfo
		{
			public IEnumerable<XElement> Snippets;
			public string SourceId;
			public Page Page;
			public Outline Outline;
			public string XmlObjectId;  // XML objectID attribute of the OE; null for page-mode embeds
		}


		private sealed class RefreshParams
		{
			public string SourceId;
			public string ObjectId;
			public string LinkId;
			public string BeginTag;
			public string EndTag;
			public EmbedFormat Format;
			public EmbedStyle Style;
		}


		private OneNote one;
		private Page page;
		private XNamespace ns;


		public EmbedCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "Embed";

		public string Description => "Refresh all embedded page content within the specified scope";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook", required: true)
			.AddString("section", "Path of the section; omit for all sections", required: false)
			.AddString("page", "Name of the page; omit for all pages", required: false)
			.AddBoolean("refresh",
				"Must be true to confirm refreshing embedded content",
				required: true, defaultValue: false);

		#endregion CLI Implementation


		// InvokeCommand applies HttpUtility.UrlDecode before handing args to Execute,
		// so args are already decoded; Uri.UnescapeDataString is a harmless second pass.
		// Replace EmptySegment sentinel with empty string so callers see a clean value.
		private static string Decode(string raw) =>
			Uri.UnescapeDataString(raw ?? string.Empty) is var v && v == EmptySegment
				? string.Empty
				: v;


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is CliParameterSet cliParams)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }

				cliParams.TryGet("refresh", out bool doRefresh);
				if (!doRefresh)
				{
					logger.WriteLine("--refresh must be 'true' to refresh embedded content");
					return;
				}

				await RefreshPageEmbeds(pageId);
				return;
			}

			if (args.Length > 0 && args[0] is string s && s == "refresh")
			{
				logger = Logger.Current;

				var sourceId = args.Length > 1 ? args[1] as string : null;
				var objectId = args.Length > 2
					? Decode(args[2] as string)
					: string.Empty;
				var linkId = args.Length > 3 ? args[3] as string : null;
				var beginTag = args.Length > 4
					? Decode(args[4] as string)
					: string.Empty;
				var endTag = args.Length > 5
					? Decode(args[5] as string)
					: string.Empty;
				var format = args.Length > 6
					&& int.TryParse(args[6] as string, out var fi)
					? (EmbedFormat)fi
					: EmbedFormat.Formatted;
				var style = args.Length > 7
					&& int.TryParse(args[7] as string, out var si)
					? (EmbedStyle)si
					: EmbedStyle.Normal;

				await UpdateContent(sourceId, objectId, linkId, beginTag, endTag, format, style);
				IsCancelled = true;
				return;
			}

			await EmbedContent();
		}


		// ========================================================================================
		// Embed...

		private async Task EmbedContent()
		{
			if (BookmarkCommand.Bookmark is not null)
			{
				await EmbedFromBookmark(BookmarkCommand.Bookmark);
				return;
			}

			// check clipboard for a OneNote URI (from Copy Link to Page/Paragraph)
			var clip = await ClipboardProvider.GetText();
			if (!string.IsNullOrEmpty(clip))
			{
				var pageMatch = Regex.Match(clip, @"page-id=(\{[^}]+\})&");
				if (pageMatch.Success)
				{
					await EmbedFromClipboard(clip, null);
					return;
				}
			}

			await using var o = new OneNote();
			o.SelectLocation(
				Resx.EmbedCommand_Select,
				Resx.EmbedCommand_SelectIntro,
				OneNote.Scope.Pages,
				async (sid) => await Callback(sid, null));
		}


		private async Task EmbedFromBookmark(Bookmark bookmark)
		{
			string sourceName;
			XElement container;

			// Phase 1: pre-dialog — load current page and resolve source name.
			await using (var o = new OneNote())
			{
				page = await o.GetPage();
				ns = page?.Namespace;
				if (page is null)
				{
					return;
				}

				sourceName = o.GetHierarchyNode(bookmark.PageId)?.Name ?? bookmark.PageId;
				container = FindContainer();
			}

			if (container is null)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			var paragraphText = bookmark.Range.Root.TextValue(stripHtml: true);
			if (!ShowEmbedDialog(sourceName,
				out _, out _,
				out var format, out var style, out var indent,
				paragraphText))
			{
				return;
			}

			// Phase 2: post-dialog — fresh instance for source extraction and page update.
			await using (one = new OneNote())
			{
				await Embed(bookmark.PageId, bookmark.ObjectId,
					string.Empty, string.Empty,
					format, style, indent, container);
			}

			BookmarkCommand.Clear();
		}


		private async Task EmbedFromClipboard(string clipUri, string objectId)
		{
			string sourceId = null;
			string sourceName = null;
			XElement container = null;

			// Phase 1: pre-dialog — resolve source page, load current page, find insert point.
			// Disposed before the dialog opens so its COM proxy cannot affect Phase 2.
			await using (var o = new OneNote())
			{
				page = await o.GetPage();
				if (page is null)
				{
					return;
				}

				ns = page?.Namespace;

				try
				{
					// Remote (OneDrive) notebooks place two lines on the clipboard; the
					// onenote: URI we need is always the last onenote: line.
					var lines = clipUri.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					var uriLine = (lines.LastOrDefault(l =>
						l.TrimStart().StartsWith("onenote:", StringComparison.OrdinalIgnoreCase))
						?? clipUri).Trim();

					var link = OneNoteLinkParser.Parse(uriLine);

					logger.WriteLine($"embed source: notebook=[{link.NotebookName}] " +
						$"section=[{link.SectionName}] page=[{link.PageName}] " +
						$"pageId=[{link.PageId}] objectId=[{link.ObjectId}] isLocal=[{link.IsLocal}]");

					var sectionParts = new List<string>(link.SectionGroups) { link.SectionName };
					var sectionPath = string.Join("/", sectionParts);

					var pageIds = await o.FindPagesByPath(link.NotebookName, sectionPath, link.PageName);
					sourceId = pageIds.FirstOrDefault();
					if (sourceId != null)
					{
						sourceName = o.GetHierarchyNode(sourceId)?.Name ?? link.PageName;

						logger.WriteLine(
							$"found source page: {sourceName} ({sourceId}) " +
							$"at path [{link.NotebookName}/{sectionPath}/{link.PageName}]");
					}
					else
					{
						logger.WriteLine(
							$"page not found at path [{link.NotebookName}/{sectionPath}/{link.PageName}]");
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("error resolving clipboard URI to page", exc);
				}

				if (string.IsNullOrEmpty(sourceId))
				{
					ShowError(Resx.EmbedCommand_NoClipboardPage);
					return;
				}

				container = FindContainer();
			}

			if (container is null)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			if (!ShowEmbedDialog(sourceName,
				out var beginTag, out var endTag, out var format, out var style, out var indent))
			{
				return;
			}

			// Phase 2: post-dialog — fresh instance; no COM history from Phase 1 map building.
			await using (one = new OneNote())
			{
				await Embed(sourceId, null, beginTag, endTag, format, style, indent, container);
			}
		}


		private async Task Callback(string sourceId, string objectId)
		{
			if (string.IsNullOrEmpty(sourceId))
			{
				return;
			}

			string sourceName;
			XElement container;

			// Phase 1: pre-dialog — load current page and resolve source name.
			await using (var o = new OneNote())
			{
				page = await o.GetPage();
				ns = page?.Namespace;

				if (page is null)
				{
					return;
				}

				sourceName = o.GetHierarchyNode(sourceId)?.Name ?? sourceId;
				container = FindContainer();
			}

			if (container is null)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			if (!ShowEmbedDialog(sourceName,
				out var beginTag, out var endTag, out var format, out var style, out var indent))
			{
				return;
			}

			// Phase 2: post-dialog — fresh instance for source extraction and page update.
			await using (one = new OneNote())
			{
				await Embed(sourceId, objectId, beginTag, endTag, format, style, indent, container);
			}
		}


		private XElement FindContainer()
		{
			page.EnsureContentContainer();
			return page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.Ancestors(ns + "OEChildren")
				.FirstOrDefault();
		}


		private bool ShowEmbedDialog(
			string sourceName,
			out string beginTag, out string endTag,
			out EmbedFormat format, out EmbedStyle style,
			out bool indent,
			string bookmarkText = null)
		{
			using var dialog = new EmbedDialog(sourceName, page.Title, bookmarkText);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				beginTag = endTag = null;
				format = default;
				style = default;
				indent = false;
				return false;
			}

			beginTag = dialog.BeginTag;
			endTag = dialog.EndTag;
			format = dialog.Format;
			style = dialog.Style;
			indent = dialog.Indent;
			return true;
		}


		private async Task Embed(
			string sourceId, string objectId,
			string beginTag, string endTag,
			EmbedFormat format, EmbedStyle style,
			bool indent,
			XElement container)
		{
			var source = await GetSource(sourceId, objectId, null, beginTag, endTag);
			if (source is null || !source.Snippets.Any())
			{
				return;
			}

			var table = new Table(ns, 1, 1) { BordersVisible = true };

			// if cursor is inside a table, inherit its width; otherwise use source outline width
			XElement hostCell = container.Parent;
			while (hostCell != null && hostCell.Name.LocalName != "Cell")
			{
				hostCell = hostCell.Parent;
			}

			if (hostCell is null)
			{
				var width = source.Outline?.GetWidth() ?? 0;
				table.SetColumnWidth(0, width == 0 ? 500 : width);
			}

			try
			{
				await FillCell(table[0][0], source, beginTag, endTag, format, style);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error in FillCell", exc);
				return;
			}

			try
			{
				var editor = new PageEditor(page);
				var inner = new Paragraph(
					new Meta(EmbedMetaName, sourceId),
					table.Root
					);

				editor.AddNextParagraph(indent
					? new Paragraph(new XElement(ns + "OEChildren", inner))
					: inner);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error adding embed paragraph", exc);
			}

			await one.Update(page);
		}


		private async Task FillCell(
			TableCell cell, SourceInfo source,
			string beginTag, string endTag,
			EmbedFormat format, EmbedStyle style)
		{
			var link = await one.GetHyperlinkWithRetry(source.Page.PageId,
				source.XmlObjectId ?? string.Empty);

			if (link is null)
			{
				throw new InvalidOperationException("unable to get hyperlink for source page");
			}

			var match = Regex.Match(link, @"page-id=({[^}]+?})");
			var linkId = match.Success ? match.Groups[1].Value : string.Empty;

			var mapper = new TagMapper(page);
			mapper.MergeTagDefsFrom(source.Page);

			var quickmap = page.MergeQuickStyles(source.Page);
			var citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;

			var encodedSourceId = Uri.EscapeDataString(source.Page.PageId);
			var encodedObjectId = Uri.EscapeDataString(source.XmlObjectId ?? EmptySegment);
			var encodedLinkId   = Uri.EscapeDataString(linkId);
			var encodedBeginTag = Uri.EscapeDataString(string.IsNullOrEmpty(beginTag) ? EmptySegment : beginTag);
			var encodedEndTag = Uri.EscapeDataString(string.IsNullOrEmpty(endTag) ? EmptySegment : endTag);

			var refreshUrl =
				$"onemore://EmbedCommand/refresh/{encodedSourceId}/" +
				$"{encodedObjectId}/{encodedLinkId}/" +
				$"{encodedBeginTag}/{encodedEndTag}/" +
				$"{(int)format}/{(int)style}";

			var from = string.Format(Resx.EmbedCommand_EmbeddedFrom, source.Page.Title);

			var text =
				$"<a href=\"{link}\">{from}</a> | <a " +
				$"href=\"{refreshUrl}\">{Resx.word_Refresh}</a>";

			var header = new Paragraph(text)
				.SetQuickStyle(citationIndex)
				.SetStyle("font-style:italic")
				.SetAlignment("right");

			header.AddFirst(new Meta(EmbedHeaderMeta, "1"));

			cell.SetContent(new XElement(ns + "OEChildren", header));

			var snippets = format == EmbedFormat.PlainText
				? ToPlainText(source.Snippets, style)
				: source.Snippets;

			foreach (var snippet in snippets)
			{
				if (format == EmbedFormat.Formatted)
				{
					page.ApplyStyleMapping(quickmap, snippet);
					mapper.RemapTags(snippet);
				}

				cell.Root.Add(snippet);
			}
		}


		private async Task<SourceInfo> GetSource(
			string sourceId, string objectId, string linkId,
			string beginTag, string endTag)
		{
			var source = await one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
			if (source is null)
			{
				if (linkId is null)
				{
					return null;
				}

				logger.WriteLine("recovering from GetPage by mapping to hyperlink");

				using var token = new CancellationTokenSource();
				var map = await new HyperlinkProvider(one)
					.BuildHyperlinkMap(OneNote.Scope.Sections, token.Token,
						async (count) => { await Task.Yield(); },
						async () => { await Task.Yield(); });

				if (map.ContainsKey(linkId))
				{
					sourceId = map[linkId].PageID;
					source = await one.GetPage(sourceId, OneNote.PageDetail.BinaryData);
				}

				if (source is null)
				{
					ShowError(Resx.EmbedCommand_NoSource);
					return null;
				}
			}

			// Paragraph mode: try a direct XML objectID lookup (new refresh URLs store the
			// internal objectID attribute); fall back to URI-format mapping for older embeds.
			if (!string.IsNullOrEmpty(objectId))
			{
				var sns = source.Namespace;
				PageNamespace.Set(sns);

				var oe = source.Root.Descendants(sns + "OE")
					.FirstOrDefault(e =>
						e.Attribute("objectID")?.Value == objectId &&
						e.Descendants(sns + "T").Any())
					?? await FindParagraphOE(source, objectId);

				if (oe is null)
				{
					logger.WriteLine($"paragraph OE not found; objectId=[{objectId}]");
					ShowError(Resx.EmbedCommand_NoParagraph);
					return null;
				}

				var oeOutlineRoot = oe.Ancestors(sns + "Outline").FirstOrDefault()
					?? source.BodyOutlines.FirstOrDefault();

				return new SourceInfo
				{
					Snippets = new[] { new XElement(ns + "OEChildren", new XElement(oe)) },
					SourceId = sourceId,
					Page = source,
					Outline = oeOutlineRoot != null ? new Outline(oeOutlineRoot) : null,
					XmlObjectId = oe.Attribute("objectID")?.Value
				};
			}

			var outlineRoots = source.BodyOutlines.ToList();
			if (outlineRoots.Count == 0)
			{
				var schema = new PageSchema();
				var child = source.Root.Elements()
					.FirstOrDefault(e => e.Name.LocalName.In(schema.OeContent));

				if (child is not null)
				{
					child.Attribute("omHash")?.Remove();
					outlineRoots.Add(new XElement(ns + "Outline",
						new XElement(ns + "OEChildren",
							new XElement(ns + "OE", child))));
				}
			}

			if (outlineRoots.Count == 0)
			{
				ShowError(Resx.EmbedCommand_NoContent);
				return null;
			}

			PageNamespace.Set(ns);
			var firstOutline = new Outline(outlineRoots[0]);

			IEnumerable<XElement> snippets;

			if (!string.IsNullOrEmpty(beginTag) || !string.IsNullOrEmpty(endTag))
			{
				// each begin/end tag pair is scoped to its own outline; scan every
				// outline independently and collect all of their blocks, in order
				var collected = new List<XElement>();
				foreach (var root in outlineRoots)
				{
					var outline = new Outline(root);
					var oeChildren = outline.Elements(ns + "OEChildren").FirstOrDefault();
					if (oeChildren is null)
					{
						continue;
					}

					var extracted = ExtractTaggedContent(ns, oeChildren, beginTag, endTag);
					if (extracted is not null)
					{
						collected.AddRange(extracted);
					}
				}

				if (collected.Count == 0)
				{
					ShowError(Resx.EmbedCommand_NoContent);
					return null;
				}

				snippets = collected;
			}
			else
			{
				snippets = firstOutline.Elements(ns + "OEChildren");
			}

			if (!snippets.Any())
			{
				ShowError(Resx.EmbedCommand_NoContent);
				return null;
			}

			return new SourceInfo
			{
				Snippets = snippets,
				SourceId = sourceId,
				Page = source,
				Outline = firstOutline
			};
		}


		/// <summary>
		/// Finds the OE element in the source page whose URI-format object-id (as generated
		/// by GetObjectHyperlink) matches the clipboard's object-id. The clipboard format
		/// differs from the page XML objectID attribute format; mapping through the API is
		/// the only reliable way to resolve the correspondence.
		/// </summary>
		private async Task<XElement> FindParagraphOE(Page source, string clipObjectId)
		{
			var sns = source.Namespace;

			foreach (var oe in source.Root.Descendants(sns + "OE")
				.Where(e => e.Attribute("objectID") != null))
			{
				var xmlId = oe.Attribute("objectID").Value;
				var link = await one.GetObjectHyperlink(source.PageId, xmlId);
				if (string.IsNullOrEmpty(link)) continue;

				var m = Regex.Match(link, @"object-id=((?:\{[^}]+\})+)");
				if (!m.Success) continue;

				if (m.Groups[1].Value.Equals(clipObjectId, StringComparison.OrdinalIgnoreCase))
				{
					return oe;
				}
			}

			return null;
		}


		/// <summary>
		/// Scans a single outline's top-level OE paragraphs for one or more begin/end
		/// tag blocks. Multiple blocks in the same outline are all returned, in order.
		/// A begin tag with no matching end tag in this outline runs to the end of the
		/// outline; that also ends the scan since nothing remains after it.
		/// </summary>
		internal static IEnumerable<XElement> ExtractTaggedContent(
			XNamespace ns, XElement oeChildren, string beginTag, string endTag)
		{
			var oes = oeChildren.Elements(ns + "OE").ToList();
			var hasBegin = !string.IsNullOrEmpty(beginTag);
			var hasEnd = !string.IsNullOrEmpty(endTag);

			var blocks = new List<XElement>();
			var cursor = 0;

			while (cursor < oes.Count)
			{
				var startIdx = cursor;
				if (hasBegin)
				{
					var found = FindTagIndex(oes, beginTag, cursor);
					if (found < 0)
					{
						break; // no more begin tags in this outline
					}

					startIdx = found + 1; // skip the begin-tag paragraph itself
				}

				var endIdx = oes.Count - 1;
				var resumeFrom = oes.Count;
				if (hasEnd)
				{
					var found = FindTagIndex(oes, endTag, startIdx);
					if (found >= 0)
					{
						endIdx = found - 1; // skip the end-tag paragraph itself
						resumeFrom = found + 1;
					}
					// else: no matching end tag in this outline; run to the end
				}

				if (endIdx >= startIdx)
				{
					blocks.Add(BuildTaggedBlock(oeChildren, oes, startIdx, endIdx));
				}

				if (!hasBegin || resumeFrom >= oes.Count)
				{
					break;
				}

				cursor = resumeFrom;
			}

			return blocks.Count > 0 ? blocks : null;
		}


		private static XElement BuildTaggedBlock(
			XElement oeChildren, List<XElement> oes, int startIdx, int endIdx)
		{
			var filtered = new XElement(oeChildren.Name);
			foreach (var attr in oeChildren.Attributes())
			{
				filtered.Add(new XAttribute(attr));
			}

			for (var i = startIdx; i <= endIdx; i++)
			{
				filtered.Add(new XElement(oes[i]));
			}

			return filtered;
		}


		internal static int FindTagIndex(List<XElement> oes, string tag, int startFrom)
		{
			for (var i = startFrom; i < oes.Count; i++)
			{
				var text = oes[i].TextValue(stripHtml: true);
				if (text.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return i;
				}
			}

			return -1;
		}


		private IEnumerable<XElement> ToPlainText(
			IEnumerable<XElement> snippets, EmbedStyle style)
		{
			var quoteIndex = -1;
			var citationIndex = -1;

			if (style == EmbedStyle.Quote)
			{
				quoteIndex = page.GetQuickStyle(Styles.StandardStyles.Quote).Index;
			}
			else if (style == EmbedStyle.Citation)
			{
				citationIndex = page.GetQuickStyle(Styles.StandardStyles.Citation).Index;
			}

			var result = new List<XElement>();
			foreach (var snippet in snippets)
			{
				var newSnippet = new XElement(ns + "OEChildren");
				CollectPlainParagraphs(snippet, newSnippet, style, quoteIndex, citationIndex);
				if (newSnippet.HasElements)
				{
					result.Add(newSnippet);
				}
			}

			return result;
		}


		private void CollectPlainParagraphs(
			XElement container, XElement target,
			EmbedStyle style, int quoteIndex, int citationIndex)
		{
			foreach (var oe in container.Elements(ns + "OE"))
			{
				// only process OEs that have direct T content
				if (oe.Elements(ns + "T").Any())
				{
					var rawText = string.Concat(
						oe.Elements(ns + "T").Select(t => t.TextValue(stripHtml: true)));

					if (!string.IsNullOrWhiteSpace(rawText))
					{
						var para = new Paragraph(rawText.Trim());

						switch (style)
						{
							case EmbedStyle.Italic:
								para.SetStyle("font-style:italic");
								break;
							case EmbedStyle.Gray:
								para.SetStyle("color:#595959");
								break;
							case EmbedStyle.Quote when quoteIndex >= 0:
								para.SetQuickStyle(quoteIndex);
								break;
							case EmbedStyle.Citation when citationIndex >= 0:
								para.SetQuickStyle(citationIndex);
								break;
						}

						target.Add(para);
					}
				}

				// recurse into nested OEChildren
				foreach (var children in oe.Elements(ns + "OEChildren"))
				{
					CollectPlainParagraphs(children, target, style, quoteIndex, citationIndex);
				}
			}
		}


		// ========================================================================================
		// Update...

		private async Task RefreshPageEmbeds(string pageId)
		{
			await using (one = new OneNote())
			{
				var targetPage = await one.GetPage(pageId, OneNote.PageDetail.BinaryData);
				if (targetPage is null) { return; }

				page = targetPage;
				ns = page.Namespace;

				var metas = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name")?.Value == EmbedMetaName)
					.ToList();

				if (!metas.Any())
				{
					logger.WriteLine($"no embedded content found on page [{page.Title}]");
					return;
				}

				var updated = false;
				foreach (var meta in metas)
				{
					var tableRoot = meta.ElementsAfterSelf(ns + "Table").FirstOrDefault();
					if (tableRoot is null) { continue; }

					var refreshUrl = FindRefreshUrl(tableRoot);
					if (refreshUrl is null)
					{
						logger.WriteLine("embed table found but Refresh URL could not be located");
						continue;
					}

					var p = ParseRefreshUrl(refreshUrl);
					if (p is null)
					{
						logger.WriteLine($"unrecognized refresh URL format: {refreshUrl}");
						continue;
					}

					logger.WriteLine($"refreshing embed from source [{p.SourceId}]");

					try
					{
						var source = await GetSource(p.SourceId, p.ObjectId, p.LinkId, p.BeginTag, p.EndTag);
						if (source is null || !source.Snippets.Any()) { continue; }

						var table = new Table(tableRoot);
						await FillCell(table[0][0], source, p.BeginTag, p.EndTag, p.Format, p.Style);
						updated = true;
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error refreshing embed from source [{p.SourceId}]", exc);
					}
				}

				if (updated)
				{
					await one.Update(page);
					logger.WriteLine($"updated page [{page.Title}]");
				}
			}
		}


		private string FindRefreshUrl(XElement tableRoot)
		{
			var headerOE = tableRoot.Descendants(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == EmbedHeaderMeta)
				?.Parent;

			if (headerOE is null) { return null; }

			var cdata = headerOE.Elements(ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null);

			if (cdata is null) { return null; }

			var match = Regex.Match(cdata.Value,
				@"href=""(onemore://EmbedCommand/refresh/[^""]+)""");
			return match.Success ? match.Groups[1].Value : null;
		}


		private static RefreshParams ParseRefreshUrl(string url)
		{
			const string prefix = "onemore://EmbedCommand/refresh/";
			if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) { return null; }

			var parts = url.Substring(prefix.Length).Split('/');
			if (parts.Length < 7) { return null; }

			return new RefreshParams
			{
				SourceId = Decode(parts[0]),
				ObjectId = Decode(parts[1]),
				LinkId   = Decode(parts[2]),
				BeginTag = Decode(parts[3]),
				EndTag   = Decode(parts[4]),
				Format   = int.TryParse(parts[5], out var fi) ? (EmbedFormat)fi : EmbedFormat.Formatted,
				Style    = int.TryParse(parts[6], out var si) ? (EmbedStyle)si : EmbedStyle.Normal
			};
		}


		private async Task UpdateContent(
			string sourceId, string objectId, string linkId,
			string beginTag, string endTag,
			EmbedFormat format, EmbedStyle style)
		{
			await using (one = new OneNote(out page, out ns))
			{
				var metas = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == EmbedMetaName);

				if (!string.IsNullOrEmpty(sourceId))
				{
					metas = metas.Where(e => e.Attribute("content").Value == sourceId);
				}

				if (!metas.Any())
				{
					ShowError(Resx.EmbedCommand_NoSource);
					return;
				}

				var updated = false;
				foreach (var meta in metas.ToList())
				{
					var tableRoot = meta.ElementsAfterSelf(ns + "Table").FirstOrDefault();
					if (tableRoot is null)
					{
						continue;
					}

					var currentSourceId = meta.Attribute("content").Value;
					var source = await GetSource(currentSourceId, objectId, linkId, beginTag, endTag);
					if (source is null || !source.Snippets.Any())
					{
						ShowError(Resx.EmbedCommand_NoSource);
						return;
					}

					var table = new Table(tableRoot);
					await FillCell(table[0][0], source, beginTag, endTag, format, style);

					updated = true;
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}
	}
}
