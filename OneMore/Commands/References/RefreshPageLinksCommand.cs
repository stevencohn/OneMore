//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Updates all pages in scope that reference the current page, refreshing the displayed
	/// title of the current page with any changes made after the initial hyperlinks were set.
	/// </summary>
	internal class RefreshPageLinksCommand : Command
	{
		private OneNote.Scope scope;
		private string title;
		private string keys;


		public RefreshPageLinksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			//using (var dialog = new LinkDialog())
			//{
			//	if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
			//	{
			//		return;
			//	}

			//	scope = dialog.Scope;
			//}
			scope = OneNote.Scope.Sections;

			using (var one = new OneNote(out var page, out _))
			{
				// cleaned page title
				title = Unstamp(page.Title);

				// page's hyperlink section-id + page-id
				var uri = one.GetHyperlink(page.PageId, string.Empty);
				var match = Regex.Match(uri, "section-id={[0-9A-F-]+}&page-id={[0-9A-F-]+}&");
				if (match.Success)
				{
					// hyperlink doesn't escape ampersands but they will be escapes in XML
					keys = match.Groups[0].Value.Replace("&", "&amp;");
				}
				else
				{
					logger.WriteLine($"error finding section/page IDs in page hyperlink");
					return;
				}
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

			var updates = 0;

			using (var one = new OneNote())
			{
				var hierarchy = GetHierarchy(one);
				var ns = one.GetNamespace(hierarchy);

				var pageList = hierarchy.Descendants(ns + "Page")
					.Where(e => e.Attribute("isInRecycleBin") == null)
					.ToList();

				progress.SetMaximum(pageList.Count);
				progress.SetMessage($"Scanning {pageList.Count} pages");

				// OneNote likes to inject \n\r before the href attribute so match any spaces
				var editor = new Regex(
					$"(<a\\s+href=[^>]+{keys}[^>]+>)([^<]*)(</a>)",
					RegexOptions.Compiled | RegexOptions.Multiline);

				foreach (var item in pageList)
				{
					progress.Increment();
					progress.SetMessage(item.Attribute("name").Value);

					var xml = one.GetPageXml(item.Attribute("ID").Value, OneNote.PageDetail.Basic);

					// initial string scan before instantiating entire XElement DOM
					if (xml.Contains(keys))
					{
						var page = new Page(XElement.Parse(xml));

						var blocks = page.Root.DescendantNodes().OfType<XCData>()
							.Where(n => n.Value.Contains(keys))
							.ToList();

						foreach (var block in blocks)
						{
							block.Value = editor.Replace(block.Value, $"$1{title}$3");
						}

						await one.Update(page);
						updates++;
					}

					if (token.IsCancellationRequested)
					{
						logger.WriteLine("cancelled");
						break;
					}
				}
			}

			logger.WriteTime("refresh complete");
			logger.End();

			if (updates > 0)
			{
				UIHelper.ShowInfo($"Updated {updates} pages");
			}
			else
			{
				UIHelper.ShowInfo($"No references found to this page");
			}
		}


		private XElement GetHierarchy(OneNote one)
		{
			switch (scope)
			{
				case OneNote.Scope.Notebooks:
					return one.GetNotebooks(OneNote.Scope.Pages);

				case OneNote.Scope.Sections:
					return one.GetNotebook(OneNote.Scope.Pages);

				default:
					return one.GetSection();
			}
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
	}
}
