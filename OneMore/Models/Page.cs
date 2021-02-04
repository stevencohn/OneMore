//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

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
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a page with helper methods
	/// </summary>
	internal partial class Page
	{

		// Page meta to keep track of rotating highlighter index
		public static readonly string HighlightMetaName = "omHighlightIndex";
		// Page is a reference map, so don't include it in subsequent maps
		public static readonly string PageMapMetaName = "omPageMap";
		// Outline meta to mark visible word bank
		public static readonly string TagBankMetaName = "omTaggingBank";
		// Page meta to specify page tag list
		public static readonly string TaggingMetaName = "omTaggingLabels";

		private bool reverseScanning;


		/// <summary>
		/// Initialize a new instance with the given page
		/// </summary>
		/// <param name="page"></param>
		public Page(XElement page)
		{
			if (page != null)
			{
				Root = page;
				Namespace = page.GetNamespaceOfPrefix(OneNote.Prefix);

				PageId = page.Attribute("ID")?.Value;
			}
		}


		public bool IsValid => Root != null;


		public string PageId { get; private set; }


		/// <summary>
		/// Gets the root element of the page
		/// </summary>
		public XElement Root { get; private set; }


		/// <summary>
		/// Gest the namespace used to create new elements for the page
		/// </summary>
		public XNamespace Namespace { get; private set; }


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

				return titles == null ? null : string.Concat(titles);
			}

			set
			{
				// overwrite the entire title
				var title = Root.Elements(Namespace + "Title")
					.Elements(Namespace + "OE").FirstOrDefault();

				if (title != null)
				{
					title.ReplaceNodes(new XElement(Namespace + "T", new XCData(value)));
				}
			}
		}


		/// <summary>
		/// Appends content to the current page
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public XElement AddContent(IEnumerable<XElement> content)
		{
			var container = EnsureContentContainer();
			container.Add(content);
			return container;
		}


		/// <summary>
		/// Appends HTML content to the current page
		/// </summary>
		/// <param name="html"></param>
		public void AddHtmlContent(string html)
		{
			var container = EnsureContentContainer();

			container.Add(new XElement(Namespace + "HTMLBlock",
				new XElement(Namespace + "Data", new XCData(html))
				));
		}


		/// <summary>
		/// Adds the given content after the selected insertion point; this will not
		/// replace selected regions.
		/// </summary>
		/// <param name="content">The content to add</param>
		public void AddNextParagraph(XElement content)
		{
			var current = Root.Descendants(Namespace + "OE")
				.LastOrDefault(e => e.Elements(Namespace + "T").Attributes("selected").Any(a => a.Value == "all"));

			if (current != null)
			{
				if (content.Name.LocalName != "OE")
				{
					content = new XElement(Namespace + "OE", content);
				}

				current.AddAfterSelf(content);
			}
		}


		/// <summary>
		/// Adds the given QuickStyleDef element in the proper document order, just after
		/// the TagDef elements if there are any
		/// </summary>
		/// <param name="def"></param>
		public void AddQuickStyleDef(XElement def)
		{
			var tagdef = Root.Elements(Namespace + "TagDef").LastOrDefault();
			if (tagdef == null)
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
			//<one:TagDef index="0" type="0" symbol="140" fontColor="automatic" highlightColor="none" name="Calculated" />
			var tags = Root.Elements(Namespace + "TagDef");

			int index = 0;
			if (tags?.Any() == true)
			{
				var tag = tags.FirstOrDefault(e => e.Attribute("symbol").Value == symbol);
				if (tag != null)
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


		/// <summary>
		/// Apply the given quick style mappings to all descendents of the specified outline.
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="outline"></param>
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

					if (elements?.Any() == true)
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
		/// Invokes an edit function on the selected text. The selection may be either infered
		/// from the current cursor position or explicitly highlighted as a selected region.
		/// No assumptions are made as to the resultant content or the order in which parts of
		/// context are edited.
		/// </summary>
		/// <param name="edit">
		/// A Func that accepts an XNode and returns an XNode. The accepted XNode is either an
		/// XText or a "span" XElement. The returned XNode can be either the original unmodified,
		/// the original modified, or a new XNode. Regardless, the returned XNode will replace
		/// the current XNode in the content.
		/// </param>
		/// <returns></returns>
		public bool EditSelected(Func<XNode, XNode> edit)
		{
			var updated = false;
			var cursor = GetTextCursor();

			if (cursor != null)
			{
				// T elements can only be a child of an OE but can also have other T siblings...
				// Each T will have one CDATA with one or more XText and SPAN XElements.
				// OneNote handles nested spans by normalizing them into consecutive spans

				// Just FYI, because we use XElement.Parse to build the DOM, XText nodes will be
				// singular but may be surrounded by SPAN elements; i.e., there shouldn't be two
				// consecutive XText nodes next to each other

				// indicate to GetSelected() that we're scanning in reverse
				reverseScanning = true;

				// is there a preceding T?
				if ((cursor.PreviousNode is XElement prev) && !prev.GetCData().EndsWithWhitespace())
				{
					var cdata = prev.GetCData();
					var wrapper = cdata.GetWrapper();
					var nodes = wrapper.Nodes().ToList();

					// reverse, extracting text and stopping when matching word delimiter
					for (var i = nodes.Count - 1; i >= 0; i--)
					{
						if (nodes[i] is XText text)
						{
							// ends with delimiter so can't be part of current word
							if (text.Value.EndsWithWhitespace())
								break;

							// extract last word and pump through the editor
							var pair = text.Value.ExtractLastWord();
							if (pair.Item1 == null)
							{
								// entire content of this XText
								edit(text);
							}
							else
							{
								// last word of this XText
								text.Value = pair.Item2;
								text.AddAfterSelf(edit(new XText(pair.Item1)));
							}

							// remaining text has a word delimiter
							if (text.Value.StartsWithWhitespace())
								break;
						}
						else if (nodes[i] is XElement span)
						{
							// ends with delimiter so can't be part of current word
							if (span.Value.EndsWithWhitespace())
								break;

							// extract last word and pump through editor
							var word = span.ExtractLastWord();
							if (word == null)
							{
								// edit entire contents of SPAN
								edit(span);
							}
							else
							{
								// last word of this SPAN
								var spawn = new XElement(span.Name, span.Attributes(), word);
								edit(spawn);
								span.AddAfterSelf(spawn);
							}

							// remaining text has a word delimiter
							if (span.Value.StartsWithWhitespace())
								break;
						}
					}

					// rebuild CDATA with edited content
					cdata.Value = wrapper.GetInnerXml();
					updated = true;
				}

				// indicate to GetSelected() that we're scanning forward
				reverseScanning = false;

				// is there a following T?
				if ((cursor.NextNode is XElement next) && !next.GetCData().StartsWithWhitespace())
				{
					var cdata = next.GetCData();
					var wrapper = cdata.GetWrapper();
					var nodes = wrapper.Nodes().ToList();

					// extract text and stop when matching word delimiter
					for (var i = 0; i < nodes.Count; i++)
					{
						if (nodes[i] is XText text)
						{
							// starts with delimiter so can't be part of current word
							if (text.Value.StartsWithWhitespace())
								break;

							// extract first word and pump through editor
							var pair = text.Value.ExtractFirstWord();
							if (pair.Item1 == null)
							{
								// entire content of this XText
								edit(text);
							}
							else
							{
								// first word of this XText
								text.Value = pair.Item2;
								text.AddBeforeSelf(edit(new XText(pair.Item1)));
							}

							// remaining text has a word delimiter
							if (text.Value.EndsWithWhitespace())
								break;
						}
						else if (nodes[i] is XElement span)
						{
							// ends with delimiter so can't be part of current word
							if (span.Value.StartsWithWhitespace())
								break;

							// extract first word and pump through editor
							var word = span.ExtractFirstWord();
							if (word == null)
							{
								// eidt entire contents of SPAN
								edit(span);
							}
							else
							{
								// first word of this SPAN
								var spawn = new XElement(span.Name, span.Attributes(), word);
								edit(spawn);
								span.AddBeforeSelf(spawn);
							}

							// remaining text has a word delimiter
							if (span.Value.EndsWithWhitespace())
								break;
						}
					}

					// rebuild CDATA with edited content
					cdata.Value = wrapper.GetInnerXml();
					updated = true;
				}
			}
			else
			{
				// detect all selected text (cdata within T runs)
				var cdatas = Root.Descendants(Namespace + "T")
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all")
						&& e.FirstNode?.NodeType == XmlNodeType.CDATA)
					.Select(e => e.FirstNode as XCData);

				if (cdatas?.Any() == true)
				{
					foreach (var cdata in cdatas)
					{
						// edit every XText and SPAN in the T wrapper
						var wrapper = cdata.GetWrapper();

						// use ToList, otherwise enumeration will stop after first FeplaceWith
						foreach (var node in wrapper.Nodes().ToList())
						{
							node.ReplaceWith(edit(node));
						}

						var text = wrapper.GetInnerXml();

						// special case for <br> + EOL
						text = text.Replace("<br>", "<br>\n");

						// build CDATA with editing content
						cdata.Value = text;
					}

					updated = true;
				}
			}

			return updated;
		}


		/// <summary>
		/// Ensures the page contains at least one OEChildren elements and returns it
		/// </summary>
		public XElement EnsureContentContainer()
		{
			XElement container;
			var outline = Root.Elements(Namespace + "Outline").LastOrDefault();
			if (outline == null)
			{
				container = new XElement(Namespace + "OEChildren");
				Root.Add(new XElement(Namespace + "Outline", container));
			}
			else
			{
				container = outline.Elements(Namespace + "OEChildren").LastOrDefault();
				if (container == null)
				{
					container = new XElement(Namespace + "OEChildren");
					outline.Add(container);
				}
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

			if (element != null)
			{
				var attr = element.Attribute("width");
				if (attr != null)
				{
					var outlinePoints = double.Parse(attr.Value, CultureInfo.InvariantCulture);

					// measure line to ensure page width is sufficient

					using (var g = Graphics.FromHwnd(handle))
					{
						using (var font = new Font(fontFamily, fontSize))
						{
							var stringSize = g.MeasureString(line, font);
							var stringPoints = stringSize.Width * 72 / g.DpiX;

							if (stringPoints > outlinePoints)
							{
								attr.Value = stringPoints.ToString("#0.00", CultureInfo.InvariantCulture);

								// must include isSetByUser or width doesn't take effect!
								if (element.Attribute("isSetByUser") == null)
								{
									element.Add(new XAttribute("isSetByUser", "true"));
								}
							}
						}
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
		/// Gets the currently selected text. If the text cursor is positioned over a word but
		/// with zero selection length then that word is returned; othewise, text from the selected
		/// region is returned.
		/// </summary>
		/// <returns>A string of the selected text</returns>
		public string GetSelectedText()
		{
			var builder = new StringBuilder();

			// not editing... just using EditSelected to extract the current text,
			// ignoring inline span styling

			EditSelected((s) =>
			{
				if (s is XText text)
				{
					if (reverseScanning)
						builder.Insert(0, text.Value);
					else
						builder.Append(text.Value);
				}
				else
				{
					if (reverseScanning)
						builder.Insert(0, ((XElement)s).Value);
					else
						builder.Append(((XElement)s).Value);
				}

				return s;
			});

			return builder.ToString();
		}


		/// <summary>
		/// Gets the T element of a zero-width selection. Visually, this appears as the current
		/// cursor insertion point and can be used to infer the current word or phrase in text.
		/// </summary>
		/// <returns>
		/// The one:T XElement or null if there is a selected range greater than zero
		/// </returns>
		public XElement GetTextCursor()
		{
			var selected = Root.Elements(Namespace + "Outline")
				.Descendants(Namespace + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

			if (selected.Any() && selected.Count() == 1)
			{
				var cursor = selected.First();
				if (cursor.FirstNode.NodeType == XmlNodeType.CDATA)
				{
					var cdata = cursor.FirstNode as XCData;

					// empty or link because we can't tell the difference between a zero-selection
					// zero-selection link and link a partial or fully selected link
					if (cdata.Value.Length == 0 || Regex.IsMatch(cdata.Value, @"<a href.+?</a>"))
					{
						return cursor;
					}
				}
			}

			// zero or more than one empty cdata are selected
			return null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Style GetQuickStyle(StandardStyles key)
		{
			string name = key.ToName();

			var style = Root.Elements(Namespace + "QuickStyleDef")
				.Where(e => e.Attribute("name").Value == name)
				.Select(p => new Style(new QuickStyleDef(p)))
				.FirstOrDefault();

			if (style == null)
			{
				var quick = key.GetDefaults();

				var sibling = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();
				if (sibling == null)
				{
					quick.Index = 0;
					Root.AddFirst(quick.ToElement(Namespace));
				}
				else
				{
					quick.Index = int.Parse(sibling.Attribute("index").Value) + 1;
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
		/// copy on this page
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
			black = Office.IsBlackThemeEnabled();

			var color = Root.Element(Namespace + "PageSettings").Attribute("color")?.Value;
			if (string.IsNullOrEmpty(color) || color == "automatic")
			{
				automatic = true;
				return Color.White;
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
		/// Finds the index of the tag by its specified symbol
		/// </summary>
		/// <param name="symbol">The symbol of the tag to find</param>
		/// <returns>The index value or null if not found</returns>
		public string GetTagDefIndex(string symbol)
		{
			var tag = Root.Elements(Namespace + "TagDef")
				.FirstOrDefault(e => e.Attribute("symbol").Value == symbol);

			if (tag != null)
			{
				return tag.Attribute("index").Value;
			}

			return null;
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
				if (quick == null)
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
		/// Replaces the selected range on the page with the given content, keeping
		/// the cursor after the newly inserted content.
		/// <para>
		/// This attempts to replicate what Word might do when pasting content in a
		/// document with a selection range.
		/// </para>
		/// </summary>
		/// <param name="page">The page root node</param>
		/// <param name="content">The content to insert</param>
		public void ReplaceSelectedWithContent(XElement content)
		{
			var elements = Root.Descendants(Namespace + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if ((elements.Count() == 1) &&
				(elements.First().GetCData().Value.Length == 0))
			{
				// zero-width selection so insert just before cursor
				elements.First().AddBeforeSelf(content);
			}
			else
			{
				// replace one or more [one:T @select=all] with status, placing cursor after
				var element = elements.Last();
				element.AddAfterSelf(content);
				elements.Remove();

				content.AddAfterSelf(new XElement(Namespace + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)
					));
			}
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

			if (meta == null)
			{
				meta = new XElement(Namespace + "Meta",
					new XAttribute("name", name),
					new XAttribute("content", value)
					);

				// add into schema sequence...
				var after = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();

				if (after == null)
				{
					after = Root.Elements(Namespace + "TagDef").LastOrDefault();
				}

				if (after == null)
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
	}
}
