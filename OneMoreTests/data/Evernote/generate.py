#!/usr/bin/env python3
"""
Generates a small synthetic ENEX test corpus for exercising an
Evernote -> Markdown -> OneNote conversion pipeline.

Each file isolates one fidelity risk called out in the conversion spec.
Resource hashes (MD5 of decoded binary) are computed for real so the
en-media hash= references are valid, matching how actual Evernote
exports work.
"""

import base64
import hashlib
import os

OUT_DIR = os.path.dirname(os.path.abspath(__file__))

EXPORT_DATE = "20260803T120000Z"

# A tiny valid 1x1 transparent PNG, used as a real embedded image resource.
TINY_PNG_B64 = (
    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk"
    "+A8AAQUBAScY42YAAAAASUVORK5CYII="
)

# A tiny plain-text "attachment" resource.
TINY_TXT_BYTES = b"This is a sample attached text file used for testing.\n"
TINY_TXT_B64 = base64.b64encode(TINY_TXT_BYTES).decode("ascii")


def md5_of_b64(b64_data: str) -> str:
    raw = base64.b64decode(b64_data)
    return hashlib.md5(raw).hexdigest()


def wrap_export(notes_xml: str, application="Evernote", version="10.98.1") -> str:
    return f"""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE en-export SYSTEM "http://xml.evernote.com/pub/evernote-export4.dtd">
<en-export export-date="{EXPORT_DATE}" application="{application}" version="{version}">
{notes_xml}
</en-export>
"""


def wrap_content(inner_xhtml: str) -> str:
    # Content must be CDATA-wrapped ENML, itself a mini XML document.
    return (
        "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>"
        "<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">"
        f"<en-note>{inner_xhtml}</en-note>]]>"
    )


def note(
    title,
    inner_xhtml,
    created,
    updated=None,
    tags=None,
    resources_xml="",
    source_url=None,
    author=None,
):
    tags = tags or []
    tag_xml = "".join(f"  <tag>{t}</tag>\n" for t in tags)
    updated_xml = f"  <updated>{updated}</updated>\n" if updated else ""

    attrs = []
    if source_url:
        attrs.append(f"    <source-url>{source_url}</source-url>")
    if author:
        attrs.append(f"    <author>{author}</author>")
    note_attrs_xml = ""
    if attrs:
        note_attrs_xml = "  <note-attributes>\n" + "\n".join(attrs) + "\n  </note-attributes>\n"

    return f"""<note>
  <title>{title}</title>
  <content>{wrap_content(inner_xhtml)}</content>
  <created>{created}</created>
{updated_xml}{tag_xml}{note_attrs_xml}{resources_xml}</note>
"""


def resource_block(b64_data, mime, filename, width=None, height=None):
    h = md5_of_b64(b64_data)
    dims = ""
    if width and height:
        dims = f"  <width>{width}</width>\n  <height>{height}</height>\n"
    return f"""<resource>
  <data encoding="base64">
{b64_data}
  </data>
  <mime>{mime}</mime>
{dims}  <resource-attributes>
    <file-name>{filename}</file-name>
  </resource-attributes>
</resource>
""", h


# ---------------------------------------------------------------------------
# 01: Basic note — sanity baseline, single tag, plain text + simple formatting.
# ---------------------------------------------------------------------------
n1 = note(
    title="Grocery List",
    inner_xhtml=(
        "<div>Weekly groceries:</div>"
        "<div><b>Produce</b>: apples, spinach, carrots</div>"
        "<div><i>Pantry</i>: rice, olive oil</div>"
    ),
    created="20240110T140000Z",
    updated="20240112T091500Z",
    tags=["errands"],
)
with open(os.path.join(OUT_DIR, "01-basic-note.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n1))


# ---------------------------------------------------------------------------
# 02: Encrypted section — en-crypt block with a hint, mixed with plain text.
# NOTE: the ciphertext below is NOT valid Evernote AES ciphertext — it's a
# placeholder string. It's sufficient for testing "detect en-crypt, skip
# with warning, surface the hint" logic; it is not decryptable and should
# not be treated as real encrypted content.
# ---------------------------------------------------------------------------
n2 = note(
    title="Account Recovery Notes",
    inner_xhtml=(
        "<div>General notes about account recovery process.</div>"
        "<div>Backup codes:</div>"
        "<div>"
        '<en-crypt hint="mom&apos;s maiden name" cipher="AES" length="128">'
        "PLACEHOLDER_NOT_REAL_CIPHERTEXT_0123456789=="
        "</en-crypt>"
        "</div>"
        "<div>Remember to rotate these annually.</div>"
    ),
    created="20230501T081200Z",
    updated="20250602T173045Z",
    tags=["security", "sensitive"],
)
with open(os.path.join(OUT_DIR, "02-encrypted-section.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n2))


# ---------------------------------------------------------------------------
# 03: Multi-tag note — exercises tag-to-metadata mapping (not folder-per-tag).
# ---------------------------------------------------------------------------
n3 = note(
    title="Q3 Planning Ideas",
    inner_xhtml=(
        "<div>Brainstorm for Q3 roadmap.</div>"
        "<div>Needs input from design and eng.</div>"
    ),
    created="20250115T093000Z",
    updated="20250320T110000Z",
    tags=["work", "planning", "roadmap", "q3-2025"],
)
with open(os.path.join(OUT_DIR, "03-multi-tag.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n3))


# ---------------------------------------------------------------------------
# 04: Checklist — en-todo checked/unchecked items, mapping to GFM task lists.
# ---------------------------------------------------------------------------
n4 = note(
    title="Trip Packing List",
    inner_xhtml=(
        "<div>Packing for the conference trip:</div>"
        '<div><en-todo checked="true"/>Passport</div>'
        '<div><en-todo checked="true"/>Laptop charger</div>'
        '<div><en-todo checked="false"/>Badge printout</div>'
        '<div><en-todo checked="false"/>Business cards</div>'
    ),
    created="20260214T160000Z",
    updated="20260216T083000Z",
    tags=["travel"],
)
with open(os.path.join(OUT_DIR, "04-checklist.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n4))


# ---------------------------------------------------------------------------
# 05: Complex/nested table — stresses table conversion fidelity.
# ---------------------------------------------------------------------------
table_html = """
<div>Budget comparison:</div>
<table>
  <tbody>
    <tr>
      <td><div><b>Category</b></div></td>
      <td><div><b>2024</b></div></td>
      <td><div><b>2025</b></div></td>
      <td><div><b>Notes</b></div></td>
    </tr>
    <tr>
      <td><div>Hosting</div></td>
      <td><div>$1,200</div></td>
      <td><div>$1,450</div></td>
      <td>
        <div>
          <table>
            <tbody>
              <tr><td><div>Nested breakdown</div></td></tr>
              <tr><td><div>- Compute: $900</div></td></tr>
              <tr><td><div>- Storage: $300</div></td></tr>
            </tbody>
          </table>
        </div>
      </td>
    </tr>
    <tr>
      <td><div>Marketing</div></td>
      <td><div>$3,000</div></td>
      <td><div>$2,750</div></td>
      <td><div>Reduced ad spend</div></td>
    </tr>
  </tbody>
</table>
"""
n5 = note(
    title="Budget Comparison Table",
    inner_xhtml=table_html,
    created="20250601T100000Z",
    updated="20250815T120000Z",
    tags=["finance"],
)
with open(os.path.join(OUT_DIR, "05-nested-table.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n5))


# ---------------------------------------------------------------------------
# 06: Resources — one embedded image (inline en-media) + one file attachment.
# ---------------------------------------------------------------------------
img_block, img_hash = resource_block(
    TINY_PNG_B64, "image/png", "diagram.png", width=1, height=1
)
txt_block, txt_hash = resource_block(
    TINY_TXT_B64, "text/plain", "notes-attachment.txt"
)
n6_inner = (
    "<div>Whiteboard photo from the design review:</div>"
    f'<div><en-media type="image/png" hash="{img_hash}"/></div>'
    "<div>Full write-up attached below:</div>"
    f'<div><en-media type="text/plain" hash="{txt_hash}"/></div>'
)
n6 = note(
    title="Design Review Notes",
    inner_xhtml=n6_inner,
    created="20260110T093000Z",
    updated="20260110T113000Z",
    tags=["design"],
    resources_xml=img_block + txt_block,
)
with open(os.path.join(OUT_DIR, "06-resources-image-attachment.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n6))


# ---------------------------------------------------------------------------
# 07: Timestamp edge cases — no <updated>, very old <created>, note with a
# source URL (web clip) and author, to exercise metadata line generation.
# ---------------------------------------------------------------------------
n7a = note(
    title="Old Note, Never Modified",
    inner_xhtml="<div>Created once, never touched since.</div>",
    created="19991231T235900Z",
    # deliberately no <updated> element at all
    tags=["archive"],
)
n7b = note(
    title="Web Clip Example",
    inner_xhtml="<div>Clipped article excerpt for reference.</div>",
    created="20260301T140000Z",
    updated="20260301T140000Z",
    tags=["clipped"],
    source_url="https://example.com/articles/some-article",
    author="Jane Reporter",
)
with open(os.path.join(OUT_DIR, "07-timestamp-edge-cases.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(n7a + n7b))


# ---------------------------------------------------------------------------
# 08: Kitchen sink — a small "notebook" combining several of the above in one
# file, to test batch/multi-note handling within a single .enex (this is
# what a real notebook export looks like: many <note> elements per file).
# ---------------------------------------------------------------------------
img_block2, img_hash2 = resource_block(
    TINY_PNG_B64, "image/png", "sketch.png", width=1, height=1
)
kitchen_notes = (
    note(
        title="Meeting Notes 2026-01-05",
        inner_xhtml=(
            "<div>Attendees: Sam, Priya, Deval</div>"
            '<div><en-todo checked="true"/>Send follow-up doc</div>'
            '<div><en-todo checked="false"/>Schedule next sync</div>'
            f'<div><en-media type="image/png" hash="{img_hash2}"/></div>'
        ),
        created="20260105T090000Z",
        updated="20260105T101500Z",
        tags=["work", "meetings"],
        resources_xml=img_block2,
    )
    + note(
        title="Recipe: Weeknight Stir Fry",
        inner_xhtml=(
            "<div><b>Ingredients</b></div>"
            "<div>- Chicken thigh</div>"
            "<div>- Broccoli</div>"
            "<div>- Soy sauce</div>"
            "<div><b>Steps</b></div>"
            "<div>1. Slice chicken.</div>"
            "<div>2. Stir fry veg.</div>"
        ),
        created="20250620T190000Z",
        updated="20250620T193000Z",
        tags=["recipes", "dinner"],
    )
    + note(
        title="Untitled Note",
        inner_xhtml="<div><br/></div>",  # essentially empty note
        created="20260701T000000Z",
        tags=[],
    )
)
with open(os.path.join(OUT_DIR, "08-kitchen-sink-notebook.enex"), "w", encoding="utf-8") as f:
    f.write(wrap_export(kitchen_notes))

print("Generated files:")
for fn in sorted(os.listdir(OUT_DIR)):
    if fn.endswith(".enex"):
        print(" -", fn)
