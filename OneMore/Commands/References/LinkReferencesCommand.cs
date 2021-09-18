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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LinkReferencesCommand : Command
	{
		// OE meta for linked references paragraph
		private const string LinkRefsMeta = "omLinkedReferences";

		// temporary attributes used for in-memory processing, not stored
		private const string NameAttr = "omName";
		private const string LinkedAttr = "omLinked";

		private const string RefreshStyle = "font-style:italic;font-size:9.0pt;color:#808080";

		private OneNote one;
		private OneNote.Scope scope;
		private Page page;
		private XNamespace ns;


		public LinkReferencesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var prompt = true;
			if (args.Length > 0 && args[0] is string refresh && refresh == "refresh")
			{
				prompt = !Refresh();
			}

			if (prompt)
			{
				using (var dialog = new LinkDialog())
				{
					if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
					{
						return;
					}

					scope = dialog.Scope;
				}
			}

			var progressDialog = new UI.ProgressDialog(Execute);
			await progressDialog.RunModeless();
		}


		private bool Refresh()
		{
			using (one = new OneNote(out page, out ns))
			{
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
				var results = one.Search(startId, title);

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
					logger.WriteLine("no referals found");
					return;
				}

				// initialize search-and-replace editor...

				var whatText = $@"\b{SearchAndReplaceEditor.EscapeEscapes(title)}\b";
				var pageLink = one.GetHyperlink(page.PageId, string.Empty);

				var withElement = new XElement("A",
					new XAttribute("href", pageLink),
					page.Title
					);

				var editor = new SearchAndReplaceEditor(whatText, withElement,
					useRegex: true,
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

					logger.WriteLine($"searching for '{whatText}' on {refpage.Title}");

					var count = editor.SearchAndReplace(refpage);
					if (count > 0)
					{
						await one.Update(refpage);
						referal.SetAttributeValue(LinkedAttr, "true");
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
			// ignore the date stamp prefix in a page title

			var match = Regex.Match(title, @"^\d{4}-\d{2}-\d{2}\s");
			if (match.Success)
			{
				title = title.Substring(match.Length);
			}

			return title.Trim();
		}


		private IEnumerable<XElement> FlattenPages(XElement results, string pageId)
		{
			var pages = results.Descendants(ns + "Page")
				.Where(e => e.Attribute("ID").Value != pageId);

			foreach (var page in pages)
			{
				// add omName attribute to page with its notebook/section/page path

				page.Add(new XAttribute(NameAttr,
					page.Ancestors().InDocumentOrder()
						.Where(e => e.Name.LocalName != "Notebooks")
						.Aggregate(string.Empty, (a, b) => a + b.Attribute("name").Value + "/")
						+ page.Attribute("name").Value
					));
			}

			return pages;
		}


		private void AppendReferalBlock(Page page, IEnumerable<XElement> referals)
		{
			var children = new XElement(ns + "OEChildren");

			var refresh = "<a href=\"onemore://LinkReferencesCommand/refresh\">" +
				$"<span style='{RefreshStyle}'>{Resx.InsertTocCommand_Refresh}</span></a>";

			var block = new XElement(ns + "OE",
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

				children.Add(new XElement(ns + "OE",
					new XElement(ns + "List", new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
					new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name.Value}</a>"))
					));
			}

			var container = page.EnsureContentContainer();
			container.Add(new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))));
			container.Add(block);
		}
	}
}
