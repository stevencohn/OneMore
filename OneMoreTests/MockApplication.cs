//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests
{
	using Microsoft.Office.Interop.OneNote;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml.Linq;


	/// <summary>
	/// In-memory implementation of IApplication for unit testing.
	/// Register with ApplicationFactory.RegisterApplication(mock) before running a command.
	/// </summary>
	public class MockApplication : IApplication
	{
		private readonly Dictionary<string, string> pages = new();
		private string hierarchyXml;
		private readonly Dictionary<string, string> hierarchyById = new();
		private readonly Dictionary<string, string> parentById = new();
		private readonly MockWindows windows;

		public string CurrentPageId { get; set; } = "page-1";
		public string CurrentSectionId { get; set; } = "section-1";
		public string CurrentSectionGroupId { get; set; } = string.Empty;
		public string CurrentNotebookId { get; set; } = "notebook-1";


		public MockApplication()
		{
			windows = new MockWindows(this);
			hierarchyXml =
				"<one:Notebooks xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\" />";
		}


		public void SetPage(string pageId, string xml) => pages[pageId] = xml;

		public string GetPage(string pageId) =>
			pages.TryGetValue(pageId, out var xml) ? xml : null;

		public void SetHierarchyXml(string xml) => hierarchyXml = xml;

		/// <summary>
		/// Registers hierarchy XML to return for a specific start node ID, for tests that
		/// need GetHierarchy to return different shapes for different node IDs (e.g. a
		/// notebooks list for the empty/root ID and a single notebook for a specific ID).
		/// Falls back to the default hierarchy XML for unregistered IDs.
		/// </summary>
		public void SetHierarchyXml(string id, string xml) => hierarchyById[id ?? string.Empty] = xml;

		/// <summary>
		/// Registers the parent ID that GetHierarchyParent should return for the given
		/// child ID, for tests that exercise upward hierarchy walks (e.g. section group
		/// ancestry). Unregistered IDs resolve to no parent, same as the default no-op.
		/// </summary>
		public void SetHierarchyParent(string id, string parentId) =>
			parentById[id ?? string.Empty] = parentId;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// IApplication implementation

		// COM properties: do NOT define get_X() methods — define the property only.
		// C# auto-generates get_X() from the property, satisfying the COM accessor contract.

		public Windows Windows => windows;
		public bool Dummy1 => false;
		public object COMAddIns => null;
		public object LanguageSettings => null;


		public void GetPageContent(
			string bstrPageID, out string pbstrPageXmlOut,
			PageInfo pageInfoToExport, XMLSchema xsSchema)
		{
			pages.TryGetValue(bstrPageID, out pbstrPageXmlOut);
		}

		public void GetPageContent(
			string bstrPageID, out string pbstrPageXmlOut, PageInfo pageInfoToExport)
		{
			pages.TryGetValue(bstrPageID, out pbstrPageXmlOut);
		}

		public void GetPageContent(string bstrPageID, out string pbstrPageXmlOut)
		{
			pages.TryGetValue(bstrPageID, out pbstrPageXmlOut);
		}


		public void UpdatePageContent(
			string bstrPageChangesXmlIn, DateTime dateExpectedLastModified,
			XMLSchema xsSchema, bool force)
		{
			StorePageXml(bstrPageChangesXmlIn);
		}

		public void UpdatePageContent(
			string bstrPageChangesXmlIn, DateTime dateExpectedLastModified, XMLSchema xsSchema)
		{
			StorePageXml(bstrPageChangesXmlIn);
		}

		public void UpdatePageContent(string bstrPageChangesXmlIn, DateTime dateExpectedLastModified)
		{
			StorePageXml(bstrPageChangesXmlIn);
		}

		public void UpdatePageContent(string bstrPageChangesXmlIn)
		{
			StorePageXml(bstrPageChangesXmlIn);
		}

		private void StorePageXml(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				var pageId = doc.Root?.Attribute("ID")?.Value;
				if (pageId != null)
				{
					pages[pageId] = xml;
				}
			}
			catch
			{
				// malformed XML in tests is a test bug; swallow to avoid masking the real assertion
			}
		}


		public void GetHierarchy(
			string bstrStartNodeID, HierarchyScope hsScope,
			out string pbstrHierarchyXmlOut, XMLSchema xsSchema)
		{
			pbstrHierarchyXmlOut = ResolveHierarchyXml(bstrStartNodeID);
		}

		public void GetHierarchy(
			string bstrStartNodeID, HierarchyScope hsScope, out string pbstrHierarchyXmlOut)
		{
			pbstrHierarchyXmlOut = ResolveHierarchyXml(bstrStartNodeID);
		}

		private string ResolveHierarchyXml(string startNodeId) =>
			hierarchyById.TryGetValue(startNodeId ?? string.Empty, out var xml) ? xml : hierarchyXml;


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// No-op implementations of remaining IApplication methods

		public void UpdateHierarchy(string bstrChangesXmlIn, XMLSchema xsSchema) { }
		public void UpdateHierarchy(string bstrChangesXmlIn) { }

		public void OpenHierarchy(string bstrPath, string bstrRelativeToObjectID,
			out string pbstrObjectID, CreateFileType cftIfNotExist)
		{
			pbstrObjectID = string.Empty;
		}

		public void OpenHierarchy(string bstrPath, string bstrRelativeToObjectID,
			out string pbstrObjectID)
		{
			pbstrObjectID = string.Empty;
		}

		public void DeleteHierarchy(string bstrObjectID,
			DateTime dateExpectedLastModified, bool deletePermanently)
		{ }

		public void DeleteHierarchy(string bstrObjectID, DateTime dateExpectedLastModified) { }
		public void DeleteHierarchy(string bstrObjectID) { }

		public void CreateNewPage(string bstrSectionID, out string pbstrPageID,
			NewPageStyle npsNewPageStyle)
		{
			pbstrPageID = Guid.NewGuid().ToString("B").ToUpperInvariant();
		}

		public void CreateNewPage(string bstrSectionID, out string pbstrPageID)
		{
			pbstrPageID = Guid.NewGuid().ToString("B").ToUpperInvariant();
		}

		public void CloseNotebook(string bstrNotebookID, bool force) { }
		public void CloseNotebook(string bstrNotebookID) { }

		public void GetHierarchyParent(string bstrObjectID, out string pbstrParentID)
		{
			pbstrParentID = parentById.TryGetValue(bstrObjectID ?? string.Empty, out var parentId)
				? parentId
				: string.Empty;
		}

		public void GetBinaryPageContent(string bstrPageID, string bstrCallbackID,
			out string pbstrBinaryObjectB64Out)
		{
			pbstrBinaryObjectB64Out = string.Empty;
		}

		public void DeletePageContent(string bstrPageID, string bstrObjectID,
			DateTime dateExpectedLastModified, bool force)
		{ }

		public void DeletePageContent(string bstrPageID, string bstrObjectID,
			DateTime dateExpectedLastModified)
		{ }

		public void DeletePageContent(string bstrPageID, string bstrObjectID) { }

		public void NavigateTo(string bstrHierarchyObjectID, string bstrObjectID,
			bool fNewWindow)
		{ }

		public void NavigateTo(string bstrHierarchyObjectID, string bstrObjectID) { }
		public void NavigateTo(string bstrHierarchyObjectID) { }
		public void NavigateToUrl(string bstrUrl, bool fNewWindow) { }
		public void NavigateToUrl(string bstrUrl) { }

		public void Publish(string bstrHierarchyID, string bstrTargetFilePath,
			PublishFormat pfPublishFormat, string bstrCLSIDofExporter)
		{ }

		public void Publish(string bstrHierarchyID, string bstrTargetFilePath,
			PublishFormat pfPublishFormat)
		{ }

		public void Publish(string bstrHierarchyID, string bstrTargetFilePath) { }

		public void OpenPackage(string bstrPathPackage, string bstrPathDest,
			out string pbstrPathOut)
		{
			pbstrPathOut = string.Empty;
		}

		public void GetHyperlinkToObject(string bstrHierarchyID,
			string bstrPageContentObjectID, out string pbstrHyperlinkOut)
		{
			pbstrHyperlinkOut = string.Empty;
		}

		public void FindPages(string bstrStartNodeID, string bstrSearchString,
			out string pbstrHierarchyXmlOut, bool fIncludeUnindexedPages,
			bool fDisplay, XMLSchema xsSchema)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindPages(string bstrStartNodeID, string bstrSearchString,
			out string pbstrHierarchyXmlOut, bool fIncludeUnindexedPages, bool fDisplay)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindPages(string bstrStartNodeID, string bstrSearchString,
			out string pbstrHierarchyXmlOut, bool fIncludeUnindexedPages)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindPages(string bstrStartNodeID, string bstrSearchString,
			out string pbstrHierarchyXmlOut)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindMeta(string bstrStartNodeID, string bstrSearchStringName,
			out string pbstrHierarchyXmlOut, bool fIncludeUnindexedPages, XMLSchema xsSchema)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindMeta(string bstrStartNodeID, string bstrSearchStringName,
			out string pbstrHierarchyXmlOut, bool fIncludeUnindexedPages)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void FindMeta(string bstrStartNodeID, string bstrSearchStringName,
			out string pbstrHierarchyXmlOut)
		{
			pbstrHierarchyXmlOut = string.Empty;
		}

		public void GetSpecialLocation(SpecialLocation slToGet, out string pbstrSpecialLocationPath)
		{
			pbstrSpecialLocationPath = System.IO.Path.GetTempPath();
		}

		public void MergeFiles(string bstrBaseFile, string bstrClientFile,
			string bstrServerFile, string bstrTargetFile)
		{ }

		public IQuickFilingDialog QuickFiling() => null;

		public void SyncHierarchy(string bstrHierarchyID) { }

		public void SetFilingLocation(FilingLocation flToSet,
			FilingLocationType fltToSet, string bstrFilingSectionID)
		{ }

		public void MergeSections(string bstrSectionSourceId, string bstrSectionDestinationId) { }

		public void GetWebHyperlinkToObject(string bstrHierarchyID,
			string bstrPageContentObjectID, out string pbstrHyperlinkOut)
		{
			pbstrHyperlinkOut = string.Empty;
		}
	}


	/// <summary>
	/// Minimal Windows collection that wraps MockApplication's current window info
	/// </summary>
	public class MockWindows : Windows
	{
		private readonly MockApplication app;
		private readonly List<MockWindow> items;

		public MockWindows(MockApplication app)
		{
			this.app = app;
			items = new List<MockWindow> { new MockWindow(app) };
		}

		// COM interface properties — define only the property, not get_X() method
		public Window CurrentWindow => items[0];
		public uint Count => (uint)items.Count;
		public Window this[uint index] => items[(int)(index - 1)];

		public IEnumerator GetEnumerator() => items.GetEnumerator();
	}


	/// <summary>
	/// Minimal Window that surfaces MockApplication's current page/section/notebook IDs
	/// </summary>
	public class MockWindow : Window
	{
		private readonly MockApplication app;

		public MockWindow(MockApplication app)
		{
			this.app = app;
		}

		// COM interface properties — define only the property form
		public string CurrentPageId => app.CurrentPageId;
		public string CurrentSectionId => app.CurrentSectionId;
		public string CurrentSectionGroupId => app.CurrentSectionGroupId;
		public string CurrentNotebookId => app.CurrentNotebookId;
		public ulong WindowHandle => 0x1234;
		public bool FullPageView { get => false; set { } }
		public bool Active { get => true; set { } }
		public DockLocation DockedLocation { get => DockLocation.dlNone; set { } }
		public bool SideNote => false;

		// Window.Application property type is the concrete Application COM class, not IApplication.
		// We can't return our MockApplication here, so use explicit impl returning null.
		Application Window.Application => null;

		public void NavigateTo(string bstrHierarchyObjectID, string bstrObjectID) { }
		public void NavigateToUrl(string bstrUrl) { }
		public void SetDockedLocation(DockLocation dlNewLocation, tagPOINT ptNewWindowLocation) { }
	}
}
