//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#define xLOG

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;
	using Hap = HtmlAgilityPack;
	using Resx = Properties.Resources;


	/// <summary>
	/// Create a bi-directional link between two selected words or phrases, either across pages
	/// or on the same page. This is done in two steps, first setting a Bookmark and then
	/// finishing it with this command to link between the bookmark and the second word or phrase.
	/// </summary>
	internal class BiLinkCommand : Command
	{
		private string error;


		public BiLinkCommand()
		{
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (BookmarkCommand.Bookmark is null)
			{
				ShowError(Resx.BiLinkCommand_NoAnchor);
				return;
			}

			if (!await CreateLinks())
			{
				ShowError(string.Format(Resx.BiLinkCommand_BadTarget, error));
				return;
			}

			BookmarkCommand.Clear();
		}


		private async Task<bool> CreateLinks()
		{
			await using var one = new OneNote();

			// - - - - anchor...

			var bookmark = BookmarkCommand.Bookmark;

			var anchorPage = await one.GetPage(bookmark.PageId);
			if (anchorPage is null)
			{
				logger.WriteLine($"lost anchor page {bookmark.PageId}");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			var candidate = anchorPage.Root.Descendants()
				.FirstOrDefault(e => e.Attributes("objectID").Any(a => a.Value == bookmark.ObjectId));

			if (candidate is null)
			{
				logger.WriteLine($"lost anchor paragraph {bookmark.ObjectId}");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			// ensure anchor selection hasn't changed and is still selected!
			if (AnchorModified(candidate, bookmark.Range.Root))
			{
				logger.WriteLine($"anchor paragraph may have changed");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			// - - - - target...

			Page targetPage = anchorPage;
			var targetPageId = bookmark.PageId;
			if (one.CurrentPageId != bookmark.PageId)
			{
				targetPage = await one.GetPage();
				targetPageId = targetPage.PageId;
			}

			var range = new SelectionRange(targetPage);
			var targetRun = range.GetSelection(true);
			if (targetRun is null)
			{
				logger.WriteLine("no selected target content");
				error = Resx.BiLinkCommand_NoTarget;
				return false;
			}

			var target = new SelectionRange(targetRun.Parent);
			var targetId = target.ObjectId;
			if (bookmark.ObjectId == targetId)
			{
				logger.WriteLine("cannot link a phrase to itself");
				error = Resx.BiLinkCommand_Circular;
				return false;
			}

			// - - - - action...

			// anchorPageId -> anchorPage -> anchorId -> anchor
			// targetPageId -> targetPage -> targetId -> target

			var anchorLink = one.GetHyperlink(bookmark.PageId, bookmark.ObjectId);
			var targetLink = one.GetHyperlink(targetPageId, targetId);

			ApplyHyperlink(anchorPage, bookmark.Range, targetLink);
			ApplyHyperlink(targetPage, target, anchorLink);

			candidate.ReplaceAttributes(bookmark.Range.Root.Attributes());
			candidate.ReplaceNodes(bookmark.Range.Root.Nodes());

			if (targetPageId == bookmark.PageId)
			{
				// avoid invalid selection by leaving only partials without an all
				candidate.DescendantsAndSelf().Attributes("selected").Remove();
			}

#if LOG
			logger.WriteLine();
			logger.WriteLine("LINKING");
			logger.WriteLine($" anchorPageId = {anchorPageId}");
			logger.WriteLine($" anchorId     = {anchorId}");
			logger.WriteLine($" anchorLink   = {anchorLink}");
			logger.WriteLine($" candidate    = '{candidate}'");
			logger.WriteLine($" targetPageId = {targetPageId}");
			logger.WriteLine($" targetId     = {targetId}");
			logger.WriteLine($" targetLink   = {targetLink}");
			logger.WriteLine($" target       = '{target}'");
			logger.WriteLine();
			logger.WriteLine("---------------------------------------------");
			logger.WriteLine(targetPage.Root);
#endif
			await one.Update(targetPage);

			if (targetPageId != bookmark.PageId)
			{
				// avoid invalid selection by leaving only partials without an all
				anchorPage.Root.DescendantsAndSelf().Attributes("selected").Remove();
				await one.Update(anchorPage);
			}

			return true;
		}


		private bool AnchorModified(XElement candidate, XElement anchor)
		{
			// special deep comparison, excluding the selected attributes to handle
			// case where anchor is on the same page as the target element

			var oldxml = MakeComparableXml(anchor);
			var newxml = MakeComparableXml(candidate);

			if (oldxml != newxml)
			{
#if LOG
				logger.WriteLine("differences found in anchor/candidate");
				logger.WriteLine($"oldxml/anchor {oldxml.Length}");
				logger.WriteLine(oldxml);
				logger.WriteLine($"newxml/candidate {newxml.Length}");
				logger.WriteLine(newxml);
#endif
				for (int i = 0; i < oldxml.Length && i < newxml.Length; i++)
				{
					if (oldxml[i] != newxml[i])
					{
						logger.WriteLine($"diff at index {i}");
						break;
					}
				}
			}

			return oldxml != newxml;
		}


		private string MakeComparableXml(XElement element)
		{
			var original = element.Clone();

			// ignore last mod timestamp incase it drifts
			if (original.Attribute("lastModifiedTime") is XAttribute lmt) lmt.Remove();

			// remove selections and optimize continguous runs
			var range = new SelectionRange(original);
			range.Deselect();

			// Solves one specific case where CDATA contains <span lang=code> without quotes
			// but is compared to <span lang='code'> with quotes. So this routine normalizes
			// those inner CDATA attribute values so they can be compared for equality.
			range.Root.DescendantNodes().OfType<XCData>().ToList().ForEach(c =>
			{
				var doc = new Hap.HtmlDocument
				{
					GlobalAttributeValueQuote = Hap.AttributeValueQuote.SingleQuote
				};

				c.Value = c.Value.Replace("; ", ";");

				doc.LoadHtml(c.Value);
				c.ReplaceWith(new XCData(doc.DocumentNode.InnerHtml));
			});

			// collapse linebreaks to single space
			var xml = Regex.Replace(range.ToString(), @"[\r\n]+", " ");
			// collapse embedded CSS to normalize no-space between 'properties;properties'
			return Regex.Replace(xml, @"('[^']+)(;\s)([^']+')", "$1;$3");
		}


		private void ApplyHyperlink(Page page, SelectionRange range, string link)
		{
			var count = 0;

			var editor = new PageEditor(page);
			var selection = range.GetSelection(true);
			if (range.Scope == SelectionScope.TextCursor)
			{
				editor.EditNode(selection, (s) =>
				{
					if (s is XText text)
					{
						count++;
						return new XElement("a", new XAttribute("href", link), new XText(text.Value));
					}

					var span = (XElement)s;
					span.ReplaceNodes(new XElement("a", new XAttribute("href", link), span.Value));

					count++;
					return span;
				});
			}
			else
			{
				editor.EditSelected(range.Root, (s) =>
				{
					count++;
					return new XElement("a", new XAttribute("href", link), s);
				});
			}

			// combine doubled-up <a/><a/>...
			// WARN: this could loose styling

			if (count > 0 && range.Scope == SelectionScope.TextCursor)
			{
				var cdata = selection.GetCData();

				if (selection.PreviousNode is XElement prev)
				{
					var cprev = prev.GetCData();
					var wrapper = cprev.GetWrapper();
					if (wrapper.LastNode is XElement node)
					{
						cdata.Value = $"{node.ToString(SaveOptions.DisableFormatting)}{cdata.Value}";
						node.Remove();
						cprev.Value = wrapper.GetInnerXml();
					}

					if (cprev.Value.Length == 0)
					{
						prev.Remove();
					}
				}

				if (selection.NextNode is XElement next)
				{
					var cnext = next.GetCData();
					var wrapper = cnext.GetWrapper();
					if (wrapper.FirstNode is XElement node)
					{
						cdata.Value = $"{cdata.Value}{node.ToString(SaveOptions.DisableFormatting)}";
						node.Remove();
						cnext.Value = wrapper.GetInnerXml();
					}

					if (cnext.Value.Length == 0)
					{
						next.Remove();
					}
				}
			}
		}
	}
}
