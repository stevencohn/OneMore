# PageModels

A self-contained DOM and manipulation API for OneNote page XML, built on top of the
`one:Page` schema defined in `Reference/0336.OneNoteApplication_2013.xsd`.

All classes live under `River.OneMoreAddIn.PageModels` and have **no dependencies** on
the existing `Models/`, `Styles/`, or `Helpers/` code. Migration from the old `Page` /
`Table` / `TableCell` wrappers can happen one command at a time.

---

## Design principles

**XElement as source of truth.** Every node is a thin wrapper around an `XElement`.
There are no shadow copies and no separate object graph. Mutations write directly to the
element, so the tree can be serialized back at any time with `page.ToXml()`.

**LINQ-native.** Collection nodes implement `IEnumerable<T>` or expose typed
`IEnumerable<T>` properties, so standard LINQ chains work without a special query API.

**Cascade-aware.** OneNote text attributes cascade from `QuickStyleDef` → `OE style` →
`T style`. `StyleString.Merge()` and `OENode.FormattedSpans()` resolve the cascade so
callers never have to walk the ancestor chain manually.

**Isolated and incremental.** Nothing in `PageModels` modifies existing files.
New commands can adopt it immediately; existing commands continue unchanged until
explicitly migrated.

---

## Node hierarchy

```
PageNode                        wraps <one:Page>
  ├─ QuickStyleIndex            manages all <one:QuickStyleDef> children
  ├─ TagDefIndex                manages all <one:TagDef> children
  ├─ TitleNode                  wraps <one:Title>
  └─ OutlineNode[]              wraps each <one:Outline>
       └─ OEChildrenNode        wraps <one:OEChildren>  (IEnumerable<OENode>)
            └─ OENode[]         wraps each <one:OE>
                 ├─ TextRun[]   wraps each <one:T>
                 ├─ TableNode   wraps <one:Table>  (exclusive with TextRuns)
                 ├─ ImageNode   wraps <one:Image>  (exclusive with TextRuns)
                 └─ OEChildrenNode  (nested indent — recursive)

TableNode
  └─ TableRowNode[]
       └─ TableCellNode[]
            └─ OEChildrenNode   (cell body — same recursive structure)
```

---

## File inventory

| File | Description |
|------|-------------|
| `OneNoteNode.cs` | Abstract base class. Holds the `XElement`, exposes typed attribute helpers (`Attr`, `AttrBool`, `AttrInt`), the shared `NS` namespace constant, and the `E()` element factory. |
| `OneNoteExtensions.cs` | LINQ extension methods for `IEnumerable<OENode>`: `ForEach`, `WithQuickStyle`, `WithText`, `SelectTables`, `SelectImages`. |
| `StyleString.cs` | Parses and builds the CSS-like `style` attribute used on `Outline`, `OE`, and `T` elements. Provides typed properties (`FontFamily`, `FontSizePt`, `Color`, `Background`, `Bold`, `Italic`, `Underline`, `Strikethrough`, `Superscript`, `Subscript`) and a `Merge()` method for applying the three-tier cascade. |
| `TextRun.cs` | Wraps `<one:T>`. Exposes `PlainText` (HTML spans stripped), `RawContent` (CDATA as-is), and `Style` (character-level `StyleString`). |
| `FormattedSpan.cs` | Read-only POCO returned by `OENode.FormattedSpans()`. Holds `Text` and a fully-resolved `StyleString` after cascade is applied. |
| `OENode.cs` | The central paragraph node. Exposes `Runs`, `PlainText`, `FormattedSpans()`, `AppendRun()`, `SetPlainText()`, `SetStyle()`, `InsertTable()`, `InsertImage()`, `EnsureChildren()`, bullet/number list helpers, and `AddTag()`. |
| `OEChildrenNode.cs` | Container for one indent level. Implements `IEnumerable<OENode>` for direct LINQ use. Key methods: `AllDescendants()`, `Descendants<T>()`, `AppendItem()`, `InsertItemAfter()`, `Indent()`, `Outdent()`, `MakeBulletList()`, `MakeNumberedList()`. |
| `OutlineNode.cs` | Wraps `<one:Outline>`. Convenience methods: `AllParagraphs()`, `AllTables()`, `AllImages()`, `Descendants<T>()`. |
| `TitleNode.cs` | Wraps `<one:Title>`. Get/set `Text` on the first OE. |
| `PageNode.cs` | Root entry point. `Parse(xml)` / `FromElement(root)` to load; `ToXml()` to serialize. Page-wide traversal: `AllParagraphs()`, `AllTables()`, `AllImages()`, `Descendants<T>()`. Exposes `QuickStyles` and `TagDefs` index accessors. |
| `TableNode.cs` | Wraps `<one:Table>`. Accessors: `Rows`, `Cell(r,c)`, `Cell("B3")`. Mutations: `AddRow()`, `AddColumn()`, `RemoveRow()`, `RemoveColumn()`. |
| `TableRowNode.cs` | Wraps `<one:Row>`. Exposes `Cells`, `Cell(col)`, `AddCell()`, `RemoveCell()`. |
| `TableCellNode.cs` | Wraps `<one:Cell>`. Properties: `ShadingColor`, `Content` (an `OEChildrenNode`). Convenience: `PlainText`, `SetText()`. |
| `ImageNode.cs` | Wraps `<one:Image>`. Properties: `Data` (base64), `Format`, `Width`, `Height`, `HyperlinkUrl`, `AltText`. |
| `QuickStyleIndex.cs` | Manages `<one:QuickStyleDef>` elements. `Get(index)`, `FindByName()`, `EnsureStyle()` (find-or-create by value), `Apply(oe, index)`, `GetStyle(index)` → `StyleString`. |
| `TagDefIndex.cs` | Manages `<one:TagDef>` elements. `Get(index)`, `FindByName()`, `EnsureTag()`. |
| `PageBuilder.cs` | Fluent builder for constructing pages programmatically. See usage example below. |

---

## StyleString and the three-tier cascade

OneNote applies text formatting at three levels, with later values overriding earlier ones:

```
1. QuickStyleDef  (paragraph template — font, size, color)
2. OE style attr  (paragraph-level override)
3. T style attr   (character-level override, possibly fragmented across runs)
```

`StyleString` models a single level. `StyleString.Merge()` applies the cascade:

```csharp
var resolved = StyleString.Merge(
    page.QuickStyles.GetStyle(oe.QuickStyleIndex ?? 0),  // level 1
    oe.Style,                                             // level 2
    run.Style);                                           // level 3
```

`OENode.FormattedSpans(quickStyles)` does this automatically for every run in a
paragraph and returns a list of `FormattedSpan` objects ready for analysis.

The style attribute format is a semicolon-separated CSS-like string:

```
font-family:Calibri;font-size:11.0pt;color:#1F4E79;font-style:italic
```

Quirks handled by `StyleString`:
- `font-size` is always written as `11.0pt` (one decimal place, `pt` suffix).
- `color:automatic` and `color:none` normalise to `null` on read.
- `background` and `mso-highlight` are kept in sync for Office compatibility.
- `text-decoration` can hold multiple values (`underline line-through`).

---

## LINQ traversal examples

```csharp
// Load a page
var page = PageNode.Parse(xmlFromGetPageContent);

// Iterate all tables and their rows
foreach (var table in page.AllTables())
    foreach (var row in table.Rows)
        foreach (var cell in row.Cells)
            Console.WriteLine(cell.PlainText);

// Apply a quick style to every body paragraph
int bodyIdx = page.QuickStyles.FindByName("p");
page.AllParagraphs()
    .WithQuickStyle(bodyIdx)
    .ForEach(oe => page.QuickStyles.Apply(oe, bodyIdx));

// Find all paragraphs containing "TODO"
var todos = page.AllParagraphs().WithText("TODO").ToList();

// All tables via Descendants<T>
var tables = page.Descendants<TableNode>().ToList();

// Cells in the second column that contain a dollar amount
page.AllTables()
    .SelectMany(t => t.Rows)
    .Select(r => r.Cell(1))
    .Where(c => c.PlainText.StartsWith("$"));
```

---

## Fluent PageBuilder

For constructing new pages from scratch:

```csharp
var page = PageBuilder.New("Meeting Notes")
    .Heading1("Attendees")
    .BulletList(b => b
        .Item("Alice")
        .Item("Bob", sub => sub
            .Item("joined late")))
    .Heading2("Action Items")
    .NumberedList(b => b
        .Item("Follow up on budget")
        .Item("Schedule next meeting"))
    .Table(3, 2, (t, r, c) => t.Cell(r, c).SetText(r == 0 ? $"Header {c + 1}" : $"R{r}C{c}"))
    .Build();

string xml = page.ToXml();  // ready for UpdatePageContent
```

---

## Round-trip usage

```csharp
// Load
string rawXml = /* from one.GetPageContent(...) */;
var page = PageNode.Parse(rawXml);

// Mutate
page.AllParagraphs()
    .Where(oe => oe.PlainText.StartsWith("**"))
    .ForEach(oe =>
    {
        oe.SetPlainText(oe.PlainText.TrimStart('*').Trim());
        oe.Style = new StyleString { Bold = true };  // not valid — use AppendRun
        oe.SetStyle(s => s.Bold = true);
    });

// Save
await one.Update(/* page wrapping page.Element */);
// or directly:
// onenote.UpdatePageContent(page.ToXml(), DateTime.MinValue, XMLSchema.xs2013, true);
```

---

## What is NOT here

- **No COM interop.** `PageNode` holds XML only. Load and save via the existing
  `OneNote.GetPage` / `OneNote.Update` methods in `OneNote.cs`.
- **No rendering or WinForms.** This is pure XML manipulation.
- **No ink, math, or `InsertedFile` nodes.** Those elements are preserved in the XML but
  have no typed wrappers yet. Access them directly via `oe.Element.Element(NS + "...")`.
- **No merge/split of table cells.** The OneNote schema has no native colspan/rowspan;
  that is an advanced topic for a future iteration.
