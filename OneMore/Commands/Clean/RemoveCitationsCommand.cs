//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Removes citations that OneNote auto-generates when you paste screen clipping and parts
	/// of Web pages into OneNote, for example From <https://www....
	/// </summary>
	internal class RemoveCitationsCommand : Command
	{
		public RemoveCitationsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);
			logger.StartClock();

			var style = page.GetQuickStyles()
				.Find(s => s.Name == "cite");

			if (style == null)
			{
				return;
			}

			var index = style.Index.ToString(CultureInfo.InvariantCulture);

			// TODO: not sure if this entire string needs localizing to accomodate RTL
			var regex = new Regex($@"{Resx.RemoveCitations_From} &lt;\s*<a\shref=.+?</a>\s*&gt;");

			var elements =
				(from e in page.Root.Descendants(ns + "OE")
				 where e.Attributes("quickStyleIndex").Any(a => a.Value == index)
				 let text = e.Element(ns + "T").GetCData().Value
				 where text.Contains(Resx.RemoveCitations_Clippings) || regex.Match(text).Success
				 select e)
				.ToList();

			if (elements.Any())
			{
				foreach (var element in elements)
				{
					element.Remove();
				}

				logger.WriteTime("removed citations, now saving...");

				await one.Update(page);
			}

			logger.StopClock();
		}
	}
}
