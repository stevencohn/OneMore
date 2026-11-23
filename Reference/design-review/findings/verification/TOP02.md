Title: SettingsProvider non-atomic, unlocked writes; saves defaults after failed read
Verdict: confirmed

# TOP02 - Make settings writes atomic and locked, and never save after a failed read

Sources: synthesis.md Top 10 #2; robustness.md R2 (also ARCH A3, MAINT #5).

## Claims checked against code

1. **Failed read silently becomes defaults - CONFIRMED.**
   `OneMore/Commands/Settings/SettingsProvider.cs:40-50` wraps `XElement.Load(path)` in a
   catch-all that only logs. `root` stays null, and lines 59-67 replace it with a minimal
   default (`<settings><images width="500"/></settings>`). Nothing records that the load
   failed. No flag, no read-only mode, no `.bak` fallback.

2. **A later Save() overwrites the real file with that default - CONFIRMED.**
   `Save()` (lines 169-173) is `root.Save(path, SaveOptions.None)` with no guard. Any caller
   that does Get/Set/Save on such an instance writes the default root plus its one
   collection over the user's file. That wipes every other collection (favorites, plugins,
   backup options, keybindings and so on).

3. **Non-atomic write - CONFIRMED.**
   `XElement.Save(string)` truncates and rewrites in place. There is no temp file,
   `File.Replace` or backup. A crash or kill mid-write leaves a truncated file. The next
   load then hits path 1 and 2.

4. **No locking; concurrent read-modify-write loses updates - CONFIRMED.**
   No lock or mutex exists anywhere in `SettingsProvider`. The only named mutexes in the
   solution are `DatabaseProvider` (migration), the tray single instance and the calendar
   single instance. Each caller builds a fresh provider (116 `new SettingsProvider()`
   sites across 77 files), loads the whole file, changes one collection and saves the
   whole snapshot. These writers exist:
   - `CommandProvider.SaveToMRU` (`CommandProvider.cs:219-267`, `290-323`), called from
     `CommandFactory.cs:174` after commands run, and from `CommandPaletteCommand.cs:83`.
   - `DatabaseBackup.RecordSuccess` (`DatabaseBackup.cs:548-555`), called at line 277 from
     the backup flow, which can run from the background pipeline stage. Its own comment
     admits that "saving rewrites the entire settings file".
   - `SettingsDialog` builds its provider when the dialog opens (`SettingsDialog.cs:95`)
     and saves at line 327 when the user clicks OK. Anything written while the dialog is
     open (an MRU entry, `lastBackup`) is lost when that stale snapshot is saved. The
     window is minutes long, not milliseconds.
   - A second OneNote process, or another add-in instance on the same roaming AppData,
     has no cross-process coordination at all.

5. **Same pattern in other stores (ROB W5) - plausible, not re-verified in depth.**
   This was not traced line by line. The synthesis's extension to `SnippetMacros`,
   `TableThemeProvider`, `ThemeProvider` and `RecentColors` matches W5's cited plain
   overwrite writes.

## Attempts to disprove

- **Is the failure path reachable?** Yes. Any `XmlException`/`IOException` gets there,
  including a sharing violation while another thread or process is mid-`Save`. Because the
  catch is catch-all, transient lock errors count too, not just corruption.
- **Handled elsewhere?** No. No caller checks load state (there is no API to check it).
  `ContextMenuSheet.UpgradeSettings` runs only when the load succeeded. Nothing restores or
  backs up the file.
- **Minor overstatement.** The synthesis lists "tray" and "CLI" as *writers*. In the code
  they only read: `OneMoreTray/ScanningJob.cs:64,168` and `Cli/CliCommandFactory.cs:16` only
  call `GetCollection`. That does not weaken the finding. A reader that hits a mid-write file
  still gets defaults silently, so it behaves wrongly, and the in-process writers above are
  enough for data loss.
- **Frequency.** Losing a write to a concurrent save is likely (MRU saves after almost every
  command, plus a long-lived dialog snapshot). Full wipe-to-defaults needs a failed read
  followed by a save on the same instance. That is rarer but realistic: the instance
  that failed to read can be the very `SaveToMRU`/`RecordSuccess` instance, which always
  saves right away.

## Conclusion

Every core claim holds: silent default on read failure, an unconditional save of that
default, an in-place non-atomic write, and no in-process or cross-process locking around
whole-file read-modify-write. The only inaccuracy is naming tray and CLI as writers. The
recommended fix (temp file + `File.Replace` with `.bak`, a load-failed read-only flag,
a named mutex around reload/merge/save) addresses the real defects.
