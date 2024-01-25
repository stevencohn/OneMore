﻿//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Scans all notebooks for hashtags
	/// </summary>
	internal class HashtagScanner : Loggable, IDisposable
	{
		public const int DefaultThrottle = 40;

		private readonly OneNote one;
		private readonly string lastTime;
		private readonly HashtagPageSannerFactory factory;
		private readonly SettingsCollection settings;
		private readonly int throttle;
		private HashtagProvider provider;
		private XNamespace ns;
		private bool disposed;


		/// <summary>
		/// 
		/// </summary>
		public HashtagScanner()
		{
			settings = new SettingsProvider().GetCollection("HashtagSheet");
			throttle = settings.Get("delay", DefaultThrottle);

			one = new OneNote();
			provider = new HashtagProvider();

			factory = new HashtagPageSannerFactory(
				GetStyleTemplate(),
				settings.Get<bool>("unfiltered"));

			lastTime = provider.ReadScanTime();
			//logger.Verbose($"HashtagScanner lastTime {lastTime}");
		}


		private XElement GetStyleTemplate()
		{
			var styleIndex = settings.Get("styleIndex", 0);

			if (styleIndex == 1)
			{
				return new XElement("span",
					new XAttribute("style", "color:red"),
					"$1");
			}
			else if (styleIndex == 2)
			{
				return new XElement("span",
					new XAttribute("style", "background:#FFFF99"),
					"$1");
			}
			else if (styleIndex > 2)
			{
				var styleName = settings.Get<string>("styleName");
				var theme = new ThemeProvider().Theme;
				var style = theme.GetStyles().Find(s => s.Name == styleName);
				if (style != null)
				{
					style.ApplyColors = true;

					return new XElement("span",
						new XAttribute("style", style.ToCss()),
						"$1");
				}
			}

			return null;
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
		public async Task<(int, int)> Scan()
		{
			int dirtyPages = 0;
			int totalPages = 0;

			// get all notebooks
			var root = await one.GetNotebooks();
			if (root == null)
			{
				logger.WriteLine("error HashtagScanner one.GetNotebooks()");
				return (0, 0);
			}

			ns = one.GetNamespace(root);

			var notebooks = root.Elements(ns + "Notebook");
			if (notebooks.Any())
			{
				foreach (var notebook in notebooks)
				{
					// gets sections for this notebook
					var notebookID = notebook.Attribute("ID").Value;
					var sections = await one.GetNotebook(notebookID);
					if (sections != null)
					{
						var (dp, tp) = await Scan(
							sections, notebookID, $"/{notebook.Attribute("name").Value}");

						dirtyPages += dp;
						totalPages += tp;
					}
				}
			}

			provider.WriteScanTime();

			return (dirtyPages, totalPages);
		}


		private async Task<(int, int)> Scan(XElement parent, string notebookID, string path)
		{
			//logger.Verbose($"scanning parent {path}");

			int dirtyPages = 0;
			int totalPages = 0;

			var sectionRefs = parent.Elements(ns + "Section")
				.Where(e =>
					e.Attribute("isRecycleBin") == null &&
					e.Attribute("isInRecycleBin") == null &&
					e.Attribute("locked") == null);

			if (sectionRefs.Any())
			{
				foreach (var sectionRef in sectionRefs)
				{
					// get pages for this section
					var section = await one.GetSection(sectionRef.Attribute("ID").Value);
					if (section != null)
					{
						var pages = section.Elements(ns + "Page")
							.Where(e => e.Attribute("isInRecycleBin") == null);

						if (pages.Any())
						{
							totalPages += pages.Count();
							var pids = new List<string>();

							var sectionID = section.Attribute("ID").Value;
							var sectionPath = $"{path}/{section.Attribute("name").Value}";
							//logger.Verbose($"scanning section {sectionPath} ({pages.Count()} pages)");

							foreach (var page in pages)
							{
								var pid = page.Attribute("ID").Value;
								pids.Add(pid);

								if (page.Attribute("lastModifiedTime").Value.CompareTo(lastTime) > 0)
								{
									if (await ScanPage(pid, notebookID, sectionID, sectionPath))
									{
										dirtyPages++;
									}
								}

								// throttle the workload to give breathing room to OneNote UI
								if (throttle > 0)
								{
									await Task.Delay(throttle);
								}
							}

							provider.DeletePhantoms(pids, sectionID, sectionPath);
						}
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
					var (dp, tp) = await Scan(
						group, notebookID, $"{path}/{group.Attribute("name").Value}");

					dirtyPages += dp;
					totalPages += tp;
				}
			}

			return (dirtyPages, totalPages);
		}


		private async Task<bool> ScanPage(
			string pageID, string notebookID, string sectionID, string path)
		{
			Page page;

			try
			{
				page = await one.GetPage(pageID, OneNote.PageDetail.Basic);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error scanning page, possibly locked", exc);
				return false;
			}

			// avoid defect https://github.com/stevencohn/OneMore/issues/1268
			// GetPage throws generic COM exception and returns null...
			if (page == null)
			{
				logger.WriteLine($"skipping null page {pageID} '{path}'");
				return false;
			}

			var title = page.Title;
			var titleID = page.TitleID;
			var scanner = factory.CreatePageScanner(page);

			// validate MoreID, might be duplicate if page is duplicate or copied
			if (!scanner.UpdateMeta && !provider.UniqueMoreID(page.PageId, scanner.MoreID))
			{
				scanner.SetMoreID();
			}

			// scan and resolve...

			var candidates = scanner.Scan();

			var saved = provider.ReadPageTags(pageID);
			var discovered = new Hashtags();
			var updated = new Hashtags();

			foreach (var candidate in candidates)
			{
				var found = saved.Find(s => s.Equals(candidate));
				if (found == null)
				{
					discovered.Add(candidate);
				}
				else
				{
					if (candidate.LastModified.CompareTo(lastTime) > 0)
					{
						updated.Add(candidate);
					}

					saved.Remove(found);
				}
			}

			var dirtyPage = false;

			if (saved.Any())
			{
				// remaining saved entries were not matched with candidates
				// on page so should be deleted
				provider.DeleteTags(saved);
				dirtyPage = true;
			}

			if (updated.Any())
			{
				// tag context updated since last scan
				provider.UpdateSnippet(updated);
				dirtyPage = true;
			}

			if (discovered.Any())
			{
				// discovered entries are new on the page and not found in saved

				provider.WriteTags(discovered);
				dirtyPage = true;
			}

			// if first time hashtags were discovered on this page then set omPageID
			if ((scanner.UpdateMeta || scanner.UpdateStyle) && dirtyPage)
			{
				page.SetMeta(MetaNames.PageID, scanner.MoreID);
				await one.Update(page);
			}

			if (dirtyPage)
			{
				// will likely rewrite same data but needed in case old page is moved

				// TODO: could track moreID+pageID to determine if REPLACE is needed; but then
				// need to read that info first as well; see where the design goes...

				// TODO: should this be wrapped in a tx along with the above statements?

				provider.WritePageInfo(
					scanner.MoreID, pageID, titleID, notebookID, sectionID, path, title);

				logger.WriteLine($"updating tags on page {path}/{title}");
				return true;
			}

			return false;
		}
	}
}
