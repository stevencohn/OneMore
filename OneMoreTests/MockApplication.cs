//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.Office.Interop.OneNote;
	using System;
	using System.Linq;
	using System.Xml.Linq;

	public class MockApplication : IApplication, IOneNoteEvents_Event
	{
		private readonly MockWindows windows;

		public MockApplication()
		{
			windows = new MockWindows(this);
		}

		public Windows Windows => windows;

		public bool Dummy1 => throw new NotImplementedException();

		public dynamic COMAddIns => throw new NotImplementedException();

		public dynamic LanguageSettings => throw new NotImplementedException();


		public void GetHierarchy(
			string bstrStartNodeID, HierarchyScope hsScope, out string pbstrHierarchyXmlOut,
			XMLSchema xsSchema = XMLSchema.xs2013)
		{
			var root = XElement.Load(@".\Data\Hierarchy.xml");
			var ns = root.GetNamespaceOfPrefix("one");

			switch (hsScope)
			{
				case HierarchyScope.hsNotebooks:
					root.Elements(ns + "Notebook").Elements().Remove();
					pbstrHierarchyXmlOut = root.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsSections:
					pbstrHierarchyXmlOut = new XElement(ns + "Sections",
						root.Elements(ns + "Notebook")
							.Where(e => e.Attribute("isCurrentlyViewed")?.Value == "true")
							.Elements())
						.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsPages:
					pbstrHierarchyXmlOut = new XElement(ns + "Pages",
						root.Descendants(ns + "Section")
							.Where(e => e.Attribute("isCurrentlyViewed")?.Value == "true")
							.Elements(ns + "Page"))
						.ToString(SaveOptions.DisableFormatting);
					break;

				default:
					pbstrHierarchyXmlOut = null;
					break;
			}
		}


		public void UpdateHierarchy(string bstrChangesXmlIn, XMLSchema xsSchema = XMLSchema.xs2013)
		{
			throw new NotImplementedException();
		}

		public void OpenHierarchy(
			string bstrPath, string bstrRelativeToObjectID, out string pbstrObjectID,
			CreateFileType cftIfNotExist = CreateFileType.cftNone)
		{
			throw new NotImplementedException();
		}

		public void DeleteHierarchy(
			string bstrObjectID, DateTime dateExpectedLastModified, bool deletePermanently = false)
		{
			throw new NotImplementedException();
		}

		public void CreateNewPage(
			string bstrSectionID, out string pbstrPageID, NewPageStyle npsNewPageStyle = NewPageStyle.npsDefault)
		{
			throw new NotImplementedException();
		}

		public void CloseNotebook(string bstrNotebookID, bool force = false)
		{
			throw new NotImplementedException();
		}

		public void GetHierarchyParent(string bstrObjectID, out string pbstrParentID)
		{
			throw new NotImplementedException();
		}

		public void GetPageContent(
			string bstrPageID, out string pbstrPageXmlOut, PageInfo pageInfoToExport = PageInfo.piBasic,
			XMLSchema xsSchema = XMLSchema.xs2013)
		{
			throw new NotImplementedException();
		}

		public void UpdatePageContent(
			string bstrPageChangesXmlIn, DateTime dateExpectedLastModified,
			XMLSchema xsSchema = XMLSchema.xs2013, bool force = false)
		{
			throw new NotImplementedException();
		}

		public void GetBinaryPageContent(string bstrPageID, string bstrCallbackID, out string pbstrBinaryObjectB64Out)
		{
			throw new NotImplementedException();
		}

		public void DeletePageContent(
			string bstrPageID, string bstrObjectID, DateTime dateExpectedLastModified, bool force = false)
		{
			throw new NotImplementedException();
		}

		public void NavigateTo(string bstrHierarchyObjectID, string bstrObjectID = "", bool fNewWindow = false)
		{
			throw new NotImplementedException();
		}

		public void NavigateToUrl(string bstrUrl, bool fNewWindow = false)
		{
			throw new NotImplementedException();
		}

		public void Publish(
			string bstrHierarchyID, string bstrTargetFilePath,
			PublishFormat pfPublishFormat = PublishFormat.pfOneNote, string bstrCLSIDofExporter = "")
		{
			throw new NotImplementedException();
		}

		public void OpenPackage(string bstrPathPackage, string bstrPathDest, out string pbstrPathOut)
		{
			throw new NotImplementedException();
		}

		public void GetHyperlinkToObject(
			string bstrHierarchyID, string bstrPageContentObjectID, out string pbstrHyperlinkOut)
		{
			throw new NotImplementedException();
		}

		public void FindPages(
			string bstrStartNodeID, string bstrSearchString, out string pbstrHierarchyXmlOut,
			bool fIncludeUnindexedPages = false, bool fDisplay = false, XMLSchema xsSchema = XMLSchema.xs2013)
		{
			throw new NotImplementedException();
		}

		public void FindMeta(
			string bstrStartNodeID, string bstrSearchStringName, out string pbstrHierarchyXmlOut,
			bool fIncludeUnindexedPages = false, XMLSchema xsSchema = XMLSchema.xs2013)
		{
			throw new NotImplementedException();
		}

		public void GetSpecialLocation(SpecialLocation slToGet, out string pbstrSpecialLocationPath)
		{
			switch (slToGet)
			{
				case SpecialLocation.slBackUpFolder: pbstrSpecialLocationPath = @"\backup"; break;
				case SpecialLocation.slDefaultNotebookFolder: pbstrSpecialLocationPath = @"\default"; break;
				case SpecialLocation.slUnfiledNotesSection: pbstrSpecialLocationPath = @"\unfiled"; break;
				default: pbstrSpecialLocationPath = "?"; break;
			}
		}

		public void MergeFiles(string bstrBaseFile, string bstrClientFile, string bstrServerFile, string bstrTargetFile)
		{
			throw new NotImplementedException();
		}

		public IQuickFilingDialog QuickFiling()
		{
			throw new NotImplementedException();
		}

		public void SyncHierarchy(string bstrHierarchyID)
		{
			throw new NotImplementedException();
		}

		public void SetFilingLocation(FilingLocation flToSet, FilingLocationType fltToSet, string bstrFilingSectionID)
		{
			throw new NotImplementedException();
		}

		public void MergeSections(string bstrSectionSourceId, string bstrSectionDestinationId)
		{
			throw new NotImplementedException();
		}

		public void GetWebHyperlinkToObject(
			string bstrHierarchyID, string bstrPageContentObjectID, out string pbstrHyperlinkOut)
		{
			throw new NotImplementedException();
		}

		#region unused
		public event IOneNoteEvents_OnNavigateEventHandler OnNavigate;
		public event IOneNoteEvents_OnHierarchyChangeEventHandler OnHierarchyChange;
		#endregion unused
	}
}