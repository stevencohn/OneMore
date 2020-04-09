//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Xml.Linq;


	internal enum StatusColor
	{
		Blue,
		Gray,
		Green,
		Red,
		Yellow
	}

	internal class InsertStatusCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertStatusCommand() : base()
		{
		}


		public void Execute(StatusColor statusColor)
		{
			try
			{
				InsertStatus(statusColor);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertStatusCommand)}", exc);
			}
		}


		private void InsertStatus(StatusColor statusColor)
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var element = page.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.LastOrDefault();

				if (element != null)
				{
					string color = "black";
					string background = "yellow";

					switch (statusColor)
					{
						case StatusColor.Gray:
							color = "white";
							background = "#42526E";
							break;

						case StatusColor.Red:
							color = "white";
							background = "#BF2600";
							break;

						case StatusColor.Yellow:
							color = "172B4D";
							background = "#FF991F";
							break;

						case StatusColor.Green:
							color = "white";
							background = "#00875A";
							break;

						case StatusColor.Blue:
							color = "white";
							background = "#0052CC";
							break;
					}

					var status = $"";

					var cdata = element.DescendantNodes().OfType<XCData>().LastOrDefault();
					if (cdata != null)
					{
						var span = new XElement("span",
							new XAttribute("style", $"font-family:'Segoe UI';font-size:10.0pt;font-weight:bold;color:{color};background:{background}"),
							$"     STATUS     "
							);

						cdata.Value += span.ToString(SaveOptions.DisableFormatting);

						manager.UpdatePageContent(page);
					}
				}
			}
		}
	}
}
