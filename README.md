﻿# ![logo](../../wiki/images/Logo.jpg "logo") OneMore - a OneNote Add-in

A OneNote add-in with powerful yet simple and effective features.

See the [**project wiki**](../../wiki) for full details. Meanwhile, here's some pleasant reading material...

* Integrated with the OneNote ribbon, [context menus](../../wiki#extended-context-menus), and [keyboard shortcuts](../../wiki#keys) for easy access
* [Customize the Text context menu](../../wiki/Settings) to add OneMore commands or a a custom search engine
* Apply [colorized syntax highlighting](../../wiki/Edit-Commands) to snippets of source code
* Create/edit/apply [custom styles](../../wiki/Custom-Styles) with advanced options
* Manage menu of [Favorites](../../wiki/Favorites) for one-click link to your most referenced pages
* Save and reuse [custom snippets](../../wiki/Favorites) of content anywhere on any page
* Add [formulas](../../wiki/Table-Commands) to table cell using Excel-like expressions


*Want more from OneMore? OneMore has more...* [Click here to see a list of all features](#features)

![screenshot](../../wiki/images/Screenshot.png)

<a name="features"></a>
## Features (126)

[Clean commands](../../wiki/Clean-Commands) (9)

* Change spacing between sentences to one or two spaces
* Clear background color and reset text color of selected text
* Remove author/citation information from paragraphs
* Remove pasted screen clipping and URL citation lines from page
* Remove empty headers and "extra" empty lines between paragraphs
* Remove spacing between paragraphs and headings
* Show/Hide the page date and time stamps under the title on page or all pages in section
* Trim leading whitespace from selected text
* Trim trailing whitespace from selected text

[Custom Styles](../../wiki/Custom-styles) (6)

* Apply custom styles to selected text
* Load a custom theme of styles, user-defined and some provided with OneMore
* Edit custom style themes
* Create new styles based on the selected text
* Apply an entire theme to a page
* Change page theme inluding background and styles, optioanally with dark-mode awareness

[Edit Commands](../../wiki/Edit-Commands) (18)
* Colorize selected text as a chosen programming language - _syntax highlighting_
* Highlight selected text using a rotating array of colors (Ctrl + Shift + H)
* Paste Rich Text (preserve colors when pasting code from Visual Studio)<sup>1</sup> (Ctrl + Alt + V)
* Enabled spell-checking on the current page, resets to the current culture language
* Disable spell-check on the current page (F4)
* Change selected text to UPPERCASE (Ctrl+Shift+Alt+U), lowercase (Ctrl+Shift+U), Or Title Case
* Increase (Ctrl+Alt+Plus) or Decrease (Ctrl+Alt+Minus) the font size of all content on the current page
* Expand or Collapse indented outlines, save and restore outlining
* Invert text selection
* Join Paragraph to remove soft-breaks and join lines into a single flowing paragraph
* Select all images on the page
* Select all text on a page with similar formatting to the currently selected text

[Favorites and Custom Snippets](../../wiki/Favorites) (5)

* Add any page in OneNote to the Favorites menu for quick access
* Sort or reorder favorites using the Favorites Manager
* Save and reuse custom snippets of content anywhere on any page
* Quick access to user-defined plugins
* Add a link to a special page containing all OneNote and OneMore keyboard shortcuts

[Image commands](../../wiki/Image-Commands) (4)

* Add caption to a selected images
* Precisely crop a selected image
* Resize a selected image or all images on the page
* Rotate an image to any angle

[Numbering commands](../../wiki/Numbering-Commands) (5)

* Number page headings with numeric or alpha-numeric outline prefixes
* Number all sections in the current notebook
* Number all pages in the current section with numeric or alpha-numeric prefixes
* Remove numbering from sections
* Remove numbering from pages

[Reference Commands](../../wiki/Reference-Commands) (15)
* Insert a new footnote (endnote) at the current location (Ctrl + Alt + F)
* Remove a footnote from either the label or from the footer description (Ctrl + Shift + F)
* Create bidirectional links between pages or paragraphs on a page
* Embed the contents of one page into the current pages
* Insert QR code representation of selected text
* Map Linked Pages to report all pages that link to other pages
* Link to current page from other pages that reference this page title
* Refresh text of hyperlinks back to this page based on its title
* Replace hyperlinked URLs with their Web page titles
* Replace hyperlinked URLs with their downloaded images

[Search commands](../../wiki/Search-Commands) (4)

* Search and replace text on the current page (Ctrl + H)
* Search for keywords and copy or move selected pages (Alt + F)
* Add arbitrary tags to a page (Alt + T)
* Search arbitrary page tags to index, copy, or move selected pages (Ctrl + Alt + T)

[Snippets commands](../../wiki/Snippets-Commands) (16)

* Insert single (Shift + Alt + F11) or double (Shift + Alt + F12) horizontal line
* Insert breadcrumb at top of page
* Insert Table of Contents of headers, pages, or sections
* Insert small or large monthly calendar for a specific month
* Insert sortable date, similar to 2020-12-23 (Ctrl + Shift + D)
* Insert a text box, a single-cell table, and optionally wrap selecting content
* Insert Code box similar to Confluence Code macro, optionally wrap selected content (F6)
* Insert Info/Warn boxes similar to Confluence Info and Warn macros
* Insert collapsable sections to hide secondary or sensitive information
* Insert status labels similar to the Confluence status macro

[Table commands](../../wiki/Table-Commands) (12)

* Calculate Excel-like formulas in tables (F5)
* Delete formula from selected table cells
* Highlight all cells on the page with custom formulas
* Recalculate all formulas in selected tables on the page (Shift + F5)
* Convert select text to a table
* Insert Table Cells, shifting content as expected
* Paste copied table cells by overlaying cells rather than inserting a nested table
* Copy across and copy down to fill all or selected cells with a copy of a cell
* Fill across and fill down (Ctrl + D) to fill all or selected cells with increment values
* Split table

[Extra commands](../../wiki/Extra-Commands) (12)

* Generate a report of the size of notebooks, section, pages, and image and file attachments on pages
* Add special icon to the page title, also appears in page hierarchy
* Collapse the page hierarchy to see only top-level pages
* Prepend page titles with the created date of each page
* Insert pronunciation of words from over a dozen languages
* Merge pages, preserving formatting and position of outlines
* Split current page into multiple pages
* Sort pages, sections, or notebooks
* Start and display a visual timer (Alt + F2) and insert the timer value (F2)
* Toggle strikethrough text next to all completed/incompleted tags
* Report number of words on the page or in the selected region

[Main Menu](../../wiki/Tools) (7)

* Replay the last OneMore action with a quick keyboard shortcut (Alt + Shift + R)
* Import MSWord, PowerPoint, Markdown and others into the current page or a new page
* Import a Web page from a specified URL
* Export the current page or selected pages as HTML, PDF, MSWord, or as raw OneNote XML
* Invoke an external custom plugin to process a page
* View and edit the internal OneNote XML of the current page (Ctrl + Shift + Alt + X)
* Edit OneMore settings
* Check for updates and install upgrades on-demand

[Context Menus](../../wiki#extended-context-menus)
  * Notebook context menu
    * Archive the entire notebook to a zip of HTML files, including all images and attachments
    * Number the sections in the notebook
    * Remove section numbering
  * Section context menu
    * Add the section to the Favorites menu
    * Archive the entire section to a zip of HTML files, including all images and attachments
    * Prefix all page titles with a date stamp representing the data the page was created
  * Section Group context menu
    * Copy section group and its entire contents to another location (OneNote only has a _move_ command)
  * Page context menu
    * Export the page as HTML, PDF, Word, XML, or a .one file
    * Merge two or more selected pages into one page
    * Split the current page on _Heading 1_ boundaries or other options
    * Click in a blank area of the Pages panel to add or remove page numbering
  * Image context menu
    * Add a centered caption to the image
    * Crop and rotate the image
    * Resize the image

### Why?

I wanted something more than what OneNote provided. I was overwhelmed by _OneNote Gem_,
aghast at its bloated useless overloads of what was already otherwise available or features that
I would never use in a million years - and, oh yeah, that price! I admit I was intrigued by the
_Onetastic Macro_ approach but thought it limited and obtuse, decipherable only by programmers.
And both of these charge money for something that should be open source and neither provided exactly
what I wanted. So I did what any self respecting software engineer would do... I built my own.

**Please Support**  
I do this as a hobby. I will never charge you to use OneMore. So it is with great humility and
appreciation that I humbly request that you consider a small donation to support the development
of OneMore. In exchange, I pledge to continue listening with an open mind and to respond to your
questions and tips in a timely manner.

_Click here, click now, click often! >>_  
>  [![Donate](../../wiki/images/Donate.png)](https://paypal.me/stevenmcohn?locale.x=en_US)


### Minimum Prerequisites for Development

* Developed for Windows 10
* Microsoft Visual Studio 2019<sup>1</sup>, C# 7
* Microsoft [Windows 10 SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk/)<sup>2</sup>
* Microsoft [Visual Studio 2019 Installer Projects extension](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects)
* .NET Framework 4.8
* Microsoft OneNote 2016 32-bit or 64-bit

<sup>1</sup>_VSCode cannot be used since it doesn't support COMReference entries in csproj files_ 

<sup>2</sup>_The Windows 10 SDK is required to reference the Windows.winmd meta file located
at "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.**version**.0\Windows.winmd"
where **version** is the version of the SDK you have installed, e.g. 19041. If your SDK has
a different version then you must replace the **Windows** reference in OneMoreAddin.csproj_

Tested recently with:
* Windows 10 21H1 19043.1165
* VS2019 16.10.4
* Win10 SDK 10.0.19041.0
* OneNote 2019/O365 16.0.14326.20164 64-bit

#### Dependencies

* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack) - nuget, MIT license
* [MarkdownDeep](https://github.com/toptensoftware/markdowndeep) - DLL in external folder
* [PuppeteerSharp](https://www.nuget.org/packages/PuppeteerSharp/) - nuget, MIT license


### How to Install OneMore

1. Close OneNote if it is currently running (See below if you need to install OneNote)
2. Download the [latest installer from here](https://github.com/stevencohn/OneMore/releases/latest)
3. Right-click the downloaded installer msi and choose Properties, then tick the Unblock box and click OK
4. Run the installer
   - If OneNote is installed for _all users_ then you must install OneMore for _all users_ as well
5. Run OneNote and enjoy


### How to Install OneNote

Microsoft has been pushing people to use the OneNote app and OneNote online, which suck in my opinion, and have removed OneNote from the Office 2019 installer. But you can still install it after installing Office or even install it standalone!

1. Optionally install Office - do not run the Setup.exe; instead, run Office\Setup64.exe
1. Download OfficeSetup.exe [from here](https://support.microsoft.com/en-us/office/install-or-reinstall-onenote-for-windows-c08068d8-b517-4464-9ff2-132cb9c45c08)
   1. If run standalone, it will install 32-bit OneNote
   1. If run after installing Office, it will install 32 or 64 bit based on the bitness of Office
   1. The 64-bit installer is [here](http://www.onenote.com/download/win32/x64/en-US)
   1. The 32-bit installer is [here](http://www.onenote.com/download/win32/x86/en-US)

---

### Developing OneMore

See the [Developer Notes](../../wiki/~-Developer-Notes) page in the Wiki where I keep a list of 
technical references and information regarding developing and debugging this OneNote add-in.
