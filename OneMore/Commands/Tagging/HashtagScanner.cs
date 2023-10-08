//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Scans all notebooks for hashtags
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
		/// Scan all notebooks for all hashtags
		/// </summary>
		/// <returns></returns>
		public async Task<int> Scan()
		{
			int totalPages = 0;

			// get all notebooks
			var root = await one.GetNotebooks();
			ns = one.GetNamespace(root);

			var notebooks = root.Elements(ns + "Notebook");
			if (notebooks.Any())
			{
				foreach (var notebook in notebooks)
				{
					// gets sections for this notebook
					var sections = await one.GetNotebook(notebook.Attribute("ID").Value);
					totalPages += await Scan(sections, $"/{notebook.Attribute("name").Value}");
				}
			}

			provider.WriteLastScanTime();

			return totalPages;
		}


		private async Task<int> Scan(XElement parent, string path)
		{
			//logger.WriteLine($"scanning parent {path}");

			int totalPages = 0;

			var sectionsRefs = parent.Elements(ns + "Section")
				.Where(e =>
					e.Attribute("isRecycleBin") == null &&
					e.Attribute("isInRecycleBin") == null &&
					e.Attribute("locked") == null);

			foreach (var sectionRef in sectionsRefs)
			{
				// get pages for this sectio
				var section = one.GetSection(sectionRef.Attribute("ID").Value);

				var pages = section.Elements(ns + "Page")
					.Where(e => e.Attribute("isInRecycleBin") == null);

				totalPages += pages.Count();

				var sectionPath = $"{path}/{section.Attribute("name").Value}";
				//logger.WriteLine($"scanning section {sectionPath} ({pages.Count()} pages)");

				foreach (var page in pages)
				{
					if (page.Attribute("lastModifiedTime").Value.CompareTo(lastTime) > 0)
					{
						await ScanPage(page.Attribute("ID").Value, sectionPath);
					}
				}
			}

			var groups = parent.Elements(ns + "SectionGroup")
				.Where(e =>
					e.Attribute("isRecycleBin") == null &&
					e.Attribute("isInRecycleBin") == null &&
					e.Attribute("locked") == null);

			if (groups.Any())
			{
				foreach (var group in groups)
				{
					totalPages = await Scan(group, $"{path}/{group.Attribute("name").Value}");
				}
			}

			return totalPages;
		}


		private async Task ScanPage(string pageID, string path)
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
				logger.WriteLine($"updating tags on page {path}/{page.Title}");
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

			// if first time hashtags were discovered on this page then set omPageID
			if (scanner.UpdateMeta && updated)
			{
				page.SetMeta(MetaNames.PageID, scanner.MoreID);
				await one.Update(page);
			}

			if (updated)
			{
				// will likely rewrite same data but needed in case old page is moved
				// TODO: could track moreID+pageID to determine if REPLACE is needed; but then
				// need to read that info first as well; see where the design goes...
				// TODO: how to clean up deleted pages?
				provider.WritePageInfo(scanner.MoreID, path, page.Title);
			}
		}
	}

}
