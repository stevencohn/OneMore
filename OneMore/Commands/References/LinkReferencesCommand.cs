//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S6605 // Collection-specific "Exists" method should be used (Array!!)

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// This is an automated approach to bidirectional linking that discovers all mentions of the
	/// title of the current page on other pages and creates bi-directional reference links
	/// between the two. Also, a Linked References paragraph is appended to this page with links
	/// back to all referring pages. If the current page title has a date prefix of the form 
	/// yyyy-mm-dd then that is ignored and the remainder of the title is used to search for back
	/// references.
	/// </summary>
	[CommandService]
	internal class LinkReferencesCommand : Command
	{
		private sealed class Referral
		{
			public string PageID { get; set; }
			public string Title { get; set; }
			public string Synopsis { get; set; }
		}


		// OE meta for linked references paragraph
		public const string LinkRefsMeta = "omLinkedReferences";
		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";
		private const int SynopsisLength = 110;

		private OneNote one;
		private Page anchorPage;
		private string anchorTitle;
		private OneNote.Scope scope;
		private bool refreshReferences;
		private bool showSynopsis;
		private SearchAndReplaceEditor editor;
		private List<Referral> referrals;


		public LinkReferencesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			refreshReferences = false;
			if (args.Length > 0 && args[0] is string refresh && refresh == "refresh")
			{
				refreshReferences = true;
				showSynopsis = args.Any(a => a as string == "synopsis");
			}

			var pageLink = await GetAnchorPage();

			if (!refreshReferences)
			{
				using var dialog = new LinkDialog(anchorTitle);
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
				showSynopsis = dialog.Synopsis;
			}

			PageNamespace.Set(anchorPage.Namespace);

			// initialize search-and-replace editor...

			var whatText = $@"(?:^|\b|\s)({anchorTitle.EscapeForRegex()})(?:$|\b|\s)";

			var withElement = new XElement("a",
				new XAttribute("href", pageLink),
				anchorTitle
				);

			editor = new SearchAndReplaceEditor(whatText, withElement,
				enableRegex: true,
				caseSensitive: false
				);

			// get busy...

			var progressDialog = new UI.ProgressDialog(Scan);
			progressDialog.RunModeless();
		}


		private async Task<string> GetAnchorPage()
		{
			await using var onx = new OneNote(out anchorPage, out var ns, OneNote.PageDetail.Basic);

			// read page title, ignore the date stamp prefix and emoji prefixes in a page title
			anchorTitle = CleanTitle(anchorPage.Title);

			var link = onx.GetHyperlink(anchorPage.PageId, string.Empty);

			// parse meta...

			var meta = anchorPage.BodyOutlines.Descendants(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == LinkRefsMeta);

			if (meta is null)
			{
				return link;
			}

			var parts = meta.Attribute("content").Value.Split(',');
			if (!Enum.TryParse(parts[0], out scope))
			{
				scope = OneNote.Scope.Pages;
			}

			if (parts.Length > 1)
			{
				bool.TryParse(parts[1], out showSynopsis);
			}

			if (refreshReferences)
			{
				// remove containing OE so it can be regenerated fully
				meta.Parent.Remove();
			}

			return link;
		}


		private static string CleanTitle(string title)
		{
			// strip date stamp
			var match = Regex.Match(title, @"^\d{4}-\d{2}-\d{2}\s");
			if (match.Success)
			{
				title = title.Substring(match.Length);
			}

			// strip emojis (Segoe UI Emoji font)
			using var emojis = new Emojis();
			return emojis.RemoveEmojis(title).Trim();
		}


		private async Task Scan(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			referrals = new List<Referral>();

			try
			{
				await using (one = new OneNote())
				{
					if (scope == OneNote.Scope.Notebooks)
					{
						var notebooks = await one.GetNotebooks();
						var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);
						foreach (var notebook in notebooks.Elements(ns + "notebook"))
						{
							var book = await one.GetNotebook(
								notebooks.Attribute("ID").Value, OneNote.Scope.Pages);

							// skip recycle bin sections by removing those nodes
							book.Descendants(ns + "Section")
								.Where(e => e.Attribute("isRecycleBin") is not null)
								.Remove();

							await ScanPages(progress, token, book);
							if (token.IsCancellationRequested)
							{
								logger.WriteLine($"{nameof(LinkReferencesCommand)} cancelled");
								break;
							}
						}
					}
					else if (scope == OneNote.Scope.Sections)
					{
						var notebook = await one.GetNotebook(OneNote.Scope.Pages);
						var ns = notebook.GetNamespaceOfPrefix(OneNote.Prefix);

						// skip recycle bin sections by removing those nodes
						notebook.Descendants(ns + "Section")
							.Where(e => e.Attribute("isRecycleBin") is not null)
							.Remove();

						await ScanPages(progress, token, notebook);
					}
					else
					{
						var section = await one.GetSection();
						await ScanPages(progress, token, section);
					}

					if (referrals.Any())
					{
						// even if cancellation is request, must update page with referals that were
						// modified, otherwise, there will be referal pages that link to this page
						// without this page referring back!

						AppendReferrals();
						logger.Verbose($"saving {anchorTitle}");
						await one.Update(anchorPage);
					}
					else if (refreshReferences)
					{
						logger.Verbose($"saving {anchorTitle} with no referrals");
						await one.Update(anchorPage);
					}
				}
			}
			finally
			{
				logger.WriteTime("manual linking complete");
				logger.End();
			}
		}


		private async Task ScanPages(
			UI.ProgressDialog progress, CancellationToken token, XElement hierarchy)
		{
			var ns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);
			var pageIDs = hierarchy.Descendants(ns + "Page")
				.Select(e => e.Attribute("ID").Value);

			var total = pageIDs.Count();
			progress.SetMaximum(total);

			var count = 0;

			foreach (var pageID in pageIDs.Where(e => e != anchorPage.PageId))
			{
				if (token.IsCancellationRequested)
				{
					logger.WriteLine($"{nameof(LinkReferencesCommand)} cancelled at {count} of {total} pages");
					break;
				}

				count++;
				progress.Increment();

				var page = await one.GetPage(pageID, OneNote.PageDetail.Basic);
				progress.SetMessage(page.Title);

				var title = CleanTitle(page.Title);
				logger.Verbose($"searching for references on {title}");

				// search and replace...

				var replacements = 0;

				// exclude the omLinkedReferences block
				var block = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == LinkRefsMeta);

				if (block is null)
				{
					// no omLinkedReferences so just scan the whole page
					replacements = editor.SearchAndReplace(page);
				}
				else
				{
					// skip the omLinkedReferences block and its child OEs
					var blocked = new List<XElement> { block.Parent };
					var children = block.Parent.Descendants(ns + "OE");
					if (children.Any()) blocked.AddRange(children);

					// scan the remaining content of the page
					foreach (var oe in page.Root.Descendants(ns + "OE").Except(blocked))
					{
						replacements += editor.SearchAndReplace(oe);
					}
				}

				//var replacements = editor.SearchAndReplace(page);
				if (replacements > 0 && !token.IsCancellationRequested)
				{
					// update the referring page with links back to the anchor page
					logger.Debug($"saving {title}");
					await one.Update(page);

					referrals.Add(new Referral
					{
						PageID = pageID,
						Title = title,
						Synopsis = showSynopsis ? GetSynopsis(page) : string.Empty
					});
				}
			}
		}


		private string GetSynopsis(Page page)
		{
			var body = page.BodyOutlines.FirstOrDefault();
			if (body is null)
			{
				return null;
			}

			// page here is the referring page which has already been updated so any changes
			// to the XML will be discarded... remove descendant Images to avoid an issue where
			// TextValue() can't parse embedded XML snippets in image OCR
			body.Descendants(page.Namespace + "Image").Remove();

			// remove the linked references block on this page so we don't include it in
			// the page synopsis
			body.Descendants(page.Namespace + "Meta")
				.Where(e => e.Attribute("name").Value == LinkRefsMeta)
				.Select(e => e.Parent)
				.Remove();

			// extract snippet of text surrounding first occurances of title within body...
			// Note that attemps were made to find the sentence containing the title but this
			// gets complicated when it contains decimals (4.5) or names (Mr. John Q. Public)

			var text = body.TextValue();
			var start = text.IndexOf(anchorTitle);
			if (start < 0)
			{
				return text.Length <= SynopsisLength ? text : text.Substring(0, SynopsisLength);
			}

			start = Math.Max(start - ((SynopsisLength - anchorTitle.Length) / 2), 0);
			return text.Substring(start, Math.Min(SynopsisLength, text.Length - start));
		}


		private void AppendReferrals()
		{
			var ns = anchorPage.Namespace;
			var children = new XElement(ns + "OEChildren");
			var citeStyle = anchorPage.GetQuickStyle(Styles.StandardStyles.Citation);

			PageNamespace.Set(ns);

			var cmd = $"onemore://LinkReferencesCommand/refresh/{scope}";
			if (showSynopsis) cmd = $"{cmd}/synopsis";

			var href = $"<a href=\"{cmd}\"><span style='{RefreshStyle}'>{Resx.word_Refresh}</span></a>";

			var block = new Paragraph(
				new Meta(LinkRefsMeta, scope.ToString()),
				new XElement(ns + "T", new XCData(
					$"<span style='font-weight:bold'>{Resx.LinkedReferencesCommand_Title}</span> " +
					$"<span style='{RefreshStyle}'>[{href}]</span>"
					)),
				children
				);

			foreach (var referral in referrals.OrderBy(e => e.Title))
			{
				var link = one.GetHyperlink(referral.PageID, string.Empty);

				children.Add(
					new Paragraph(
						new XElement(ns + "List", new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
						new XElement(ns + "T", new XCData($"<a href=\"{link}\">{referral.Title}</a>"))
					));

				if (showSynopsis && !string.IsNullOrWhiteSpace(referral.Synopsis))
				{
					children.Add(
						new Paragraph(referral.Synopsis).SetQuickStyle(citeStyle.Index),
						new Paragraph(string.Empty)
						);
				}
			}

			// double-check if there is an existing block and replace it
			var existing = anchorPage.Root
				.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value == LinkRefsMeta)
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (existing != null)
			{
				existing.ReplaceWith(block);
				return;
			}

			var container = anchorPage.EnsureContentContainer();

			if (!refreshReferences)
			{
				container.Add(new Paragraph(string.Empty));
			}

			container.Add(block);
		}
	}
}
