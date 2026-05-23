//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:OE (Outline Element) — the fundamental paragraph node. An OE contains
	/// either a sequence of text runs (T elements), a single embedded object (Table or Image),
	/// and optionally nested OEChildren for indented sub-items.
	/// </summary>
	internal sealed class OENode : OneNoteNode
	{
		internal OENode(XElement el) : base(el) { }


		/// <summary>Creates a new OE element containing a single plain-text run.</summary>
		public static OENode Create(string text = "")
			=> new OENode(E("OE", E("T", new XCData(text ?? string.Empty))));


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Formatting

		/// <summary>
		/// Quick style index referencing a QuickStyleDef on the page. Null = not set (uses default).
		/// </summary>
		public int? QuickStyleIndex
		{
			get => AttrInt("quickStyleIndex");
			set => AttrInt("quickStyleIndex", value);
		}


		/// <summary>Inline CSS-like style string on the OE element (paragraph-level override).</summary>
		public string InlineStyle
		{
			get => Attr("style");
			set => Attr("style", value);
		}


		public StyleString Style
		{
			get => StyleString.Parse(Attr("style"));
			set => Attr("style", value?.IsEmpty == false ? value.ToString() : null);
		}


		public string Lang
		{
			get => Attr("lang");
			set => Attr("lang", value);
		}


		/// <summary>Horizontal alignment: "left", "center", "right", or "justify".</summary>
		public string Alignment
		{
			get => Attr("alignment") ?? "left";
			set => Attr("alignment", value);
		}


		public bool RTL
		{
			get => AttrBool("RTL") == true;
			set => AttrBool("RTL", value);
		}


		/// <summary>
		/// True when this paragraph is body text (not a heading). OneNote sets this on
		/// paragraphs under a collapsed heading to keep them hidden when collapsed.
		/// </summary>
		public bool BodyText
		{
			get => AttrBool("bodyText") == true;
			set => AttrBool("bodyText", value);
		}


		/// <summary>True when this heading paragraph is collapsed (sub-items hidden).</summary>
		public bool Collapsed
		{
			get => AttrBool("collapsed") == true;
			set => AttrBool("collapsed", value);
		}


		/// <summary>
		/// True when the body text under this collapsed heading has content. OneNote uses
		/// this to show/hide the expand chevron without loading the full sub-tree.
		/// </summary>
		public bool CollapsedBodyText
		{
			get => AttrBool("collapsedBodyText") == true;
			set => AttrBool("collapsedBodyText", value);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Text content

		/// <summary>All T text-run children, in document order.</summary>
		public IReadOnlyList<TextRun> Runs
			=> el.Elements(NS + "T")
				.Select(e => new TextRun(e))
				.ToList();


		/// <summary>Concatenated plain text of all runs.</summary>
		public string PlainText
		{
			get
			{
				var sb = new StringBuilder();
				foreach (var run in Runs)
					sb.Append(run.PlainText);
				return sb.ToString();
			}
			set => SetPlainText(value);
		}


		/// <summary>Replaces all text runs with a single run containing the given text.</summary>
		public void SetPlainText(string text)
		{
			// remove existing T elements
			el.Elements(NS + "T").Remove();
			el.Add(E("T", new XCData(text ?? string.Empty)));
		}


		/// <summary>Removes all text runs.</summary>
		public void ClearRuns() => el.Elements(NS + "T").Remove();


		/// <summary>
		/// Appends a new text run and returns it. Use the style action to configure formatting.
		/// </summary>
		public TextRun AppendRun(string text, Action<StyleString> configure = null)
		{
			StyleString style = null;
			if (configure is not null)
			{
				style = StyleString.Empty;
				configure(style);
			}
			var run = TextRun.Create(text, style);
			el.Add(run.Element);
			return run;
		}


		/// <summary>
		/// Returns formatted spans with cascade applied (QuickStyleDef + OE style + T style).
		/// Use this for read-only analysis of character formatting. Fragment reassembly is
		/// handled by merging each run's style on top of the OE's resolved style.
		/// </summary>
		public IReadOnlyList<FormattedSpan> FormattedSpans(QuickStyleIndex quickStyles = null)
		{
			var baseStyle = quickStyles is not null && QuickStyleIndex is int qi
				? StyleString.Merge(quickStyles.GetStyle(qi), Style)
				: Style;

			return Runs
				.Select(r => new FormattedSpan(r.PlainText, StyleString.Merge(baseStyle, r.Style)))
				.ToList();
		}


		/// <summary>Applies a style mutation to all runs uniformly.</summary>
		public void SetStyle(Action<StyleString> mutate)
		{
			foreach (var run in Runs)
			{
				var s = run.Style;
				mutate(s);
				run.Style = s;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Selection

		/// <summary>
		/// Selection state of this paragraph. None = not touched by selection,
		/// Partial = selection passes through (some T children are selected="all"),
		/// All = entire OE is selected.
		/// </summary>
		public SelectionScope SelectionScope
		{
			get
			{
				var v = Attr("selected");
				return v == "all" ? SelectionScope.All
					: v == "partial" ? SelectionScope.Partial
					: SelectionScope.None;
			}
		}


		/// <summary>T runs with selected="all" within this paragraph.</summary>
		public IEnumerable<TextRun> SelectedRuns
			=> Runs.Where(r => r.IsSelected);


		/// <summary>
		/// True when the selection is a zero-length cursor (one selected run, empty CDATA).
		/// This is distinct from IsSelectionSpecial where the run is non-empty but contains HTML.
		/// </summary>
		public bool IsCursorPosition
		{
			get
			{
				var selected = SelectedRuns.ToList();
				if (selected.Count != 1) return false;
				var raw = selected[0].RawContent ?? string.Empty;
				return raw.Length == 0;
			}
		}


		/// <summary>
		/// True when a zero-length cursor is positioned inside a hyperlink or MathML equation.
		/// OneNote marks the entire link/equation run as selected="all" even though no user-
		/// visible text is selected. The CDATA contains &lt;a href&gt; (hyperlink) or a
		/// comment node &lt;!-- (MathML).
		/// </summary>
		public bool IsSelectionSpecial
		{
			get
			{
				var selected = SelectedRuns.ToList();
				if (selected.Count != 1) return false;
				var raw = selected[0].RawContent ?? string.Empty;
				return raw.Contains("<a ") || raw.Contains("<!--");
			}
		}


		/// <summary>
		/// Returns the plain text of all selected runs, or an empty string when the
		/// selection is a cursor position or cursor-on-special (hyperlink / MathML).
		/// </summary>
		public string GetSelectedText()
		{
			if (SelectionScope == SelectionScope.None
				|| IsCursorPosition
				|| IsSelectionSpecial)
				return string.Empty;

			return string.Concat(SelectedRuns.Select(r => r.PlainText));
		}


		/// <summary>
		/// Applies a text transform to the current selection.
		///
		/// Cursor mode (IsCursorPosition == true): expands to the word surrounding the cursor,
		/// even when the word spans adjacent runs with different styles, and applies the
		/// transform to the combined word. Mirrors Word's "apply to whole word" behaviour.
		///
		/// Selection mode: applies the transform independently to the plain text of each
		/// selected run, preserving each run's style attributes.
		///
		/// No-op when SelectionScope is None or IsSelectionSpecial is true.
		/// </summary>
		public void EditSelected(Func<string, string> transform)
		{
			if (SelectionScope == SelectionScope.None || IsSelectionSpecial) return;

			if (IsCursorPosition)
				EditWordAtCursor(transform);
			else
				foreach (var run in SelectedRuns.ToList())
					run.PlainText = transform(run.PlainText);
		}


		private void EditWordAtCursor(Func<string, string> transform)
		{
			var runEls = el.Elements(NS + "T").ToList();
			int cursorIdx = runEls.FindIndex(e => e.Attribute("selected")?.Value == "all");
			if (cursorIdx < 0) return;

			// Extract trailing word characters from the preceding run (if any)
			string trailWord = string.Empty, trailRest = string.Empty;
			if (cursorIdx > 0)
				(trailRest, trailWord) = SplitTrailingWord(new TextRun(runEls[cursorIdx - 1]).PlainText);

			// Extract leading word characters from the following run (if any)
			string leadWord = string.Empty, leadRest = string.Empty;
			if (cursorIdx < runEls.Count - 1)
				(leadWord, leadRest) = SplitLeadingWord(new TextRun(runEls[cursorIdx + 1]).PlainText);

			var word = trailWord + leadWord;
			if (word.Length == 0) return;

			// Apply transform, then write results back
			var transformed = transform(word);

			if (cursorIdx > 0)
				new TextRun(runEls[cursorIdx - 1]).PlainText = trailRest;

			// Promote the cursor run to hold the transformed word; remove selected marker
			runEls[cursorIdx].ReplaceNodes(new XCData(transformed));
			runEls[cursorIdx].Attribute("selected")?.Remove();

			if (cursorIdx < runEls.Count - 1)
				new TextRun(runEls[cursorIdx + 1]).PlainText = leadRest;

			// Prune runs left empty by the extraction (keep the cursor run even if empty)
			foreach (var runEl in runEls.Where((e, i) => i != cursorIdx))
				if (new TextRun(runEl).PlainText.Length == 0)
					runEl.Remove();
		}


		// Returns (textBeforeWord, trailingWord) — splits off the last run of word characters.
		private static (string before, string word) SplitTrailingWord(string text)
		{
			int i = text.Length;
			while (i > 0 && IsWordChar(text[i - 1])) i--;
			return (text.Substring(0, i), text.Substring(i));
		}


		// Returns (leadingWord, textAfterWord) — splits off the first run of word characters.
		private static (string word, string after) SplitLeadingWord(string text)
		{
			int i = 0;
			while (i < text.Length && IsWordChar(text[i])) i++;
			return (text.Substring(0, i), text.Substring(i));
		}


		private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Embedded content

		/// <summary>The embedded Table, or null if this OE contains text or an image.</summary>
		public TableNode Table
		{
			get
			{
				var tableEl = el.Element(NS + "Table");
				return tableEl is null ? null : new TableNode(tableEl);
			}
		}


		/// <summary>The embedded Image, or null if this OE contains text or a table.</summary>
		public ImageNode Image
		{
			get
			{
				var imageEl = el.Element(NS + "Image");
				return imageEl is null ? null : new ImageNode(imageEl);
			}
		}


		/// <summary>
		/// Replaces the content of this OE with a new table and returns it.
		/// Any existing text runs are removed.
		/// </summary>
		public TableNode InsertTable(int rows, int columns, double columnWidthPx = 120)
		{
			el.Elements(NS + "T").Remove();
			el.Elements(NS + "Table").Remove();
			var table = TableNode.Create(rows, columns, columnWidthPx);
			el.Add(table.Element);
			return table;
		}


		/// <summary>
		/// Replaces the content of this OE with a new image and returns it.
		/// Any existing text runs are removed.
		/// </summary>
		public ImageNode InsertImage(string base64Data, string format, double widthPx, double heightPx)
		{
			el.Elements(NS + "T").Remove();
			el.Elements(NS + "Image").Remove();
			var img = ImageNode.Create(base64Data, format, widthPx, heightPx);
			el.Add(img.Element);
			return img;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Nested content (indentation)

		/// <summary>Nested OEChildren for indented sub-items, or null if none.</summary>
		public OEChildrenNode Children
		{
			get
			{
				var childEl = el.Element(NS + "OEChildren");
				return childEl is null ? null : new OEChildrenNode(childEl);
			}
		}


		/// <summary>Returns the nested OEChildren, creating it if absent.</summary>
		public OEChildrenNode EnsureChildren()
		{
			var childEl = el.Element(NS + "OEChildren");
			if (childEl is null)
			{
				childEl = E("OEChildren");
				el.Add(childEl);
			}
			return new OEChildrenNode(childEl);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Lists

		internal void SetBullet(string bulletId, string fontSize)
		{
			el.Elements(NS + "List").Remove();
			InsertList(E("List", E("Bullet",
				new XAttribute("bullet", bulletId),
				new XAttribute("fontSize", fontSize))));
		}


		internal void SetNumber(int sequence, string format, string font)
		{
			el.Elements(NS + "List").Remove();
			InsertList(E("List", E("Number",
				new XAttribute("numberSequence", sequence),
				new XAttribute("numberFormat", format),
				new XAttribute("font", font))));
		}


		private void InsertList(XElement list)
		{
			// List appears after Meta (and Tag, MediaIndex) per OE schema sequence
			var anchor = el.Elements(NS + "Meta").LastOrDefault()
				?? el.Elements(NS + "Tag").LastOrDefault()
				?? el.Elements(NS + "MediaIndex").LastOrDefault();
			if (anchor is null) el.AddFirst(list);
			else anchor.AddAfterSelf(list);
		}


		/// <summary>Removes any bullet or number list marker from this paragraph.</summary>
		public void ClearList() => el.Elements(NS + "List").Remove();


		/// <summary>The bullet list marker on this paragraph, or null if not a bullet item.</summary>
		public BulletDef Bullet
		{
			get
			{
				var bulletEl = el.Element(NS + "List")?.Element(NS + "Bullet");
				return bulletEl is null ? null : new BulletDef(bulletEl);
			}
		}


		/// <summary>The numbered list marker on this paragraph, or null if not a numbered item.</summary>
		public NumberDef Number
		{
			get
			{
				var numberEl = el.Element(NS + "List")?.Element(NS + "Number");
				return numberEl is null ? null : new NumberDef(numberEl);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Tags

		/// <summary>All note tags on this paragraph.</summary>
		public IReadOnlyList<TagNode> Tags
			=> el.Elements(NS + "Tag").Select(e => new TagNode(e)).ToList();


		/// <summary>
		/// Adds a tag for the given TagDef index (no-op if already present) and returns it.
		/// </summary>
		public TagNode AddTag(int tagDefIndex)
		{
			var existing = el.Elements(NS + "Tag")
				.FirstOrDefault(e => e.Attribute("index")?.Value == tagDefIndex.ToString());
			if (existing is not null) return new TagNode(existing);

			var tag = TagNode.Create(tagDefIndex);
			// Tag appears after MediaIndex per OE schema sequence
			var lastMedia = el.Elements(NS + "MediaIndex").LastOrDefault();
			if (lastMedia is not null) lastMedia.AddAfterSelf(tag.Element);
			else el.AddFirst(tag.Element);
			return tag;
		}


		/// <summary>Removes the tag with the given TagDef index.</summary>
		public void RemoveTag(int tagDefIndex)
		{
			el.Elements(NS + "Tag")
				.Where(e => e.Attribute("index")?.Value == tagDefIndex.ToString())
				.Remove();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Spacing

		public double? SpaceBefore
		{
			get => ParseSpacing("spaceBefore");
			set => Attr("spaceBefore", value?.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
		}


		public double? SpaceAfter
		{
			get => ParseSpacing("spaceAfter");
			set => Attr("spaceAfter", value?.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
		}


		private double? ParseSpacing(string name)
		{
			var v = Attr(name);
			return double.TryParse(v, System.Globalization.NumberStyles.Any,
				System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
		}
	}
}
