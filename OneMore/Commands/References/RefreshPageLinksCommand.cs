﻿//************************************************************************************************
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
	using Resx = Properties.Resources;


	/// <summary>
	/// After changing the name of a page which is referenced by other pages (after initial
	/// hyperlinks were set), this command will update those referring pages in scope with the
	/// new title of the current page.
	/// </summary>
	internal class RefreshPageLinksCommand : Command
	{
		private OneNote.Scope scope;
		private string title;
		private string keys;
		private int updates;


		public RefreshPageLinksCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new RefreshPageLinksDialog();
			if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			scope = dialog.Scope;

			using var one = new OneNote(out var page, out _);

			// cleaned page title
			title = Unstamp(page.Title);

			// page's hyperlink section-id + page-id
			var uri = one.GetHyperlink(page.PageId, string.Empty);
			var match = Regex.Match(uri, "section-id={[0-9A-F-]+}&page-id={[0-9A-F-]+}&");
			if (match.Success)
			{
				// ampersands aren't escaped in hyperlinks but they will be in XML
				keys = match.Groups[0].Value.Replace("&", "&amp;");
			}
			else
			{
				logger.WriteLine($"error finding section/page IDs in page hyperlink");
				return;
			}

			var progressDialog = new UI.ProgressDialog(Execute);
			await progressDialog.RunModeless((s, a) =>
			{
				if (updates > 0)
				{
					UIHelper.ShowInfo(string.Format(Resx.RefreshPageLinksCommand_updated, updates));
				}
				else
				{
					UIHelper.ShowInfo(Resx.RefreshPageLinksCommand_none);
				}
			});
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			using var one = new OneNote();

			var hierarchy = await GetHierarchy(one);
			var ns = one.GetNamespace(hierarchy);

			var pageList = hierarchy.Descendants(ns + "Page")
				.Where(e => e.Attribute("isInRecycleBin") == null);

			var pageCount = pageList.Count();
			progress.SetMaximum(pageCount);
			progress.SetMessage($"Scanning {pageCount} pages");

			// OneNote likes to inject \n\r before the href attribute so match any spaces
			// also the A element may contain an optional SPAN (just one?)
			var editor = new Regex(
				$"(.*<a\\s+href=[^>]+{keys}[^>]+>(?:<span[^>]+>)?)([^<]*)((?:</span>)?</a>.*)",
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

			logger.WriteTime("refresh complete");
			logger.End();
		}


		private async Task<XElement> GetHierarchy(OneNote one)
		{
			return scope switch
			{
				OneNote.Scope.Notebooks => await one.GetNotebooks(OneNote.Scope.Pages),
				OneNote.Scope.Sections => await one.GetNotebook(OneNote.Scope.Pages),
				_ => one.GetSection(),
			};
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
	}
}
