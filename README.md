# ![logo](Screenshots/Logo.jpg "logo") OneMore - a OneNote Add-in

A OneNote add-in with the following primary features:

* Access all features from OneMore group menus added to the Home ribbon
* Create/edit/apply **custom styles** with advanced options
* Apply all custom styles to the current page with one click
* Disable **spell-check** on the current page
* Manage menu of **Favorite** pages
* Paste Rich Text (preserve colors when **pasting code** from Visual Studio)<sup>1</sup>
* **Search and replace** text on the current page
* Add or remove **footnotes** (endnotes)

*And these secondary features:*

* Add special icon to page title, also appears in page hierarchy
* Collapse the page hierarchy to see only top-level pages
* Change selected text to UPPERCASE or lowercase
* Insert single or double horizontal line
* Insert [**Table of Contents**](#exTOC) including all headers on page
* Insert [Info/Warn boxes](#exInfoBoxes) similar to Confluence Info and Warn macros
* Insert [Code box](#exCodeBox) similar to Confluence Code macro
* Insert [status labels](#exStatusLabels) similar to the Confluence status macro
* Increase/Decrease the font size of all content on the current page
* Sort pages, sections, or notebooks
* Trim trailing whitespace from selected text
* View and edit page XML (a diagnostic, debugging, advanced-user tool)


#### Key Bindings
While all commands can be accessed from the OneMore ribbon group menus, some
commands also have their own key bindings:

| Command                       | Key Binding |
| ----------------------------- | ----------- |
| Add footnote                  | Ctrl + Alt + F
| Remove footnote               | Ctrl + Shift + F
| Insert horizontal line        | Shift + Alt + Minus
| Insert double horizontal line | Shift + Alt + Equals
| No Spell Check                | F4
| Paste Rich Text               | Ctrl + Alt + V
| Search and Replace            | Ctrl + H
| To uppercase                  | Ctrl + Shift + Alt + U
| To lowercase                  | Ctrl + Shift + U
| Increase font size            | Cltr + Alt + Plus
| Decrease font size            | Ctrl + Alt + Minus
| Show XML                      | Ctrl + Shift + Alt + X

<sup>1</sup> If text copied from Visual Studio is pasted as plain text instead of rich text
when using the Paste Rich Text command (Ctrl+Alt+V) then look at the VS Tools... Options...
Text Editor... Advanced, and tick the box *Copy rich text on copy/cut*.

**Why?**

For myself. I wanted something more than what OneNote provided. I was overwhelmed by _OneNote Gem_,
aghast at its bloated useless overloads of what was already otherwise available or features that
I would never use in a million years - and, oh yeah, that price! I admit I was intrigued by the
_Onetastic Macro_ approach but thought it limited and obtuse, decipherable only by programmers.
And both of these charge money for something that should be open source and neither provided exactly
what I wanted. So I did what any self respecting software engineer would do... I built my own.
Deal with it.

**Minimum Prerequisites**

* Developed for Windows 10
* Microsoft Visual Studio 2019, C# 7
* Microsoft Visual Studio 2019 Installer Projects extension
* .NET Framework 4.8
* Microsoft OneNote 2016 32-bit

Tested recently with Windows 10 1909, VS2019, and OneNote 2019/O365

---

## Examples of What OneMore Can Do...

<a name="exTOC"></a>
#### Table of Contents

Generate a Table of Contents based on both standard headings and custom user-defined headings
where each line is hyperlinked to its heading and indented according to its heading level.

**Table of Contents**</br>
[Heading 1](#exampToc)</br>
. . [Heading 2](#exampToc)</br>
. . . . [Heading 3](#exampToc)

<a name="exStatusLabels"></a>
#### Status Labels

Insert colored status labels similar to the Status macros found in Confluence. These
are not as smart as the Confluence macros but instead are just simple text with a highlight
background that are fully editable inline.

![Status Labels](Screenshots/StatusLabels.jpg)

<a name="exInfoBoxes"></a>
#### Information and Warning Boxes

Insert an information box or warning box with fully customizable headers and content.
These are snippets of content that are actually nested tables.

![Info Boxes](Screenshots/InfoBoxes.jpg)

<a name="exCodeBox"></a>
#### Code Box

Insert a code box with heading and area for your code. Use the Paste Rich Text command
to paste syntax-highlighted code directly from Visual Studio<sup>1</sup>

![Code Box](Screenshots/CodeBox.jpg)


---
## Screenshots

|     |     |
| --- | --- |
| **OneMore Command Menu**                              | **Favorites Menu** |
| ![Command Menu](Screenshots/MoreMenu.png)              | ![Favorites Menu](Screenshots/FavoritesMenu.png) |
| **Custom Styles**                                     | **Custom Styles Dialog** |
| ![Styles](Screenshots/CustomStyles.png)                | ![Styles Dialog](Screenshots/CustomStylesDialog.png) |
| **Title Icons Dialog**                                | **Sort Dialog** |
| ![Title Icon Dialog](Screenshots/TItleIconsDialog.png) | ![Sort Dialog](Screenshots/SortDialog.png) |

**XML Dialog**

![XML Dialog](Screenshots/XmlDialog.jpg)

---

## Developing OneMore

Microsoft' OneNote Developer Reference 
[is here](https://docs.microsoft.com/en-us/office/client-developer/onenote/onenote-developer-reference).

### Direct Development

To avoid continually copying to the installation folder, you can modify the Registry setting to point to your project build output folder instead. You'll
need to restart OneNote every time you want to rebuild but it's still easier. T
The Registry key is here:

    [HKEY_CLASSES_ROOT\WOW6432Node\CLSID\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}\InprocServer32]

and the key would be something like this:

	"CodeBase"="C:\\OneMore\\OneMore\\bin\\x86\\Debug\\River.OneMoreAddIn.dll"

*There are additional haphazard notes in the OneMore project folder readme.txt.*


### Debugging

To start the debugger, add this line to your code where you want a breakpoint:

    System.Diagnostics.Debugger.Launch();

This will cause Visual Studio to display the "attach debugger" dialog. If this dialog does
not appear and instead a new instance of VS is opened then 
[check this out from Microsoft about the Just in Time Debugger settings](https://docs.microsoft.com/en-us/visualstudio/debugger/debug-using-the-just-in-time-debugger).
