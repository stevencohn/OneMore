//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class RemoveCitationsCommand : Command
	{
		public RemoveCitationsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				logger.StartClock();

				var style = page.GetQuickStyles()
					.FirstOrDefault(s => s.Name == "cite");

				if (style == null)
				{
					return;
				}

				var index = style.Index.ToString(CultureInfo.InvariantCulture);
				var pattern = new Regex(Resx.RemoveCitations_FromUrl);

				var elements =
					(from e in page.Root.Descendants(ns + "OE")
					 where e.Attributes("quickStyleIndex").Any(a => a.Value == index)
					 let text = e.Element(ns + "T").GetCData().Value
					 where text.Contains(Resx.RemoveCitations_Clippings) || pattern.Match(text).Success
					 select e)
					.ToList();

				if (elements?.Count > 0)
				{
					foreach (var element in elements)
					{
						element.Remove();
					}

					logger.WriteTime("removed citations, now saving...");

					one.Update(page);
				}

				logger.StopClock();
			}
		}
	}
}
/*
<one:OE quickStyleIndex="3">
  <one:T><![CDATA[Screen clipping taken: 9/21/2020 5:19 PM]]></one:T>
</one:OE>

one:OE quickStyleIndex="3">
  <one:T><![CDATA[From &lt;<a
href="...">...</a>&gt; ]]></one:T>
</one:OE>

 */