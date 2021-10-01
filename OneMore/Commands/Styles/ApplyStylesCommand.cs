//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Apply the user-defined custom styles of the current them to all content within the page
	/// by attempting to match standard styles with custom styles. Ror example, it will apply
	/// custom-heading-1 to all standard-heading-1 content. This is done by applying the styles
	/// directly to the QuickStyleDefs declarations.
	/// </summary>
	internal class ApplyStylesCommand : Command
	{

		private Page page;
		private XNamespace ns;
		private readonly Stylizer stylizer;


		public ApplyStylesCommand()
		{
			// using blank Style just so we have a valid Stylizer
			stylizer = new Stylizer(new Style());
		}


		/// <summary>
		/// Invoked from Custom Styles/Apply Styles to Page command
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out page, out ns))
			{
				if (ApplyCurrentTheme())
				{
					await one.Update(page);
				}
			}
		}


		/// <summary>
		/// Invoked from ChangePageColor command
		/// </summary>
		/// <param name="page"></param>
		public void Apply(Page page)
		{
			this.page = page;
			ns = page.Namespace;
			ApplyCurrentTheme();
		}


		private bool ApplyCurrentTheme()
		{
			var styles = new ThemeProvider().Theme.GetStyles();
			if (ApplyStyles(styles))
			{
				// lists require some inline styling
				ApplyToLists(styles);

				if (page.GetPageColor(out _, out _).GetBrightness() < 0.5)
				{
					// hyperlinks require some inline styling
					ApplyToHyperlinks();
				}

				return true;
			}

			return false;
		}


		private bool ApplyStyles(List<Style> styles)
		{
			var applied = false;

			var quickStyles = page.Root.Elements(ns + "QuickStyleDef");
			if (quickStyles?.Any() == true)
			{
				var foundP = false;

				foreach (var quick in quickStyles)
				{
					var name = quick.Attribute("name").Value;

					// only affect the first QuickStyleDef[@name='p']
					if (!foundP || name != "p")
					{
						var style = FindStyle(styles, name);
						if (style != null)
						{
							//logger.WriteLine(
							//	$"~ name:{quick.Attribute("name").Value} style:{style.Name}");

							// could use QuickStyleDef class here but this is faster
							// than replacing the element...

							quick.Attribute("font").Value = style.FontFamily;

							quick.Attribute("spaceBefore").Value = style.SpaceBefore;
							quick.Attribute("spaceAfter").Value = style.SpaceAfter;
							// must also apply to paragraphs otherwise OneNote applies x10 values!
							SetSpacing(quick.Attribute("index").Value, style.SpaceBefore, style.SpaceAfter);

							quick.Attribute("fontColor").Value = style.Color;
							quick.Attribute("highlightColor").Value = style.Highlight;

							quick.SetAttributeValue("italic", style.IsItalic.ToString().ToLower());
							quick.SetAttributeValue("bold", style.IsBold.ToString().ToLower());
							quick.SetAttributeValue("underline", style.IsUnderline.ToString().ToLower());

							if (name == "p")
							{
								if (!foundP)
								{
									quick.Attribute("fontSize").Value = style.FontSize;
									foundP = true;
								}
							}
							else
							{
								quick.Attribute("fontSize").Value = style.FontSize;
							}

							applied = true;
						}

						var index = quick.Attribute("index").Value;
						ClearInlineStyles(index, name == "p");
					}
				}
			}

			return applied;
		}


		private static Style FindStyle(List<Style> styles, string name)
		{
			Style style = null;

			switch (name)
			{
				case "p":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "normal")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "body")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "p");
					break;

				case "cite":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "citation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "cite");
					break;

				case "blockquote":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "quote")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "quotation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "blockquote");
					break;

				case "code":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "code")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "source code");
					break;

				case "PageTitle":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "page title")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "pagetitle")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "title");
					break;

				default:
					var nmatch = Regex.Match(name, @"^h(\d)$");
					if (nmatch.Success)
					{
						var index = nmatch.Groups[1].Captures[0].Value;

						// match any of (h1, head1, head 1, heading1, heading 1)
						style = styles.SingleOrDefault(s =>
							Regex.IsMatch(s.Name, $@"^[Hh](?:ead)?.*?{index}$"));
					}
					break;
			}

			return style;
		}


		private void SetSpacing(string index, string spaceBefore, string spaceAfter)
		{
			var paragraphs = page.Root.Descendants(ns + "OE")
				.Where(e => e.Attribute("quickStyleIndex")?.Value == index);

			foreach (var paragraph in paragraphs)
			{
				paragraph.SetAttributeValue("spaceBefore", spaceBefore);
				paragraph.SetAttributeValue("spaceAfter", spaceAfter);
			}
		}


		// when applying styles, we want to preserve any special treatments to paragraphs
		// because that would be tedious for the user to restore manually if their intention 
		// was to keep those colors, but we want to clear all color styling from every other
		// element type like headings, et al.
		private void ClearInlineStyles(string index, bool paragraph)
		{
			var elements = page.Root.Descendants()
				.Where(e => e.Attribute("quickStyleIndex")?.Value == index);

			if (elements != null)
			{
				foreach (var element in elements)
				{
					stylizer.Clear(element, paragraph ? Stylizer.Clearing.Gray : Stylizer.Clearing.All);
				}
			}
		}


		private void ApplyToLists(List<Style> styles)
		{
			var style = styles.SingleOrDefault(s =>
				s.Name.ToLower() == "normal" ||
				s.Name.ToLower() == "body" ||
				s.Name.ToLower() == "p");

			if (style == null)
			{
				style = new Style
				{
					Color = page.GetPageColor(out _, out _).GetBrightness() < 0.5 ? "#FFFFFF" : "#000000"
				};
			}

			var elements = page.Root.Descendants(ns + "Bullet");
			if (elements?.Any() == true)
			{
				ApplyToListItems(elements, style, false);
			}

			elements = page.Root.Descendants(ns + "Number");
			if (elements?.Any() == true)
			{
				ApplyToListItems(elements, style, true);
			}
		}

		private static void ApplyToListItems(IEnumerable<XElement> elements, Style style, bool withFamily)
		{
			foreach (var element in elements)
			{
				element.SetAttributeValue("fontColor", style.Color);
				element.SetAttributeValue("fontSize", style.FontSize);

				if (withFamily)
				{
					element.SetAttributeValue("font", style.FontFamily);
				}
			}
		}


		/*
		 * 
		 * This doesn't work. OneNote post-processes the page and override our changes :-(
		 * 
		 */

		private void ApplyToHyperlinks()
		{
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var elements = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => regex.IsMatch(c.Value))
				.Select(e => e.Parent)
				.ToList();

			if (elements?.Count > 0)
			{
				foreach (var element in elements)
				{
					var cdata = element.GetCData();

					if (cdata.Value.EndsWith("</a>"))
					{
						// OneNote applies styles at the OE level for link-only content
						// so apply color to CDATA's one:OE
						ColorizeElement(element.Parent, "#5B9BD5");
					}
					else
					{
						// OneNote applies styles inline when link is accompanied by other text

						var wrapper = cdata.GetWrapper();
						var a = wrapper.Element("a");
						if (a != null)
						{
							ColorizeElement(a, "#5B9BD5");
						}

						cdata.ReplaceWith(wrapper.GetInnerXml());
					}
				}
			}
		}

		private static void ColorizeElement(XElement element, string color)
		{
			var attr = element.Attribute("style");
			if (attr != null)
			{
				var style = new Style(attr.Value)
				{
					Color = color
				};

				attr.Value = style.ToCss();
			}
			else
			{
				element.Add(new XAttribute("style", "color:#5B9BD5"));
			}
		}
	}
}
