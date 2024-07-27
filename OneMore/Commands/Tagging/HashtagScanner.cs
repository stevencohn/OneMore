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

		private readonly string lastTime;
		private readonly HashtagPageSannerFactory factory;
		private readonly SettingsCollection settings;
		private readonly int throttle;
		private HashtagProvider provider;
		private string[] notebookFilters;
		private bool disposed;


		/// <summary>
		/// 
		/// </summary>
		public HashtagScanner()
		{
			settings = new SettingsProvider().GetCollection("HashtagSheet");
			throttle = settings.Get("delay", DefaultThrottle);

			provider = new HashtagProvider();

			factory = new HashtagPageSannerFactory(
				GetStyleTemplate(),
				settings.Get<bool>("unfiltered"));

			lastTime = provider.ReadScanTime();
			//logger.Verbose($"HashtagScanner lastTime {lastTime}");
		}


		/// <summary>
		/// A list of notebook IDs to target, used for rescans and rebuilds
		/// </summary>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
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
				if (style is not null)
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

			await using var one = new OneNote();

			// get all notebooks
			var root = await one.GetNotebooks();
			if (root is null)
			{
				logger.WriteLine("error HashtagScanner one.GetNotebooks()");
				return (0, 0);
			}

			var ns = one.GetNamespace(root);

			var notebooks = root.Elements(ns + "Notebook");
			if (notebooks.Any())
			{
				var knownNotebooks = provider.ReadKnownNotebooks();

				foreach (var notebook in notebooks)
				{
					// gets sections for this notebook
					var notebookID = notebook.Attribute("ID").Value;
					var name = notebook.Attribute("name").Value;

					var known = knownNotebooks.Find(n => n.NotebookID == notebookID);

					// Filter on two levels...
					//
					// knownNotebooks
					//   If knownNotebooks is empty, then we assume that this is the first scan
					//   and will allow all; otherwise we only scan known notebooks to avoid
					//   pulling in large data from newly added notebooks - user must schedule
					//   a scan to pull in those new notebooks explicitly.
					//
					// notebookFilters
					//   If notebookFilters is empty then allow any notebook that has passed the
					//   knownNotebook test; otherwise, the user has explicitly requested a scan
					//   of notebooks specified in the notebookFilters list.
					//

					if (knownNotebooks.Count == 0 ||
						(notebookFilters is null
							? known is not null
							: notebookFilters.Contains(notebookID)))
					{
						//logger.Verbose($"scanning notebook {notebookID} \"{name}\"");

						var dp = 0;

						var sections = await one.GetNotebook(notebookID);
						if (sections is not null)
						{
							int tp;
							(dp, tp) = await Scan(
								one, sections, notebookID, $"/{name}",
								known?.LastModified == string.Empty);

							dirtyPages += dp;
							totalPages += tp;
						}

						// record the notebook regardless of whether we find tags; must be done
						// on initial discovery or user would have to explicitly pull it in
						provider.WriteNotebook(notebookID, name, dp > 0);
					}
					else
					{
						logger.Verbose($"skipping notebook {notebookID} \"{name}\"");
					}
				}
			}

			provider.WriteScanTime();

			return (dirtyPages, totalPages);
		}


		private async Task<(int, int)> Scan(
			OneNote one, XElement parent, string notebookID, string path, bool forceThru)
		{
			//logger.Verbose($"scanning parent {path}");

			int dirtyPages = 0;
			int totalPages = 0;
			var ns = one.GetNamespace(parent);

			var sectionRefs = parent.Elements(ns + "Section")
				.Where(e =>
					e.Attribute("isRecycleBin") is null &&
					e.Attribute("isInRecycleBin") is null &&
					e.Attribute("locked") is null);

			if (sectionRefs.Any())
			{
				foreach (var sectionRef in sectionRefs)
				{
					// get pages for this section
					var section = await one.GetSection(sectionRef.Attribute("ID").Value);
					if (section is not null)
					{
						var pages = section.Elements(ns + "Page")
							.Where(e => e.Attribute("isInRecycleBin") is null);

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

								if (forceThru || 
									page.Attribute("lastModifiedTime").Value.CompareTo(lastTime) > 0)
								{
									if (await ScanPage(one,
										pid, notebookID, sectionID, sectionPath, forceThru))
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
					e.Attribute("isRecycleBin") is null &&
					e.Attribute("isInRecycleBin") is null &&
					e.Attribute("locked") is null);

			if (groups.Any())
			{
				foreach (var group in groups)
				{
					var (dp, tp) = await Scan(
						one, group, notebookID, $"{path}/{group.Attribute("name").Value}", forceThru);

					dirtyPages += dp;
					totalPages += tp;
				}
			}

			return (dirtyPages, totalPages);
		}


		private async Task<bool> ScanPage(
			OneNote one, string pageID, string notebookID, string sectionID,
			string path, bool forceThru)
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
			if (page is null)
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

			// saved tags will be in document-order but not have DocumentOrder set,
			// we can rely on tag + objectID to continue resolving
			var saved = provider.ReadPageTags(pageID);

			var discovered = new Hashtags();
			var updated = new Hashtags();

			foreach (var candidate in candidates)
			{
				var found = saved.Find(s => s.Equals(candidate));
				if (found is null)
				{
					discovered.Add(candidate);
				}
				else
				{
					if (forceThru ||
						candidate.LastModified.CompareTo(lastTime) > 0 ||
						candidate.DocumentOrder != found.DocumentOrder)
					{
						updated.Add(candidate);
					}

					saved.Remove(found);
				}
			}

			var dirtyPage = false;

			if (saved.Any() || updated.Any() || discovered.Any())
			{
				// much simpler to purge old and rewrite new, even if that means recreating a
				// few copied records. should scale without issue into the many tens-of-tags
				provider.WriteTags(pageID, candidates);
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

				logger.WriteLine($"updated tags found on page {path}/{title}");
				return true;
			}

			return false;
		}
	}
}
