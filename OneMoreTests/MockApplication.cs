//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.Office.Interop.OneNote;
	using System;
	using System.IO;
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bstrStartNodeID">Notebook, section group, or section</param>
		/// <param name="hsScope">Get from start node down to this level</param>
		/// <param name="pbstrHierarchyXmlOut"></param>
		/// <param name="xsSchema"></param>
		public void GetHierarchy(
			string bstrStartNodeID, HierarchyScope hsScope, out string pbstrHierarchyXmlOut,
			XMLSchema xsSchema = XMLSchema.xs2013)
		{
			var hierarchy = XElement.Load(@".\Data\Hierarchy.xml");
			var ns = hierarchy.GetNamespaceOfPrefix("one");

			if (!string.IsNullOrEmpty(bstrStartNodeID))
			{
				hierarchy = hierarchy.Descendants()
					.FirstOrDefault(e => e.Attribute("ID")?.Value == bstrStartNodeID);

				if (hierarchy == null)
				{
					pbstrHierarchyXmlOut = null;
					return;
				}
			}

			switch (hsScope)
			{
				case HierarchyScope.hsNotebooks:
					if (hierarchy.Name.LocalName != "Notebooks")
					{
						pbstrHierarchyXmlOut = null;
						return;
					}
					hierarchy.Elements(ns + "Notebook").Elements().Remove();
					pbstrHierarchyXmlOut = hierarchy.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsSections:
					if (hierarchy.Name.LocalName == "Page")
					{
						pbstrHierarchyXmlOut = null;
						return;
					}
					hierarchy.Descendants(ns + "Section").ToList().ForEach((e) =>
					{
						if (e.HasElements)
							e.Elements().Remove();
					});
					pbstrHierarchyXmlOut = hierarchy.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsPages:
					pbstrHierarchyXmlOut = hierarchy.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsChildren:
					hierarchy.Elements().ToList().ForEach((e) =>
					{
						if (e.HasElements)
							e.Elements().Remove();
					});
					pbstrHierarchyXmlOut = hierarchy.ToString(SaveOptions.DisableFormatting);
					break;

				case HierarchyScope.hsSelf:
					if (hierarchy.HasElements)
					{
						hierarchy.Elements().Remove();
					}
					pbstrHierarchyXmlOut = hierarchy.ToString(SaveOptions.DisableFormatting);
					break;

				default:
					pbstrHierarchyXmlOut = null;
					break;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bstrChangesXmlIn"></param>
		/// <param name="xsSchema"></param>
		public void UpdateHierarchy(string bstrChangesXmlIn, XMLSchema xsSchema = XMLSchema.xs2013)
		{
			//
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bstrPageID"></param>
		/// <param name="pbstrPageXmlOut"></param>
		/// <param name="pageInfoToExport"></param>
		/// <param name="xsSchema"></param>
		public void GetPageContent(
			string bstrPageID, out string pbstrPageXmlOut, PageInfo pageInfoToExport = PageInfo.piBasic,
			XMLSchema xsSchema = XMLSchema.xs2013)
		{
			var hierarchy = XElement.Load(@".\Data\Hierarchy.xml");
			var ns = hierarchy.GetNamespaceOfPrefix("one");

			var name = hierarchy.Descendants(ns + "Page")
				.Where(e => e.Attribute("ID").Value == bstrPageID)
				.Select(e => e.Attribute("name").Value)
				.FirstOrDefault();

			if (string.IsNullOrEmpty(name))
			{
				pbstrPageXmlOut = null;
				return;
			}

			var path = $@".\Data\Page-{name}.xml";
			if (!File.Exists(path))
			{
				pbstrPageXmlOut = null;
				return;
			}

			pbstrPageXmlOut = XElement.Load(path).ToString(SaveOptions.DisableFormatting);
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bstrHierarchyID"></param>
		/// <param name="bstrPageContentObjectID"></param>
		/// <param name="pbstrHyperlinkOut"></param>
		public void GetHyperlinkToObject(
			string bstrHierarchyID, string bstrPageContentObjectID, out string pbstrHyperlinkOut)
		{
			pbstrHyperlinkOut = null;
			if (string.IsNullOrEmpty(bstrHierarchyID))
				return;

			var hierarchy = XElement.Load(@".\Data\Hierarchy.xml");
			var ns = hierarchy.GetNamespaceOfPrefix("one");

			var element = hierarchy.Descendants()
				.FirstOrDefault(e => e.Attribute("ID").Value == bstrHierarchyID);

			if (element == null)
				return;

			var name = element.Attribute("name").Value;
			string path;

			var id = element.Attribute("ID").Value;
			id = id.Substring(0, id.IndexOf('}') + 1);

			if (element.Name.LocalName != "Page")
			{
				path = element.Attribute("path").Value;
				var key = element.Name.LocalName.ToLower();
				pbstrHyperlinkOut = $"onenote:#{name}&{key}-id={id}&end&base-path={path}";
				return;
			}

			path = element.Parent.Attribute("path").Value;
			var sectionId = element.Parent.Attribute("ID").Value;
			sectionId = sectionId.Substring(0, sectionId.IndexOf('}') + 1);
			pbstrHyperlinkOut = $"onenote:#{name}&section-id={sectionId}&page-id={id}&end&base-path={path}";
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