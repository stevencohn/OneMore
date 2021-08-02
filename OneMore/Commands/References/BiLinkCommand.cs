//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
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
		private static string pageOneId;
		private static string paragraphId;
		private static XElement paragraph;


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

					var text = paragraph.TextValue();
					if (text.Length > 20) { text = $"{text.Substring(0,20)}..."; }
					logger.WriteLine($"MessageBox: marked \"{text}\", {pageOneId} paragraph {paragraphId}");
					logger.WriteLine(paragraph);
				}
				else
				{
					if (string.IsNullOrEmpty(pageOneId))
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
					pageOneId = null;
					paragraphId = null;
					paragraph = null;
				}
			}

			await Task.Yield();
		}


		private bool MarkAnchor(OneNote one)
		{
			var page = one.GetPage();

			//var run = page.GetSelectedElements(all: false).FirstOrDefault();
			var run = page.GetTextCursor(true);
			if (run == null)
			{
				logger.WriteLine("no selected content");
				return false;
			}

			paragraph = run.Parent;

			if (!paragraph.GetAttributeValue("objectID", out paragraphId) || string.IsNullOrEmpty(paragraphId))
			{
				logger.WriteLine("missing objectID");
				paragraph = null;
				return false;
			}

			pageOneId = one.CurrentPageId;
			return true;
		}


		private async Task<bool> CreateLinks(OneNote one)
		{
			var pageOne = one.GetPage(pageOneId);
			if (pageOne == null)
			{
				logger.WriteLine($"lost anchor page {pageOneId}");
				return false;
			}

			var paraOne = pageOne.Root.Descendants()
				.FirstOrDefault(e => e.Attributes("objectID").Any(a => a.Value == paragraphId));

			if (paraOne == null)
			{
				logger.WriteLine($"lost anchor paragraph {paragraphId}");
				return false;
			}

			paraOne.RemoveTextCursor();

			// ensure anchor selection hasn't changed and is still selected!
			if (AnchorModified(paraOne, paragraph))
			{
				logger.WriteLine($"anchor paragraph may have changed");
				logger.WriteLine("original");
				logger.WriteLine(paraOne.ToString());
				logger.WriteLine("modified");
				logger.WriteLine(paragraph.ToString());
				return false;
			}

			var page = one.GetPage();
			var run = page.GetTextCursor(true);
			if (run == null)
			{
				logger.WriteLine("no selected target content");
				return false;
			}

			var paraTwo = run.Parent;

			paraOne.GetAttributeValue("objectID", out var p1);
			paraTwo.GetAttributeValue("objectID", out var p2);
			if (p1 == p2)
			{
				logger.WriteLine("cannot link a phrase to itself");
				return false;
			}

			logger.WriteLine();
			logger.WriteLine("paraOne");
			logger.WriteLine(paraOne);
			logger.WriteLine("paraTwo");
			logger.WriteLine(paraTwo.ToString());
			logger.WriteLine();

			logger.WriteLine($"link to {pageOneId} from {one.CurrentPageId} paragraph {paragraphId}");
			logger.WriteLine(paragraph.ToString());

			await Task.Yield();
			return true;
		}


		private bool AnchorModified(XElement snapshot, XElement anchor)
		{
			// special deep comparison, excluding the selected attributes to handle
			// case where anchor is on the same page as the target element

			var oldcopy = new XElement(snapshot);
			oldcopy.Attributes().Where(a => a.Name.LocalName == "selected").Remove();
			oldcopy.DescendantNodes().OfType<XAttribute>().Where(a => a.Name.LocalName == "selected").Remove();

			var newcopy = new XElement(anchor);
			newcopy.Attributes().Where(a => a.Name.LocalName == "selected").Remove();
			newcopy.DescendantNodes().OfType<XAttribute>().Where(a => a.Name.LocalName == "selected").Remove();

			var oldxml = oldcopy.ToString(SaveOptions.DisableFormatting);
			var newxml = newcopy.ToString(SaveOptions.DisableFormatting);

			logger.WriteLine($"old {oldxml}");
			logger.WriteLine($"new {newxml}");

			return oldxml != newxml;
		}
	}
}
