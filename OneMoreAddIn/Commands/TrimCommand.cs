//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;


	internal class TrimCommand : Command
	{
		public TrimCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var selections =
					from e in page.Descendants(ns + "OE").Elements(ns + "T")
					where e.Attributes("selected").Any(a => a.Value.Equals("all"))
					select e;

				foreach (var selection in selections)
				{
					if (selection.FirstNode?.NodeType == XmlNodeType.CDATA)
					{
						var wrapper = XElement.Parse("<x>" + selection.FirstNode.Parent.Value + "</x>");

						foreach (var part in wrapper.DescendantNodes().OfType<XText>().ToList())
						{
							part.ReplaceWith(part.Value.Trim());
						}

						selection.FirstNode.ReplaceWith(
							new XCData(
								string.Concat(wrapper.Nodes().Select(x => x.ToString()).ToArray())
								)
							);
					}
				}

				manager.UpdatePageContent(page);
			}
		}
	}
}
