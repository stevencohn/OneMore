//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text;
    using System.Xml;
    using System.Xml.Linq;


	internal class ApplyStyleCommand : Command
	{

		public ApplyStyleCommand() : base()
		{
		}


		public void Execute(int selectedIndex)
		{
			using (var style = new StylesProvider().GetStyle(selectedIndex, false))
			{
				using (var manager = new ApplicationManager())
				{
					var page = manager.CurrentPage();
					if (page != null)
					{
						Evaluate(page, style, manager);
					}
				}
			}
		}


		private void Evaluate(XElement page, CustomStyle style, ApplicationManager manager)
		{
			var ns = page.GetNamespaceOfPrefix("one");

			// find all selections; may be multiple if text is selected across multiple paragraphs
			var selections = page.Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if (selections == null)
			{
				// shouldn't happen, but...
				return;
			}

			var stylizer = new Stylizer(new CssInfo(style));

			foreach (var selection in selections)
			{
				if (selection.Parent.Nodes().Count() == 1)
				{
					// OE parent must have only this T

					stylizer.ApplyStyle(selection);
				}
				else
				{
					// OE parent has multiple Ts so test if we need to merge them

					logger.WriteLine("selection.parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));

					var cdata = selection.GetCData();

					// text cursor is positioned but selection length is 0
					if (cdata.IsEmpty())
					{
						// navigate to closest word

						var word = new StringBuilder();

						var prev = selection.PreviousNode as XElement;
						if ((prev != null) && (prev is XElement))
						{
							logger.WriteLine("prev:" + prev.ToString(SaveOptions.None));

							if (!prev.GetCData().EndsWithWhitespace())
							{
								// grab previous part of word
								word.Append(prev.ExtractLastWord());
								logger.WriteLine("word with prev:" + word.ToString());
								logger.WriteLine("prev updated:" + prev.ToString(SaveOptions.None));
								logger.WriteLine("parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));
							}
						}

						var next = selection.NextNode as XElement;
						if ((next != null) && (next	is XElement))
						{
							logger.WriteLine("next:" + next.ToString(SaveOptions.None));

							if (!next.GetCData().StartsWithWhitespace())
							{
								// grab following part of word
								word.Append(next.ExtractFirstWord());
								logger.WriteLine("word with next:" + word.ToString());
								logger.WriteLine("next updated:" + next.ToString(SaveOptions.None));
								logger.WriteLine("parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));
							}
						}

						if (word.Length > 0)
						{
							selection.DescendantNodes()
								.Where(e => e.NodeType == XmlNodeType.CDATA)
								.First()
								.ReplaceWith(new XCData(word.ToString()));

							logger.WriteLine("parent udpated:" + (selection.Parent as XElement).ToString(SaveOptions.None));
						}

						if (prev.GetCData().Value.Length == 0)
						{
							prev.Remove();
						}

						if (next.GetCData().Value.Length == 0)
						{
							next.Remove();
						}

						logger.WriteLine("parent final:" + (selection.Parent as XElement).ToString(SaveOptions.None));
					}

					stylizer.ApplyStyle(selection);
				}
#if FOO
				if (style.StyleType != StyleType.Character)
				{
					// apply spacing to parent OE; we may have selected text across multiple OEs
					// but we'll just reapply if all Ts are within the same OE, no biggie
					var oe = selection.Parent;
					ApplySpacing(oe, "spaceBefore", style.SpaceBefore);
					ApplySpacing(oe, "spaceAfter", style.SpaceAfter);
				}
#endif
			}

			manager.UpdatePageContent(page);
		}


		private void ApplySpacing(XElement paragraph, string name, int space)
		{
			var attr = paragraph.Attribute(name);
			if (attr == null)
			{
				if (space > 0)
				{
					paragraph.Add(new XAttribute(name, space.ToString()));
				}
			}
			else
			{
				if (space > 0)
				{
					attr.Value = space.ToString();
				}
				else
				{
					attr.Remove();
				}
			}
		}
	}

	/*
	 * one:OE -------------------------
	 *
	 * T=all but OE=partial because EOL is not selected - NOTE ONE CHILD
	  <one:OE creationTime="2020-03-15T23:29:18.000Z" lastModifiedTime="2020-03-15T23:29:18.000Z" objectID="{BF7825D6-1EE4-46C0-AC87-B2FFA76137D1}{15}{B0}" alignment="left" quickStyleIndex="1" selected="partial">
        <one:T selected="all"><![CDATA[This is the fourth line]]></one:T>
      </one:OE>
	 *
	 * T=all and OE=all because EOL is selected - NOTE ONE CHILD
	  <one:OE creationTime="2020-03-15T23:29:18.000Z" lastModifiedTime="2020-03-15T23:29:18.000Z" objectID="{BF7825D6-1EE4-46C0-AC87-B2FFA76137D1}{15}{B0}" selected="all" alignment="left" quickStyleIndex="1">
        <one:T selected="all"><![CDATA[This is the fourth line]]></one:T>
      </one:OE>
	 * 
	 * one:T --------------------------
	 * 
	 * middle of word
      <one:OE creationTime="2020-03-15T23:29:18.000Z" lastModifiedTime="2020-03-15T23:29:18.000Z" objectID="{BF7825D6-1EE4-46C0-AC87-B2FFA76137D1}{15}{B0}" alignment="left" quickStyleIndex="1" selected="partial">
        <one:T><![CDATA[This is the fo]]></one:T>
        <one:T selected="all"><![CDATA[]]></one:T>
        <one:T><![CDATA[urth line]]></one:T>
      </one:OE>
	 * 
	 * selected word
      <one:OE creationTime="2020-03-15T23:29:18.000Z" lastModifiedTime="2020-03-15T23:29:18.000Z" objectID="{BF7825D6-1EE4-46C0-AC87-B2FFA76137D1}{15}{B0}" alignment="left" quickStyleIndex="1" selected="partial">
        <one:T><![CDATA[This is the ]]></one:T>
        <one:T selected="all"><![CDATA[fourth ]]></one:T>
        <one:T><![CDATA[line]]></one:T>
      </one:OE>
	 *
 	 */
}
