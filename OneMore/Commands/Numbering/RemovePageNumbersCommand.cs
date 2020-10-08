//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using System.Linq;
	using System.Text.RegularExpressions;


	internal class RemovePageNumbersCommand : Command
	{
		private readonly Regex npattern;
		private readonly Regex apattern;
		private readonly Regex ipattern;


		public RemovePageNumbersCommand()
		{
			npattern = new Regex(@"^(\((?:\d+\.{0,1})+\)\s).+");
			apattern = new Regex(@"^(\([a-z]+\)\s).+");
			ipattern = new Regex(@"^(\((?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})\)\s).+");
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				if (section != null)
				{
					var ns = section.GetNamespaceOfPrefix("one");

					var pageIds = section.Elements(ns + "Page")
						.Select(e => e.Attribute("ID").Value)
						.ToList();

					using (var progress = new ProgressDialog())
					{
						progress.SetMaximum(pageIds.Count);
						progress.Show(owner);

						foreach (var pageId in pageIds)
						{
							var page = manager.GetPage(pageId, Microsoft.Office.Interop.OneNote.PageInfo.piBasic);
							var name = page.Attribute("name").Value;
							
							progress.SetMessage(string.Format(
								Properties.Resources.RemovingPageNumber_Message, name));

							progress.Increment();

							if (string.IsNullOrEmpty(name))
							{
								continue;
							}

							if (RemoveNumbering(name, out string clean))
							{
								page.Element(ns + "Title")
									.Element(ns + "OE")
									.Element(ns + "T")
									.GetCData().Value = clean;

								manager.UpdatePageContent(page);
							}
						}
					}
				}
			}
		}

		public bool RemoveNumbering(string name, out string clean)
		{
			// numeric 1.
			var match = npattern.Match(name);

			// alpha a.
			if (!match.Success)
			{
				match = apattern.Match(name);
			}

			// alpha i.
			if (!match.Success)
			{
				match = ipattern.Match(name);
			}

			if (match.Success)
			{
				clean = name.Substring(match.Groups[1].Length);
				return true;
			}

			clean = name;
			return false;
		}
	}
}
