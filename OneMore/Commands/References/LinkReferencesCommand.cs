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

				var referals = results.Descendants(ns + "Page")
					.Where(e => e.Attribute("ID").Value != page.PageId);

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


		private void AppendReferalBlock(Page page, IEnumerable<XElement> referals)
		{
			var block = new XElement(ns + "OE");

			foreach (var referal in referals)
			{

			}

			var container = page.EnsureContentContainer();
			container.Add(block);
		}


		/*
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
		*/
	}
}
