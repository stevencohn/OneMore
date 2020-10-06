//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;


	internal class SearchEngineCommand : Command
	{
		public SearchEngineCommand() : base()
		{
		}


		public void Execute(string uri)
		{
			var word = new StringBuilder();

			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var cursor = page.Descendants(ns + "T")
					.FirstOrDefault(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						((XCData)e.FirstNode).Value.Length == 0);

				if (cursor != null)
				{
					if ((cursor.PreviousNode is XElement prev) && !prev.GetCData().EndsWithWhitespace())
					{
						word.Append(prev.ExtractLastWord());
					}

					if ((cursor.NextNode is XElement next) && !next.GetCData().StartsWithWhitespace())
					{
						word.Append(next.ExtractFirstWord());
					}
				}
				else
				{
					var selections =
						from e in page.Descendants(ns + "OE").Elements(ns + "T")
						where e.Attributes("selected").Any(a => a.Value.Equals("all"))
						select e;

					if (selections?.Any() == true)
					{
						foreach (var selection in selections)
						{
							if (selection.FirstNode?.NodeType == XmlNodeType.CDATA)
							{
								var wrapper = XElement.Parse("<x>" + selection.FirstNode.Parent.Value + "</x>");

								foreach (var part in wrapper.DescendantNodes().OfType<XText>().ToList())
								{
									word.Append(part);
								}
							}
						}
					}
				}
			}

			if (word.Length > 0)
			{
				var url = string.Format(uri, word.ToString());
				logger.WriteLine($"Search query {url}");
				System.Diagnostics.Process.Start(url);
			}
			else
			{
				logger.WriteLine("Search phrase is empty");
			}
		}
	}
}
