//************************************************************************************************
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
	/// If text is selected, applies matching theme styles inline only to the selected paragraphs
	/// rather than re-styling the whole page.
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

			var hasSelection = page.Root
				.Descendants(ns + "T")
				.Any(e => e.Attributes("selected").Any(a => a.Value == "all"));

			var changed = false;

			if (hasSelection)
			{
				// selection mode: apply theme styles only to selected paragraphs inline;
				// do not change the page background color
				changed = ApplyToSelectedParagraphs();
			}
			else
			{
				// whole-page mode: existing behavior unchanged

				if (!string.IsNullOrWhiteSpace(theme.Color))
				{
					var cmd = new PageColorCommand();
					cmd.SetLogger(logger);
					changed = cmd.UpdatePageColor(page, theme.Color);
				}

				if (ApplyThemeStyles())
				{
					changed = true;
				}
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

				var style = FindStyle(styles, name);
				if (style is not null)
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

					// code styles never repaint the page's code quickstyle, so the
					// user's syntax-highlighting (or any custom fontColor on the
					// QuickStyleDef) is preserved; other styles honor their own
					// ApplyColors setting
					if (style.ApplyColors && !style.IsCode)
					{
						quick.SetAttributeValue("fontColor", style.Color);
						quick.SetAttributeValue("highlightColor", style.Highlight);
					}

					quick.SetAttributeValue("italic", style.IsItalic.ToString().ToLower());
					quick.SetAttributeValue("bold", style.IsBold.ToString().ToLower());
					quick.SetAttributeValue("underline", style.IsUnderline.ToString().ToLower());

					quick.Attribute("fontSize").Value = style.FontSize;

					applied = true;
				}
				else if (name != "code")
				{
					// No custom theme style matched this QuickStyleDef. Sync font to the
					// current OneNote default so pages created under an old default font
					// render correctly after the user changes their default font.
					quick.Attribute("font").Value = StyleBase.DefaultFontFamily;
					if (name == "p")
					{
						quick.Attribute("fontSize").Value =
							StyleBase.DefaultFontSize.ToString("0.0", CultureInfo.InvariantCulture);
					}

					applied = true;
				}

				var index = quick.Attribute("index").Value;

				// preserve inline colors for code paragraphs (syntax highlighting)
				// and for any style that opted out of ApplyColors; otherwise keep
				// the historic behavior: Gray-clear normal "p" paragraphs (to keep
				// deliberate accents) and All-clear everything else
				var clearing =
					style is not null && (style.IsCode || !style.ApplyColors)
						? Stylizer.Clearing.Gray
						: name == "p"
							? Stylizer.Clearing.Gray
							: Stylizer.Clearing.All;

				ClearInlineStyles(index, clearing);
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
					// prefer the explicit IsCode flag (set by the user in the Edit
					// Styles dialog, or migrated by StyleRecord for legacy themes)
					// and fall back to conventional names so themes that haven't
					// been resaved yet still resolve
					style = styles.FirstOrDefault(s => s.IsCode)
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "code")
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
						a.Attribute("name").Value.StartsWith(MetaNames.TaggingBank))
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
		private void ClearInlineStyles(string index, Stylizer.Clearing clearing)
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
					stylizer.Clear(element, clearing);
				}
			}
		}


		private void ApplyToLists(IEnumerable<Style> styles)
		{
			var quickDefs = page.Root.Elements(ns + "QuickStyleDef").ToList();

			var normalStyle = styles.SingleOrDefault(s =>
				s.Name.ToLower() == "normal" ||
				s.Name.ToLower() == "body" ||
				s.Name.ToLower() == "p")
				?? new Style { Color = page.GetBestTextColor().ToRGBHtml() };

			Style StyleFor(XElement listItem)
			{
				var qsi = listItem.Parent?.Parent?.Attribute("quickStyleIndex")?.Value;
				if (qsi == null) return normalStyle;
				var quickName = quickDefs
					.FirstOrDefault(q => q.Attribute("index")?.Value == qsi)
					?.Attribute("name")?.Value;
				return quickName != null ? (FindStyle(styles, quickName) ?? normalStyle) : normalStyle;
			}

			foreach (var element in page.Root.Descendants(ns + "Bullet").ToList())
			{
				var s = StyleFor(element);
				element.SetAttributeValue("fontColor", s.Color);
				element.SetAttributeValue("fontSize", s.FontSize);
			}

			foreach (var element in page.Root.Descendants(ns + "Number").ToList())
			{
				var s = StyleFor(element);
				element.SetAttributeValue("fontColor", s.Color);
				element.SetAttributeValue("fontSize", s.FontSize);
				element.SetAttributeValue("font", s.FontFamily);
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


		private bool ApplyToSelectedParagraphs()
		{
			var styles = theme.GetStyles();

			// spaceBetween applies only to "p" paragraphs (mirrors ApplyStyles logic)
			string normalSpacing = null;
			if (FindStyle(styles, "p") is Style normalStyle &&
				double.TryParse(normalStyle.Spacing,
					NumberStyles.Any, CultureInfo.InvariantCulture, out var spc) && spc > 0.0)
			{
				normalSpacing = normalStyle.Spacing;
			}

			// index -> name lookup for all QuickStyleDefs on the page
			var quickDefs = page.Root
				.Elements(ns + "QuickStyleDef")
				.ToDictionary(
					q => q.Attribute("index").Value,
					q => q.Attribute("name").Value);

			// selected OEs = parents of selected T elements, deduplicated
			var selectedOEs = page.Root
				.Descendants(ns + "T")
				.Where(t => t.Attributes("selected").Any(a => a.Value == "all"))
				.Select(t => t.Parent)
				.Distinct()
				.ToList();

			if (!selectedOEs.Any())
			{
				return false;
			}

			var applied = false;

			foreach (var oe in selectedOEs)
			{
				// skip guarded paragraphs (same guards as ClearInlineStyles / SetSpacing)
				var meta = oe.Element(ns + "Meta");
				if (meta is not null)
				{
					var metaName = meta.Attribute("name")?.Value ?? string.Empty;
					if (metaName.StartsWith("omfootnote") ||
						metaName.StartsWith(MetaNames.TaggingBank) ||
						(metaName == Style.HintMeta &&
						 meta.Attribute("content")?.Value == "skip"))
					{
						continue;
					}
				}

				// resolve QuickStyleDef name for this OE; default "p" if absent
				var qsiAttr = oe.Attribute("quickStyleIndex");
				var quickName = "p";
				if (qsiAttr is not null &&
					quickDefs.TryGetValue(qsiAttr.Value, out var resolvedName))
				{
					quickName = resolvedName;
				}

				var style = FindStyle(styles, quickName);
				if (style is null)
				{
					continue;
				}

				// clearing mode mirrors ApplyStyles()
				var clearing =
					style.IsCode || !style.ApplyColors
						? Stylizer.Clearing.Gray
						: quickName == "p"
							? Stylizer.Clearing.Gray
							: Stylizer.Clearing.All;

				stylizer.Clear(oe, clearing, deep: false);

				// apply theme style CSS inline to the OE
				var css = style.ToCss();
				var attr = oe.Attribute("style");
				if (attr is null)
				{
					oe.Add(new XAttribute("style", css));
				}
				else if (!style.ApplyColors)
				{
					// preserve existing colors; only update non-color properties
					var merged = new Style(style) { ApplyColors = true };
					merged.MergeColors(new Style(attr.Value));
					attr.Value = merged.ToCss();
				}
				else
				{
					attr.Value = css;
				}

				// paragraph spacing
				oe.SetAttributeValue("spaceBefore", style.SpaceBefore);
				oe.SetAttributeValue("spaceAfter", style.SpaceAfter);
				if (quickName == "p" && normalSpacing is not null)
				{
					oe.SetAttributeValue("spaceBetween", normalSpacing);
				}

				// list bullet/number inline colors
				ApplyToListItem(oe, style);

				applied = true;
			}

			return applied;
		}


		private void ApplyToListItem(XElement oe, Style style)
		{
			var item = oe.Elements(ns + "List").Elements()
				.FirstOrDefault(e =>
					e.Name.LocalName == "Bullet" ||
					e.Name.LocalName == "Number");

			if (item is null)
			{
				return;
			}

			if (style.ApplyColors)
			{
				item.SetAttributeValue("fontColor", style.Color);
			}

			item.SetAttributeValue("fontSize", style.FontSize);

			if (item.Name.LocalName == "Number")
			{
				item.SetAttributeValue("font", style.FontFamily);
			}
		}
	}
}
