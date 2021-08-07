//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Create a bi-directional link between two selected words or phrases, either across pages
	/// or on the same page. This is invoked as two commands, the first to mark the first word
	/// or phrase and the second to select and create the link with the second words or phrase.
	/// </summary>
	internal class BiLinkCommand : Command
	{
		// TODO: consider moving these to a global state cache that can be pruned
		// rather than holding on to them indefinitely as statics....

		private static string anchorPageId;
		private static string anchorId;
		private static SelectionRange anchor;

		private string anchorText;
		private string error;


		public BiLinkCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				if ((args[0] is string cmd) && (cmd == "mark"))
				{
					if (!MarkAnchor(one))
					{
						UIHelper.ShowInfo(one.Window, Resx.BiLinkCommand_BadAnchor);
						IsCancelled = true;
						return;
					}

					if (anchorText.Length > 20) { anchorText = $"{anchorText.Substring(0,20)}..."; }
					UIHelper.ShowInfo(one.Window, string.Format(Resx.BiLinkCommand_Marked, anchorText));
				}
				else
				{
					if (string.IsNullOrEmpty(anchorPageId))
					{
						UIHelper.ShowInfo(one.Window, Resx.BiLinkCommand_NoAnchor);
						IsCancelled = true;
						return;
					}

					if (!await CreateLinks(one))
					{
						UIHelper.ShowInfo(one.Window, string.Format(Resx.BiLinkCommand_BadTarget, error));
						IsCancelled = true;
						return;
					}

					// reset
					anchorPageId = null;
					anchorId = null;
					anchor = null;
				}
			}

			await Task.Yield();
		}


		private bool MarkAnchor(OneNote one)
		{
			var page = one.GetPage();
			var range = new SelectionRange(page.Root);

			// get selected runs but preserve cursor if there is one so we can edit from it later
			var run = range.GetSelection();
			if (run == null)
			{
				logger.WriteLine("no selected content");
				return false;
			}

			// anchor is the surrounding OE
			anchor = new SelectionRange(run.Parent);

			anchorId = anchor.ObjectId;
			if (string.IsNullOrEmpty(anchorId))
			{
				logger.WriteLine("missing objectID");
				anchor = null;
				return false;
			}

			anchorPageId = one.CurrentPageId;
			anchorText = page.GetSelectedText();
			return true;
		}


		private async Task<bool> CreateLinks(OneNote one)
		{
			// - - - - anchor...

			var anchorPage = one.GetPage(anchorPageId);
			if (anchorPage == null)
			{
				logger.WriteLine($"lost anchor page {anchorPageId}");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			var candidate = anchorPage.Root.Descendants()
				.FirstOrDefault(e => e.Attributes("objectID").Any(a => a.Value == anchorId));

			if (candidate == null)
			{
				logger.WriteLine($"lost anchor paragraph {anchorId}");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			// ensure anchor selection hasn't changed and is still selected!
			if (AnchorModified(candidate, anchor.Root))
			{
				logger.WriteLine($"anchor paragraph may have changed");
				error = Resx.BiLinkCommand_LostAnchor;
				return false;
			}

			// - - - - target...

			Page targetPage = anchorPage;
			var targetPageId = anchorPageId;
			if (one.CurrentPageId != anchorPageId)
			{
				targetPage = one.GetPage();
				targetPageId = targetPage.PageId;
			}

			var range = new SelectionRange(targetPage.Root);
			var targetRun = range.GetSelection();
			if (targetRun == null)
			{
				logger.WriteLine("no selected target content");
				error = Resx.BiLinkCommand_NoTarget;
				return false;
			}

			var target = new SelectionRange(targetRun.Parent);
			var targetId = target.ObjectId;
			if (anchorId == targetId)
			{
				logger.WriteLine("cannot link a phrase to itself");
				error = Resx.BiLinkCommand_Circular;
				return false;
			}

			// - - - - action...

			// anchorPageId -> anchorPage -> anchorId -> anchor
			// targetPageId -> targetPage -> targetId -> target

			var anchorLink = one.GetHyperlink(anchorPageId, anchorId);
			var targetLink = one.GetHyperlink(targetPageId, targetId);

			ApplyHyperlink(anchorPage, anchor, targetLink);
			ApplyHyperlink(targetPage, target, anchorLink);

			candidate.ReplaceAttributes(anchor.Root.Attributes());
			candidate.ReplaceNodes(anchor.Root.Nodes());

			if (targetPageId == anchorPageId)
			{
				// avoid invalid selection by leaving only partials without an all
				candidate.DescendantsAndSelf().Attributes("selected").Remove();
			}

			//logger.WriteLine();
			//logger.WriteLine("LINKING");
			//logger.WriteLine($" anchorPageId = {anchorPageId}");
			//logger.WriteLine($" anchorId     = {anchorId}");
			//logger.WriteLine($" anchorLink   = {anchorLink}");
			//logger.WriteLine($" candidate    = '{candidate}'");
			//logger.WriteLine($" targetPageId = {targetPageId}");
			//logger.WriteLine($" targetId     = {targetId}");
			//logger.WriteLine($" targetLink   = {targetLink}");
			//logger.WriteLine($" target       = '{target}'");
			//logger.WriteLine();
			//logger.WriteLine("---------------------------------------------");
			//logger.WriteLine(targetPage.Root);

			await one.Update(targetPage);

			if (targetPageId != anchorPageId)
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

			var oldcopy = new SelectionRange(anchor.Clone());
			oldcopy.Deselect();

			var newcopy = new SelectionRange(candidate.Clone());
			newcopy.Deselect();

			var oldxml = oldcopy.ToString();
			var newxml = newcopy.ToString();

			//if (oldxml != newxml)
			//{
			//	logger.WriteLine("differences found in candidate/anchor");
			//	logger.WriteLine("oldxml/candidate");
			//	logger.WriteLine(oldxml);
			//	logger.WriteLine("newxml/anchor");
			//	logger.WriteLine(newxml);
			//}

			return oldxml != newxml;
		}


		private void ApplyHyperlink(Page page, SelectionRange range, string link)
		{
			var count = 0;

			var selection = range.GetSelection();
			if (range.SelectionScope == SelectionScope.Empty)
			{
				page.EditNode(selection, (s) =>
				{
					count++;
					return new XElement("a", new XAttribute("href", link), s);
				});
			}
			else
			{
				page.EditSelected(range.Root, (s) =>
				{
					count++;
					return new XElement("a", new XAttribute("href", link), s);
				});
			}

			// combine doubled-up <a/><a/>...
			// WARN: this could loose styling

			if (count > 0 && range.SelectionScope == SelectionScope.Empty)
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
