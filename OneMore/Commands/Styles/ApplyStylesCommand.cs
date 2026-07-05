//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1192   // use const               
#pragma warning disable S125    // remove commented code                            

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
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
	internal class ApplyStylesCommand : Command, ICliPageCommand
	{

		private const string MediumBlue = "#5B9BD5";

		private static readonly HashSet<string> MonospaceFonts =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Consolas", "Courier", "Courier New", StyleBase.DefaultCodeFamily,
				"Monaco", "Cascadia Code", "Fira Code", "Fira Mono",
				"Source Code Pro", "Inconsolata", "Hack", "DejaVu Sans Mono"
			};

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


		internal ApplyStylesCommand(Theme theme)
		{
			stylizer = new Stylizer(new Style());
			this.theme = theme;
		}


		#region ICliCommand

		public string CommandName => "ApplyStyles";
		public string Description => "Apply the current style theme to one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		#endregion ICliCommand


		/// <summary>
		/// Invoked from Custom Styles/Apply Styles to Page command
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				await ExecuteAsCli(cliParams);
				return;
			}

			await using var one = new OneNote(out page, out ns);

			var range = new SelectionRange(page);
			_ = range.GetSelections();
			var hasSelection = range.Scope != SelectionScope.TextCursor;

			// selection mode: apply theme styles only to selected paragraphs inline;
			// do not change the page background color
			var changed = hasSelection
				? ApplyToSelectedParagraphs()
				: ApplyToPage();

			if (changed)
			{
				await one.Update(page);
			}
		}


		private async Task ExecuteAsCli(CliParameterSet cliParams)
		{
			cliParams.TryGet("pageId", out string pageId);
			if (string.IsNullOrWhiteSpace(pageId)) return;

			await using var one = new OneNote();
			page = await one.GetPage(pageId, OneNote.PageDetail.All);
			ns = page.Namespace;

			if (ApplyToPage())
			{
				logger.Verbose($"applied styles to page: {page.Title}");
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


		private bool ApplyToPage()
		{
			var changed = false;

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

			return changed;
		}


		private bool ApplyThemeStyles()
		{
			var styles = theme.GetStyles();

			// promote "p" paragraphs whose inline style matches a heading theme style
			// (e.g. content pasted from Word retains quickStyleIndex="p" but carries
			// heading-like inline CSS) before ApplyStyles/ClearInlineStyles run, so the
			// reassigned QuickStyleDef is picked up by the main styling pass below
			var promoted = PromoteHeadingLikeNormalParagraphs(styles);

			// collect monospace Normal paragraphs while their OE-level font is still intact,
			// before ApplyStyles/ClearInlineStyles strips it
			var monoTargets = CollectMonospaceNormalOEs();

			// mark inline code spans BEFORE ClearInlineStyles strips their font-family
			MarkInlineCodeSpans(styles);

			var changed = ApplyStyles(styles);

			if (promoted)
			{
				changed = true;
			}

			if (changed)
			{
				// lists require some inline styling
				ApplyToLists(styles);

				if (page.GetPageColor(out _, out _).GetBrightness() < 0.5)
				{
					// hyperlinks require some inline styling
					ApplyToHyperlinks();
				}
			}

			if (ApplyToTitle(styles))
			{
				changed = true;
			}

			// re-apply inline code style after ClearInlineStyles has run
			if (RestoreInlineCodeSpans(styles))
			{
				changed = true;
			}

			// promote pre-collected monospace Normal paragraphs to the code style
			if (ApplyToMonospaceNormalParagraphs(styles, monoTargets))
			{
				changed = true;
			}

			return changed;
		}


		private bool ApplyToTitle(List<Style> styles)
		{
			var style = styles.FirstOrDefault(s => s.StyleType == StyleType.PageTitle)
				?? FindStyle(styles, "PageTitle");

			if (style is null)
			{
				return false;
			}

			var title = page.Root.Element(ns + "Title");
			if (title is null)
			{
				return false;
			}

			var oe = title.Element(ns + "OE");
			if (oe is null)
			{
				return false;
			}

			// find the page's PageTitle QuickStyleDef; create one if the page doesn't have it
			var quick = page.Root.Elements(ns + "QuickStyleDef")
				.FirstOrDefault(q => q.Attribute("name")?.Value == "PageTitle");

			if (quick is null)
			{
				var qsd = page.GetQuickStyle(StandardStyles.PageTitle);
				quick = page.Root.Elements(ns + "QuickStyleDef")
					.FirstOrDefault(q => q.Attribute("index")?.Value == qsd.Index.ToString());
			}

			if (quick is not null)
			{
				quick.SetAttributeValue("font", style.FontFamily);
				quick.SetAttributeValue("fontSize", style.FontSize);
				quick.SetAttributeValue("spaceBefore", style.SpaceBefore);
				quick.SetAttributeValue("spaceAfter", style.SpaceAfter);
				quick.SetAttributeValue("italic", style.IsItalic.ToString().ToLower());
				quick.SetAttributeValue("bold", style.IsBold.ToString().ToLower());
				quick.SetAttributeValue("underline", style.IsUnderline.ToString().ToLower());

				if (style.ApplyColors)
				{
					quick.SetAttributeValue("fontColor", style.Color);
					quick.SetAttributeValue("highlightColor", style.Highlight);
				}

				// keep the title OE pointing at this QuickStyleDef
				oe.SetAttributeValue("quickStyleIndex", quick.Attribute("index").Value);
			}

			// clear any inline overrides on the title OE and re-apply from the theme style
			stylizer.Clear(oe, Stylizer.Clearing.All);

			var css = style.ToCss();
			var attr = oe.Attribute("style");
			if (attr is null)
				oe.Add(new XAttribute("style", css));
			else
				attr.Value = css;

			oe.SetAttributeValue("spaceBefore", style.SpaceBefore);
			oe.SetAttributeValue("spaceAfter", style.SpaceAfter);

			return true;
		}


		private bool ApplyStyles(List<Style> styles)
		{
			var quickStyles = page.Root.Elements(ns + "QuickStyleDef");
			if (quickStyles == null || !quickStyles.Any())
			{
				return false;
			}

			string spacing = null;
			if (FindStyle(styles, "p") is Style normal &&
				double.TryParse(
					normal.Spacing, NumberStyles.Any, CultureInfo.InvariantCulture, out var spc) && spc > 0.0)
			{
				spacing = normal.Spacing;
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
					if (style.ApplyColors && !(style.IsCode || style.StyleType == StyleType.Code))
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
					style is not null && 
					(style.IsCode || style.StyleType == StyleType.Code || !style.ApplyColors)
						? Stylizer.Clearing.Gray
						: name == "p" ? Stylizer.Clearing.Gray : Stylizer.Clearing.All;

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
					style = styles.FirstOrDefault(s => s.StyleType == StyleType.Citation)
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "citation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "cite");
					break;

				case "blockquote":
					style = styles.FirstOrDefault(s => s.StyleType == StyleType.Quote)
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "quote")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "quotation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "blockquote");
					break;

				case "code":
					// prefer the explicit IsCode flag (set by the user in the Edit
					// Styles dialog, or migrated by StyleRecord for legacy themes)
					// and fall back to conventional names so themes that haven't
					// been resaved yet still resolve
					style = styles.FirstOrDefault(s => s.IsCode)
						?? styles.FirstOrDefault(s => s.StyleType == StyleType.Code)
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "code")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "source code");
					break;

				case "PageTitle":
					style = styles.FirstOrDefault(s => s.StyleType == StyleType.PageTitle)
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "page title")
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


		/// <summary>
		/// Scans "p" (Normal) paragraphs whose OE-level inline style carries a font-family
		/// and font-size matching one of the theme's Heading styles, and reassigns them to
		/// the matching heading QuickStyleDef. This recovers content pasted from Word (or
		/// other editors) that bakes heading appearance into inline CSS while leaving
		/// quickStyleIndex pointing at the generic "p" QuickStyleDef.
		/// </summary>
		private bool PromoteHeadingLikeNormalParagraphs(List<Style> styles)
		{
			if (!styles.Any(s => s.StyleType == StyleType.Heading))
			{
				return false;
			}

			// collect all "p" QuickStyleDef indices; also include OEs with no quickStyleIndex
			var pIndices = page.Root.Elements(ns + "QuickStyleDef")
				.Where(q => q.Attribute("name")?.Value == "p")
				.Select(q => q.Attribute("index")?.Value)
				.Where(v => v != null)
				.ToHashSet();

			var paragraphs = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(oe =>
				{
					var idx = oe.Attribute("quickStyleIndex")?.Value;
					return (idx == null || pIndices.Contains(idx)) && oe.Attribute("style") != null;
				})
				.ToList();

			var promoted = false;

			foreach (var oe in paragraphs)
			{
				var headingStyle = FindMatchingHeadingStyle(styles, oe.Attribute("style").Value);
				var standard = headingStyle is null ? null : ToHeadingStandard(headingStyle.Name);
				if (standard is null)
				{
					continue;
				}

				var qsd = page.GetQuickStyle(standard.Value);
				oe.SetAttributeValue("quickStyleIndex", qsd.Index.ToString());
				promoted = true;
			}

			return promoted;
		}


		/// <summary>
		/// Finds a theme Heading style whose font-family and font-size match the given
		/// inline CSS, or null if the CSS doesn't declare both or none match.
		/// </summary>
		private static Style FindMatchingHeadingStyle(IEnumerable<Style> styles, string css)
		{
			if (string.IsNullOrWhiteSpace(css))
			{
				return null;
			}

			var inline = new Style(css, setDefaults: false);
			if (string.IsNullOrEmpty(inline.FontFamily) ||
				!double.TryParse(inline.FontSize, NumberStyles.Any, CultureInfo.InvariantCulture, out var inlineSize) ||
				inlineSize <= 0)
			{
				return null;
			}

			return styles.FirstOrDefault(s =>
				s.StyleType == StyleType.Heading &&
				!string.IsNullOrEmpty(s.FontFamily) &&
				s.FontFamily.Equals(inline.FontFamily, StringComparison.OrdinalIgnoreCase) &&
				double.TryParse(s.FontSize, NumberStyles.Any, CultureInfo.InvariantCulture, out var size) &&
				Math.Abs(size - inlineSize) < 0.05);
		}


		/// <summary>
		/// Maps a theme Heading style's display name (e.g. "Heading 2") to its
		/// corresponding StandardStyles.HeadingN value, or null if it can't be determined.
		/// </summary>
		private static StandardStyles? ToHeadingStandard(string styleName)
		{
			var match = Regex.Match(styleName ?? string.Empty, @"(\d)\s*$");
			if (!match.Success)
			{
				return null;
			}

			var level = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
			if (level < 1 || level > 6)
			{
				return null;
			}

			return (StandardStyles)((int)StandardStyles.Heading1 + level - 1);
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


		private static bool IsMonospaceFont(string fontFamily)
		{
			if (string.IsNullOrEmpty(fontFamily)) return false;
			foreach (var font in fontFamily.Split(','))
			{
				if (MonospaceFonts.Contains(font.Trim().Trim('\'', '"'))) return true;
			}
			return false;
		}


		private HashSet<string> GetCodeQuickStyleIndices()
		{
			return page.Root.Elements(ns + "QuickStyleDef")
				.Where(q => q.Attribute("name")?.Value == "code")
				.Select(q => q.Attribute("index")?.Value)
				.Where(v => v != null)
				.ToHashSet();
		}


		/// <summary>
		/// Marks monospace-font spans in non-Code paragraphs with an omcode attribute so they
		/// can be re-styled after ClearInlineStyles strips their font-family
		/// </summary>
		private void MarkInlineCodeSpans(List<Style> styles)
		{
			var codeIndices = GetCodeQuickStyleIndices();

			var paragraphs = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(oe =>
				{
					var idx = oe.Attribute("quickStyleIndex")?.Value;
					return idx == null || !codeIndices.Contains(idx);
				});

			foreach (var para in paragraphs)
			{
				foreach (var run in para.Elements(ns + "T"))
				{
					var cdata = run.GetCData();
					if (cdata == null ||
						cdata.Value.IndexOf("font-family:", StringComparison.OrdinalIgnoreCase) < 0)
					{
						continue;
					}

					var wrapper = cdata.GetWrapper();
					var marked = false;

					foreach (var span in wrapper.Descendants("span").ToList())
					{
						var attr = span.Attribute("style")?.Value;
						if (attr != null && HasMonospaceFont(attr))
						{
							span.SetAttributeValue("omcode", "1");
							marked = true;
						}
					}

					if (marked)
					{
						cdata.Value = wrapper.GetInnerXml();
					}
				}
			}
		}


		/// <summary>
		/// Finds spans marked by MarkInlineCodeSpans and applies the theme's Character-type
		/// monospace style, or the standard Lucida Console 9pt default if none exists
		/// </summary>
		private bool RestoreInlineCodeSpans(List<Style> styles)
		{
			var inlineStyle = styles.FirstOrDefault(s =>
				s.StyleType == StyleType.Character &&
				IsMonospaceFont(s.FontFamily));

			var css = inlineStyle?.ToCss()
				?? $"font-family:'{StyleBase.DefaultCodeFamily}';font-size:9.0pt";
			var updated = false;

			foreach (var cdata in page.Root.DescendantNodes()
				.OfType<XCData>()
				.Where(c => c.Value.Contains("omcode")).ToList())
			{
				var wrapper = cdata.GetWrapper();
				var spanUpdated = false;

				foreach (var span in wrapper.Descendants("span")
					.Where(s => s.Attribute("omcode")?.Value == "1").ToList())
				{
					span.Attribute("omcode").Remove();
					span.SetAttributeValue("style", css);
					spanUpdated = true;
				}

				if (spanUpdated)
				{
					cdata.Value = wrapper.GetInnerXml();
					updated = true;
				}
			}

			return updated;
		}


		/// <summary>
		/// Returns true when the OE uses a monospace font (either at the OE level via its
		/// style attribute, or explicitly on every content-bearing inline span) and no span
		/// overrides that with a non-monospace font-family.
		/// </summary>
		private bool IsAllMonospaceParagraph(XElement oe)
		{
			// Primary check: monospace font-family set on the OE element itself
			var oeStyle = oe.Attribute("style")?.Value;
			var oeMonospace = oeStyle != null && HasMonospaceFont(oeStyle);

			var runs = oe.Elements(ns + "T").ToList();
			if (!runs.Any()) return false;

			var hasContent = false;
			foreach (var run in runs)
			{
				var cdata = run.GetCData();
				if (cdata == null) continue;

				var value = cdata.Value;
				var plainText = Regex.Replace(value, "<[^>]+>", string.Empty).Trim();
				if (string.IsNullOrEmpty(plainText)) continue;

				hasContent = true;

				var wrapper = cdata.GetWrapper();

				if (oeMonospace)
				{
					// OE sets monospace; any inline span that explicitly overrides to a
					// non-monospace font-family disqualifies the paragraph
					foreach (var span in wrapper.Descendants("span"))
					{
						var spanStyle = span.Attribute("style")?.Value;
						if (spanStyle != null &&
							spanStyle.IndexOf("font-family:", StringComparison.OrdinalIgnoreCase) >= 0 &&
							!HasMonospaceFont(spanStyle))
						{
							return false;
						}
					}
				}
				else
				{
					// No OE-level monospace; every piece of content must be explicitly in a
					// monospace span — no bare text, no non-monospace font-family
					if (value.IndexOf("font-family:", StringComparison.OrdinalIgnoreCase) < 0)
					{
						return false;
					}

					if (wrapper.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value)))
					{
						return false;
					}

					foreach (var span in wrapper.Descendants("span"))
					{
						var spanStyle = span.Attribute("style")?.Value;
						if (spanStyle != null &&
							spanStyle.IndexOf("font-family:", StringComparison.OrdinalIgnoreCase) >= 0 &&
							!HasMonospaceFont(spanStyle))
						{
							return false;
						}
					}
				}
			}

			return hasContent;
		}


		/// <summary>
		/// Scans for Normal ("p") paragraphs whose OE-level style sets a monospace font,
		/// collecting them before ApplyStyles/ClearInlineStyles strips that font.
		/// </summary>
		private List<XElement> CollectMonospaceNormalOEs()
		{
			// collect all "p" QuickStyleDef indices; also include OEs with no quickStyleIndex
			var pIndices = page.Root.Elements(ns + "QuickStyleDef")
				.Where(q => q.Attribute("name")?.Value == "p")
				.Select(q => q.Attribute("index")?.Value)
				.Where(v => v != null)
				.ToHashSet();

			return page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(oe =>
				{
					var idx = oe.Attribute("quickStyleIndex")?.Value;
					return (idx == null || pIndices.Contains(idx)) && IsAllMonospaceParagraph(oe);
				})
				.ToList();
		}


		/// <summary>
		/// Promotes pre-collected monospace Normal paragraphs to the code QuickStyleDef,
		/// applying the theme's custom code style when one exists or falling back to the
		/// built-in Code QuickStyleDef. Respects ApplyColors: when false, inline colors
		/// already preserved by Gray-clearing are kept; when true they are overwritten.
		/// </summary>
		private bool ApplyToMonospaceNormalParagraphs(List<Style> styles, List<XElement> targets)
		{
			if (!targets.Any()) return false;

			var codeStyle = FindStyle(styles, "code");
			var codeQsd = page.GetQuickStyle(StandardStyles.Code);
			var codeIndex = codeQsd.Index.ToString();

			foreach (var oe in targets)
			{
				oe.SetAttributeValue("quickStyleIndex", codeIndex);

				if (codeStyle != null)
				{
					var attr = oe.Attribute("style");
					string css;
					if (codeStyle.ApplyColors)
					{
						css = codeStyle.ToCss();
					}
					else
					{
						var merged = new Style(codeStyle) { ApplyColors = true };
						if (attr != null) merged.MergeColors(new Style(attr.Value));
						css = merged.ToCss();
					}

					if (attr == null) oe.Add(new XAttribute("style", css));
					else attr.Value = css;

					oe.SetAttributeValue("spaceBefore", codeStyle.SpaceBefore);
					oe.SetAttributeValue("spaceAfter", codeStyle.SpaceAfter);
				}
			}

			return true;
		}


		private static bool HasMonospaceFont(string css)
		{
			// extract font-family value from the CSS string and check against the known set
			var idx = css.IndexOf("font-family:", StringComparison.OrdinalIgnoreCase);
			if (idx < 0) return false;

			var start = idx + "font-family:".Length;
			var end = css.IndexOf(';', start);
			var fontValue = end >= 0
				? css.Substring(start, end - start)
				: css.Substring(start);

			return IsMonospaceFont(fontValue);
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

				// Detect Normal paragraphs whose inline style matches a heading theme style
				if (quickName == "p")
				{
					var headingStyle = FindMatchingHeadingStyle(styles, oe.Attribute("style")?.Value);
					var standard = headingStyle is null ? null : ToHeadingStandard(headingStyle.Name);
					if (standard is not null)
					{
						var headingQsd = page.GetQuickStyle(standard.Value);
						oe.SetAttributeValue("quickStyleIndex", headingQsd.Index.ToString());
						style = headingStyle;
						quickName = standard.Value.ToName();
					}
				}

				// Detect Normal paragraphs whose text is entirely monospace → treat as code
				if (quickName == "p" && IsAllMonospaceParagraph(oe))
				{
					var codeStyle = FindStyle(styles, "code");
					var codeQsd = page.GetQuickStyle(StandardStyles.Code);
					oe.SetAttributeValue("quickStyleIndex", codeQsd.Index.ToString());

					if (codeStyle != null)
					{
						style = codeStyle;
						quickName = "code";
					}
					else
					{
						// built-in code fallback: reassign QuickStyleDef and Gray-clear so
						// the code QSD font takes effect (colors are preserved)
						stylizer.Clear(oe, Stylizer.Clearing.Gray, deep: false);
						applied = true;
						continue;
					}
				}

				// clearing mode mirrors ApplyStyles()
				var clearing =
					style.IsCode || style.StyleType == StyleType.Code || !style.ApplyColors
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
