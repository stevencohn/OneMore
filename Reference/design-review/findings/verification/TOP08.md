Title: onemore: trusted protocol registered for All Office Applications
Verdict: partially

Finding: synthesis.md Top 10 #8 (source: security.md W1). The claim is that the `onemore:` trusted-protocol key is
written under `All Applications`, so a link in an email, Word/Excel document or shared notebook page runs
`[CommandService]` commands with one click and no Office warning, including commands that write Outlook contacts
or send content to kroki.io/PlantUML.

## What holds (confirmed at the code level)

- `OneMoreSetupActions/Actions/TrustedProtocolAction.cs:32` builds
  `Microsoft\Office\{version}\Common\Security\Trusted Protocols\All Applications\onemore:` and `Install()` creates it
  under `HKEY_USERS\{sid}\Software\Policies\...` (lines 88-116). `Uninstall()` repeats the same path at lines 167-168.
  No per-application scope is used.
- `CommandFactory.Invoke` (`OneMore/Commands/CommandFactory.cs:388-411`) runs any type marked `[CommandService]`
  and passes the URL segments straight in as string arguments. Only the attribute is checked. There is no check on the
  caller or the source.
- 15 types carry `[CommandService]`: InsertToc, LinkReferences, SummarizeHighlights, FormControlsDemo, FormControl,
  RefreshFootnotes, Diagram (PlantUml/Mermaid), ReportReminders, Remind, Embed, EmbedSubpageProxy, OutlookContact,
  ImportOutlookTasks, ImportOutlookContacts, Goto.

So the policy scope is broader than OneNote, and the commands it lets through have no confirmation step.

## What is overstated

1. **Most protocol calls only act on data already on the user's current page, matched by an ID the attacker has to
   know.**
   - `OutlookContactCommand save` -> `ContactGenerator.SaveReport` (`ContactGenerator.cs:175-215`) looks up the report
     table and its `EntryID` meta *on the current page* by GUID. With an unknown GUID it shows "report not found" and
     stops. With a known GUID it saves the values the user's own page already holds back to the contact that page was
     imported from. The attacker can't supply any field values. The real risk is an unwanted save of edits the user
     hadn't chosen to commit. It is not an arbitrary write to Outlook.
   - `DiagramCommand` with args (`DiagramCommand.cs:90-116`) refreshes or extracts the diagram whose ID matches a meta
     on the current page (`RefreshDiagram`, line 480). It sends the user's own diagram text to the renderer they
     already use. This is not new exfiltration, because that text was already sent to kroki/PlantUML when the diagram
     was made.
   - The refresh paths of `ImportOutlookContacts`, `ImportOutlookTasks`, `ReportReminders`, `InsertToc`,
     `LinkReferences` and `Embed` take a refresh token plus a GUID or scope and regenerate existing report content.
     Their non-refresh paths open a dialog (e.g. `ImportOutlookTasksCommand.cs:76`, `LinkReferencesCommand.cs:77`) and
     need user interaction.
   - With no arguments, `RefreshFootnotes` (`RefreshFootnotesCommand.cs:23-31`) relabels the current page's footnotes.
     This changes the page but is idempotent and harmless.
   - `GotoCommand` / `RemindCommand <arg>` navigate OneNote. This is a nuisance, not a write.
   One click can still trigger page refreshes or a forced contact save, but this is "unwanted refresh or save of the
   user's own content", not attacker-controlled modification. A Medium rating is generous. Low-Medium fits better.

2. **The proposed fix doesn't cover the shared-notebook vector.** A crafted `onemore:` link on a shared OneNote page is
   clicked *inside OneNote*. A OneNote-scoped trusted key, which the fix keeps on purpose so OneMore's own refresh links
   work without a prompt, would still trust it. Scoping only closes the email/Word/Excel vector.

3. **The key only suppresses a warning. It isn't the gate.** Without it, Office shows its "hyperlink may be unsafe"
   prompt, which users can click through. The change is defense in depth, not a hard boundary. It is also unverified
   that the Office `Trusted Protocols` policy honors a per-application subkey (e.g. `...\Trusted Protocols\OneNote\`).
   The security report itself hedges ("if your Office version supports per-app keys"). That has to be tested on a real
   Office build before the key is moved. If it isn't supported, the key can't be scoped this way.

## Net

- The code fact is confirmed: the trusted-protocol policy is written for All Applications.
- The impact is overstated. The protocol commands need IDs that match the current page, and they work on the user's own
  content. There's no path that lets an attacker write their own data.
- The fix is partial: it doesn't address links on shared OneNote pages, and support for per-app scoping is unverified.
- Still worth doing as a cheap hardening step if Office honors a per-app key. Gating `OutlookContactCommand save`
  behind a confirmation when it's invoked by protocol is the more useful part of the remedy.
