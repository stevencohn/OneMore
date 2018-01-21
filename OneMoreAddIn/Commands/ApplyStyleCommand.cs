//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	internal class ApplyStyleCommand : Command
	{

		public ApplyStyleCommand () : base()
		{
		}


		public void Execute (int selectedIndex)
		{
			using (var style = new StylesProvider().GetStyle(selectedIndex, false))
			{
				using (var manager = new ApplicationManager())
				{
					var page = manager.CurrentPage();
					if (page != null)
					{
						EvaluatePage(page, style, manager);
					}
				}
			}
		}


		//<one:T><![CDATA[One two thre]]></one:T>
		//<one:T selected="all"><![CDATA[]]></one:T>
		//<one:T><![CDATA[e]]></one:T>

		private void EvaluatePage (XElement page, CustomStyle style, ApplicationManager manager)
		{
			var ns = page.GetNamespaceOfPrefix("one");

			// find all selections; may be multiple if text is selected across multiple paragraphs

			var selections = page.Descendants(ns + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if (selections != null)
			{
				foreach (var selection in selections)
				{
					var phrase = new Phrase(selection);

					if (phrase.IsEmpty)
					{
						// infer selected word by combining adjacent non-whitespace characters to
						// the left of the cursor and to the right of the cursor into a single word

						string word = string.Empty;

						if ((selection.PreviousNode != null) && (selection.PreviousNode is XElement))
						{
							var prev = new Phrase(selection.PreviousNode as XElement);
							if (!prev.EndsWithSpace)
							{
								word += prev.ExtractLastWord();
							}
						}

						if ((selection.NextNode != null) && (selection.NextNode is XElement))
						{
							var next = new Phrase(selection.NextNode as XElement);
							if (!next.StartsWithSpace)
							{
								word += next.ExtractFirstWord();
							}
						}

						if (word.Length > 0)
						{
							phrase.CData.Value =
								new XElement("span", new XAttribute("style", style.ToCss(true)), word)
								.ToString(SaveOptions.DisableFormatting);
						}
						else
						{
							// cannot apply style to an empty CDATA because OneNote will
							// strip the styling off, so instead need to apply to parent one:T instead

							var span = selection.Attribute("span");
							if (span == null)
							{
								selection.Add(new XAttribute("span", style.ToCss(true)));
							}
							else
							{
								span.Value = style.ToCss(true);
							}
						}
					}
					else
					{
						phrase.ClearFormatting();

						phrase.CData.Value =
							new XElement("span", new XAttribute("style", style.ToCss(true)), phrase.CData.Value)
							.ToString(SaveOptions.DisableFormatting);
					}

					// apply spacing to parent OE; we may have selected text across multiple OEs
					// but we'll just reapply if all Ts are within the same OE, no biggie

					var oe = selection.Parent;
					ApplySpacing(oe, "spaceBefore", style.SpaceBefore);
					ApplySpacing(oe, "spaceAfter", style.SpaceAfter);
				}

				manager.UpdatePageContent(page);
			}
		}

		private void ApplySpacing (XElement paragraph, string name, int space)
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
}
