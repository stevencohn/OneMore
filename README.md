# ![logo](Screenshots/Logo.jpg "logo") OneMore - a OneNote Add-in

A OneNote add-in with the following primary features:

* Create/edit/apply **custom styles** with advanced options
* Disable **spell-check** on the current page
* Manage menu of **Favorite** pages
* Paste Rich Text (preserve colors when **pasting code** from Visual Studio)
* **Search and replace** text on the current page
* Add or remove **footnotes** (endnotes)

*And these secondary features:*

* Add special icon to page title, also appears in page hierarchy
* Collapse the page hierarchy to see only top-level pages
* Change selected text to UPPERCASE or lowercase
* Insert single or double horizontal line
* Insert **Table of Contents** including all headers on page
* Increase/Decrease the font size of all content on the current page
* Sort pages, sections, or notebooks
* Trim trailing whitespace from selected text
* View and edit page XML (a diagnostic, debugging, advanced-user tool)


All commands are accessed from a small group added to the main ribbon bar and 
most also have their own key bindings as follows:

| Command                       | Key Binding
| ----------------------------- | -----------
| Add footnote                  | Ctrl + Alt + F
| Remove footnote               | Ctrl + Shift + Alt + F
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

|   |
| - | -
| **OneMore Command Menu**                               | **Favorites Menu**
| ![Command Menu](Screenshots/MoreMenu.png)              | ![Favorites Menu](Screenshots/FavoritesMenu.png)
| **Custom Styles**                                      | **Custom Styles Dialog**
| ![Styles](Screenshots/CustomStyles.png)                | ![Styles Dialog](Screenshots/CustomStylesDialog.png)
| **Title Icons Dialog**                                 | **Sort Dialog**
| ![Title Icon Dialog](Screenshots/TItleIconsDialog.png) | ![Sort Dialog](Screenshots/SortDialog.png)

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
