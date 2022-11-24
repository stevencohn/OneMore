//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // Sections of code should not be commented out

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ApplyStyleCommand : Command
	{
		private Page page;
		private XNamespace ns;
		private Stylizer stylizer;
		private Style style;


		public ApplyStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var selectedIndex = (int)args[0];

			style = new ThemeProvider().Theme.GetStyle(selectedIndex);
			if (style == null)
			{
				// could be from a CtrlAltShift+# but that indexed style doesn't exist
				// e.g. there are only 5 custom styles but the user pressed CtrlAltShift+6
				// so just do nothing
				return;
			}

			logger.StartClock();

			using var one = new OneNote(out page, out ns);
			if (page != null)
			{
				logger.WriteTime($"loaded page; applying style {style.Name}", true);

				stylizer = new Stylizer(style);

				bool success = style.StyleType == StyleType.Character
					? StylizeWords()
					: StylizeParagraphs();

				if (success)
				{
					logger.WriteTime("applied style; saving page", true);

					await one.Update(page);

					logger.WriteTime("saved page");
				}
			}
		}


		private bool StylizeWords()
		{
			// find all selected T element; may be multiple if text is selected across 
			// multiple paragraphs - OEs - but partial paragraphs may be selected too...

			var selections = page.Root.Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if (selections == null)
			{
				// shouldn't happen, but...
				return false;
			}

			foreach (var selection in selections)
			{
				bool empty = true;

				if (selection.Parent.Nodes().Count() == 1)
				{
					// OE parent must have only this T

					stylizer.ApplyStyle(selection);
				}
				else
				{
					// OE parent has multiple Ts so test if we need to merge them

					//logger.WriteLine("selection.parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));

					var cdata = selection.GetCData();

					// text cursor is positioned but selection length is 0
					if (cdata.IsEmpty())
					{
						// inside a word, adjacent to a word, or somewhere in whitespace?

						var prev = selection.PreviousNode as XElement;
						if ((prev != null) && prev.GetCData().EndsWithWhitespace())
						{
							prev = null;
						}

						var next = selection.NextNode as XElement;
						if ((next != null) && next.GetCData().StartsWithWhitespace())
						{
							next = null;
						}

						if ((prev != null) && (next != null))
						{
							empty = false;

							// navigate to closest word

							var word = new StringBuilder();

							if (prev != null)
							{
								//logger.WriteLine("prev:" + prev.ToString(SaveOptions.None));

								if (!prev.GetCData().EndsWithWhitespace())
								{
									// grab previous part of word
									word.Append(prev.ExtractLastWord());
									//logger.WriteLine("word with prev:" + word.ToString());
									//logger.WriteLine("prev updated:" + prev.ToString(SaveOptions.None));
									//logger.WriteLine("parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));

									if (prev.GetCData().Value.Length == 0)
									{
										prev.Remove();
									}
								}
							}

							if (next != null)
							{
								//logger.WriteLine("next:" + next.ToString(SaveOptions.None));

								if (!next.GetCData().StartsWithWhitespace())
								{
									// grab following part of word
									word.Append(next.ExtractFirstWord());
									//logger.WriteLine("word with next:" + word.ToString());
									//logger.WriteLine("next updated:" + next.ToString(SaveOptions.None));
									//logger.WriteLine("parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));

									if (next.GetCData().Value.Length == 0)
									{
										next.Remove();
									}
								}
							}

							if (word.Length > 0)
							{
								selection.DescendantNodes().OfType<XCData>()
									.First()
									.ReplaceWith(new XCData(word.ToString()));

								//logger.WriteLine("parent udpated:" + (selection.Parent as XElement).ToString(SaveOptions.None));
							}
							else
							{
								empty = true;
							}

							//logger.WriteLine("parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));
						}
					}

					if (empty)
					{
						stylizer.ApplyStyle(selection.GetCData());
						//logger.WriteLine("final empty parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));
					}
					else
					{
						stylizer.ApplyStyle(selection);
						//logger.WriteLine("final parent:" + (selection.Parent as XElement).ToString(SaveOptions.None));
					}
				}
			}

			return true;
		}


		private bool StylizeParagraphs()
		{
			// find all paragraphs - OE elements - that have selections
			// TODO: filter out MetaNames.TaggingBank?
			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")))
				.Select(p => p.Parent);

			if (elements?.Any() != true)
			{
				return false;
			}

			var css = style.ToCss();

			var applied = new Style(style)
			{
				ApplyColors = true
			};

			foreach (var element in elements)
			{
				// clear any existing style on or within the paragraph
				stylizer.Clear(element, style.ApplyColors ? Stylizer.Clearing.All : Stylizer.Clearing.None);

				SetQuickStyle(page, element, style);

				// style may still exist if apply colors if false and there are colors
				var attr = element.Attribute("style");
				if (attr == null)
				{
					// blast style onto paragraph, let OneNote normalize across
					// children if it wants
					attr = new XAttribute("style", css);
					element.Add(attr);
				}
				else
				{
					applied.MergeColors(new Style(attr.Value));
					attr.Value = applied.ToCss();
				}

				ApplySpacing(element, "spaceBefore", style.SpaceBefore);
				ApplySpacing(element, "spaceAfter", style.SpaceAfter);
				ApplySpacing(element, "spaceBetween", style.Spacing);

				ApplyToList(element, style);
			}

			return true;
		}


		private void SetQuickStyle(Page page, XElement element, Style style)
		{
			if (style.StyleType == StyleType.Heading)
			{
				// force override quick style to correct heading index...

				var quick = page.GetQuickStyle((StandardStyles)style.Index);
				var attr = element.Attribute("quickStyleIndex");
				if (attr == null)
				{
					element.Add(new XAttribute("quickStyleIndex", quick.Index));
				}
				else
				{
					attr.Value = quick.Index.ToString();
				}
			}
			else
			{
				// force to normal quickstyle only if currently heading...
				// do not override quote, cite, etc with normal.

				var attr = element.Attribute("quickStyleIndex");
				if (attr != null)
				{
					if (int.TryParse(attr.Value, out var index))
					{
						var quick = page.GetQuickStyle((StandardStyles)(index - 1));
						if (quick.StyleType == StyleType.Heading)
						{
							quick = page.GetQuickStyle(StandardStyles.Normal);
							attr.Value = quick.Index.ToString();
						}
					}
				}
			}
		}


		private static void ApplySpacing(XElement element, string name, string space)
		{
			var attr = element.Attribute(name);
			if (attr == null)
			{
				element.Add(new XAttribute(name, space));
			}
			else
			{
				attr.Value = space;
			}
		}


		private void ApplyToList(XElement element, Style style)
		{
			var item = element.Elements(ns + "List").Elements()
				.FirstOrDefault(e => e.Name.LocalName == "Bullet" || e.Name.LocalName == "Number");

			if (item != null)
			{
				item.SetAttributeValue("fontColor", style.Color);
				item.SetAttributeValue("fontSize", style.FontSize);

				if (item.Name.LocalName == "Number")
				{
					item.SetAttributeValue("font", style.FontFamily);
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
