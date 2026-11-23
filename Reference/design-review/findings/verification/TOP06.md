Title: Navigator reads the full all-notebooks hierarchy on each navigation and dropdown open
Verdict: confirmed

# TOP06: verification

Finding: synthesis.md Top 10 #6, "Navigator: stop reading every notebook's hierarchy on each navigation and each dropdown open". The sources are PERF P1 and P2 in `findings/performance.md`.

I read the code and tried to disprove the finding. The code paths are reachable and nothing else handles them. Some wording in the finding is a little loose, as noted below, but that does not change the verdict.

## P1: each settled navigation runs a full `hsPages` read

- `NavigationService.Scan` (`OneMore/Commands/Navigator/NavigationService.cs:80-107`) is called by `ServiceLoop` every `pollingInterval`. The default is 1250 ms (line 26). `RecordHistory` runs when `commitment == 1`, which means once per page the user stays on for one interval. It does not run on every poll tick. So the cost is "once per navigation", which is how the synthesis words it. It is not "every 1.25 s".
- `NavigationProvider.RecordHistory` (`NavigationProvider.cs:295-314`) calls `Resolve(pageID)` and then `IsDeleted(pageID)`.
  - `Resolve` calls `OneNote.GetPageInfo` (`OneNote.cs:1173-1203`). That does `GetPage(pageId, PageDetail.Basic)`, a full page-content fetch that also builds a `Page` object, which hashes its content. It then calls `GetHyperlinkWithRetry`. Finally it calls `GetPageHierarchyInfo` (`OneNote.cs:1212-1244`), which makes one `GetParent` call plus one `GetHierarchy(id, hsSelf)` call per ancestor.
  - `IsDeleted` (`NavigationProvider.cs:394-406`) calls `one.GetRecycledPageIds().Contains(pageID)`. `GetRecycledPageIds` (`OneNote.cs:1279-1295`) does `onenote.GetHierarchy(null, HierarchyScope.hsPages, ...)`, which covers every page of every open notebook. It parses the result with `XElement.Parse` and runs `AncestorsAndSelf().Any(...)` for every `Page`. All of that work answers a yes/no question about one page.
- The ancestor walk in `GetPageHierarchyInfo` already reads each ancestor's `hsSelf` element. Those elements carry `isRecycleBin`/`isInRecycleBin`, so the deleted check could be answered during that walk. The pattern is used elsewhere, for example `IdentityGatherer.cs:121,159` and `ResetCursorCommand.cs:386-390`. Nothing caches the recycled set or reuses it from an `IdentitySnapshot`. Each call creates a new `OneNote` and makes a new hierarchy call.

## P2: the ribbon dropdown blocks on the same read

- `AddinRibbon.GetHistoryContent` (`AddinRibbon.cs:577-581`) is the `getContent` callback of the `ribNavigatorButton` dynamicMenu (`RibbonTabOneMore.xml:34-41`, `RibbonTabHome.xml:1307-1312`). It calls `HistoryMenu.LoadMenu` (`HistoryMenu.cs:48-71`), which runs `Task.Run(() => provider.ReadHistoryLog()).GetAwaiter().GetResult()`. The callback is synchronous, so the ribbon thread blocks until `ReadHistoryLog` returns.
- `ReadHistoryLog` (`NavigationProvider.cs:219-245`) runs these steps:
  1. `LoadPinned` opens `PinnedProvider` (SQLite).
  2. `OpenPinned` may also open `PageIdentityProvider`, but only on a migration that has not run yet.
  3. `FindDeleted` (`248-285`) calls `GetRecycledPageIds(names, out guids)`. That is the same full `hsPages` read, plus `GetHyperlink` for each recycled page whose name matches a history entry.
  4. Then it calls `PageExists` (one `GetParent` COM call) for each record whose link GUID matches.
- **Partial mitigation, not a refutation:** the dynamicMenu has no `invalidateContentOnDrop`, so Office caches the content until the control is invalidated. Repeated opens with no navigation in between do not re-run the callback. However, `NavigationService.Scan` calls `ribbon.SafeInvalidateControl("ribNavigatorButton")` after every successful `RecordHistory` (`NavigationService.cs:101-104`), and Navigator.json is updated on almost every navigation. So in practice the first dropdown open after each navigation pays the full cost. "Each dropdown open" overstates this slightly. The real behavior is "each open after a navigation", which is the normal usage pattern.
- **Extra cost the finding mentions:** while the Navigator window is open, `NavigatorWindow` subscribes to `provider.Navigated` (`NavigatorWindow.cs:142`). The `FileSystemWatcher` handler (`NavigationProvider.cs:121-148`) then calls `ReadHistoryLog()` on every Navigator.json write, and `RecordHistory` rewrites that file on every navigation. With the window open, one navigation therefore costs two full `hsPages` reads: one in `IsDeleted` and one in the watcher's `FindDeleted`. Opening the dropdown afterwards makes a third. `HistoryCommand.cs:63`, `HistoryDialog.cs:59` and `NavigatorWindow.cs:722,1738` also call `ReadHistoryLog`, but those run only when the user asks.

## Assumptions checked

- **Is it reachable?** Yes. The service starts unless the navigator is disabled and `scrollPane` is off (`NavigationService.cs:67`). The dropdown is on both the Home and OneMore tabs.
- **Is it handled elsewhere?** No. There is no cache, debounce or snapshot reuse for the recycled-page set, and the ribbon path has no background purge.
- **How large is the cost?** I did not measure it, and neither did the source. The claim that the cost grows with total pages and not with the visited page follows directly from `GetHierarchy(null, hsPages)`. The milliseconds and megabytes in the source are estimates.

## Conclusion

The finding is confirmed. Two wording points need tightening, and neither affects the recommended fixes:

- `RecordHistory` runs once per settled navigation, not on every poll.
- Office caches the dropdown content, but the service invalidates it after each recorded navigation.

The fixes still stand:

- **P2:** take `FindDeleted` out of the menu path, and purge deleted entries in the background.
- **P1:** answer the deleted check during the existing ancestor walk, and get `name` from `hsSelf` instead of fetching the page content.
