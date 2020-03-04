# ![logo](Screenshots/Logo.jpg "logo") OneMore - a OneNote Add-in

A OneNote add-in with these features:

* Add icon to page title, also appears in page hierarchy
* Create/edit/apply custom styles
* Collapse the page hierarchy to see only top-level pages
* Change selected text to UPPERCASE
* Change selected text to lowercase
* Disable spell-check on the current page
* Insert single horizontal line
* Insert double horizontal line
* Insert Table of Contents including all headers on page
* Increase/Decrease the font size of all content in the current page
* Search and replace text on the current page
* Trim trailing whitespace from selected text
* View and edit page XML (a diagnostic, debugging, advanced-user tool)

All commands are accessed by two new buttons added to the main ribbon bar.

**Minimum Prerequisites**

* Developed for Windows 10
* Microsoft Visual Studio 2017, C# 7
* Microsoft Visual Studio 2017 Installer Projects extension
* .NET Framework 4.6.1
* Microsoft OneNote 2016 32-bit

Tested recently with Windows 10 1909, VS2019, and OneNote 2019/O365

**Nuget Dependencies**

* MSTest.TestAdapter
* MSTest.TestFramework
* System.ValueType - restore packages before building to avoid compiler errors

---

**OneMore Command Menu**

![Command Menu](Screenshots/MoreMenu.jpg)

**Custom Styles**

![Styles](Screenshots/CustomStyles.jpg)

**Custom Styles Dialog**

![Styles Dialog](Screenshots/CustomStylesDialog.jpg)

**Title Icons Dialog**

![Title Icon Dialog](Screenshots/TItleIconsDialog.jpg)

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

	"CodeBase"="C:\\OneMore\\OneMoreAddIn\\bin\\x86\\Debug\\River.OneMoreAddIn.dll"

*There are additional haphazard notes in the OneMore project folder readme.txt.*


### Debugging

To start the debugger, add this line to your code where you want a breakpoint:

    System.Diagnostics.Debugger.Launch();

This will cause Visual Studio to display the "attach debugger" dialog.
