//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;


	internal class ToggleDttmCommand : Command
	{
		public ToggleDttmCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				var title = page.Root.Element(page.Namespace + "Title");
				if (title != null)
				{
					var attr = title.Attribute("showDate");
					if (attr == null || attr.Value == "true")
					{
						title.SetAttributeValue("showDate", "false");
					}
					else if (attr != null)
					{
						attr.Remove();
					}

					attr = title.Attribute("showTime");
					if (attr == null || attr.Value == "true")
					{
						title.SetAttributeValue("showTime", "false");
					}
					else if (attr != null)
					{
						attr.Remove();
					}

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
