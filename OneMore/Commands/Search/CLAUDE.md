# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Directory Purpose

This directory implements all search-related commands for the OneMore OneNote add-in. It contains three user-facing commands and a set of supporting engines, dialogs, and controls.

## Commands

| Class | Entry point | What it does |
|---|---|---|
| `SearchCommand` | `Execute()` | Modeless dialog to search pages across sections/notebooks, then move or copy the selected pages to a target section |
| `SearchAndReplaceCommand` | `Execute()` | Find-and-replace across a page, section, or all notebooks; supports regex, capture group substitution, and replacing matched text with a raw `XElement` |
| `SearchWebCommand` | `Execute(uri)` | Grabs selected text from the current page and opens a formatted web search URL in the default browser |

## Architecture

### OneNote XML model

OneNote page content is XML. Text runs are `one:T` elements whose content is a CDATA block containing inner HTML (plain text, `<span>`, `<br>`, etc.). When modifying text, the code:
1. Extracts the CDATA value and wraps it in a temporary `XElement`
2. Iterates the wrapper's child nodes as **Atoms**
3. Applies replacements backward (to preserve character offsets)
4. Writes the modified inner XML back into the CDATA

### Atoms subsystem (`Atoms/`)

Abstracts over OneNote's mixed-content CDATA (interleaved `XText` and `XElement` SPAN/BR nodes):

- `IAtom` — common interface (Length, Append, Remove, Replace by index/length)
- `TextAtom` — wraps a bare `XText` node
- `ElementAtom` — wraps an `XElement` (e.g. `<span>`) and delegates to its first text node
- `AtomicFactory.MakeAtom(node)` — picks the right implementation

### `SearchAndReplaceEditor`

The core replace engine used by both `SearchAndReplaceCommand` and `LinkReferencesCommand` (external caller). Two constructor overloads:
- `(pattern, replacement string, enableRegex, caseSensitive)` — replaces with a string, supports `$1`…`$n` capture group substitution
- `(pattern, replacement XElement, enableRegex, caseSensitive)` — replaces matched text with an XML element (e.g. a hyperlink)

`ScanElement` works backwards through `Regex.Matches` to avoid cumulative offset drift. After all replacements, `PatchEndingBreaks` appends `&nbsp;` to any `T` that ends with `<br>\n` to match OneNote's own behavior.

### `TextMatchBuilder`

Converts a simplified SQL-like query string into a compiled `Regex`. Supported syntax: `AND`, `OR`, `NOT`, parentheses for grouping, `*` wildcard, quoted phrases.

```
(error* OR fail*) AND NOT warning*
```

Used by `SearchDialogTextControl` for the full-text body search feature.

### `SearchDialogTextControl` (experimental)

A full-text search UI panel. It is hidden unless the `GeneralSheet → experimental` setting is enabled. Searches `one:OE/one:T` paragraphs using `TextMatchBuilder`, displays results in a virtual-mode owner-draw `ListView`, and navigates OneNote to the matching paragraph via `one.GetHyperlink(pageId, objectID)`.

**Results rendering** — all results are stored in `List<ResultItem>`. The `ListView` uses `VirtualMode = true` + `OwnerDraw = true`: only visible rows are painted, no child controls exist per row. Two item kinds:
- `IsGroup = true` — section path header, drawn with a colored left swatch (the section's accent color, 15%-tinted background) and bold text. Not navigable.
- `IsGroup = false` — a matching paragraph, drawn as plain text with a purple highlight when selected.

`AddPageResults` batches one group header + all hits for a page into a single `VirtualListSize` update, so repaints are proportional to pages searched, not individual matches. This handles thousands of results without performance degradation.

**Threading** — `one.GetPage()` (COM) runs on the UI thread. Paragraph text extraction and `Regex.IsMatch` run on `Task.Run()` background thread. `one.GetHyperlink()` (COM) runs back on the UI thread after the background work completes. The `TextMatchBuilder`/`Regex` is compiled once before the search loop and shared across all pages.

### Text index (`Services\TextIndex\`)

When the user has turned on the search index (Settings, Search Index), `SearchDialog.Search` first asks
`IndexedHits.Load` (background thread) for the paragraphs that match. `TextMatchBuilder.BuildIndexQuery`
turns the query AST into an FTS5 trigram expression that finds a **superset** (terms under 3
characters, NOT and regex mode are left out, null means every paragraph is a candidate), and every
candidate is then tested with the same `BuildRegex` the live scan uses, so results are identical.
`SearchEngine.SearchSectionPages` is hybrid per page: a page whose index stamp (`pageID` +
`lastModifiedTime`) equals the hierarchy's is answered from the index without a COM load; stale,
never-indexed and the current page (`LivePageID`) are read as before. The index does not hold TOC
paragraphs, so "include TOC" and the single-page scope always read. The CLI path is live only.

**Image text (OCR).** `PageTextExtractor.Extract(page, includeToc, includeOcr)` also returns the
`one:Image/one:OCRData/one:OCRText` of each image as one `ParagraphKind.Image` item, collapsed to one
line and not cleaned like typed text, in document order among the paragraphs. The indexer always
passes `includeOcr: true` and stores `index_text.kind` (0 text, 1 image); queries add `kind = 0`
unless `SearchOptions.IncludeOcr` (the dialog's "Search text in images" box, off by default) is set,
so toggling the box needs no reindex. The live scan goes through the same `Extract`, which keeps both
paths identical. Image hits are shown as a `[image] ...` excerpt around the first match
(`SearchDialog.FormatImageHit`). OneNote can finish OCR after a page's `lastModifiedTime` was
recorded, in which case the text is picked up on the page's next change or an index rebuild.

**Sort and modified hint.** `SearchEngine` passes each page's `lastModifiedTime` to `onPageFound`, which
`SearchDialog.AddPageResults` stores in `CardModel.Modified`. Hovering anywhere on a card draws the
right-justified modified time over the title row. The `sortBox` toggle calls
`SearchResultsCardView.SortByModified`, which orders cards by `Modified` descending (default off keeps
arrival order via `CardModel.Ordinal`); it is applied once when a search completes, not while streaming.

### Search dialog scopes

`SearchDialog.scopeBox` items come from `SearchDialogText_scopeOptions` in fixed order: Notebook, Section group, Section, Page group, Page. "In all notebooks" (`SearchDialog_scopeAll`, `SearchEngine.SearchAllNotebooks`) is inserted at index 0 in code, apart from that list, so older translations with five lines keep working. Section group and Page group are removed by index when not applicable, so every locale resx must carry all five lines. Page group is offered only when `SearchEngine.GetPageGroup` finds the current page has subpages or is a subpage (a level-1 page plus following pages with `pageLevel > 1`); `SearchCommand.Execute` computes this and passes it to the dialog constructor.

**Notebook notation.** Unless regex mode is on, a `\` token in the find box overrides `scopeBox`:
`\\` is the current notebook, `\name` every notebook whose name contains `name` (quotes allow
spaces), `\*` all notebooks. It is parsed by `TitleQueryParser.ExtractNotebook`, stripped from the
text that is matched, and dispatched in `SearchDialog.Search` (`SearchEngine.SearchNotebooks` for names).

### `SearchServices`

Handles the actual page copy/move operations triggered by `SearchCommand`:
- **Copy**: creates a new blank page, sets its ID to the new ID, strips all `objectID` attributes so OneNote regenerates them, then calls `one.Update(page)`
- **Move**: manipulates section hierarchy XML directly — removes the `Page` element from its source section, adds it to the target, then calls `one.UpdateHierarchy()` on both sections

## Key conventions in this directory

- Commands guard against re-entry with a `static bool commandIsActive` flag.
- `SearchAndReplaceEditor.Replace()` treats the wrapper as a flat list of atoms and tracks character positions across node boundaries; the atom list must not be modified between index calculation and replacement.
- `SearchDialogTextControl` uses a `CancellationTokenSource` (`source`) for cooperative cancellation; always check `source?.IsCancellationRequested` before each page operation.
- `SearchGroupControl` is a `UserControl` rendered inline inside a `MoreListView` (not a real ListView group), because WinForms group headers cannot be fully themed.
