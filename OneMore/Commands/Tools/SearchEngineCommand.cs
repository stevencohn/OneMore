//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;


	internal class SearchEngineCommand : Command
	{
		public SearchEngineCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var uri = (string)args[0];
			var word = new StringBuilder();

			using (var one = new OneNote(out var page, out var ns))
			{
				var cursor = page.GetTextCursor();

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
						from e in page.Root.Descendants(ns + "OE").Elements(ns + "T")
						where e.Attributes("selected").Any(a => a.Value.Equals("all"))
						select e;

					if (selections?.Any() == true)
					{
						foreach (var selection in selections)
						{
							if (selection.FirstNode?.NodeType == XmlNodeType.CDATA)
							{
								var wrapper = selection.FirstNode.Parent.Value.ToXmlWrapper();

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
				logger.WriteLine($"search query {url}");
				System.Diagnostics.Process.Start(url);
			}
			else
			{
				logger.WriteLine("search phrase is empty");
			}
		}
	}
}
