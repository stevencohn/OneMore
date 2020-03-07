
OneNote Developer Reference 2013
https://docs.microsoft.com/en-us/office/client-developer/onenote/onenote-developer-reference
https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote

Developing (and Debugging!) OneNote 2010 Add-Ins
http://www.malteahrens.com/#/blog/howto-onenote-dev/

	* To start the debugger, add this to your code:
	  System.Diagnostics.Debugger.Launch();

	  This will cause VS to load the "attach debugger" dialog

Office File Formats and PDF downloads
https://msdn.microsoft.com/en-us/library/cc313105(v=office.12).aspx

Introduction to the Office 2010 Backstage View for Developers (PDF saved)
https://msdn.microsoft.com/en-us/library/ee691833.aspx?f=255&MSPPError=-2147217396#odc_Office2010Introduction2OutSpaceUI_DescriptionsAttributesChildInformation

Adding Custom Galleries to the 2007 Office Fluent User Interface (2007)
https://msdn.microsoft.com/en-us/library/bb736142%28v=office.12%29.aspx?f=255&MSPPError=-2147217396

Ribbon XML Markup Specification
https://docs.microsoft.com/en-us/openspecs/office_standards/ms-customui/d842006e-3187-4f66-a17d-0819a3cc94b5

Creating Custom Ribbon Galleries in Microsoft Excel 2007
https://msdn.microsoft.com/en-us/library/office/dd756403%28v=office.12%29.aspx?f=255&MSPPError=-2147217396

imageMso List (PDF saved)
https://bert-toolkit.com/imagemso-list.html

Top-Most
https://stackoverflow.com/questions/1309855/what-is-powerful-way-to-force-a-form-to-bring-front

High DPI
https://stackoverflow.com/questions/42838120/how-to-proper-auto-scale-toolstrip-in-net

----

* OneNote -> Options -> Advanced -> Other:Show add-in user interface errors

----

Direct development:

The installer registers the addin in the Regstry by pointing to
the installation folder under %ProgramFiles(x86)%\River\OneMore
To avoid continually copying to that folder on every compilation, manually alter the
Registry setting to point to the project output directory, something like...

	Windows Registry Editor Version 5.00

	[HKEY_CLASSES_ROOT\WOW6432Node\CLSID\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}\InprocServer32]
	"CodeBase"="C:\\River\\OneMore\\OneMore\\OneMoreAddIn\\bin\\x86\\Debug\\River.OneMoreAddIn.dll"

cut/paste path into regedit:

	Computer\HKEY_CLASSES_ROOT\WOW6432Node\CLSID\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}\InprocServer32

----

Registry usage:

HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\16.0\OneNote (Load Times)
HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote (LoadBehavior)
HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Office\OneNote (LoadBehavior)

----

EnabledLogging.reg (doesn't actually work :-(

Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\16.0\OneNote\Options\Logging]
"EnableLogging"=dword:00000001
"EnableTextFileLogging"=dword:00000001
"ttidLogObjectModel"=dword:00000001
"ttidLogObjectModelAddins"=dword:00000001
"ttidLogIncludeTimeDateStamp"=dword:00000001
"ttidLogMerge"=dword:00000001
"ttidLogReplicationConcise"=dword:00000001
"ttidLogCellStorageClientRequests"=dword:00000000
"ttidLogNativeReplicator"=dword:00000000
"ttidLogNotebookDiff"=dword:00000000
"ttidLogObjectSpaceStoreCell"=dword:00000000
"ttidLogReplicationScheduler"=dword:00000000
"ttidLogServerFolderReplicator"=dword:00000000
"ttidLogSharePointAndWebDAV"=dword:00000000
"ttidLogEditorsTable"=dword:00000000
"ttidLogSkyDrive"=dword:00000000
"ttidLogMultiRoundTripSuspend"=dword:00000000

