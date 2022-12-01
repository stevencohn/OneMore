//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1155 // "Any()" should be used to test for emptiness
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
#pragma warning disable S4136 // Method overloads should be grouped together

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
		//// Page meta to indicate data storage analysis report
		//public static readonly string AnalysisMetaName = "omAnalysisReport";
		//// Page meta to keep track of rotating highlighter index
		//public static readonly string HighlightMetaName = "omHighlightIndex";
		//// Page is reference linked to another page, so don't include it in subsequent links
		//public static readonly string LinkReferenceMetaName = "omLinkReference";
		//// Page is a reference map, so don't include it in subsequent maps
		//public static readonly string PageMapMetaName = "omPageMap";
		//// Outline meta to mark visible word bank
		//public static readonly string TagBankMetaName = "omTaggingBank";
		//// Page meta to specify page tag list
		//public static readonly string TaggingMetaName = "omTaggingLabels";


		/// <summary>
		/// Initialize a new instance with the given page XML root
		/// </summary>
		/// <param name="root"></param>
		public Page(XElement root)
		{
			if (root != null)
			{
				Root = root;
				Namespace = root.GetNamespaceOfPrefix(OneNote.Prefix);

				PageId = root.Attribute("ID")?.Value;
			}

			SelectionScope = SelectionScope.Unknown;
		}


		public bool IsValid => Root != null;


		/// <summary>
		/// Gest the namespace used to create new elements for the page
		/// </summary>
		public XNamespace Namespace { get; private set; }


		/// <summary>
		/// Gets the unique ID of the page
		/// </summary>
		public string PageId { get; private set; }


		/// <summary>
		/// Used as a signal to GetSelectedText that editor scanning is in reverse
		/// </summary>
		public bool ReverseScanning { get; set; }


		/// <summary>
		/// Gets the root element of the page
		/// </summary>
		public XElement Root { get; private set; }


		/// <summary>
		/// Gets the most recently known selection state where none means unknown, all
		/// means there is an obvious selection region, and partial means the selection
		/// region is zero.
		/// </summary>
		public SelectionScope SelectionScope { get; private set; }


		/// <summary>
		/// Gets an indication that the text cursor is positioned over either a hyperlink
		/// or a MathML equation, both of which return zero-length selection ranges.
		/// </summary>
		public bool SelectionSpecial { get; private set; }


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

				return titles == null ? null : string.Concat(titles);
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
			InsertParagraph(content, false);
		}


		public void AddNextParagraph(params XElement[] content)
		{
			// consumer will build content array in document-order but InsertParagraph inserts
			// just prior to the insertion point which will reverse the order of content items
			// so insert them in reverse order intentionally so they show up correctly
			for (var i = content.Length - 1; i >= 0; i--)
			{
				InsertParagraph(content[i], false);
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


		public void AddTagDef(TagDef tagdef)
		{
			var tags = Root.Elements(Namespace + "TagDef");
			if (tags?.Any() == true)
			{
				var tag = tags.FirstOrDefault(e => e.Attribute("symbol").Value == tagdef.Symbol);
				if (tag != null)
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
		/// Apply the given tagdef mappings to all descendants of the specified outline
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="outline"></param>
		public void ApplyTagDefMapping(List<TagDefMapping> mapping, XElement outline)
		{
			// reverse sort the indexes so logic doesn't overwrite subsequent index references
			foreach (var map in mapping.OrderByDescending(s => s.TagDef.IndexValue))
			{
				if (map.OriginalIndex != map.TagDef.Index)
				{
					// apply new index to child outline elements
					var elements = outline.Descendants(Namespace + "Tag")
						.Where(e => e.Attribute("index")?.Value == map.OriginalIndex);

					if (elements?.Any() == true)
					{
						foreach (var element in elements)
						{
							element.Attribute("index").Value = map.TagDef.Index;
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
			var cursor = GetTextCursor(true);
			if (cursor != null)
			{
				return EditNode(cursor, edit);
			}

			return EditSelected(Root, edit);
		}


		public bool EditNode(XElement cursor, Func<XNode, XNode> edit)
		{
			var updated = false;

			// T elements can only be a child of an OE but can also have other T siblings...
			// Each T will have one CDATA with one or more XText and SPAN XElements.
			// OneNote handles nested spans by normalizing them into consecutive spans

			// Just FYI, because we use XElement.Parse to build the DOM, XText nodes will be
			// singular but may be surrounded by SPAN elements; i.e., there shouldn't be two
			// consecutive XText nodes next to each other

			// indicate to GetSelectedText() that we're scanning in reverse
			ReverseScanning = true;

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
						var pair = text.Value.SplitAtLastWord();
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

			// indicate to GetSelectedText() that we're scanning forward
			ReverseScanning = false;

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
						var pair = text.Value.SplitAtFirstWord();
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

			return updated;
		}


		public bool EditSelected(XElement root, Func<XNode, XNode> edit)
		{
			// detect all selected text (cdata within T runs)
			var cdatas = root.Descendants(Namespace + "T")
				.Where(e => e.Attributes("selected").Any(a => a.Value == "all")
					&& e.FirstNode?.NodeType == XmlNodeType.CDATA)
				.Select(e => e.FirstNode as XCData);

			if (cdatas?.Any() != true)
			{
				return false;
			}

			foreach (var cdata in cdatas)
			{
				// edit every XText and SPAN in the T wrapper
				var wrapper = cdata.GetWrapper();

				// use ToList, otherwise enumeration will stop after first ReplaceWith
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

			return true;
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
				outline = new XElement(Namespace + "Outline", container);
				Root.Add(outline);
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

			// check Outline size
			var size = outline.Elements(Namespace + "Size").FirstOrDefault();
			if (size == null)
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
				.LastOrDefault()
				.Elements(Namespace + "Size")
				.FirstOrDefault();

			if (element == null)
			{
				return;
			}

			var attr = element.Attribute("width");
			if (attr != null)
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
					if (element.Attribute("isSetByUser") == null)
					{
						element.Add(new XAttribute("isSetByUser", "true"));
					}
				}
			}
		}


		/// <summary>
		/// Extract the selected content on the current page, wrapped in its own
		/// OEChildren container
		/// </summary>
		/// <param name="firstParent">
		/// The cloest element preceding the selected content; use this as a reference
		/// when inserting replacement content.
		/// </param>
		/// <returns>A new OEChildren containing the selected content</returns>
		public XElement ExtractSelectedContent(out XElement firstParent)
		{
			var content = new XElement(Namespace + "OEChildren");
			firstParent = null;

			var runs = Root.Elements(Namespace + "Outline")
				.Descendants(Namespace + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.ToList();

			if (!runs.Any())
			{
				runs = Root.Elements(Namespace + "Outline")
					.Descendants(Namespace + "Image")
					.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
					.ToList();
			}

			if (runs.Any())
			{
				// content will eventually be added after the first parent
				firstParent = runs[0].Parent;

				// if text is in the middle of a soft-break block then need to split the block
				// into two so the code box can be inserted, maintaining its relative position
				if (runs[runs.Count - 1].NextNode != null)
				{
					var nextNodes = runs[runs.Count - 1].NodesAfterSelf().ToList();
					nextNodes.Remove();

					firstParent.AddAfterSelf(new XElement(Namespace + "OE",
						firstParent.Attributes(),
						nextNodes
						));
				}

				// collect the content
				foreach (var run in runs)
				{
					// new OE for run
					var oe = new XElement(Namespace + "OE", run.Parent.Attributes());

					// remove run from current parent
					run.Remove();

					// add run into new OE parent
					oe.Add(run);

					// add new OE to content
					content.Add(oe);
				}
			}

			return content;
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
		/// Gets a collection of fully selected text runs
		/// </summary>
		/// <param name="all">
		/// If no selected elements are found and this value is true then return all elements;
		/// otherwise if no selection and this value is false then return an empty collection.
		/// Default value is true.
		/// </param>
		/// <returns>
		/// A collection of fully selected text runs. The collection will be empty if the
		/// selected range is zero characters or one of the known special cases
		/// </returns>
		public IEnumerable<XElement> GetSelectedElements(bool all = true)
		{
			var selected = Root.Elements(Namespace + "Outline")
				.Where(e => !e.Elements(Namespace + "Meta")
					.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)))
				.Descendants(Namespace + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

			if (selected == null || selected.Count() == 0)
			{
				SelectionScope = SelectionScope.Unknown;

				return all
					? Root.Elements(Namespace + "Outline").Descendants(Namespace + "T")
					: new List<XElement>();
			}

			// if exactly one then it could be an empty [] or it could be a special case
			if (selected.Count() == 1)
			{
				var cursor = selected.First();
				if (cursor.FirstNode.NodeType == XmlNodeType.CDATA)
				{
					var cdata = cursor.FirstNode as XCData;

					// empty or link or xml-comment because we can't tell the difference between
					// a zero-selection zero-selection link and a partial or fully selected link.
					// Note that XML comments are used to wrap mathML equations
					if (cdata.Value.Length == 0 ||
						Regex.IsMatch(cdata.Value, @"<a\s+href.+?</a>", RegexOptions.Singleline) ||
						Regex.IsMatch(cdata.Value, @"<!--.+?-->", RegexOptions.Singleline))
					{
						SelectionScope = SelectionScope.Empty;

						return all
							? Root.Elements(Namespace + "Outline").Descendants(Namespace + "T")
							: new List<XElement>();
					}
				}
			}

			SelectionScope = SelectionScope.Region;

			// return zero or more elements
			return selected;
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
					if (ReverseScanning)
						builder.Insert(0, text.Value);
					else
						builder.Append(text.Value);
				}
				else if (s is not XComment)
				{
					if (ReverseScanning)
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
		/// <param name="allowPageTitle">
		/// True to include the page title, otherwise just the body of the page
		/// </param>
		/// <returns>
		/// Returns the one:T XElement of an empty cursor and Page.SelectionScope
		/// is set to Empty or Special if the cursor is on a hyperlink or mathML equation
		/// and sets SelectionSpecial to true if the selection range is not empty.
		/// Returns null if there is a selected range greater than zero and sets
		/// the SelectionScope to Region.
		/// </returns>
		public XElement GetTextCursor(bool allowPageTitle = false)
		{
			var root = allowPageTitle ? Root.Elements() : Root.Elements(Namespace + "Outline");

			var selected = root.Descendants(Namespace + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

			if (!selected.Any())
			{
				SelectionScope = SelectionScope.Empty;
				return null;
			}

			var count = selected.Count();
			if (count == 1)
			{
				var cursor = selected.First();
				var cdata = cursor.GetCData();

				// empty, link, or xml-comment because we can't tell the difference between
				// a zero-selection link and a partial or fully selected link. Note that XML
				// comments are used to wrap mathML equations
				if (cdata.Value.Length == 0 ||
					Regex.IsMatch(cdata.Value, @"<a\s+href.+?</a>", RegexOptions.Singleline) ||
					Regex.IsMatch(cdata.Value, @"<!--.+?-->", RegexOptions.Singleline))
				{
					SelectionScope = SelectionScope.Empty;
					SelectionSpecial = cdata.Value.Length > 0;
					return cursor;
				}
			}

			SelectionScope = SelectionScope.Region;
			return null;
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
		/// Finds the symbol of the tag by its given index
		/// </summary>
		/// <param name="index">The index of the tag to find</param>
		/// <returns>The symbol value or null if not found</returns>
		public string GetTagDefSymbol(string index)
		{
			var tag = Root.Elements(Namespace + "TagDef")
				.FirstOrDefault(e => e.Attribute("index").Value == index);

			if (tag != null)
			{
				return tag.Attribute("symbol").Value;
			}

			return null;
		}


		/// <summary>
		/// Gets the TagDef mappings for the current page. Used to copy or merge
		/// content on this page
		/// </summary>
		/// <returns></returns>
		public List<TagDefMapping> GetTagDefMap()
		{
			return Root.Elements(Namespace + "TagDef")
				.Select(e => new TagDefMapping(e))
				.ToList();
		}


		/// <summary>
		/// Adds the given content immediately before or after the selected insertion point;
		/// this will not replace selected regions.
		/// </summary>
		/// <param name="content">The content to insert</param>
		/// <param name="before">
		/// If true then insert before the insertion point; otherwise insert after the insertion point
		/// </param>
		public void InsertParagraph(XElement content, bool before = true)
		{
			var current = Root.Descendants(Namespace + "OE").LastOrDefault(e =>
				e.Elements(Namespace + "T").Attributes("selected").Any(a => a.Value == "all"));

			if (current != null)
			{
				if (content.Name.LocalName != "OE")
				{
					content = new XElement(Namespace + "OE", content);
				}

				if (before)
					current.AddBeforeSelf(content);
				else
					current.AddAfterSelf(content);
			}
		}


		public void InsertParagraph(params XElement[] content)
		{
			foreach (var e in content)
			{
				InsertParagraph(e, false);
			}
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
		/// Merges the TagDefs from a source page with the TagDefs on the current page,
		/// adjusting index values to avoid collisions with pre-existing definitions
		/// </summary>
		/// <param name="sourcePage">
		/// The page from which to copy TagDefs into this page. The value of the index
		/// attribute of the TagDefs are updated for each definition
		/// </param>
		public List<TagDefMapping> MergeTagDefs(Page sourcePage)
		{
			var sourcemap = sourcePage.GetTagDefMap();
			var map = GetTagDefMap();

			var index = map.Any() ? map.Max(t => t.TagDef.IndexValue) + 1 : 0;

			foreach (var source in sourcemap)
			{
				var tagdef = map.Find(t => t.TagDef.Equals(source.TagDef));
				if (tagdef == null)
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					source.TagDef.IndexValue = index++;

					source.Element = new XElement(source.Element);
					source.Element.Attribute("index").Value = source.TagDef.Index;

					map.Add(source);
					AddTagDef(source.TagDef);
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

				SelectionScope = SelectionScope.Region;
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
				var after = Root.Elements(Namespace + "XPSFile").LastOrDefault();
				if (after == null)
				{
					after = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();
					after ??= Root.Elements(Namespace + "TagDef").LastOrDefault();
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
