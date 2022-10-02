//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Clean;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Scan pages looking for and removing duplicates
	/// </summary>
	internal class RemoveDuplicatesCommand : Command
	{
		private sealed class HashNode
		{
			public string PageID;
			public string XmlHash;
			public string TextHash;
			public string Title;
			public string Path;
			public List<HashNode> Siblings = new List<HashNode>();
		}

		private OneNote one;
		private XNamespace ns;
		private readonly MD5CryptoServiceProvider cruncher;
		private readonly List<HashNode> hashes;


		public RemoveDuplicatesCommand()
		{
			hashes = new List<HashNode>();
			cruncher = new MD5CryptoServiceProvider();
		}


		public override async Task Execute(params object[] args)
		{
			UI.SelectorScope scope;
			IEnumerable<string> notebooks;
			RemoveDuplicatesDialog.DepthKind depth;

			using (var dialog = new RemoveDuplicatesDialog())
			{
				if (dialog.ShowDialog(Owner) != DialogResult.OK)
				{
					return;
				}

				depth = dialog.Depth;
				scope = dialog.Scope;
				notebooks = dialog.SelectedNotebooks;
			}

			using (one = new OneNote(out _, out ns))
			{
				var hierarchy = await BuildHierarchy(scope, notebooks);

				hierarchy.Descendants(ns + "Page").ForEach(p =>
				{
					// get the XML text rather than the Page so we don't end up
					// converting it back and forth more than once...
					string xml = depth == RemoveDuplicatesDialog.DepthKind.Deep
						? one.GetPageXml(p.Attribute("ID").Value, OneNote.PageDetail.BinaryDataFileType)
						: one.GetPageXml(p.Attribute("ID").Value, OneNote.PageDetail.Basic);

					var node = CalculateHash(xml, depth);
					logger.WriteLine($"text hash [{node.TextHash}] xml hash [{node.XmlHash}]");

					var sibling = hashes.FirstOrDefault(n =>
						n.TextHash == node.TextHash || n.XmlHash == node.XmlHash);

					if (sibling != null)
					{
						logger.WriteLine($"match [{node.Title}] with [{sibling.Title}]");
						sibling.Siblings.Add(node);
					}
					else
					{
						logger.WriteLine($"new [{node.Title}]");
						hashes.Add(node);
					}
				});

				hashes.RemoveAll(n => !n.Siblings.Any());
				logger.WriteLine($"{hashes.Count} duplicate main pages");
			}

			await Task.Yield();
		}


		private async Task<XElement> BuildHierarchy(
			UI.SelectorScope scope, IEnumerable<string> notebooks)
		{
			XElement hierarchy;

			switch (scope)
			{
				case UI.SelectorScope.Section:
					hierarchy = one.GetSection();
					break;

				case UI.SelectorScope.Notebook:
					hierarchy = await one.GetNotebook(OneNote.Scope.Pages);
					break;

				case UI.SelectorScope.Notebooks:
					hierarchy = await one.GetNotebooks(OneNote.Scope.Pages);
					break;

				default:
					hierarchy = one.GetSection();
					break;
			}

			return hierarchy;
		}


		private HashNode CalculateHash(string xml, RemoveDuplicatesDialog.DepthKind depth)
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
				Title = page.Title,

				TextHash = Convert.ToBase64String(
					cruncher.ComputeHash(Encoding.Default.GetBytes(page.Root.Value)))
			};

			if (depth != RemoveDuplicatesDialog.DepthKind.Basic)
			{
				node.XmlHash = Convert.ToBase64String(
					cruncher.ComputeHash(Encoding.Default.GetBytes(xml)));
			}

			return node;
		}
	}
}
