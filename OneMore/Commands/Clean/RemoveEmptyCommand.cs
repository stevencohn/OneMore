//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Collapse multiple consecutive empty lines into a single empty line. Also removes empty
	/// headers, custom and standard.
	/// </summary>
	internal class RemoveEmptyCommand : Command
	{
		public RemoveEmptyCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{

				// first un-indent indented lines so we can then easily check if there are consecutive
				// empty lines that need to be collapse...

				var children = page.Root.Elements(ns + "Outline")?.Elements(ns + "OEChildren");
				if (children == null)
				{
					return;
				}

				foreach (var child in children)
				{
					UndentEmptyLines(child, ns);
				}

				// second, find consecutive empty lines that need to be collapsed...

				var elements =
					(from e in page.Root.Descendants(page.Namespace + "OE")
					 where e.Elements().Count() == 1
					 let t = e.Elements().First()
					 where (t != null) && (t.Name.LocalName == "T") && (t.Value.Length == 0)
					 select e)
					.ToList();

				if (elements != null)
				{
					var quickStyles = page.GetQuickStyles()
						.Where(s => s.StyleType == StyleType.Heading);

					var customStyles = new StyleProvider().GetStyles()
						.Where(e => e.StyleType == StyleType.Heading)
						.ToList();

					var modified = false;

					foreach (var element in elements)
					{
						// is this a known Heading style?
						var attr = element.Attribute("quickStyleIndex");
						if (attr != null)
						{
							var index = int.Parse(attr.Value, CultureInfo.InvariantCulture);
							if (quickStyles.Any(s => s.Index == index))
							{
								// remove empty standard heading
								element.Remove();
								modified = true;
								continue;
							}
						}

						// is this a custom Heading style?
						var style = new Style(element.CollectStyleProperties(true));
						if (customStyles.Any(s => s.Equals(style)))
						{
							// remove empty custom heading
							element.Remove();
							modified = true;
							continue;
						}

						// is this an empty paragraph preceded by an empty paragraph?
						if (element.PreviousNode != null &&
							element.PreviousNode.NodeType == System.Xml.XmlNodeType.Element)
						{
							var prev = element.PreviousNode as XElement;

							if (prev.Name.LocalName == "OE" &&
								prev.Elements().Count() == 1)
							{
								var t = prev.Elements().First();
								if (t.Name.LocalName == "T" && t.Value.Length == 0)
								{
									// remove consecutive empty line
									element.Remove();
									modified = true;
								}
							}
						}
					}

					if (modified)
					{
						one.Update(page);
					}
				}
			}
		}


		private void UndentEmptyLines(XElement parent, XNamespace ns)
		{
			var children = parent.Elements(ns + "OE")?.Elements(ns + "OEChildren");
			if (children == null)
			{
				return;
			}

			foreach (var child in children)
			{
				if (!child.HasElements)
				{
					continue;
				}

				var oe = child.Elements(ns + "OE").FirstOrDefault();
				if (oe.NextNode == null && oe.Value.Trim() == string.Empty)
				{
					child.Remove();
					parent.Add(oe);
				}

				var grands = child.Elements(ns + "OE")?.Elements(ns + "OEChildren");
				if (grands != null)
				{
					foreach (var grand in grands)
					{
						UndentEmptyLines(grand, ns);
					}
				}
			}
		}
	}
}
/*
    <one:OEChildren selected="partial">
      <one:OE alignment="left" quickStyleIndex="2" selected="partial">
        <one:T selected="all"><![CDATA[test]]></one:T>
        <one:OEChildren>
          <one:OE alignment="left" quickStyleIndex="2">
            <one:T><![CDATA[]]></one:T>
            <one:OEChildren>
              <one:OE alignment="left" quickStyleIndex="2">
                <one:T><![CDATA[]]></one:T>
              </one:OE>
            </one:OEChildren>
          </one:OE>
        </one:OEChildren>
      </one:OE>
      <one:OE alignment="left" quickStyleIndex="2">
        <one:T><![CDATA[]]></one:T>
      </one:OE>
      <one:OE alignment="left" quickStyleIndex="2">
        <one:T><![CDATA[Foo]]></one:T>
      </one:OE>
      <one:OE alignment="left" quickStyleIndex="2">
        <one:T><![CDATA[]]></one:T>
      </one:OE>
    </one:OEChildren>
*/