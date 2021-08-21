# ![logo](../../wiki/images/Logo.jpg "logo") OneMore - a OneNote Add-in

A OneNote add-in with powerful yet simple and effective features.

See the [**project wiki**](../../wiki) for full details. Meanwhile, here's some pleasant reading material...

* Integrated with the OneNote ribbon, context menus, and [keyboard shortcuts](../../wiki#keys) for easy access
* [Customize the Text context menu](../../wiki/Settings) to add OneMore commands or a a custom search engine
* Apply [colorized syntax highlighting](../../wiki/Edit-Commands) to snippets of source code
* Create/edit/apply [custom styles](../../wiki/Custom-Styles) with advanced options
* Manage menu of [Favorites](../../wiki/Favorites) for one-click link to your most referenced pages
* Save and reuse [custom snippets](../../wiki/Favorites) of content anywhere on any page
* Add [formulas](../../wiki/Table-Commands) to table cell using Excel-like expressions


*Want more from OneMore? OneMore has more...* [Click here to see a list of all features](#features)

![screenshot](../../wiki/images/Screenshot.png)

<a name="features"></a>
## Features

[Clean commands](../../wiki/Clean-Commands)

* Change spacing between sentences to one or two spaces
* Clear background color and reset text color of selected text
* Remove author/citation information from paragraphs
* Remove pasted screen clipping and URL citation lines from page
* Remove empty headers and "extra" empty lines between paragraphs
* Remove spacing between paragraphs and headings
* Show/Hide the page date and time stamps under the title on page or all pages in section
* Trim leading or trailing whitespace from selected text

[Custom Styles](../../wiki/Custom-styles)

* Manage custom style themes. Save, load separate themes
* Apply custom styles to selected text
* Apply an entire theme to a page
* Change page background to any custom color with dark-mode awareness

[Edit Commands](../../wiki/Edit-Commands)
* Colorize selected text as a chosen programming language - _syntax highlighting_
* Disable spell-check on the current page (F4)
* Paste Rich Text (preserve colors when pasting code from Visual Studio)<sup>1</sup> (Ctrl+Alt+V)
* Change selected text to UPPERCASE, lowercase, Or Title Case
* Increase/Decrease the font size of all content on the current page
* Highlight selected text using a rotating array of colors
* Invert text selection
* Expand or Collapse indented outlines, save and restore outlining
* Join Paragraph to remove soft-breaks and join lines into a single flowing paragraph

[Favorites and Custom Snippets](../../wiki/Favorites)

* Add any page in OneNote to the Favorites menu for quick access
* Sort or reorder favorites using the Favorites Manager
* Save and reuse custom snippets of content anywhere on any page
* Add a link to a special page containing all OneNote and OneMore keyboard shortcuts

[Image commands](../../wiki/Image-Commands)

* Add caption to a selected images
* Precisely crop a selected image
* Resize a selected image or all images on the page

[Numbering commands](../../wiki/Numbering-Commands)

* Number page headings with numeric or alpha-numeric outline prefixes
* Number all sections in the current notebook
* Number all pages in the current section with numeric or alpha-numeric prefixes
* Remove numbering from sections
* Remove numbering from pages

[Reference Commands](../../wiki/Reference-Commands)
* Insert a new footnote (endnote) at the current location
* Remove a footnote from either the label or from the footer description
* Create bi-directional links between pages or paragraphs on a page
* Link to current page from other pages that reference this page title
* Map Linked Pages to report all pages that link to other pages
* Replace hyperlinked URLs with their Web page titles
* Replace hyperlinked URLs with their downloaded images

[Search commands](../../wiki/Search-Commands)

* Search and replace text on the current page (Ctrl+H)
* Search for keywords and copy or move selected pages (Alt + F)
* Add arbitrary tags to a page
* Search arbitrary page tags to index, copy, or move selected pages

[Snippets commands](../../wiki/Snippets-Commands)

* Insert single or double horizontal line
* Insert Table of Contents of headers, pages, or sections
* Insert small or large monthly calendar for a specific month
* Insert sortable date, similar to 2020-12-23
* Insert Info/Warn boxes similar to Confluence Info and Warn macros
* Insert single-cell table, optionally wrap selecting content
* Insert Code box similar to Confluence Code macro, optionally wrap selected content
* Insert collapsable sections to hide secondary or sensitive information
* Insert status labels similar to the Confluence status macro

[Table commands](../../wiki/Table-Commands)

* Calculate Excel-like formulas in tables (F5)
* Delete formula from selected table cells
* Highlight all cells on the page with custom formulas
* Recalculate all formulas in selected tables on the page (Shift + F5)
* Convert select text to a table
* Insert Table Cells, shifting content as expected
* Paste copied table cells by overlaying cells rather than inserting a nested table
* Split table

[Extra commands](../../wiki/Extra-Commands)

* Add special icon to the page title, also appears in page hierarchy
* Collapse the page hierarchy to see only top-level pages
* Copy Section Groups and all child sections and pages to other notebooks and section groups
* Insert pronunciation of words from over a dozen languages
* Merge pages, preserving formatting and position of outlines
* Prepend page titles with the created date of each page
* Sort pages, sections, or notebooks
* Split current page into multiple pages
* Toggle strikethrough text next to all completed/incompleted tags

[Tools](../../wiki/Tools)

* Analyze the size of notebooks, section, pages, and image and file attachments on pages
* Archive a section or an entire notebook to a zip file with attachments
* Import MSWord, PowerPoint, Markdown and others into the current page or a new page
* Import a Web page from a specified URL
* Export the current page or selected pages as HTML, PDF, MSWord, or as raw OneNote XML
* Invoke an external custom plugin to process a page
* Replay the last OneMore action with a quick keyboard shortcut
* View and edit the internal OneNote XML of the current page
* Check for updates and install upgrades on-demand

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
* Microsoft Visual Studio 2019, C# 7
* Microsoft [Visual Studio 2019 Installer Projects extension](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects)
* .NET Framework 4.8
* Microsoft OneNote 2016 32-bit or 64-bit

_Note that VSCode cannot be used since it doesn't support COMReference entries in csproj files_ 

Tested recently with Windows 10 2004 (19041.450), VS2019, and OneNote 2019/O365

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

---

### Developing OneMore

See the [Developer Notes](../../wiki/~-Developer-Notes) page in the Wiki where I keep a list of 
technical references and information regarding developing and debugging this OneNote add-in.
