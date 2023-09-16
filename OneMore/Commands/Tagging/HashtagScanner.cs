//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Scans a given page for ##hashtags
	/// </summary>
	internal class HashtagScanner : Loggable, IDisposable
	{
		private readonly OneNote one;
		private readonly string lastTime;
		private readonly HashtagPageSannerFactory factory;
		private HashtagProvider provider;
		private XNamespace ns;
		private bool disposed;


		/// <summary>
		/// 
		/// </summary>
		public HashtagScanner()
		{
			one = new OneNote();
			provider = new HashtagProvider();
			factory = new HashtagPageSannerFactory();

			lastTime = provider.ReadLastScanTime();
			//logger.WriteLine($"HashtagScanner lastTime {lastTime}");
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					provider.Dispose();
					provider = null;
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public async Task<int> Scan()
		{
			int totalPages = 0;

			var notebooks = await GetNotebooks();
			foreach (var notebook in notebooks)
			{
				//logger.WriteLine($"scanning notebook {notebook.Attribute("name").Value}");

				var sections = await GetSections(notebook.Attribute("ID").Value);
				foreach (var section in sections)
				{
					var pages = GetPages(section.Attribute("ID").Value);
					var name = section.Attribute("name").Value;
					totalPages += pages.Count();

					//logger.WriteLine($"scanning section {name} ({pages.Count()} pages)");

					foreach (var page in pages)
					{
						if (page.Attribute("lastModifiedTime").Value.CompareTo(lastTime) > 0)
						{
							await ScanPage(page.Attribute("ID").Value);
						}
					}
				}
			}

			provider.WriteLastScanTime();

			return totalPages;
		}


		private async Task<IEnumerable<XElement>> GetNotebooks()
		{
			var root = await one.GetNotebooks();
			ns = one.GetNamespace(root);
			return root.Elements(ns + "Notebook");
		}


		private async Task<IEnumerable<XElement>> GetSections(string ownerID)
		{
			var root = await one.GetNotebook(ownerID);

			// by using Descendants, it will discover Sections nested within SectionGroups
			return root.Descendants(ns + "Section")
				.Where(e =>
					e.Attribute("isRecycleBin") == null &&
					e.Attribute("isInRecycleBin") == null &&
					e.Attribute("locked") == null);
		}


		private IEnumerable<XElement> GetPages(string ownerID)
		{
			var root = one.GetSection(ownerID);
			return root.Elements(ns + "Page").Where(e => e.Attribute("isInRecycleBin") == null);
		}


		private async Task ScanPage(string pageID)
		{
			var page = one.GetPage(pageID, OneNote.PageDetail.Basic);

			var scanner = factory.CreatePageScanner(page.Root);
			var candidates = scanner.Scan();

			// resolve...

			var saved = provider.ReadPageTags(pageID);
			var discovered = new Hashtags();

			foreach (var candidate in candidates)
			{
				var found = saved.Find(s => s.Equals(candidate));
				if (found == null)
				{
					discovered.Add(candidate);
				}
				else
				{
					//logger.WriteLine($"found tag {found.Tag}");
					saved.Remove(found);
				}
			}

			var updated = saved.Any() || discovered.Any();
			if (updated)
			{
				logger.WriteLine($"updating tags on page {page.Title}");
			}

			if (saved.Any())
			{
				// remaining saved entries were not matched with candidates
				// on page so should be deleted
				provider.DeleteTags(saved);
			}

			if (discovered.Any())
			{
				// discovered entries are new on the page and not found in saved
				provider.WriteTags(discovered);
			}

			if (scanner.UpdateMeta && updated)
			{
				page.SetMeta(MetaNames.PageID, scanner.MoreID);
				await one.Update(page);
			}
		}
	}

}
