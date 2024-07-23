﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Apply the currently loaded user-defined custom styles to all content within the page
	/// by attempting to match standard styles with custom styles. For example, it will apply
	/// custom-heading-1 to all standard-heading-1 content. This is done by applying the styles
	/// directly to the QuickStyleDefs declarations.
	/// </summary>
	internal class ApplyStylesCommand : Command
	{

		private const string MediumBlue = "#5B9BD5";

		private Page page;
		private XNamespace ns;
		private readonly Theme theme;
		private readonly Stylizer stylizer;


		public ApplyStylesCommand()
		{
			// using blank Style just so we have a valid Stylizer
			stylizer = new Stylizer(new Style());
			theme = new ThemeProvider().Theme;
		}


		/// <summary>
		/// Invoked from Custom Styles/Apply Styles to Page command
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out page, out ns);

			// apply theme page color..

			var changed = false;
			if (!string.IsNullOrWhiteSpace(theme.Color))
			{
				var cmd = new PageColorCommand();
				cmd.SetLogger(logger);
				changed = cmd.UpdatePageColor(page, theme.Color);
			}

			// apply theme styles...

			if (ApplyThemeStyles())
			{
				changed = true;
			}

			if (changed)
			{
				await one.Update(page);
			}
		}


		/// <summary>
		/// Invoked from PageColorCommand. Sets only the theme styles, presuming
		/// PageColorCommand has already set the page background color.
		/// </summary>
		/// <param name="page">The page to modify</param>
		public void ApplyTheme(Page page)
		{
			this.page = page;
			ns = page.Namespace;
			ApplyThemeStyles();
		}


		private bool ApplyThemeStyles()
		{
			var styles = theme.GetStyles();
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
			var quickStyles = page.Root.Elements(ns + "QuickStyleDef");
			if (quickStyles == null || !quickStyles.Any())
			{
				return false;
			}

			string spacing = null;
			if (FindStyle(styles, "p") is Style normal)
			{
				if (double.TryParse(normal.Spacing, NumberStyles.Any, CultureInfo.InvariantCulture, out var spc) && spc > 0.0)
				{
					spacing = normal.Spacing;
				}
			}

			var applied = false;
			foreach (var quick in quickStyles)
			{
				var name = quick.Attribute("name").Value;

				// NOTE previously, affected only the first occurance of the "p" quick style
				// and ignore all additional "p" occurances defined, QuickStyleDef[@name='p']
				// I don't remember why but pulled out that logic Feb 7 2022

				if (FindStyle(styles, name) is Style style)
				{
					//logger.WriteLine(
					//	$"~ name:{quick.Attribute("name").Value} style:{style.Name}");

					// could use QuickStyleDef class here but this is faster
					// than replacing the element...

					quick.Attribute("font").Value = style.FontFamily;

					quick.Attribute("spaceBefore").Value = style.SpaceBefore;
					quick.Attribute("spaceAfter").Value = style.SpaceAfter;

					// must also apply to paragraphs otherwise OneNote applies x10 values!
					SetSpacing(quick.Attribute("index").Value,
						style.SpaceBefore, style.SpaceAfter,
						// spaceBetween should only apply to normal paragraphs
						name == "p" ? spacing : null);

					quick.Attribute("fontColor").Value = style.Color;
					quick.Attribute("highlightColor").Value = style.Highlight;

					quick.SetAttributeValue("italic", style.IsItalic.ToString().ToLower());
					quick.SetAttributeValue("bold", style.IsBold.ToString().ToLower());
					quick.SetAttributeValue("underline", style.IsUnderline.ToString().ToLower());

					quick.Attribute("fontSize").Value = style.FontSize;

					applied = true;
				}

				var index = quick.Attribute("index").Value;
				ClearInlineStyles(index, name == "p");
			}

			return applied;
		}


		private static Style FindStyle(IEnumerable<Style> styles, string name)
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


		private void SetSpacing(string index, string spaceBefore, string spaceAfter, string spacing)
		{
			var paragraphs = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(e => e.Attribute("quickStyleIndex")?.Value == index &&
					!e.Elements(ns + "Meta").Any(a => 
						a.Attribute("name").Value.StartsWith("omfootnote") ||
						a.Attribute("name").Value.StartsWith("omtaggingbank"))
					);

			foreach (var paragraph in paragraphs)
			{
				paragraph.SetAttributeValue("spaceBefore", spaceBefore);
				paragraph.SetAttributeValue("spaceAfter", spaceAfter);

				if (spacing != null)
				{
					paragraph.SetAttributeValue("spaceBetween", spacing);
				}
			}
		}


		// when applying styles, we want to preserve any special treatments to paragraphs
		// because that would be tedious for the user to restore manually if their intention 
		// was to keep those colors, but we want to clear all color styling from every other
		// element type like headings, et al.
		private void ClearInlineStyles(string index, bool paragraph)
		{
			var elements = page.Root.Descendants()
				.Where(e => e.Attribute("quickStyleIndex")?.Value == index)
				.Select(e => new
				{
					Element = e,
					Meta = e.Element(ns + "Meta")
				})
				.Where(o =>
					o.Meta == null ||
					// filter out omfootnote paragraphs
					!(o.Meta.Attribute("name").Value.StartsWith("omfootnote") ||
					// filter out omStyleHint=skip paragraphs; use in InfoBox symbol cells
					(o.Meta.Attribute("name").Value == Style.HintMeta &&
					 o.Meta.Attribute("content").Value == "skip")))
				.Select(o => o.Element);

			if (elements.Any())
			{
				foreach (var element in elements)
				{
					stylizer.Clear(element,
						paragraph ? Stylizer.Clearing.Gray : Stylizer.Clearing.All);
				}
			}
		}


		private void ApplyToLists(IEnumerable<Style> styles)
		{
			var style = styles.SingleOrDefault(s =>
				s.Name.ToLower() == "normal" ||
				s.Name.ToLower() == "body" ||
				s.Name.ToLower() == "p");

			style ??= new Style
			{
				Color = page.GetBestTextColor().ToRGBHtml()
			};

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

			if (elements.Count > 0)
			{
				foreach (var element in elements)
				{
					var cdata = element.GetCData();

					if (cdata.Value.EndsWith("</a>"))
					{
						var color = (element.Parent.Elements(ns + "Meta")
							.Any(m => m.Attribute("name").Value.StartsWith("omfootnote")))
							? null
							: MediumBlue;

						// OneNote applies styles at the OE level for link-only content
						// so apply color to CDATA's one:OE
						ColorizeElement(element.Parent, color);
					}
					else
					{
						// OneNote applies styles inline when link is accompanied by other text

						var wrapper = cdata.GetWrapper();
						var a = wrapper.Element("a");
						if (a != null)
						{
							ColorizeElement(a, MediumBlue);
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
					Color = color ?? StyleBase.Automatic
				};

				attr.Value = style.ToCss();
			}
			else if (color != null)
			{
				element.Add(new XAttribute("style", $"color:{MediumBlue}"));
			}
		}
	}
}
