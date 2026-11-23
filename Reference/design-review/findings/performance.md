# OneMore: performance review

**Lens:** startup cost, how responsive the OneNote UI thread stays, COM/interop overhead, and behavior on large notebooks and pages.
**Method:** I read the code. Every item cites a file and line. Severity reflects impact on a heavy user, for example about 10k pages across 10+ notebooks, or pages with many images, tables and ink.
**Scope:** read-only. I did not build, run or profile anything. Wherever I give a number of milliseconds or megabytes, it is an estimate from the code path, unless the code's own comments state a measurement.

---

## Confirmed issues

### P1. Every page visit reads the full page hierarchy of all notebooks, and fetches the visited page's content
- **Severity:** high
- **Confidence:** high that the code path runs. Medium on its magnitude: to confirm, log the length of `xml` and the elapsed time in `GetRecycledPageIds` on a large profile.
- **Location:**
  - `OneMore/Commands/Navigator/NavigationService.cs:93-104`
  - `OneMore/Commands/Navigator/NavigationProvider.cs:308-313, 394-427`
  - `OneMore/OneNote.cs:1173-1203, 1279-1295`

```csharp
// NavigationProvider.RecordHistory
var resolved = await Resolve(pageID);                 // -> GetPageInfo
if (resolved != null && await IsDeleted(pageID))      // -> GetRecycledPageIds()
...
// OneNote.GetRecycledPageIds
onenote.GetHierarchy(null, HierarchyScope.hsPages, out var xml, XMLSchema.xs2013);
foreach (var page in root.Descendants().Where(e => e.Name.LocalName == "Page"))
{
    var recycled = page.AncestorsAndSelf().Any(...);
```

- **Why it matters:** The navigation poller runs every 1.25 s. Each time the user rests on a new page for one interval, `RecordHistory` runs. Each run makes these calls:
  1. `GetPageInfo` calls `GetPageContent(piBasic)` on the page the user just opened, which can be a very large page. It needs only `name` and the title's `objectID`. It then calls `GetHyperlinkToObject`, and one `GetHierarchyParent` + `hsSelf` round trip per ancestor.
  2. `IsDeleted` calls `GetHierarchy(null, hsPages)`. That is every page of every open notebook. It is serialized by OneNote and sent across the process boundary to dllhost. Then it is parsed into an XDocument and walked with an O(pages × depth) `AncestorsAndSelf` scan.
- **Impact estimate:** With 10k pages, the hierarchy string is several MB, and parsing it costs tens of milliseconds and several MB of short-lived objects. OneNote has to build that XML on its own side, which costs OneNote CPU while the user is reading or typing. So the cost grows with the size of every open notebook, not with the page being visited. It is paid on every navigation, all day.
- **Suggested fix:**
  - Decide whether a page is deleted from that page alone. Walk `GetParent` from the page, which `GetPageHierarchyInfo` already does, and check `isInRecycleBin`/`isRecycleBin` on each ancestor's `hsSelf` element. Alternatively, look the page up in the latest `IdentitySnapshot`, because `IdentityGatherer` already skips recycle-bin pages.
  - Combine this with `GetPageHierarchyInfo`, which walks the same ancestors, so there is one walk instead of two.
  - In `GetPageInfo`, get `name` from `GetHierarchy(pageId, hsSelf)`. Fetch page content only when `TitleID` is truly needed, for the Quick Notes test.

  ```csharp
  // sketch: one ancestor walk answers path, section, notebook AND "deleted?"
  var hinfo = GetPageHierarchyInfo(pageId, out bool recycled); // check flags while walking
  ```
- **Effort:** M

### P2. The Navigator ribbon dropdown blocks on a full hierarchy read plus per-record COM calls
- **Severity:** high
- **Confidence:** high
- **Location:**
  - `OneMore/Ribbon/AddinRibbon.cs:577-582`
  - `OneMore/Commands/Navigator/HistoryMenu.cs:68`
  - `OneMore/Commands/Navigator/NavigationProvider.cs:219-285`

```csharp
// getContent callback: synchronous by contract
var log = Task.Run(() => provider.ReadHistoryLog()).GetAwaiter().GetResult();
...
// ReadHistoryLog -> LoadPinned (opens SQLite + PageIdentityProvider) -> FindDeleted:
var recycled = one.GetRecycledPageIds(names, out var guids);   // full hsPages again
foreach (var record in records)
    ... !one.PageExists(record.PageId)                        // one COM call per matching record
```

- **Why it matters:** `GetHistoryContent` is a ribbon `getContent` callback. OneNote's UI thread waits for it to return before it can show the menu. While it waits, it runs:
  - the full all-notebooks `hsPages` read from P1;
  - `GetHyperlinkToObject` for every recycled page whose name matches a history entry;
  - one `PageExists` call per matching record;
  - opening the SQLite database (`PinnedProvider`, plus `PageIdentityProvider`, which opens two connections; see `DatabaseProvider.cs:127, 394`).

  The same path also runs on every `Navigator.json` change while the Navigator window is open (`NavigationHandler`, `NavigationProvider.cs:136`). That file is rewritten on every recorded navigation.
- **Impact estimate:** On large profiles, opening the dropdown takes as long as one full hierarchy read, and the user feels that as the ribbon freezing on click. The time grows with the total number of pages, not with the 20 items the menu shows.
- **Suggested fix:**
  - Drop the deleted-page check from the menu path. Have `HistoryMenu.LoadMenu` read only `Navigator.json` and `PinnedProvider`.
  - Purge deleted entries in the background instead: in `NavigationService.Scan` after it records, or in a pipeline stage that already holds an `IdentitySnapshot`.
  - Optionally, cache the last menu XML and invalidate it on `RecordHistory`.
- **Effort:** S–M

### P3. Every `Page` load serializes and SHA1-hashes all top-level elements, even when the page is only read
- **Severity:** medium-high
- **Confidence:** high
- **Location:**
  - `OneMore/Models/Page.cs:36-79`
  - on save: `Page.cs:87-110` and `OneNote.cs:1977-2033`

```csharp
public Page(XElement root) { ... ComputeHashes(root); ... }
...
foreach (var child in root.Elements())
    ... algo.GetHashString(child.ToString(SaveOptions.DisableFormatting))
```

- **Why it matters:** `OneNote.GetPage` always builds a `Page`, so every reader pays for a full re-serialization plus SHA1 of the page, and adds a hash attribute to each child. That includes paths that never save:
  - the background `PageContentPass.LoadPage` (hashtags and the text index; the index comments estimate about 62 pages per pass);
  - `SearchEngine.SearchSectionPages`, once per page searched;
  - `ReminderService.ReminderIsValid`;
  - `GetPageInfo`, on every navigation (P1).

  A command that does save serializes the page four times:
  1. hashing at load;
  2. hashing again in `OptimizeForSave`;
  3. XSD validation in `ValidateSchema` (P5);
  4. `Root.ToString()` for `UpdatePageContent`.
- **Impact estimate:** For a page loaded with `PageDetail.All` or `BinaryData`, which many image commands use, outlines hold base64 image data. Each `ToString` then allocates a string twice the size of that data, as UTF-16, on the Large Object Heap. A 10 MB image page means about 20 MB of LOH garbage per pass and about 80 MB per command. That causes Gen2 GCs and visible pauses in dllhost. In the background pass, a read-only workload pays about 2× the parsing cost for no benefit.
- **Suggested fix:**
  - Compute hashes only when they will be used. Add a `Page(XElement root, bool trackChanges)` overload, or make `ComputeHashes` lazy and trigger it from `OptimizeForSave` callers through a `BeginEdit()`.
  - Pass `trackChanges:false` from `PageContentPass.LoadPage`, `SearchEngine`, `GetPageInfo` and `ReminderService`.
  - For savers, hash with a streaming `XmlWriter` into the hash algorithm's `CryptoStream`, instead of `ToString()`, to avoid the LOH strings.
- **Effort:** M. The constructor is used widely, so audit which callers call `Update`.

### P4. Several services read the same notebook hierarchy independently
- **Severity:** medium
- **Confidence:** high
- **Location:**
  - `OneMore/Commands/Reminders/ReminderService.cs:26-34`, with `RemindScheduler.cs:23,28`: every 5 min
  - `OneMore/OneNote.cs:2446-2449`: `SearchMeta` uses `GetHierarchy(nodeId, hsPages)`
  - `OneMore/Services/Identity/IdentityPass.cs:205-217`: every 2 min, `hsPages` per notebook
  - P1 and P2: each navigation, and each time the menu or Navigator refreshes

```csharp
// ReminderService.Scan
var hierarchy = await one.SearchMeta(string.Empty, MetaNames.Reminder); // full hsPages of all notebooks
```

- **Why it matters:** The identity stage already reads every notebook with its pages, including page-level `Meta` (it uses it in `IdentityGatherer.IsTagIndex`). The reminder service then reads the same data again every 5 minutes, through a different `OneNote` instance, and filters it with LINQ `Remove()` over `Descendants()`. Navigator does it a third time.
- **Impact estimate:** With 10k pages, that adds about 12 more full-hierarchy reads per hour from reminders alone, on top of the 30 from identity. Each one costs OneNote CPU and costs dllhost a multi-MB parse. Then add one per navigation from P1.
- **Suggested fix:** Make the identity snapshot the single source of hierarchy data.
  - Have `IdentityGatherer` also record the pages that carry a `omReminder` Meta, or keep the parsed `XElement` per notebook in the snapshot.
  - Turn reminders into a pipeline stage that reads `context.TryGet<IdentitySnapshot>`.
  - For Navigator, see P1.
- **Effort:** M

### P5. Full XSD validation of the page on every `Update`
- **Severity:** medium
- **Confidence:** high that it runs on every save. Medium on cost: to confirm, time `ValidateSchema` in `Update` next to the existing `updateElapsedMs` telemetry.
- **Location:** `OneMore/OneNote.cs:1997, 2036-2085`

```csharp
if (Marshal.IsComObject(onenote) && !ValidateSchema(page.Root)) { return false; }
...
var document = new XDocument(root);
document.Validate(schemaSet.Value, (o, e) => { ... });
```

- **Why it matters:** `XDocument.Validate` against the compiled OneNote 2013 schema walks every node and attribute that remains after `OptimizeForSave`. Pages loaded `PageDetail.All` and saved with `force:true` keep every outline, so they are validated in full. The schema set is cached through `Lazy`, which is good. The walk itself is O(page) for every command and every background hashtag-style write (`HashtagScanner.cs:580-583`).
- **Side note:** `new XDocument(root)` clones `root` if it already has a parent. It has none today, because the root comes from `XElement.Parse`. If a caller ever passes a parented element, the attribute fixups in the callback would land on the clone and be lost.
- **Suggested fix:**
  - Validate only on retry after OneNote returns `hrXmlIsInvalid`.
  - Or validate only the children left after `OptimizeForSave`.
  - Or limit validation to commands known to generate numeric attributes, which is the case the callback repairs (MinInclusive/MaxInclusive).
- **Effort:** S

### P6. Search re-fetches each section's page list and walks its ancestors over COM
- **Severity:** medium
- **Confidence:** high
- **Location:**
  - `OneMore/Commands/Search/SearchEngine.cs:312-316`
  - `OneMore/OneNote.cs:1841-1888`

```csharp
// SearchSectionPages: 'section' already holds its Page children from the hsPages tree
var info = await one.GetSectionInfo(sectionId);
// GetSectionInfo:
var section = await GetSection(secID);     // GetHierarchy(secID, hsPages) again
var id = info.NotebookId = GetParent(secID);
while (!string.IsNullOrEmpty(id)) { onenote.GetHierarchy(id, hsSelf, ...); id = GetParent(id); }
info.Link = GetHyperlink(sectionId, string.Empty);   // unused by search
```

- **Why it matters:** "Search all notebooks" starts from `GetNotebooks(Scope.Pages)`, which already holds the full tree. For each section it then makes extra calls:
  - one more `hsPages` read of the section;
  - about 2 × depth calls for `GetHierarchyParent` + `hsSelf`;
  - one `GetHyperlinkToObject`.
- **Impact estimate:** With 500 sections at depth 2–3, that is about 3,000 extra COM round trips before any page is read. Each one is a cross-process call that OneNote services on its own threads. This matters most when the text index answers most pages, because the per-section overhead then dominates.
- **Suggested fix:** Build `Path` and `Color` from the in-memory `XElement` ancestors (`section.Ancestors()`), and do not compute `Link`.
- **Effort:** S

### P7. The identity pass reads and matches the whole identity table twice every 2 minutes
- **Severity:** medium-low
- **Confidence:** high
- **Location:**
  - `OneMore/Services/Identity/IdentityPass.cs:233, 241`
  - `OneMore/Services/Identity/PageIdentityProvider.cs:116-117, 294-299`

```csharp
var rescue = provider.FindPagesNeedingGuid(keys, refs, skipped);   // ReadCandidates + Match
...
var resolutions = provider.Reconcile(keys, refs, skipped);         // ReadCandidates + Match again
```

- **Why it matters:** Each pass materializes every `identity_page` row for all listed notebooks into `IdentityRow` objects and runs `PageIdentityMatcher.Match`, and it does this twice. In the common steady state `rescue.Count == 0`, so the second read and match produce exactly the same result.
- **Impact estimate:** With 10k rows, that is about 20k row objects and two full match passes every 2 minutes. It is CPU and GC churn in dllhost, but it does not block OneNote.
- **Suggested fix:** When `rescue.Count == 0`, pass the first match result into `Reconcile` instead of recomputing it. When it is not zero, re-match only the rescued pages.
- **Effort:** S–M

### P8. The settings file is re-read and re-parsed on every `new SettingsProvider()`, 113 call sites, some of them hot
- **Severity:** low-medium
- **Confidence:** high
- **Location:**
  - `OneMore/Commands/Settings/SettingsProvider.cs:25-72`: `XElement.Load(path)` + `ContextMenuSheet.UpgradeSettings` on every construction
- **Hot callers:**
  - ribbon callbacks `GetFavoritesPathsPressed` / `GetHistoryPathsPressed` (`AddinRibbon.cs:566, 592`), plus `HistoryMenu.LoadMenu` and `FavoritesMenu.LoadMenu`, which run on the UI thread when a menu opens;
  - the pipeline, every cycle: `HashtagStage.HashtagsEnabled` is read 2–3 times per cycle (`HashtagStage.cs:74-83, 127, 162`);
  - `IndexConsumer.IsEnabledSetting`, which also opens an HKLM key (`IndexConsumer.cs:188-211`);
  - `HashtagScanner.ReadThrottle`;
  - `CommandProvider.SaveToMRU` reads and then writes the file after every command (`CommandProvider.cs:187-267`).
- **Why it matters:** Each construction does disk I/O, an XML parse and an upgrade check. On a ribbon callback that blocks the UI thread, the cost is small but avoidable. It also adds up across the 4–6 reads per pipeline cycle.
- **Suggested fix:** Use a process-wide cached settings root. Invalidate it on `Save()`, and also on a last-write-time check, because the tray process shares the file. Keep the `new SettingsProvider()` API, backed by the cache.
- **Effort:** M. Writers need care so they don't clobber each other.

### P9. Ribbon image callbacks read the registry before checking the cache, and search-engine images are never cached
- **Severity:** low
- **Confidence:** high
- **Location:**
  - `OneMore/Ribbon/AddinRibbon.cs:706-742, 970-989`
  - `OneMore/Helpers/Office/Office.cs:59-84, 105-129`

```csharp
public IStream GetRibbonImage(string imageName)
{
    if (Office.IsBlackThemeEnabled(true))   // HKCR CurVer + HKCU Common open on every call
...
// GetRibbonSearchImage: base64 decode + Image.FromStream on every callback, stream never put in 'trash'
return ((Bitmap)(Image.FromStream(stream))).GetReadOnlyStream();
```

- **Why it matters:** `loadImage` runs once per ribbon image (100+) at ribbon build, on OneNote's UI thread. Two registry opens per image adds a little to startup. `GetRibbonSearchImage` runs every time the context menu is built. It decodes a bitmap each time and leaks a stream and a bitmap per call, so dllhost memory grows slowly over a long session.
- **Suggested fix:**
  - Compute `IsBlackThemeEnabled` once per `GetCustomUI` and when the theme changes.
  - Route search images through `GetCachedImage(control.Tag, ...)`.
- **Effort:** S

### P10. Every command creates an extra OneNote COM wrapper just to find the owner window
- **Severity:** low
- **Confidence:** high
- **Location:**
  - `OneMore/Commands/CommandFactory.cs:209-220`
  - `OneMore/OneNote.cs:170-176`: sync-over-async ctor used at 129 sites

```csharp
await using var one = new OneNote();   // CreateApplication + Windows/CurrentWindow/WindowHandle
owner = one.OwnerWindow;
...
// most commands then do:  using var one = new OneNote(out var page, out var ns);
page = Task.Run(async () => { return await GetPage(detail); }).Result;
```

- **Why it matters:**
  - Every command creates and releases an `Application` RCW, then usually creates another one straight away. The code itself notes about 300 ms for a cold first connection, which the startup warm-up now pays (`AddIn.cs:346-353`). Warm, it is a few ms plus 3–4 cross-process calls.
  - The `out Page` constructor blocks a thread-pool thread on `.Result` while a second pool thread does the work. That is harmless today, but under batch CLI runs it could add to thread-pool starvation.
- **Suggested fix:**
  - Pass the owner-discovery `OneNote` instance into the command, for example through `SetOneNote`, so the command can reuse it.
  - Longer term, add an async factory: `await OneNote.OpenWithPage(detail)`.
- **Effort:** M. This touches many commands.

---

## Worth investigating

These are code-level suspicions that I could not size without measurements.

1. **Text-index first build and the busy-OneNote case.** `IndexConsumer.PassBudget = 5000` ms of reads per 2-minute pass, with `ReadGap = 25` ms (`IndexConsumer.cs:40, 46`). The comments record about 62 pages per pass with "no lag noticed". However, `HashtagServicePause.IsPaused` only stretches the delay between pages (`PageContentPass.cs:252`). It does not stop a pass that is in progress.
   - *Check:* while a long `GetPageContent` is running, type continuously in OneNote with a 5k+ page profile on first index and measure input latency.
2. **`GetPageInfo(sized:true)` loads `PageDetail.BinaryData`** just to count characters (`OneNote.cs:1177, 1193`). Only `ShowXmlDialog` uses it (`ShowXmlDialog.cs:131`), so its impact is limited to that dialog. On image-heavy pages it can take seconds and use a lot of memory.
   - *Check:* time it on a page with 20+ images.
3. **`GetRecycledPageIds` per-page `AncestorsAndSelf`** is O(pages × depth) with string compares (`OneNote.cs:1289-1295`). Even after P1/P2 are fixed, any remaining callers should prune the recycle-bin subtree once instead of testing each page's ancestors.
   - *Check:* list the remaining callers after P1/P2.
4. **The `NavigationService` poll creates a new `Application` RCW every 1.25 s** (`NavigationService.cs:82`). That is about 2,900 create/release cycles per hour, plus 3 COM calls each. The code releases RCWs explicitly, which is good. Whether creating the RCW is cheap once OneNote is warm is unverified.
   - *Check:* measure `ApplicationFactory.CreateApplication` warm latency. Consider keeping one `OneNote` for the lifetime of the loop, and recreating it on `InvalidComObjectException`.
5. **Each SQLite provider construction opens two connections** (`DatabaseProvider.CatalogExists` + `OpenDatabase`) and runs PRAGMAs plus a schema-version read. This happens in `NavigationProvider.OpenIdentity` and `OpenPinned`, both inside P2's menu path.
   - *Check:* time one `new PageIdentityProvider()` cold and warm. If it is over 10 ms, share one connection per provider type.
6. **Logger flushes on every `WriteLine` under a global lock** (`Logger.cs:428-445`). With verbose logging on, the identity and pipeline phase lines plus per-page lines all serialize on disk I/O.
   - *Check:* compare the pipeline cycle time with `logger.IsVerbose` on and off.

---

## Top 5 by impact

| # | Item | Why first |
|---|------|-----------|
| 1 | **P1** Full all-notebook hierarchy read plus page-content fetch on every navigation | Paid continuously during normal reading. The cost grows with total notebook size and lands on OneNote's own process. |
| 2 | **P2** Navigator dropdown blocks the UI on a hierarchy read and per-record COM calls | Directly freezes the ribbon on click. The fix is small: drop the deleted-page check from the menu path. |
| 3 | **P3** Every `Page` load serializes and hashes the whole page; four full serializations per save | Hits every command and every background or search page read. LOH churn on image-heavy pages. |
| 4 | **P4** Several services read the same notebook hierarchy | Makes P1 worse. Reminders re-read all notebooks every 5 min when the identity snapshot already has the data. |
| 5 | **P6** Search: per-section duplicate `hsPages` read, ancestor walk and hyperlink | Thousands of avoidable COM round trips on whole-notebook and all-notebook searches. The fix is small. |

Next in line: P5 (schema validation on every save; cheap to gate) and P7 (double identity match every 2 minutes).
