﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn.Models
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Media;
	using System.Security.Cryptography;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a page with helper methods
	/// </summary>
	internal partial class Page
	{
		public int TopOutlinePosition = 86;

		private const string HashAttributeName = "omHash";


		/// <summary>
		/// Initialize a new instance with the given page XML root
		/// </summary>
		/// <param name="root"></param>
		public Page(XElement root)
		{
			if (root is not null)
			{
				Namespace = root.GetNamespaceOfPrefix(OneNote.Prefix);
				PageId = root.Attribute("ID")?.Value;
				ComputeHashes(root);

				Root = root;
			}
		}


		#region Hashing
		/// <summary>
		/// Calculates a hash value for each 1st gen child element on the page, which will
		/// be used to optimize the page just prior to saving.
		/// </summary>
		/// <param name="root">The root element of the page</param>
		private static void ComputeHashes(XElement root)
		{
			// MD5 should be sufficient and performs best but is not FIPS compliant
			// so use SHA1 instead. Computers are configured to enable/disable FIPS via
			// HKLM\SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy\Enabled
			using var algo = SHA1.Create();

			// 1st generation child elements of the Page
			foreach (var child in root.Elements())
			{
				if (child.Name.LocalName != "TagDef" &&
					child.Name.LocalName != "QuickStyleDef" &&
					child.Name.LocalName != "Meta")
				{
					var att = child.Attribute(HashAttributeName);
					if (att is null)
					{
						child.Add(new XAttribute(
							HashAttributeName,
							algo.GetHashString(child.ToString(SaveOptions.DisableFormatting))
							));
					}
				}
			}
		}


		/// <summary>
		/// Keeps only modified 1st gen child elements of the page to optimize save performance
		/// especially when the page is loaded with many Ink drawings.
		/// </summary>
		/// <param name="keep">Keeps all Outlines to force full page update</param>
		public void OptimizeForSave(bool keep)
		{
			// see note above regarding SHA1 vs MD5
			using var algo = SHA1.Create();

			// 1st generation child elements of the Page
			foreach (var child in Root.Elements().ToList())
			{
				var att = child.Attribute(HashAttributeName);
				if (att is not null)
				{
					att.Remove();

					if (!keep)
					{
						var hash = algo.GetHashString(child.ToString(SaveOptions.DisableFormatting));
						if (hash == att.Value)
						{
							child.Remove();
						}
					}
				}
			}
		}
		#endregion Hashing


		/// <summary>
		/// Gets all Outlines, skipping the reserved tag bank.
		/// </summary>
		public IEnumerable<XElement> BodyOutlines => Root
			.Elements(Namespace + "Outline")
			.Where(e => !e.Elements(Namespace + "Meta")
				.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)));


		public bool IsValid => Root is not null;


		/// <summary>
		/// Gets the namespace used to create new elements for the page
		/// </summary>
		public XNamespace Namespace { get; private set; }


		/// <summary>
		/// Gets the unique ID of the page
		/// </summary>
		public string PageId { get; private set; }


		/// <summary>
		/// Gets the root element of the page
		/// </summary>
		public XElement Root { get; private set; }


		// TODO: this is inconsistent! It gets the plain text but allows setting complex CDATA
		public string Title
		{
			get
			{
				// account for multiple T runs, e.g., the cursor being in the title
				var titles = Root
					.Elements(Namespace + "Title")
					.Elements(Namespace + "OE")
					.Elements(Namespace + "T")
					.Select(e => e.GetCData().GetWrapper().Value);

				return titles.Any() ? string.Concat(titles) : null;
			}

			set
			{
				// overwrite the entire title
				var title = Root.Elements(Namespace + "Title")
					.Elements(Namespace + "OE").FirstOrDefault();

				title?.ReplaceNodes(new XElement(Namespace + "T", new XCData(value)));
			}
		}


		/// <summary>
		/// Gets the objectID of the title paragraph
		/// </summary>
		public string TitleID
		{
			get
			{
				var attribute = Root
					.Elements(Namespace + "Title")
					.Elements(Namespace + "OE")
					.Attributes("objectID")
					.FirstOrDefault();

				return attribute?.Value;
			}
		}


		/// <summary>
		/// Appends HTML content to the current page
		/// </summary>
		/// <param name="html">Must have the HTML and BODY wrappers</param>
		public void AddHtmlContent(string html)
		{
			var container = EnsureContentContainer();

			container.Add(new XElement(Namespace + "HTMLBlock",
				new XElement(Namespace + "Data", new XCData(html))
				));
		}


		/// <summary>
		/// Adds the given QuickStyleDef element in the proper document order, just after
		/// the TagDef elements if there are any
		/// </summary>
		/// <param name="def"></param>
		public void AddQuickStyleDef(XElement def)
		{
			var tagdef = Root.Elements(Namespace + "TagDef").LastOrDefault();
			if (tagdef is null)
			{
				Root.AddFirst(def);
			}
			else
			{
				tagdef.AddAfterSelf(def);
			}
		}


		/// <summary>
		/// Adds a TagDef to the page and returns its index value. If the tag already exists
		/// the index is returned with no other changes to the page.
		/// </summary>
		/// <param name="symbol">The symbol of the tag</param>
		/// <param name="name">The name to apply to the new tag</param>
		/// <returns>The index of the new tag or of the existing tag with the same symbol</returns>
		public string AddTagDef(string symbol, string name, int tagType = 0)
		{
			var tags = Root.Elements(Namespace + "TagDef");

			int index = 0;
			if (tags?.Any() == true)
			{
				var tag = tags.FirstOrDefault(e =>
					e.Attribute("symbol").Value == symbol &&
					e.Attribute("fontColor").Value == "automatic" &&
					e.Attribute("highlightColor").Value == "none");

				if (tag is not null)
				{
					return tag.Attribute("index").Value;
				}

				index = tags.Max(e => int.Parse(e.Attribute("index").Value)) + 1;
			}

			Root.AddFirst(new XElement(Namespace + "TagDef",
				new XAttribute("index", index.ToString()),
				new XAttribute("type", tagType.ToString()),
				new XAttribute("symbol", symbol),
				new XAttribute("fontColor", "automatic"),
				new XAttribute("highlightColor", "none"),
				new XAttribute("name", name)
				));

			return index.ToString();
		}


		public void AddTagDef(TagDef tagdef)
		{
			var tags = Root.Elements(Namespace + "TagDef");
			if (tags?.Any() == true)
			{
				var tag = tags.FirstOrDefault(e =>
					e.Attribute("symbol").Value == tagdef.Symbol &&
					e.Attribute("fontColor").Value == tagdef.FontColor &&
					e.Attribute("highlightColor").Value == tagdef.HighlightColor);

				if (tag is not null)
				{
					return;
				}
			}

			Root.AddFirst(tagdef);
		}


		/// <summary>
		/// Apply the given quick style mappings to all descendents of the specified outline.
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="outline"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell",
			"S2325:Methods and properties that don't access instance data should be static",
			Justification = "false positive")]
		public void ApplyStyleMapping(List<QuickStyleMapping> mapping, XElement outline)
		{
			// reverse sort the styles so logic doesn't overwrite subsequent index references
			foreach (var map in mapping.OrderByDescending(s => s.Style.Index))
			{
				if (map.OriginalIndex != map.Style.Index.ToString())
				{
					// apply new index to child outline elements
					var elements = outline.Descendants()
						.Where(e => e.Attribute("quickStyleIndex")?.Value == map.OriginalIndex);

					if (elements.Any())
					{
						var index = map.Style.Index.ToString();

						foreach (var element in elements)
						{
							element.Attribute("quickStyleIndex").Value = index;
						}
					}
				}
			}
		}


		/// <summary>
		/// Used by the ribbon to enable/disable items based on whether the focus is currently
		/// on the page body or elsewhere such as the title.
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		public bool ConfirmBodyContext(bool feedback = false)
		{
			var found = Root.Elements(Namespace + "Outline")?
				.Attributes("selected").Any(a => a.Value.Equals("all") || a.Value.Equals("partial"));

			if (found != true)
			{
				if (feedback)
				{
					Logger.Current.WriteLine("could not confirm body context");
					SystemSounds.Exclamation.Play();
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Used by the ribbon to enable/disable items based on whether an image is selected.
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		public bool ConfirmImageSelected(bool feedback = false)
		{
			var found = Root.Descendants(Namespace + "Image")?
				.Attributes("selected").Any(a => a.Value.Equals("all"));

			if (found != true)
			{
				if (feedback)
				{
					Logger.Current.WriteLine("could not confirm image selections");
					SystemSounds.Exclamation.Play();
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Ensures the page contains at least one OEChildren elements and returns it
		/// </summary>
		public XElement EnsureContentContainer(bool last = true)
		{
			XElement container;
			var outline = last
				? Root.Elements(Namespace + "Outline").LastOrDefault()
				: Root.Elements(Namespace + "Outline").FirstOrDefault();

			if (outline is null)
			{
				container = new XElement(Namespace + "OEChildren");
				outline = new XElement(Namespace + "Outline", container);
				Root.Add(outline);
			}
			else
			{
				container = last
					? outline.Elements(Namespace + "OEChildren").LastOrDefault()
					: outline.Elements(Namespace + "OEChildren").FirstOrDefault();

				if (container is null)
				{
					container = new XElement(Namespace + "OEChildren");
					outline.Add(container);
				}
			}

			// check Outline size
			var size = outline.Elements(Namespace + "Size").FirstOrDefault();
			if (size is null)
			{
				// this size is close to OneNote defaults when a new Outline is created
				outline.AddFirst(new XElement(Namespace + "Size",
					new XAttribute("width", "300.0"),
					new XAttribute("height", "14.0")
					));
			}

			return container;
		}


		/// <summary>
		/// Adjusts the width of the given page to accomodate the width of the specified
		/// string without wrapping.
		/// </summary>
		/// <param name="line">The string to measure</param>
		/// <param name="fontFamily">The font family name to apply</param>
		/// <param name="fontSize">The font size to apply</param>
		/// <param name="handle">
		/// A handle to the current window; should be: 
		/// (IntPtr)manager.Application.Windows.CurrentWindow.WindowHandle
		/// </param>
		public void EnsurePageWidth(
			string line, string fontFamily, float fontSize, IntPtr handle)
		{
			// detect page width

			var element = Root.Elements(Namespace + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Elements(Namespace + "Size")
				.FirstOrDefault();

			element ??= Root.Elements(Namespace + "Outline")
				.Last()
				.Elements(Namespace + "Size")
				.FirstOrDefault();

			if (element is null)
			{
				return;
			}

			var attr = element.Attribute("width");
			if (attr is not null)
			{
				var outlinePoints = double.Parse(attr.Value, CultureInfo.InvariantCulture);

				// measure line to ensure page width is sufficient

				using var g = Graphics.FromHwnd(handle);
				using var font = new Font(fontFamily, fontSize);
				var stringSize = g.MeasureString(line, font);
				var stringPoints = stringSize.Width * 72 / g.DpiX;

				if (stringPoints > outlinePoints)
				{
					attr.Value = stringPoints.ToString("#0.00", CultureInfo.InvariantCulture);

					// must include isSetByUser or width doesn't take effect!
					if (element.Attribute("isSetByUser") is null)
					{
						element.Add(new XAttribute("isSetByUser", "true"));
					}
				}
			}
		}


		/// <summary>
		/// Gets the content value of the named meta entry on the page
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string GetMetaContent(string name)
		{
			return Root.Elements(Namespace + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == name)?
				.Attribute("content").Value;
		}


		/// <summary>
		/// Gets the specified standard quick style and ensures it's QuickStyleDef is
		/// included on the page
		/// </summary>
		/// <param name="key">A StandardStyles value</param>
		/// <returns>A Style</returns>
		public Style GetQuickStyle(StandardStyles key)
		{
			string name = key.ToName();

			var style = Root.Elements(Namespace + "QuickStyleDef")
				.Where(e => e.Attribute("name").Value == name)
				.Select(p => new Style(new QuickStyleDef(p)))
				.FirstOrDefault();

			if (style is null)
			{
				var quick = key.GetDefaults();

				var sibling = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();
				if (sibling is null)
				{
					quick.Index = 0;
					Root.AddFirst(quick.ToElement(Namespace));
				}
				else
				{
					quick.Index = Root.Elements(Namespace + "QuickStyleDef")
						.Max(e => int.Parse(e.Attribute("index").Value)) + 1;

					sibling.AddAfterSelf(quick.ToElement(Namespace));
				}

				style = new Style(quick);
			}

			return style;
		}


		/// <summary>
		/// Construct a list of possible templates from both this page's quick styles
		/// and our own custom style definitions, choosing only Heading styles, all
		/// ordered by the internal Index.
		/// </summary>
		/// <returns>A List of Styles ordered by Index</returns>
		public List<Style> GetQuickStyles()
		{
			// collect all quick style defs

			// going to reference both heading and non-headings
			var styles = Root.Elements(Namespace + "QuickStyleDef")
				.Select(p => new Style(new QuickStyleDef(p)))
				.ToList();

			// tag the headings (h1, h2, h3, ...)
			foreach (var style in styles)
			{
				if (Regex.IsMatch(style.Name, @"h\d"))
				{
					style.StyleType = StyleType.Heading;
				}
			}

			return styles;
		}


		/// <summary>
		/// Gets the quick style mappings for the current page. Used to copy or merge
		/// content on this page
		/// </summary>
		/// <returns></returns>
		public List<QuickStyleMapping> GetQuickStyleMap()
		{
			return Root.Elements(Namespace + "QuickStyleDef")
				.Select(p => new QuickStyleMapping(p))
				.ToList();
		}


		/// <summary>
		/// Gets a Color value specifying the background color of the page
		/// </summary>
		/// <returns></returns>
		public Color GetPageColor(out bool automatic, out bool black)
		{
			/*
			 * .----------------------------------------------.
			 * |      Input     |         Output              |
			 * | Office   Page  | black    color    automatic |
			 * | -------+-------+--------+--------+-----------|
			 * | light  | auto  | false  | White  | true      |
			 * | light  | color | false  | Black  | false     |
			 * | black  | color | true   | White  | false     |
			 * | black  | auto  | true   | Black  | true      |
			 * '----------------------------------------------'
			 *   office may be 'black' if using "system default" when windows is in dark mode
			 */

			black = Office.IsBlackThemeEnabled();

			var color = Root.Element(Namespace + "PageSettings").Attribute("color")?.Value;
			if (string.IsNullOrEmpty(color) || color == "automatic")
			{
				automatic = true;
				return black ? Color.Black : Color.White;
			}

			automatic = false;

			try
			{
				return ColorTranslator.FromHtml(color);
			}
			catch
			{
				return black ? Color.Black : Color.White;
			}
		}


		/// <summary>
		/// Determines the best contract color to apply to text on the page
		/// </summary>
		/// <returns></returns>
		public Color GetBestTextColor()
		{
			var back = GetPageColor(out var automatic, out _);

			if (automatic)
			{
				// yes, this works in both light and dark cases!
				return Color.Black;
			}

			var dark = back.GetBrightness() < 0.5;
			return dark ? Color.White : Color.Black;
		}


		/// <summary>
		/// Finds the index of the tag by its specified symbol
		/// </summary>
		/// <param name="symbol">The symbol of the tag to find</param>
		/// <returns>The index value or null if not found</returns>
		public string GetTagDefIndex(string symbol)
		{
			var tag = Root.Elements(Namespace + "TagDef")
				.FirstOrDefault(e => e.Attribute("symbol").Value == symbol);

			if (tag is not null)
			{
				return tag.Attribute("index").Value;
			}

			return null;
		}


		/// <summary>
		/// Finds the symbol of the tag by its given index
		/// </summary>
		/// <param name="index">The index of the tag to find</param>
		/// <returns>The symbol value or null if not found</returns>
		public string GetTagDefSymbol(string index)
		{
			var tag = Root.Elements(Namespace + "TagDef")
				.FirstOrDefault(e => e.Attribute("index").Value == index);

			if (tag is not null)
			{
				return tag.Attribute("symbol").Value;
			}

			return null;
		}


		/// <summary>
		/// Determines if the page has an active, incomplete media file which could be
		/// either video or audio.
		/// </summary>
		/// <returns>True if there is active media content</returns>
		public bool HasActiveMedia()
		{
			// there always seems to be two MediaIndex elements, one for the media file and one
			// for the citation; only when the recording is complete will the first instance be
			// accompanied by a MediaFile element, so we need to check all unique IDs

			var empty = Guid.Empty.ToString("B");

			var mediaIDs = Root
				.Descendants(Namespace + "MediaIndex")
				.Elements(Namespace + "MediaReference")
				.Attributes("mediaID")
				.Select(a => a.Value)
				.Where(a => a != empty)
				.Distinct();

			foreach (var mediaID in mediaIDs)
			{
				var file = Root.Descendants(Namespace + "MediaFile")
					.Elements(Namespace + "MediaReference")
					.FirstOrDefault(e => e.Attribute("mediaID").Value == mediaID);

				if (file is null)
				{
					// MediaFile element exists only after recording has stopped
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Determines if the page is configured for right-to-left text or the Windows
		/// language is a right-to-left language
		/// </summary>
		/// <returns></returns>
		public bool IsRightToLeft()
		{
			return
				Root.Elements(Namespace + "PageSettings")
					.Attributes().Any(a => a.Name.LocalName == "RTL" && a.Value == "true") ||
				CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
		}


		/// <summary>
		/// Merges the given quick styles from a source page with the quick styles on the
		/// current page, adjusting index values to avoid collisions with pre-existing styles
		/// </summary>
		/// <param name="quickmap">
		/// The quick style mappings from the source page to merge into this page. The index
		/// values of the Style property are updated for each quick style
		/// </param>
		public List<QuickStyleMapping> MergeQuickStyles(Page sourcePage)
		{
			var sourcemap = sourcePage.GetQuickStyleMap();
			var map = GetQuickStyleMap();

			var index = map.Max(q => q.Style.Index) + 1;

			foreach (var source in sourcemap)
			{
				var quick = map.Find(q => q.Style.Equals(source.Style));
				if (quick is null)
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					source.Style.Index = index++;

					source.Element = new XElement(source.Element);
					source.Element.Attribute("index").Value = source.Style.Index.ToString();

					map.Add(source);
					AddQuickStyleDef(source.Element);
				}

				// else if found then the index may differ but keep it so it can be mapped
				// to content later...
			}

			return map;
		}


		/// <summary>
		/// Adds a Meta element to the page (in the proper schema sequence) with the
		/// specified name and value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetMeta(string name, string value)
		{
			var meta = Root.Elements(Namespace + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == name);

			if (meta is null)
			{
				meta = new XElement(Namespace + "Meta",
					new XAttribute("name", name),
					new XAttribute("content", value)
					);

				// add into schema sequence...
				var after = Root.Elements(Namespace + "XPSFile").LastOrDefault();
				if (after is null)
				{
					after = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();
					after ??= Root.Elements(Namespace + "TagDef").LastOrDefault();
				}

				if (after is null)
				{
					Root.AddFirst(meta);
				}
				else
				{
					after.AddAfterSelf(meta);
				}
			}
			else
			{
				meta.Attribute("content").Value = value;
			}
		}


		/// <summary>
		/// Set the title of the current page or adds a new title if one doesn't already exist
		/// </summary>
		/// <param name="title">The title to use</param>
		public void SetTitle(string title)
		{
			var ns = Namespace;
			PageNamespace.Set(ns);

			var block = Root.Elements(ns + "Title").FirstOrDefault();
			if (block is null)
			{
				var style = GetQuickStyle(StandardStyles.PageTitle);
				block = new XElement(ns + "Title",
					new Paragraph(title).SetQuickStyle(style.Index));

				var anchor = Root.Elements(ns + "PageSettings").FirstOrDefault();
				if (anchor is not null)
				{
					anchor.AddAfterSelf(block);
				}
				else
				{
					anchor = Root.Elements(ns + "Outline").FirstOrDefault();
					anchor ??= EnsureContentContainer();
					anchor.AddBeforeSelf(block);
				}
			}
			else
			{
				// overwrite the entire title
				block.Elements(ns + "OE").FirstOrDefault()?
					.ReplaceNodes(new XElement(ns + "T", new XCData(title)));
			}
		}
	}
}
