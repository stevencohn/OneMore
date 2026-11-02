//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
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
			var spanner = @"^(?<open><span[^>]*>)?(?<num>\({0}\)\s*)(?<close></span>\s*)?(?:.+)";

			npattern = new Regex(string.Format(spanner, @"(?:\d+\.{0,1})+"));
			apattern = new Regex(string.Format(spanner, @"[a-z]+"));
			ipattern = new Regex(string.Format(spanner, @"(?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})"));
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			await using var one = new OneNote();

			var section = await one.GetSection();
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
					var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);

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
				var open = match.Groups["open"];
				var close = match.Groups["close"];
				var length = (open.Success ? open.Length : 0) + match.Groups["num"].Length
					+ (close.Success ? close.Length : 0);

				// if the number lived inside a span that also wraps the title text,
				// keep the opening span so the title retains its styling
				clean = open.Success && !close.Success
					? open.Value + name.Substring(length)
					: name.Substring(length);

				return true;
			}

			clean = name;
			return false;
		}


		/// <summary>
		/// Prepends text to raw title markup without disturbing its styling; if the title
		/// starts with a span, the text is placed inside it so it adopts that styling
		/// </summary>
		/// <param name="markup">The raw CDATA content of a title run</param>
		/// <param name="prefix">The text to prepend, including any trailing space</param>
		/// <returns></returns>
		public static string PrependTitle(string markup, string prefix)
		{
			var match = Regex.Match(markup, @"^<span[^>]*>");
			return match.Success
				? match.Value + prefix + markup.Substring(match.Length)
				: prefix + markup;
		}
	}
}
