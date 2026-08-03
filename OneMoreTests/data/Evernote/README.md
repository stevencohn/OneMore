# Synthetic ENEX Test Corpus

Hand-crafted `.enex` files for manually testing an Evernote → Markdown → OneNote
conversion pipeline. Each file isolates one fidelity risk identified in the
conversion spec. All files are well-formed XML with valid nested ENML content
(validated with Python's `xml.etree.ElementTree`), and resource `hash`
attributes are real MD5 hashes of the embedded base64 data, matching how
actual Evernote exports work.

| File | Exercises |
|---|---|
| `01-basic-note.enex` | Baseline sanity check: plain text, bold/italic, single tag, normal timestamps. |
| `02-encrypted-section.enex` | An `<en-crypt>` block with a `hint` attribute, surrounded by plain text. **The ciphertext is a placeholder, not real Evernote AES output** — it's for testing "detect, skip, warn, surface hint" logic, not decryption. |
| `03-multi-tag.enex` | A note with four tags, to test tag-as-metadata handling (not folder-per-tag, which is what causes duplication in some existing tools). |
| `04-checklist.enex` | `<en-todo checked="true/false"/>` items, to test conversion to GFM task-list syntax (`- [ ]` / `- [x]`) and, downstream, OneNote To-Do tags. |
| `05-nested-table.enex` | A table containing a nested table inside one cell, plus a header row — stresses table-conversion fidelity beyond simple grids. |
| `06-resources-image-attachment.enex` | One inline image resource (tiny real 1×1 PNG) referenced via `<en-media>`, and one non-image file attachment (`.txt`) — tests resource extraction, MIME handling, and image-vs-attachment link rendering. |
| `07-timestamp-edge-cases.enex` | Two notes: one with **no `<updated>` element at all** (only `<created>`, a legal ENEX state), and one very old (1999) `<created>` date; a second note includes `source-url`/`author` (web clip metadata) to test metadata-line generation. |
| `08-kitchen-sink-notebook.enex` | A single file with **three notes**, simulating a real notebook export (ENEX is exported per-notebook, so multi-note files are the normal case) — combines a checklist+image note, a plain recipe note, and a near-empty "Untitled Note" edge case. |

## Known limitations of this corpus

- Not a substitute for testing against a real personal export — real notes
  will have messier HTML (deeply inconsistent inline styles, Evernote's
  webkit-specific CSS artifacts, malformed nesting from years of edits/copy-paste)
  that hand-crafted samples won't reproduce.
- The image/attachment resources are minimal placeholders (1×1 PNG, short text
  file) — doesn't test large-file handling, multiple images per note, or
  unusual MIME types (audio, PDF, ink).
- Only single-level tag/notebook structure is represented; doesn't include a
  multi-notebook or stack-simulation scenario, since (per the spec) stack
  membership isn't present in ENEX at all.
- Timestamps use UTC `Z`-suffixed ISO-ish Evernote format
  (`YYYYMMDDTHHMMSSZ`) throughout, which is the real ENEX format.

## Regenerating

`generate.py` in this folder produces all files programmatically (and
computes real MD5 hashes for resources) — edit and rerun it to add further
edge cases as testing surfaces new ones.
