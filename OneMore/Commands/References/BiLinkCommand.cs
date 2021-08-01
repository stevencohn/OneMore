//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Create a bi-directional link between two selected words or phrases, either across
	/// pages or on the same page. This is invoked as two commands, the first to mark the
	/// first word or phrase and the second to select and create the link with the second
	/// words or phrase.
	/// </summary>
	internal class BiLinkCommand : Command
	{
		private static string pageOneId;
		private static string objectOneId;


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
						IsCancelled = true;
						return;
					}

					logger.WriteLine($"mark {pageOneId} object {objectOneId}");
				}
				else
				{
					if (string.IsNullOrEmpty(pageOneId))
					{
						logger.WriteLine($"mark not set");
						IsCancelled = true;
						return;
					}

					logger.WriteLine($"link to {pageOneId} from {one.CurrentPageId} object {objectOneId}");
					pageOneId = null;
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
				return false;
			}

			var oe = run.Ancestors(ns + "OE").FirstOrDefault();
			if (oe == null)
			{
				return false;
			}

			if (!oe.GetAttributeValue("objectID", out objectOneId))
			{
				return false;
			}

			pageOneId = one.CurrentPageId;
			return true;
		}
	}
}
