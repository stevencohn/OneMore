//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;


	internal class RemovePageNumbersCommand : Command
	{
		private readonly Regex npattern;
		private readonly Regex apattern;
		private readonly Regex ipattern;


		public RemovePageNumbersCommand()
		{
			// <span style='font-family:"Calibri Light"' lang=en-US>(1)</span>
			var spanner = @"^((?:<span[^>]+\>)?\({0}\)\s*(?:</span>)?\s*)(?:.+)";

			npattern = new Regex(string.Format(spanner, @"(?:\d+\.{0,1})+"));
			apattern = new Regex(string.Format(spanner, @"[a-z]+"));
			ipattern = new Regex(string.Format(spanner, @"(?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})"));
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			using var one = new OneNote();

			var section = one.GetSection();
			if (section != null)
			{
				var ns = one.GetNamespace(section);

				var pageIds = section.Elements(ns + "Page")
					.Select(e => e.Attribute("ID").Value)
					.ToList();

				using var progress = new UI.ProgressDialog();
				progress.SetMaximum(pageIds.Count);
				progress.Show();

				foreach (var pageId in pageIds)
				{
					var page = one.GetPage(pageId, OneNote.PageDetail.Basic);

					var name = page.Root.Attribute("name").Value;
					progress.SetMessage(string.Format(
						Properties.Resources.RemovingPageNumber_Message, name));

					progress.Increment();

					if (string.IsNullOrEmpty(name))
					{
						continue;
					}

					var cdata = page.Root.Element(ns + "Title")
						.Element(ns + "OE")
						.Element(ns + "T")
						.GetCData();

					var text = cdata.Value;

					if (RemoveNumbering(text, out string clean))
					{
						cdata.Value = clean;
						await one.Update(page);
					}
				}
			}

			logger.StopClock();
			logger.WriteTime("removed page numbering");
		}


		/// <summary>
		/// Used by NumberPagesCommand to clean up old numbers before applying new
		/// </summary>
		/// <param name="name"></param>
		/// <param name="clean"></param>
		/// <returns></returns>
		public bool RemoveNumbering(string name, out string clean)
		{
			// numeric 1.
			var match = npattern.Match(name);

			// alpha i. -- do this prior to alpha match
			if (!match.Success)
			{
				match = ipattern.Match(name);
			}

			// alpha a.
			if (!match.Success)
			{
				match = apattern.Match(name);
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
