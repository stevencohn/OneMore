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
						logger.WriteLine($"MessageBox: could not mark anchor point; see log file for details");
						IsCancelled = true;
						return;
					}

					logger.WriteLine($"MessageBox: marked {pageOneId} paragraph {paragraphId}");
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
			var ns = page.Namespace;

			var run = page.GetSelectedElements(all: false).FirstOrDefault();
			if (run == null)
			{
				logger.WriteLine("no selected content");
				return false;
			}

			paragraph = run.Ancestors(ns + "OE").FirstOrDefault();
			if (paragraph == null)
			{
				logger.WriteLine("missing paragraph");
				return false;
			}

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

			// ensure anchor selection hasn't changed and is still selected!
			var p1 = paraOne.ToString();
			var p2 = paragraph.ToString();
			if (p1 != p2)
			{
				logger.WriteLine($"anchor paragraph may have changed");
				logger.WriteLine(p1);
				logger.WriteLine(p2);
				return false;
			}

			var page = one.GetPage();
			var run = page.GetSelectedElements(all: false).FirstOrDefault();
			if (run == null)
			{
				logger.WriteLine("no selected target content");
				return false;
			}

			logger.WriteLine($"link to {pageOneId} from {one.CurrentPageId} paragraph {paragraphId}");
			logger.WriteLine(paragraph.ToString());

			await Task.Yield();
			return true;
		}
	}
}
