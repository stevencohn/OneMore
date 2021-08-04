//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Create a bi-directional link between two selected words or phrases, either across pages
	/// or on the same page. This is invoked as two commands, the first to mark the first word
	/// or phrase and the second to select and create the link with the second words or phrase.
	/// </summary>
	internal class BiLinkCommand : Command
	{
		private static string anchorPageId;
		private static string anchorId;
		private static SelectionRange anchor;


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
						logger.WriteLine($"MessageBox: Could not mark anchor point;"
							+ "Select a word or phrase from one paragraph; "
							+ "See log file for details");
						IsCancelled = true;
						return;
					}

					var text = anchor.TextValue();
					if (text.Length > 20) { text = $"{text.Substring(0,20)}..."; }
					logger.WriteLine($"MessageBox: marked \"{text}\", {anchorPageId} paragraph {anchorId}");
					logger.WriteLine(anchor.Root);
				}
				else
				{
					if (string.IsNullOrEmpty(anchorPageId))
					{
						logger.WriteLine($"MessageBox: mark not set");
						IsCancelled = true;
						return;
					}

					if (!await CreateLinks(one))
					{
						logger.WriteLine($"MessageBox: could not link to anchor; see log file for details");
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
			return true;
		}


		private async Task<bool> CreateLinks(OneNote one)
		{
			var anchorPage = one.GetPage(anchorPageId);
			if (anchorPage == null)
			{
				logger.WriteLine($"lost anchor page {anchorPageId}");
				return false;
			}

			var candidate = anchorPage.Root.Descendants()
				.FirstOrDefault(e => e.Attributes("objectID").Any(a => a.Value == anchorId));

			if (candidate == null)
			{
				logger.WriteLine($"lost anchor paragraph {anchorId}");
				return false;
			}

			// ensure anchor selection hasn't changed and is still selected!
			if (AnchorModified(candidate, anchor.Root))
			{
				logger.WriteLine($"anchor paragraph may have changed");
				logger.WriteLine("original");
				logger.WriteLine(anchor.Root);
				logger.WriteLine("modified");
				logger.WriteLine(candidate);
				return false;
			}

			var targetPage = one.GetPage();
			var targetRun = targetPage.GetTextCursor();
			if (targetRun == null)
			{
				logger.WriteLine("no selected target content");
				return false;
			}

			var target = new SelectionRange(targetRun.Parent);
			var targetId = target.ObjectId;
			if (anchorId == targetId)
			{
				logger.WriteLine("cannot link a phrase to itself");
				return false;
			}

			// anchorPageId -> anchorPage -> anchorId -> anchor -> anchorRun
			// targetPageId -> targetPage -> targetId -> target -> targetRun

			var targetPageId = one.CurrentPageId;

			var anchorLink = one.GetHyperlink(anchorPageId, anchorId);
			var targetLink = one.GetHyperlink(targetPageId, targetId);

			ApplyHyperlink(anchorPage, anchor, targetLink);
			ApplyHyperlink(targetPage, target, anchorLink);

			candidate.ReplaceWith(anchor.Root);

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
			logger.WriteLine(anchor.Root);

			await one.Update(anchorPage);

			if (targetId != anchorId)
			{
				await one.Update(targetPage);
			}

			return true;
		}


		private bool AnchorModified(XElement candidate, XElement anchor)
		{
			// special deep comparison, excluding the selected attributes to handle
			// case where anchor is on the same page as the target element

			var oldcopy = new XElement(anchor);
			oldcopy.RemoveTextCursor();
			oldcopy.DescendantsAndSelf().Attributes("selected").Remove();

			var newcopy = new XElement(candidate);
			newcopy.RemoveTextCursor();
			newcopy.DescendantsAndSelf().Attributes("selected").Remove();

			var oldxml = oldcopy.ToString(SaveOptions.DisableFormatting);
			var newxml = newcopy.ToString(SaveOptions.DisableFormatting);

			return oldxml != newxml;
		}


		private void ApplyHyperlink(Page page, SelectionRange range, string link)
		{

			var selection = range.GetSelection();
			if (range.SelectionScope == SelectionScope.Empty)
			{
				page.EditNode(selection, (s) =>
				{
					return new XElement("a", new XAttribute("href", link), s);
				});
			}
			else
			{
				page.EditSelected(range.Root, (s) =>
				{
					return new XElement("a", new XAttribute("href", link), s);
				});
			}
		}
	}
}
