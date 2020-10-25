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
		public ToCaseCommand ()
		{
		}


		public override void Execute(params object[] args)
		{
			bool upper = (bool)args[0];

			using (var one = new OneNote(out var page, out var ns))
			{
				var updated = false;

				var cursor = page.Root.Descendants(ns + "T")
					.FirstOrDefault(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						((XCData)e.FirstNode).Value.Length == 0);

				if (cursor != null)
				{
					var word = new StringBuilder();

					if ((cursor.PreviousNode is XElement prev) && !prev.GetCData().EndsWithWhitespace())
					{
						word.Append(prev.ExtractLastWord());
						if (prev.GetCData().Value.Length == 0)
						{
							prev.Remove();
						}
					}

					if ((cursor.NextNode is XElement next) && !next.GetCData().StartsWithWhitespace())
					{
						word.Append(next.ExtractFirstWord());
						if (next.GetCData().Value.Length == 0)
						{
							next.Remove();
						}
					}

					if (word.Length > 0)
					{
						var text = upper ? word.ToString().ToUpper() : word.ToString().ToLower();

						cursor.DescendantNodes()
							.OfType<XCData>()
							.First()
							.ReplaceWith(new XCData(text));

						updated = true;
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
								// used numbered entity &#160; instead of undeclared &nbsp;
								var value = selection.FirstNode.Parent.Value.Replace("&nbsp;", "&#160;");

								var wrapper = XElement.Parse($"<x>{value}</x>");

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

						updated = true;
					}
				}

				if (updated)
				{
					one.Update(page);
				}
			}
		}
	}
}
