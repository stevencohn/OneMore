//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Analyze pages in a given context, scanning for duplicates and close-matches and lets
	/// the user cherrypick which duplicates to delete
	/// </summary>
	internal class RemoveDuplicatesCommand : Command
	{
		internal sealed class HashNode
		{
			public string GroupID;
			public string PageID;
			public string XmlHash;
			public string TextHash;
			public string Title;
			public string Xml;
			public string Path;
			public string Link;
			public int Distance;
			public List<HashNode> Siblings = new();
		}

		private OneNote one;
		private XNamespace ns;
		private readonly MD5CryptoServiceProvider hasher;
		private readonly List<HashNode> hashes;
		private UI.ProgressDialog progress;

		private UI.SelectorScope scope;
		private bool includeTitles;
		private IEnumerable<string> books;
		private RemoveDuplicatesDialog.DepthKind depth;
		private int scanCount;


		public RemoveDuplicatesCommand()
		{
			hashes = new List<HashNode>();
			hasher = new MD5CryptoServiceProvider();
		}


		public override async Task Execute(params object[] args)
		{
			DialogResult result;

			using (var dialog = new RemoveDuplicatesDialog())
			{
				result = dialog.ShowDialog();
				if (result != DialogResult.OK)
				{
					return;
				}

				depth = dialog.Depth;
				scope = dialog.Scope;
				books = dialog.SelectedNotebooks;
				includeTitles = dialog.IncludeTitles;
			}

			// analyze pages, scanning for duplicates and close matches...

			logger.StartClock();

			using (progress = new UI.ProgressDialog())
			{
				result = progress.ShowDialogWithCancel(
					async (dialog, token) => await Scan(dialog, token));

				if (result != DialogResult.OK)
				{
					return;
				}
			}

			logger.WriteTime($"{hashes.Count} pages have one or more duplicates, scanned {scanCount} pages");

			if (hashes.Count == 0)
			{
				UIHelper.ShowInfo("No duplicate pages were found");
				return;
			}

			// let user cherrypick duplicate pages to delete...
			var navigator = new RemoveDuplicatesNavigator(hashes);
			await navigator.RunModeless((sender, e) =>
			{
				var d = sender as RemoveDuplicatesNavigator;
				d.Dispose();
			}, 20);

			await Task.Yield();
		}


		private async Task<bool> Scan(UI.ProgressDialog dialog, CancellationToken token)
		{
			var deep = depth == RemoveDuplicatesDialog.DepthKind.Deep;

			var empty = new HashNode
			{
				Title = "Empty Pages"
			};

			using (one = new OneNote(out _, out ns))
			{
				var hierarchy = await BuildHierarchy(scope, books);
				dialog.SetMaximum(hierarchy.Elements().Count());

				var pageRefs = hierarchy.Descendants(ns + "Page");
				foreach (var pageRef in pageRefs)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					var page = one.GetPage(pageRef.Attribute("ID").Value,
						deep ? OneNote.PageDetail.BinaryData : OneNote.PageDetail.Basic);

					dialog.SetMessage($"Scanning {page.Title}...");
					dialog.Increment();

					var node = CalculateHash(page);
					//logger.WriteLine($"text~ [{node.TextHash}] xml~ [{node.XmlHash}]");

					if (token.IsCancellationRequested)
					{
						break;
					}

					if (node.TextHash == String.Empty)
					{
						empty.Siblings.Add(node);
						continue;
					}

					var sibling = hashes.FirstOrDefault(n =>
						n.TextHash == node.TextHash || n.XmlHash == node.XmlHash);

					if (sibling != null)
					{
						var info = one.GetPageInfo(node.PageID);
						node.Path = info.Path;
						node.Link = info.Link;
						if (sibling.Path == null)
						{
							info = one.GetPageInfo(sibling.PageID);
							sibling.Path = info.Path;
							sibling.Link = info.Link;
						}

						if (deep)
						{
							node.Distance = node.Xml.DistanceFrom(sibling.Xml);
							node.Xml = null;
						}

						//logger.WriteLine($"= [{node.Title}] with [{sibling.Title}]");
						node.GroupID = sibling.GroupID;
						sibling.Siblings.Add(node);
					}
					else
					{
						//logger.WriteLine($"+ [{node.Title}]");
						node.GroupID = node.PageID;
						hashes.Add(node);
					}

					scanCount++;
				}
			}

			if (!token.IsCancellationRequested)
			{
				dialog.SetMessage("Pruning results...");
				hashes.RemoveAll(n => !n.Siblings.Any());
				hashes.ForEach(n => n.Xml = null);

				if (empty.Siblings.Any())
				{
					hashes.Add(empty);
				}
			}

			return !token.IsCancellationRequested;
		}


		private async Task<XElement> BuildHierarchy(
			UI.SelectorScope scope, IEnumerable<string> books)
		{
			var hierarchy = new XElement("pages");

			switch (scope)
			{
				case UI.SelectorScope.Section:
					one.GetSection().Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				case UI.SelectorScope.Notebook:
					(await one.GetNotebook(OneNote.Scope.Pages)).Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				case UI.SelectorScope.Notebooks:
					(await one.GetNotebooks(OneNote.Scope.Pages)).Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				default:
					(await BuildSelectedHierarchy(books))
						.ForEach(p => hierarchy.Add(p));
					break;
			}

			// remove recyclebin nodes
			hierarchy.Descendants()
				.Where(n => n.Name.LocalName == "UnfiledNotes" ||
							n.Attribute("isRecycleBin") != null ||
							n.Attribute("isInRecycleBin") != null)
				.Remove();

			return hierarchy;
		}


		private async Task<IEnumerable<XElement>> BuildSelectedHierarchy(IEnumerable<string> books)
		{
			var pages = new List<XElement>();
			foreach (var id in books)
			{
				var book = await one.GetNotebook(id, OneNote.Scope.Pages);
				pages.AddRange(book.Descendants(ns + "Page"));
			}

			return pages;
		}


		private HashNode CalculateHash(Page page)
		{
			var node = new HashNode
			{
				PageID = page.PageId,
				Title = page.Title
			};

			// EditedByAttributes and the page ID
			page.Root.DescendantsAndSelf().Attributes().Where(a =>
				a.Name.LocalName == "ID"
				|| a.Name.LocalName == "dateTime"
				|| a.Name.LocalName == "callbackID"
				|| a.Name.LocalName == "author"
				|| a.Name.LocalName == "authorInitials"
				|| a.Name.LocalName == "authorResolutionID"
				|| a.Name.LocalName == "lastModifiedBy"
				|| a.Name.LocalName == "lastModifiedByInitials"
				|| a.Name.LocalName == "lastModifiedByResolutionID"
				|| a.Name.LocalName == "creationTime"
				|| a.Name.LocalName == "lastModifiedTime"
				|| a.Name.LocalName == "objectID")
				.Remove();

			if (!includeTitles)
			{
				page.Root.Descendants(ns + "Title").Remove();
			}

			if (depth != RemoveDuplicatesDialog.DepthKind.Basic)
			{
				var xml = page.Root.ToString(SaveOptions.DisableFormatting);

				node.XmlHash = Convert.ToBase64String(
					hasher.ComputeHash(Encoding.Default.GetBytes(xml)));

				if (depth == RemoveDuplicatesDialog.DepthKind.Deep)
				{
					node.Xml = xml;
				}
			}

			// extract plain text last, otherwise XmlHash will not be correct
			// because TextValue(true) will change the XML
			var plain = page.Root.TextValue(true).Trim();

			node.TextHash = plain.Length == 0
				? string.Empty
				: Convert.ToBase64String(hasher.ComputeHash(Encoding.Default.GetBytes(plain)));

			return node;
		}
	}
}
