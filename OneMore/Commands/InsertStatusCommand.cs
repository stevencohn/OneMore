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

				var elements = page.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all");

				if (elements != null)
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

					var span = new XElement("span",
						new XAttribute("style", $"font-family:'Segoe UI';font-size:10.0pt;font-weight:bold;color:{color};background:{background}"),
						$"     STATUS     "
						);

					var status = new XElement(ns + "T",
						new XCData(span.ToString(SaveOptions.DisableFormatting) + "&#160;")
						);


					if ((elements.Count() == 1) &&
						(elements.First().GetCData().Value.Length == 0))
					{
						// no selection so insert just before cursor
						elements.First().AddBeforeSelf(status);
					}
					else
					{
						// replace one or more one:T @select=all with status, place cursor after
						var element = elements.Last();
						element.AddAfterSelf(status);
						elements.Remove();

						status.AddAfterSelf(new XElement(ns + "T",
							new XAttribute("selected", "all"),
							new XCData(string.Empty)
							));
					}

					manager.UpdatePageContent(page);
				}
			}
		}
	}
}
