//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using HyperlinkInfo = OneNote.HyperlinkInfo;
	using Scope = OneNote.Scope;


	internal class HyperlinkProvider
	{
		private readonly OneNote one;


		public HyperlinkProvider(OneNote one)
		{
			this.one = one;
		}


		/// <summary>
		/// Creates a map of pages where the key is built from the page-id of an internal
		/// onenote: hyperlink and the value is a HyperlinkInfo item
		/// </summary>
		/// <param name="scope">Pages in section, Sections in notebook, or all Notebooks</param>
		/// <param name="countCallback">Called exactly once to report the total count of pages to map</param>
		/// <param name="stepCallback">Called for each page that is mapped to report progress</param>
		/// <returns>
		/// A Dictionary with page IDs as keys as values are HyperlinkInfo items
		/// </returns>
		/// <remarks>
		/// There's no direct way to map onenote:http URIs to page IDs so this creates a cache
		/// of all pages in the specified scope with their URIs as keys and pageIDs as values
		/// </remarks>
		public async Task<Dictionary<string, HyperlinkInfo>> BuildHyperlinkMap(
			Scope scope,
			CancellationToken token,
			Func<int, Task> countCallback = null,
			Func<Task> stepCallback = null)
		{
			var hyperlinks = new Dictionary<string, HyperlinkInfo>();

			XElement container;
			if (scope == Scope.Notebooks)
			{
				container = await one.GetNotebooks(Scope.Pages);
			}
			else if (scope == Scope.Sections || scope == Scope.Pages)
			{
				// get the notebook even if scope if Pages so we can infer the full path
				container = await one.GetNotebook(Scope.Pages);
			}
			else
			{
				await Task.FromResult(hyperlinks);
				return hyperlinks;
			}

			// ignore the recycle bin
			container.Elements()
				.Where(e => e.Attributes().Any(a => a.Name == "isRecycleBin"))
				.Remove();

			if (token.IsCancellationRequested)
			{
				await Task.FromResult(hyperlinks);
				return hyperlinks;
			}

			// get root path and trim down to intended scope

			var ns = one.GetNamespace(container);
			string rootPath = string.Empty;

			if (scope == Scope.Pages)
			{
				var section = container.Descendants(ns + "Section")
					.FirstOrDefault(e => e.Attribute("isCurrentlyViewed")?.Value == "true");

				if (section != null)
				{
					var p = section.Parent;
					while (p != null)
					{
						var a = p.Attribute("name");
						if (a != null && !string.IsNullOrEmpty(a.Value))
						{
							rootPath = rootPath.Length == 0 ? a.Value : $"{a.Value}/{rootPath}";
						}

						p = p.Parent;
					}

					container = section;
				}
			}
			else if (scope != Scope.Notebooks) // <one:Notebooks> doesn't have a name
			{
				rootPath = container.Attribute("name").Value;
			}

			if (token.IsCancellationRequested)
			{
				return hyperlinks;
			}

			// count pages so we can update countCallback and continue
			var total = container.Descendants(ns + "Page")
				.Count(e => e.Attribute("isInRecycleBin") == null);

			if (total > 0)
			{
				if (countCallback != null)
					await countCallback(total);

				await BuildHyperlinkMap(hyperlinks, container, rootPath, null, token, stepCallback);
			}

			await Task.FromResult(hyperlinks);
			return hyperlinks;
		}


		private async Task BuildHyperlinkMap(
			Dictionary<string, HyperlinkInfo> hyperlinks,
			XElement root, string fullPath, string path,
			CancellationToken token, Func<Task> stepCallback)
		{
			if (root.Name.LocalName == "Section")
			{
				if (string.IsNullOrEmpty(path))
				{
					path = root.Attribute("name").Value;
				}

				var full = $"{fullPath}/{path}";

				foreach (var element in root.Elements())
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					var ID = element.Attribute("ID").Value;
					var name = element.Attribute("name").Value;
					var link = one.GetHyperlink(ID, string.Empty);
					var hyperId = GetHyperKey(link, out var sectionID);

					if (hyperId != null && !hyperlinks.ContainsKey(hyperId))
					{
						//logger.WriteLine($"MAP path:{path} fullpath:{full} name:{name}");
						hyperlinks.Add(hyperId,
							new HyperlinkInfo
							{
								PageID = ID,
								SectionID = sectionID,
								HyperID = hyperId,
								Name = name,
								Path = path,
								FullPath = full,
								Uri = link
							});
					}

					if (stepCallback != null)
						await stepCallback();
				}
			}
			else // SectionGroup or Notebook
			{
				foreach (var element in root.Elements())
				{
					if (element.Attribute("isRecycleBin") != null ||
						element.Attribute("isInRecycleBin") != null)
					{
						//logger.WriteLine("MAP skip recycle bin");
						continue;
					}

					if (element.Name.LocalName == "Notebooks" ||
						element.Name.LocalName == "UnfiledNotes")
					{
						//logger.WriteLine($"MAP skip {element.Name.LocalName} element");
						continue;
					}

					var ea = element.Attribute("name");
					if (ea == null)
						continue;

					//logger.WriteLine($"MAP root:{root.Name.LocalName}={root.Attribute("name")?.Value} " +
					//	$"element:{element.Name.LocalName}={ea?.Value} " +
					//	$"path:{path}");

					var p = string.IsNullOrEmpty(path) ? ea.Value : $"{path}/{ea.Value}";

					await BuildHyperlinkMap(
						hyperlinks, element,
						fullPath,
						p,
						token, stepCallback);

					if (token.IsCancellationRequested)
					{
						return;
					}
				}
			}
		}


		/// <summary>
		/// Reads the page-id part of the given onenote:// hyperlink URI
		/// </summary>
		/// <param name="uri">A onenote:// hyperlink URI</param>
		/// <param name="sectionID">Gets the section ID from the URI</param>
		/// <returns>The page-id value or null if not found</returns>
		public static string GetHyperKey(string uri, out string sectionID)
		{
			var sectionEx = new Regex(@"section-id=({[^}]+?})");
			var match = sectionEx.Match(uri);
			sectionID = match.Success ? match.Groups[1].Value : null;

			var pageEx = new Regex(@"page-id=({[^}]+?})");
			match = pageEx.Match(uri);
			return match.Success ? match.Groups[1].Value : null;
		}
	}
}
