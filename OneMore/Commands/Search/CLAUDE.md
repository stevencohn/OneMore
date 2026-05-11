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

### `SearchServices`

Handles the actual page copy/move operations triggered by `SearchCommand`:
- **Copy**: creates a new blank page, sets its ID to the new ID, strips all `objectID` attributes so OneNote regenerates them, then calls `one.Update(page)`
- **Move**: manipulates section hierarchy XML directly — removes the `Page` element from its source section, adds it to the target, then calls `one.UpdateHierarchy()` on both sections

## Key conventions in this directory

- Commands guard against re-entry with a `static bool commandIsActive` flag.
- `SearchAndReplaceEditor.Replace()` treats the wrapper as a flat list of atoms and tracks character positions across node boundaries; the atom list must not be modified between index calculation and replacement.
- `SearchDialogTextControl` uses a `CancellationTokenSource` (`source`) for cooperative cancellation; always check `source?.IsCancellationRequested` before each page operation.
- `SearchGroupControl` is a `UserControl` rendered inline inside a `MoreListView` (not a real ListView group), because WinForms group headers cannot be fully themed.
