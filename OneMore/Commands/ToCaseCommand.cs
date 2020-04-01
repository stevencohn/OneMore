//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;


	internal class ToCaseCommand : Command
	{
		public ToCaseCommand () : base()
		{
		}


		public void Execute (bool upper)
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var cursor = page.Descendants(ns + "T")
					.Where(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						((XCData)e.FirstNode).Value.Length == 0)
					.FirstOrDefault();

				if (cursor != null)
				{
					var prev = cursor.PreviousNode as XElement;
					if ((prev != null) && prev.GetCData().EndsWithWhitespace())
					{
						prev = null;
					}

					var next = cursor.NextNode as XElement;
					if ((next != null) && next.GetCData().StartsWithWhitespace())
					{
						next = null;
					}

					if ((prev != null) && (next != null))
					{
						var word = new StringBuilder();

						if (prev != null)
						{
							if (!prev.GetCData().EndsWithWhitespace())
							{
								word.Append(prev.ExtractLastWord());
								if (prev.GetCData().Value.Length == 0)
								{
									prev.Remove();
								}
							}
						}

						if (next != null)
						{
							if (!next.GetCData().StartsWithWhitespace())
							{
								word.Append(next.ExtractFirstWord());
								if (next.GetCData().Value.Length == 0)
								{
									next.Remove();
								}
							}
						}

						if (word.Length > 0)
						{
							var text = upper ? word.ToString().ToUpper() : word.ToString().ToLower();

							cursor.DescendantNodes()
								.OfType<XCData>()
								.First()
								.ReplaceWith(new XCData(text));
						}
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
									part.ReplaceWith(upper ? part.Value.ToUpper() : part.Value.ToLower());
								}

								selection.FirstNode.ReplaceWith(
									new XCData(
										string.Concat(wrapper.Nodes().Select(x => x.ToString()).ToArray())
									));
							}
						}
					}
				}

				manager.UpdatePageContent(page);
			}
		}
	}
}
