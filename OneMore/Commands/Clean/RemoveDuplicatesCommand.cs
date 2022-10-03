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
	/// Analyze pages scanning for duplicates and close matches and let user cherrypick
	/// duplicates to delete.
	/// </summary>
	internal class RemoveDuplicatesCommand : Command
	{
		private sealed class HashNode
		{
			public string PageID;
			public string XmlHash;
			public string TextHash;
			public string Title;
			public string Xml;
			public string Path;
			public string Link;
			public int Distance;
			public List<HashNode> Siblings = new List<HashNode>();
		}

		private OneNote one;
		private XNamespace ns;
		private readonly MD5CryptoServiceProvider hasher;
		private readonly List<HashNode> hashes;
		private UI.ProgressDialog progress;

		private UI.SelectorScope scope;
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
			using (var dialog = new RemoveDuplicatesDialog())
			{
				if (dialog.ShowDialog(Owner) != DialogResult.OK)
				{
					return;
				}

				depth = dialog.Depth;
				scope = dialog.Scope;
				books = dialog.SelectedNotebooks;
			}

			// analyze pages, scanning for duplicates and close matches...

			logger.StartClock();

			using (progress = new UI.ProgressDialog())
			{
				progress.ShowDialogWithCancel(Owner,
					async (dialog, token) => await Scan(dialog, token));
			}

			logger.WriteTime($"{hashes.Count} duplicate main pages of {scanCount}");

			hashes.ForEach(n =>
			{
				logger.WriteLine($"{n.Title:-35} {n.TextHash} {n.XmlHash}");
				n.Siblings.ForEach(s =>
				{
					logger.WriteLine($"... {s.Title:-31} {s.TextHash} {s.XmlHash} {s.Distance}");
				});
			});

			// let user cherrypick duplicate pages to delete...

			using (var navigator = new RemoveDuplicatesNavigator())
			{
				if (navigator.ShowDialog(Owner) != DialogResult.OK)
				{
					return;
				}
			}

			await Task.Yield();
		}


		private async Task<bool> Scan(UI.ProgressDialog dialog, CancellationToken token)
		{
			var deep = depth == RemoveDuplicatesDialog.DepthKind.Deep;

			using (one = new OneNote(out _, out ns))
			{
				var hierarchy = await BuildHierarchy(scope, books);
				dialog.SetMaximum(hierarchy.Elements().Count());

				var pages = hierarchy.Descendants(ns + "Page");
				foreach (var page in pages)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					// get the XML text rather than the Page so we don't end up
					// converting it back and forth more than once...
					string xml = deep
						? one.GetPageXml(page.Attribute("ID").Value, OneNote.PageDetail.BinaryDataFileType)
						: one.GetPageXml(page.Attribute("ID").Value, OneNote.PageDetail.Basic);

					var node = CalculateHash(ref xml, depth);
					logger.WriteLine($"text hash [{node.TextHash}] xml hash [{node.XmlHash}]");

					dialog.SetMessage($"Scanning {node.Title}...");
					dialog.Increment();

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
							node.Distance = xml.DistanceFrom(sibling.Xml);
						}

						logger.WriteLine($"match [{node.Title}] with [{sibling.Title}]");
						sibling.Siblings.Add(node);
					}
					else
					{
						if (deep)
						{
							node.Xml = xml;
						}

						logger.WriteLine($"new [{node.Title}]");
						hashes.Add(node);
					}

					scanCount++;
				}
			}

			hashes.RemoveAll(n => !n.Siblings.Any());
			hashes.ForEach(n => n.Xml = null);

			return true;
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


		private HashNode CalculateHash(ref string xml, RemoveDuplicatesDialog.DepthKind depth)
		{
			var root = XElement.Parse(xml);
			var page = new Page(root);
			var pageId = page.PageId;

			// EditedByAttributes and the page ID
			root.DescendantsAndSelf().Attributes().Where(a =>
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

			var node = new HashNode
			{
				PageID = pageId,
				Title = page.Title
			};

			if (depth != RemoveDuplicatesDialog.DepthKind.Basic)
			{
				node.XmlHash = Convert.ToBase64String(
					hasher.ComputeHash(Encoding.Default.GetBytes(xml)));

				if (depth == RemoveDuplicatesDialog.DepthKind.Deep)
				{
					xml = root.ToString(SaveOptions.DisableFormatting);
				}
			}

			var plain = page.Root.TextValue(true);
			node.TextHash = Convert.ToBase64String(
				hasher.ComputeHash(Encoding.Default.GetBytes(plain)));

			return node;
		}
	}
}
