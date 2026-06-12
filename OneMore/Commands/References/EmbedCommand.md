# OneMore Embed Command Software Spec

## Command identity

- **Command name (class)**: EmbedCommand
- **Display label**: Embed
- **Command ID (ribbon)**: ribEmbedButton
- **Category/menu group**: References → Embed

## Intent and motivation

### One-liner
Embed the body of a page or a delimited section of a page into the current page with controls to
refresh to duplicated content from the source material.

### User story
As a OneNote user, I want to embed a page or a portion of a page in my current page. This allows 
me to create a single source of truth for specific content, embed that within other pages, and 
then at my command, synchronize the copy of the content with the original to keep it up to date.

### Non-goals (out of scope) 
- Does not replace the existing EmbedSubpageCommand or EmbedSubpageProxy classes; this is a new
  class in a new file: EmbedCommand.cs.
- The main embedding logic of Embed command will not be accessible from the OneMore CLI. 
  However, that should not preclude the possibility of a 
  `embed --notebook --section --page --refresh` directive that can refresh/sync the embedded
  content on specified pages (TBD).

## Entry points and UI
This command will be invoked by a Ribbon button from the OneMore tab, References group, using
existing ribPage* pattern common to other OneMore commands. It must be defined in both 
RibbonTabHome.xml and RibbonTabOneMore.xml under the ribReferencesMenu element. All static string
on the dialog should be localized using the Translator class similar to how other OneMore dialogs
do that, reusing existing word_* or phrase_* resources when available.

The command should be accessible from the Command Palette as "Embed".

### Dialog required?
If a OneNote hyperlink URI is not found on the clipboard, then use the OneMore.SelectLocation
API (example usage in EmbedSubpageCommand) to pick a source page.

Once the source page is known, an Embed Options dialog is displayed to allow the user to
specify options.

- The user should specify custom begin and end tags to look for on the page, or can leave them
  blank to copy the entire body of the page.
- The user should be able to choose whether to copy as OneNote formatted content or just text -
  and if just text, then how to style it:  Normal, Italic, Gray, Quote style, or Citation style

## Behavior and logic

### Happy path (step by step)
1. Find the source page by first looking on the clipboard for a OneNote hyperlink URI. This might
   be from the user right-clicking a page in the OneNote Page panel and choosing *Copy Link to Page*,
   or right-clicks a paragraph on the source page and choosing *Copy Link to Paragraph*.
1. User navigates to the target page and moves the text cursor to an insertion point, or selects
   content to replace.
1. User choses the Embed item from the OneMore References group.
1. If a valid OneNote hyperlink URI is on the clipboard, then use that. Otherwise, present the
   OneMore.SelectLocation dialog so the user can select the source page. Look up the page name for
   display in the next dialog.
1. Present the Embed Options dialog, showing the source page name, the target page name, and the
   options mentioned above.
1. Upon the user clicking OK, the dialog is closed and the content from the source page is copied,
   extracted and formatted as requested, and inserted at the current position or replaces the
   selected content.
    a. The content must stand apart from the rest of the content on the target page. This means
       that the copied content will always be enclosed in a single cell table, similar to the way
       EmbedSubpageCommand works.
    a. The wrapping table cell will also contains a Refresh link, again in the same way
       EmbedSubpageCommand does this, in the upper right of the cell where the user can click to
       sync the content from the source page and update the contents of wrapper table cell.
    a. The Refresh link must be of a similar form to how the Refresh link is created in
       EmbedSubpagesCommand and must include the user options specified in the Embed Options
       dialog; these options are then passed through the CommandService proxy to apply the same
       options when refreshing/syncing the content from the source page.
    a. If replacing selected content, this means special care needs to be taken to properly splice
       the current paragraph is only a portion of the paragraph is selected, or the selection
       partially spans multiple sequential paragraphs.

### Details
- If the clipboard contains a hyperlink to the page, the user has the option of entering delimeters
  such as "om__" and "__om"
   - If delimiters are entered, all content between but not including those two delimiters
     should be copied
   - If no delimiters are entered, the entirety of the page body is copied.
- OneNote hyperlink URIs, whether created from **Copy Link to Page** or **Copy Link to Paragraph**,
  should be handled consistently - we only need the page and ignore the object-id in the URI because
  it's meaningless to us. Hyperlinks are parsed using the OneNoteLinkParser class.

### Refresh
1. Upon clicking the Refresh link (with any supplied options), the source content is read from the
   source page and the content in the current wrapper table is synced with any updates.
1. If the source page or content specified by start/end delimiters no longer exists, the user
   should be shown a warning dialog box indicating that "the source content is no longer available".

### Undo / reversibility
Ctrl+Z will work as it does natively in OneNote. No further OneMore customization is required here.

### Implementation pointers
Implement EmbedCommand similar to the way all other commands are implemented to fit within the
OneMore command framework.

### Files to create / modify
- create Commands/References/EmbedCommand.cs
- create Commands/References/EmbedDialog.cs
- modify Ribbon/RibbonTabHome.xml
- modify Ribbon/RibbonTabOneMore.xml
- modify Properties/Resources.resx and .cs - but no other locale resx files

## Testing notes
The main embedding logic should be separated enough to allow unit testing from OneMoreTests.
Use /make-test to create appropriate unit tests for EmbedCommand with different formatting options
available in the Embed Options dialog.
