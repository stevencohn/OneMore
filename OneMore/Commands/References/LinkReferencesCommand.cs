//************************************************************************************************
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

				logger.WriteLine(results);

				var referals = FlattenPages(results, page.PageId);

				var total = referals.Count();
				progress.SetMaximum(total);
				progress.SetMessage($"Linking for {total} pages");

				foreach (var referal in referals)
				{
					progress.Increment();
					progress.SetMessage(referal.Attribute("name").Value);

					if (token.IsCancellationRequested)
					{
						logger.WriteLine("cancelled");
						return;
					}
				}

				if (!token.IsCancellationRequested)
				{
					AppendReferalBlock(page, referals);
					await one.Update(page);
				}
			}

			logger.WriteTime("map complete");
			logger.End();
		}


		private IEnumerable<XElement> FlattenPages(XElement results, string pageId)
		{
			var pages = results.Descendants(ns + "Page")
				.Where(e => e.Attribute("ID").Value != pageId);

			foreach (var page in pages)
			{
				// add omName attribute to page with its notebook/section/page path

				page.Add(new XAttribute("omName",
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

			var block = new XElement(ns + "OE",
				new XElement(ns + "T", new XCData("<span style='font-weight:bold'>Linked References</span>")),
				children
				);

			foreach (var referal in referals)
			{
				var link = one.GetHyperlink(referal.Attribute("ID").Value, string.Empty);
				var name = referal.Attribute("omName") ?? referal.Attribute("name");

				children.Add(new XElement(ns + "OE",
					new XElement(ns + "List", new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
					new XElement(ns + "T", new XCData($"<a href=\"{link}\">{name.Value}</a>"))
					));
			}

			var container = page.EnsureContentContainer();
			container.Add(block);
		}
	}
}
