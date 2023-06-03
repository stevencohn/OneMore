//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

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
		// OE meta for linked references paragraph
		private const string LinkRefsMeta = "omLinkedReferences";

		// TODO: deprecated
		private const string SynopsisMeta = "omShowSynopsis";

		// temporary attributes used for in-memory processing, not stored
		private const string NameAttr = "omName";
		private const string LinkedAttr = "omLinked";
		private const string SynopsisAttr = "omSynopsis";

		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";

		private const int SynopsisLength = 110;

		private OneNote one;
		private OneNote.Scope scope;
		private Page page;
		private XNamespace ns;
		private bool refreshing;
		private bool synopses;
		private bool unindexed;


		public LinkReferencesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is string refresh && refresh == "refresh")
			{
				synopses = args.Any(a => a as string == "synopsis");
				unindexed = args.Any(a => a as string == "unindexed");
				refreshing = Refresh();
			}

			if (!refreshing)
			{
				using var dialog = new LinkDialog();
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				scope = dialog.Scope;
				synopses = dialog.Synopsis;
				unindexed = dialog.Unindexed;
			}

			var progressDialog = new UI.ProgressDialog(Execute);
			await progressDialog.RunModeless();
		}


		private bool Refresh()
		{
			using (one = new OneNote(out page, out ns))
			{
				// find linked references content block...

				var meta = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == LinkRefsMeta);

				if (meta == null)
				{
					return false;
				}

				if (!Enum.TryParse(meta.Attribute("content").Value, out scope))
				{
					scope = OneNote.Scope.Pages;
				}

				// TODO: deprecated; determine options... if not on refresh URI

				if (!synopses && !unindexed)
				{
					var meta2 = meta.Parent.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == SynopsisMeta);

					if (meta2 != null)
					{
						bool.TryParse(meta2.Attribute("content").Value, out synopses);
					}
				}

				// remove the containing OE so it can be regenerated
				meta.Parent.Remove();
				return true;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			using (one = new OneNote())
			{
				if (page == null)
				{
					page = one.GetPage();
					ns = page.Namespace;
				}

				PageNamespace.Set(ns);

				var title = Unstamp(page.Title);

				string startId = string.Empty;
				switch (scope)
				{
					case OneNote.Scope.Sections: startId = one.CurrentNotebookId; break;
					case OneNote.Scope.Pages: startId = one.CurrentSectionId; break;
				}

				logger.WriteLine($"searching for '{title}'");
				var results = one.Search(startId, title, unindexed);

				if (token.IsCancellationRequested)
				{
					logger.WriteLine("cancelled");
					return;
				}

				//logger.WriteLine(results);

				var referals = FlattenPages(results, page.PageId);

				var total = referals.Count();
				if (total == 0)
				{
					UIHelper.ShowInfo(Resx.LinkReferencesCommand_noref);
					return;
				}

				// initialize search-and-replace editor...

				var whatText = $@"(?:^|\b|\s)({title.EscapeForRegex()})(?:$|\b|\s)";
				var pageLink = one.GetHyperlink(page.PageId, string.Empty);

				var withElement = new XElement("A",
					new XAttribute("href", pageLink),
					title
					);

				var editor = new SearchAndReplaceEditor(whatText, withElement,
					enableRegex: true,
					caseSensitive: false
					);

				// process pages...

				progress.SetMaximum(total);
				progress.SetMessage($"Linking for {total} pages");

				var updates = 0;
				foreach (var referal in referals)
				{
					progress.Increment();
					progress.SetMessage(referal.Attribute(NameAttr).Value);

					var refpage = one.GetPage(referal.Attribute("ID").Value, OneNote.PageDetail.Basic);
					var reftitle = Unstamp(refpage.Title);

					logger.WriteLine($"searching for matches on '{reftitle}'");

					var count = editor.SearchAndReplace(refpage);
					if (count > 0)
					{
						await one.Update(refpage);
						referal.SetAttributeValue(LinkedAttr, "true");

						if (synopses)
						{
							referal.SetAttributeValue(SynopsisAttr, GetSynopsis(refpage, title));
						}

						updates++;
					}
					else
					{
						logger.WriteLine($"search not found on {referal.Attribute(NameAttr).Value}");
					}

					if (token.IsCancellationRequested)
					{
						logger.WriteLine("cancelled");
						break;
					}
				}

				if (updates > 0)
				{
					// even if cancellation is request, must update page with referals that were
					// modified, otherwise, there will be referal pages that link to this page
					// without this page referring back!

					AppendReferalBlock(page, referals);
					await one.Update(page);
				}
			}

			logger.WriteTime("linking complete");
			logger.End();
		}


		private string Unstamp(string title)
		{
			// ignore the date stamp prefix and emoji prefixes in a page title

			// strip date stamp
			var match = Regex.Match(title, @"^\d{4}-\d{2}-\d{2}\s");
			if (match.Success)
			{
				title = title.Substring(match.Length);
			}

			// strip emojis (Segoe UI Emoji font)
			title = Emojis.RemoveEmojis(title);

			return title.Trim();
		}


		private IEnumerable<XElement> FlattenPages(XElement results, string pageId)
		{
			var pages = results.Descendants(ns + "Page")
				.Where(e => e.Attribute("ID").Value != pageId);

			foreach (var pg in pages)
			{
				// add omName attribute to page with its notebook/section/page path

				pg.Add(new XAttribute(NameAttr,
					pg.Ancestors().InDocumentOrder()
						.Where(e => e.Name.LocalName != "Notebooks")
						.Aggregate(string.Empty, (a, b) => a + b.Attribute("name").Value + "/")
						+ pg.Attribute("name").Value
					));
			}

			return pages;
		}


		private string GetSynopsis(Page page, string title)
		{
			var body = page.Root
				.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Parent.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)));

			if (body == null)
			{
				return null;
			}

			// page here is a reference to refpage which has already been updated so any
			// changes to the XML will be discarded... remove descendant Images to avoid
			// an issue where TextValue() can't parse embedded XML snippets in image OCR
			body.Descendants(ns + "Image").Remove();

			// extract snippet of text surrounding first occurances of title within body...
			// Note that attemps were made to find the sentence containing the title but this
			// gets complicated when it contains decimals (4.5) or names (Mr. John Q. Public)

			var text = body.TextValue();
			var start = text.IndexOf(title);
			if (start < 0 || string.IsNullOrWhiteSpace(title))
			{
				return text.Length <= SynopsisLength ? text : text.Substring(0, SynopsisLength);
			}

			start = Math.Max(start - ((SynopsisLength - title.Length) / 2), 0);
			return text.Substring(start, Math.Min(SynopsisLength, text.Length - start));
		}


		private void AppendReferalBlock(Page page, IEnumerable<XElement> referals)
		{
			var children = new XElement(ns + "OEChildren");
			var citeStyle = page.GetQuickStyle(Styles.StandardStyles.Citation);

			PageNamespace.Set(ns);

			var cmd = "onemore://LinkReferencesCommand/refresh";
			if (synopses) cmd = $"{cmd}/synopsis";
			if (unindexed) cmd = $"{cmd}/unindexed";

			var refresh = $"<a href=\"{cmd}\"><span style='{RefreshStyle}'>{Resx.word_Refresh}</span></a>";

			var block = new Paragraph(
				new Meta(LinkRefsMeta, scope.ToString()),
				new XElement(ns + "T", new XCData(
					$"<span style='font-weight:bold'>{Resx.LinkedReferencesCommand_Title}</span> " +
					$"<span style='{RefreshStyle}'>[{refresh}]</span>"
					)),
				children
				);

			foreach (var referal in referals.Where(e => e.Attribute(LinkedAttr) != null))
			{
				var link = one.GetHyperlink(referal.Attribute("ID").Value, string.Empty);
				var name = referal.Attribute(NameAttr) ?? referal.Attribute("name");

				children.Add(
					new Paragraph(
						new XElement(ns + "List", new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
						new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name.Value}</a>"))
					));

				if (synopses)
				{
					var synopsis = referal.Attribute(SynopsisAttr).Value ?? string.Empty;

					children.Add(
						new Paragraph(synopsis).SetQuickStyle(citeStyle.Index),
						new Paragraph(string.Empty)
						);
				}
			}

			// double-check if there is an existing block and replace it
			var existing = page.Root.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value == LinkRefsMeta)
				.Select(e => e.Parent)
				.FirstOrDefault();

			if (existing != null)
			{
				existing.ReplaceWith(block);
				return;
			}

			var container = page.EnsureContentContainer();

			if (!refreshing)
			{
				container.Add(new Paragraph(string.Empty));
			}

			container.Add(block);
		}
	}
}
