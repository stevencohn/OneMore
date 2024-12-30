//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
#pragma warning disable S3881 // IDisposable should be implemented correctly
#pragma warning disable S2583 // Conditionally executed code should be reachable

#define xVerboseDispose

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Schema;
	using Forms = System.Windows.Forms;
	using Resx = Properties.Resources;
#if VerboseDispose
	using System.Diagnostics;
#endif


	/// <summary>
	/// Wraps the OneNote interop API
	/// </summary>
	/// <see cref="https://docs.microsoft.com/en-us/office/client-developer/onenote/application-interface-onenote"/>
	internal class OneNote : IAsyncDisposable, IDisposable
	{
		public enum ExportFormat
		{
			EMF = PublishFormat.pfEMF,
			HTML = PublishFormat.pfHTML,
			MHTML = PublishFormat.pfMHTML,
			OneNote = PublishFormat.pfOneNote,
			OneNote2007 = PublishFormat.pfOneNote2007,
			OneNotePackage = PublishFormat.pfOneNotePackage,
			PDF = PublishFormat.pfPDF,
			Word = PublishFormat.pfWord,
			XPS = PublishFormat.pfXPS,
			XML = 1000,
			Markdown = 1001
		}

		public enum NodeType
		{
			Notebook = HierarchyElement.heNotebooks,
			SectionGroup = HierarchyElement.heSectionGroups,
			Section = HierarchyElement.heSections,
			Page = HierarchyElement.hePages
		}

		public enum PageDetail
		{
			All = PageInfo.piAll,
			Basic = PageInfo.piBasic,
			BinaryData = PageInfo.piBinaryData,
			BinaryDataFileType = PageInfo.piBinaryDataFileType,
			BinaryDataSelection = PageInfo.piBinaryDataSelection,
			FileType = PageInfo.piFileType,
			Selection = PageInfo.piSelection,
			SelectionFileType = PageInfo.piSelectionFileType
		}

		public enum Scope
		{
			Children = HierarchyScope.hsChildren,
			Notebooks = HierarchyScope.hsNotebooks,
			Pages = HierarchyScope.hsPages,
			Sections = HierarchyScope.hsSections,
			Self = HierarchyScope.hsSelf,
			SectionGroups = 100
		}

		public class HierarchyInfo
		{
			public string PageId;       // ID of page if this is a page
			public string TitleId;      // ID of page title OE if not a quick note
			public string SectionId;    // immediate owner regardless of depth (e.g. SectionGroups)
			public string NotebookId;   // ID of owning notebook
			public string Name;         // name of object
			public string Path;         // full path including name
			public string Link;         // onenote: hyperlink to object
			public string Color;        // node color
			public int Size;            // size in bytes of page
			public long Visited;        // last time visited in ms
		}

		public class HierarchyNode
		{
			public string Id;
			public NodeType NodeType;
			public string Name;
			public string Link;
		}

		public class HyperlinkInfo
		{
			public string PageID;       // pageID
			public string SectionID;    // sectionID
			public string HyperID;      // hyperlink section-id or page-id
			public string Name;         // section or page name
			public string Path;         // relative path within current scope (section, notebook)
			public string FullPath;     // full notebook-rooted path
			public string Uri;          // onenote:blah hyperlink
		}

		public const string Prefix = "one";

		private const int MaxInclusiveHResult = -2146231999;
		private const int ObjectDoesNotExist = -2147213292;


		private IApplication onenote;
		private bool disposed = false;
		private readonly ILogger logger;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Constructors...

		/// <summary>
		/// Initialize a new wrapper
		/// </summary>
		public OneNote()
		{
			onenote = ApplicationFactory.CreateApplication();
			logger = Logger.Current;
		}


		/// <summary>
		/// Initialize a new wrapper and return the current page
		/// </summary>
		/// <param name="page">The current Page</param>
		/// <param name="ns">The namespace of the current page</param>
		/// <param name="detail">The desired verbosity of the XML</param>
		public OneNote(out Page page, out XNamespace ns, PageDetail detail = PageDetail.Selection)
			: this()
		{
			// page may be null if in an empty section
			page = Task.Run(async () => { return await GetPage(detail); }).Result;
			ns = page?.Namespace;
		}


		#region Lifecycle
		public void Dispose()
		{
			Dispose(disposing: true);
			// DO NOT call this otherwise OneNote will not shutdown properly
			//GC.SuppressFinalize(this);
		}


		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);
			Dispose(disposing: false);

			// DO NOT call this otherwise OneNote will not shutdown properly
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!disposed)
				{
#if VerboseDispose
					var stack = new StackTrace(true);
					var trace = stack.GetFrames().Skip(1).Aggregate(string.Empty, (a, b) =>
					{
						var filename = b.GetFileName();
						return string.IsNullOrWhiteSpace(filename) ? a :
							$"{a} << {Path.GetFileNameWithoutExtension(filename)}:" +
							$"{b.GetFileLineNumber()}/{b.GetMethod().Name}";
					});

					logger.WriteLine($"OneNote.Dispose{trace}");
#endif
					if (onenote is not null)
					{
						try
						{
							Marshal.ReleaseComObject(onenote);
						}
						catch (Exception exc)
						{
							logger.WriteLine("error releasing onenote", exc);
						}
						finally
						{
							onenote = null;
						}
					}
#if VerboseDispose
					logger.WriteLine($"OneNote.Dispose{trace}");
#endif
					disposed = true;
				}
			}
		}


		protected virtual async ValueTask DisposeAsyncCore()
		{
			await Task.Yield();
		}
		#endregion Lifecycle


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Properties

		/// <summary>
		/// Gets the currently viewed page ID
		/// </summary>
		public string CurrentPageId => onenote.Windows.CurrentWindow?.CurrentPageId;


		/// <summary>
		/// Gets the currently viewed section ID
		/// </summary>
		public string CurrentSectionId => onenote.Windows.CurrentWindow?.CurrentSectionId;


		/// <summary>
		/// Gets the currently viewed notebook ID
		/// </summary>
		public string CurrentNotebookId => onenote.Windows.CurrentWindow?.CurrentNotebookId;


		/// <summary>
		/// Gets or sets whether exceptions are allowed to fall through back to caller or
		/// are caught and reported by this class. This is a special case for some consumers
		/// who wish to handle certain exceptions themselves to serve data management.
		/// </summary>
		public bool FallThrough { get; set; }


		/// <summary>
		/// Gets the active OneNote window as a Win32WindowHandle that can be passed as
		/// the owner parameter to MoreMessageBox Show methods.
		/// </summary>
		public Win32WindowHandle OwnerWindow =>
			new(new IntPtr((long)(IntPtr)onenote.Windows.CurrentWindow.WindowHandle));


		/// <summary>
		/// Gets the Win32 Window associated with the current window's handle
		/// </summary>
		public Forms.IWin32Window Window => Forms.Control.FromHandle(WindowHandle);


		/// <summary>
		/// Gets the number of open OneNote windows
		/// </summary>
		public int WindowCount => (int)onenote.Windows.Count;


		/// <summary>
		/// Gets the handle of the current window
		/// </summary>
		public IntPtr WindowHandle => (IntPtr)onenote.Windows.CurrentWindow.WindowHandle;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Invoke an action with retry
		/// </summary>
		/// <param name="work">The action to invoke</param>
		public async Task<bool> InvokeWithRetry(Action work)
		{
			int retries = 0;

			try
			{
				while (retries < 3)
				{
					try
					{
						work();

						if (retries > 0)
						{
							logger.WriteLine($"completed successfully after {retries} retries");
						}

						retries = int.MaxValue;
					}
					catch (InvalidComObjectException exc)
					{
						retries++;
						var ms = 250 * retries;
						logger.WriteLine(
							$"invalid COM object error, (HResult {exc.HResult:X}), " +
							$"retrying in {ms}ms with new Application object", exc);

						onenote = ApplicationFactory.CreateApplication();
						await Task.Delay(ms);
					}
					catch (COMException exc)
					{
						retries++;
						var ms = 250 * retries;

						if ((uint)exc.HResult == ErrorCodes.hrRpcFailed2)
						{
							// can happen if a paragraph is linked to another paragraph but
							// the first paragraph contains an equation; OneNote API defect!
							logger.WriteLine("RPC error due to bad XML schema, aborting retries");
							return false;
						}
						else if (
							(uint)exc.HResult == ErrorCodes.hrRpcFailed ||
							(uint)exc.HResult == ErrorCodes.hrRpcUnavailable)
						{
							// add extra time for new RPC connection to bind
							ms += 250;
							logger.WriteLine($"RPC error, retrying in {ms}ms", exc);
						}
						else
						{
							// this will include hrCOMBusy and hrObjectMissing
							var desc = $"{exc.ErrorCode:X} {ErrorCodes.GetDescription(exc.ErrorCode)}";
							logger.WriteLine(
								$"error {desc} (HResult {exc.HResult:X}), " +
								$"retyring in {ms}ms with new Application object");
						}

						onenote = ApplicationFactory.CreateApplication();
						await Task.Delay(ms);
					}
					// cancellation tokens will cause ThreadAbort which is normal
					catch (Exception exc) when (exc is not ThreadAbortException)
					{
						logger.WriteLine("error invoking action, aborting retries", exc);
						return false;
					}
				}
			}
			catch (Exception exc) when (exc is not ThreadAbortException)
			{
				logger.WriteLine("error recovering from failure while invoking action", exc);
				return false;
			}

			return retries == int.MaxValue;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Create...

		/// <summary>
		/// Creates a new page in the specified section
		/// </summary>
		/// <param name="sectionId">The section to contain the new page</param>
		/// <param name="pageId">The ID of the new page</param>
		public void CreatePage(string sectionId, out string pageId)
		{
			onenote.CreateNewPage(sectionId, out pageId);
		}


		/// <summary>
		/// Create a new section in the current notebook immediately after the open section
		/// </summary>
		/// <param name="name">The name of the new section</param>
		/// <returns>The Section element</returns>
		public async Task<XElement> CreateSection(string name)
		{
			// find the current section in this notebook (may be in section group)
			var notebook = await GetNotebook();
			var ns = GetNamespace(notebook);
			var current = notebook.Descendants(ns + "Section")
				.FirstOrDefault(e => e.Attribute("isCurrentlyViewed")?.Value == "true");

			XElement parent;
			var sectionIds = new List<string>();

			var section = new XElement(ns + "Section", new XAttribute("name", name));

			if (current == null)
			{
				// add first section to notebook
				notebook.Add(section);
				parent = notebook;
			}
			else
			{
				// get the parent of the current section (may be the notebook or a section group)
				// so we will then only update that parent rather than the entire hierarchy
				parent = current.Parent;

				// collect all section IDs for comparison
				sectionIds = parent.Elements(ns + "Section")
					.Where(e => e.Attribute("isRecycleBin") == null &&
								e.Attribute("isInRecycleBin") == null)
					.Attributes("ID").Select(a => a.Value).ToList();

				// add the new section (won't know the ID yet)
				current.AddAfterSelf(section);
			}

			// udpate the notebook or section group
			UpdateHierarchy(parent);

			// get the parent again so we can find the new section ID
			onenote.GetHierarchy(
				parent.Attribute("ID").Value,
				HierarchyScope.hsSections, out var xml, XMLSchema.xs2013);

			parent = XElement.Parse(xml);

			// compare the new parent with old section IDs to find the new ID
			section = parent.Elements(ns + "Section")
				.Where(e => e.Attribute("isRecycleBin") == null &&
							e.Attribute("isInRecycleBin") == null)
				.FirstOrDefault(e => !sectionIds.Contains(e.Attribute("ID").Value));

			return section;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Get...

		/// <summary>
		/// Get the screen coord bounds of the active OneNote window as a Drawing Rectangle.
		/// </summary>
		/// <returns>The bounds expressed as a Rectangle</returns>
		public System.Drawing.Rectangle GetCurrentMainWindowBounds()
		{
			var handle = (IntPtr)onenote.Windows.CurrentWindow.WindowHandle;
			var parent = handle;
			while (parent != IntPtr.Zero)
			{
				parent = Native.GetParent(handle);
				if (parent != IntPtr.Zero)
				{
					handle = parent;
				}
			}

			var r = new Native.Rectangle();
			Native.GetWindowRect(handle, ref r);
			return new System.Drawing.Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
		}


		/// <summary>
		/// Get the known paths used by OneNote; this is for diagnostic logging
		/// </summary>
		/// <returns></returns>
		public (string backupFolder, string defaultFolder, string unfiledFolder) GetFolders()
		{
			onenote.GetSpecialLocation(SpecialLocation.slBackUpFolder, out var backupFolder);
			onenote.GetSpecialLocation(SpecialLocation.slDefaultNotebookFolder, out var defaultFolder);
			onenote.GetSpecialLocation(SpecialLocation.slUnfiledNotesSection, out var unfiledFolder);
			return (backupFolder, defaultFolder, unfiledFolder);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="nodeId"></param>
		/// <returns></returns>
		public HierarchyNode GetHierarchyNode(string nodeId)
		{
			try
			{
				onenote.GetHierarchy(nodeId, HierarchyScope.hsSelf, out var xml, XMLSchema.xs2013);
				var x = XElement.Parse(xml);

				if (Enum.TryParse(x.Name.LocalName, out NodeType type))
				{
					return new HierarchyNode
					{
						Id = nodeId,
						NodeType = type,
						Name = x.Attribute("name").Value,
						Link = GetHyperlink(nodeId, string.Empty)
					};
				}
			}
			catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
			{
				logger.WriteLine($"could not find nodeID {nameof(GetHierarchyNode)}({nodeId})");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error getting hierarchy for node {nodeId}", exc);
			}

			return null;
		}


		/// <summary>
		/// Gets a onenote:hyperlink to an object on the specified page
		/// </summary>
		/// <param name="pageId">The ID of a page</param>
		/// <param name="objectId">
		/// The ID of an object on the page or string.Empty to link to the page itself
		/// </param>
		/// <returns></returns>
		public string GetHyperlink(string pageId, string objectId)
		{
			try
			{
				onenote.GetHyperlinkToObject(pageId, objectId, out var hyperlink);
				return hyperlink.SafeUrlEncode();
			}
			catch (Exception exc)
			{
				if (exc.HResult == ObjectDoesNotExist)
				{
					// objectIDs are ephemeral, generated on-the-fly from the current machine
					// so will not exist when viewing the same page on a different machine;
					// they are consistent on a single machine, probably using some hardware
					// based heuristics I presume
					logger.WriteLine("GetHyperlink, object does not exist. Possible cross-machine query");
				}
				else
				{
					logger.WriteLine("GetHyperlink error", exc);
					return null;
				}
			}

			// second try to target just page itself to work around cross-machine confusion
			if (!string.IsNullOrEmpty(objectId))
			{
				try
				{
					onenote.GetHyperlinkToObject(pageId, string.Empty, out var hyperlink);
					return hyperlink.SafeUrlEncode();
				}
				catch (Exception exc)
				{
					if (exc.HResult == ObjectDoesNotExist)
					{
						logger.WriteLine("GetHyperlink, object does not exist. Second try failed");
						return null;
					}

					logger.WriteLine("GetHyperlink error", exc);
				}
			}

			return null;
		}


		/// <summary>
		/// Gets a Web hyperlink to an object on the specified hierarchy object
		/// </summary>
		/// <param name="hierarchyID">The ID of a notebook, section, or page</param>
		/// <param name="objectId">
		/// The ID of an object on the page or string.Empty to link to the page itself
		/// </param>
		/// <returns></returns>
		public string GetWebHyperlink(string hierarchyID, string objectId)
		{
			try
			{
				onenote.GetWebHyperlinkToObject(hierarchyID, objectId, out var hyperlink);
				return hyperlink.SafeUrlEncode();
			}
			catch (Exception exc)
			{
				if (exc.HResult == ObjectDoesNotExist)
				{
					// objectIDs are ephemeral, generated on-the-fly from the current machine
					// so will not exist when viewing the same page on a different machine;
					// they are consistent on a single machine, probably using some hardware
					// based heuristics I presume
					logger.WriteLine("GetWebHyperlink, object does not exist. Possible cross-machine query");
					return null;
				}

				logger.WriteLine("GetWebHyperlink error", exc);
				return null;
			}
		}


		/// <summary>
		/// Gets the OneNote namespace for the given element
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public XNamespace GetNamespace(XElement element)
		{
			return element.GetNamespaceOfPrefix(Prefix);
		}


		/// <summary>
		/// Gets the current notebook with a hierarchy of sections
		/// </summary>
		/// <returns>A Notebook element with Section children</returns>
		public async Task<XElement> GetNotebook(Scope scope = Scope.Sections)
		{
			return await GetNotebook(CurrentNotebookId, scope);
		}


		/// <summary>
		/// Get the spcified notebook with a hierarchy of sections
		/// </summary>
		/// <param name="id">The ID of the notebook</param>
		/// <returns>A Notebook element with Section children</returns>
		public async Task<XElement> GetNotebook(string id, Scope scope = Scope.Sections)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}

			XElement root = null;

			await InvokeWithRetry(() =>
			{
				onenote.GetHierarchy(id, (HierarchyScope)scope, out var xml, XMLSchema.xs2013);
				if (!string.IsNullOrEmpty(xml))
				{
					root = XElement.Parse(xml);
				}
			});

			return root;
		}


		/// <summary>
		/// Gets a root note containing Notebook elements
		/// </summary>
		/// <returns>A Notebooks element with Notebook children</returns>
		public async Task<XElement> GetNotebooks(Scope scope = Scope.Notebooks)
		{
			XElement root = null;

			await InvokeWithRetry(() =>
			{
				onenote.GetHierarchy(
					string.Empty, (HierarchyScope)scope, out var xml, XMLSchema.xs2013);

				if (!string.IsNullOrEmpty(xml))
				{
					root = XElement.Parse(xml);
				}
			});

			return root;
		}


		/// <summary>
		/// Gets the current page.
		/// </summary>
		/// <param name="detail">The desired verbosity of the XML</param>
		/// <returns>A Page containing the root XML of the page</returns>
		public async Task<Page> GetPage(PageDetail detail = PageDetail.Selection)
		{
			return await GetPage(CurrentPageId, detail);
		}


		/// <summary>
		/// Gets the specified page.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <param name="detail">The desired verbosity of the XML</param>
		/// <returns>A Page containing the root XML of the page</returns>
		public async Task<Page> GetPage(string pageId, PageDetail detail = PageDetail.All)
		{
			if (string.IsNullOrEmpty(pageId))
			{
				return null;
			}

			try
			{
				string xml = null;
				var success = await InvokeWithRetry(() =>
				{
					onenote.GetPageContent(pageId, out xml, (PageInfo)detail, XMLSchema.xs2013);
				});

				if (success && !string.IsNullOrEmpty(xml))
				{
					return new Page(XElement.Parse(xml));
				}
			}
			catch (Exception exc) when (exc is not ThreadAbortException)
			{
				if (FallThrough)
				{
					throw;
				}

				logger.WriteLine($"error getting page {pageId}", exc);
			}

			return null;
		}


		/// <summary>
		/// Gets the raw Base64 value of the specified binary item on the page
		/// as specified by its callback ID.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <param name="callbackId">The callback ID of the object to retreive</param>
		/// <returns>A string specifying the Base64 value of the object</returns>
		public string GetPageContent(string pageId, string callbackId)
		{
			if (string.IsNullOrEmpty(pageId) || string.IsNullOrEmpty(callbackId))
			{
				return null;
			}

			try
			{
				onenote.GetBinaryPageContent(pageId, callbackId, out string base64);
				return base64;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error getting content p:{pageId} c:{callbackId}", exc);
			}

			return null;
		}


		/// <summary>
		/// Gets the raw XML of the specified page.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <param name="detail">The desired verbosity of the XML</param>
		/// <returns>A string specifying the root XML of the page</returns>
		public string GetPageXml(string pageId, PageDetail detail = PageDetail.Basic)
		{
			if (string.IsNullOrEmpty(pageId))
			{
				return null;
			}

			try
			{
				onenote.GetPageContent(pageId, out var xml, (PageInfo)detail, XMLSchema.xs2013);
				return xml;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error getting page XML {pageId}", exc);
			}

			return null;
		}


		/// <summary>
		/// Gets the name, hierarchy path, and OneNote hyperlink to the current page;
		/// used to build up Favorites
		/// </summary>
		/// <returns></returns>
		public async Task<HierarchyInfo> GetPageInfo(string pageId = null, bool sized = false)
		{
			pageId ??= CurrentPageId;

			var page = await GetPage(pageId, sized ? PageDetail.BinaryData : PageDetail.Basic);
			if (page == null)
			{
				return null;
			}

			var info = new HierarchyInfo
			{
				PageId = pageId,
				TitleId = page.TitleID,
				Name = page.Root.Attribute("name")?.Value,
				Link = GetHyperlink(page.PageId, string.Empty)
			};

			if (sized)
			{
				info.Size = page.Root.ToString(SaveOptions.DisableFormatting).Length;
			}

			var hinfo = GetPageHierarchyInfo(page.PageId);
			info.NotebookId = hinfo.NotebookId;
			info.SectionId = hinfo.SectionId;
			info.Color = hinfo.Color;
			info.Path = $"{hinfo.Path}/{info.Name}";

			return info;
		}


		/// <summary>
		/// Gets the notebook, section, section color, and parent hierarchy path of
		/// the specified page.
		/// </summary>
		/// <param name="pageId">The unique ID of the page</param>
		/// <returns>An incomplete HierarchyInfo</returns>
		public HierarchyInfo GetPageHierarchyInfo(string pageId)
		{
			var info = new HierarchyInfo();

			// parent path
			var builder = new StringBuilder();

			var id = GetParent(pageId);
			while (!string.IsNullOrEmpty(id))
			{
				onenote.GetHierarchy(id, HierarchyScope.hsSelf, out var xml, XMLSchema.xs2013);
				var parent = XElement.Parse(xml);
				var parentName = parent.Attribute("name")?.Value;

				if (parentName != null)
					builder.Insert(0, $"/{parentName}");

				if (parent.Name.LocalName == "Section" && string.IsNullOrEmpty(info.SectionId))
				{
					info.SectionId = parent.Attribute("ID").Value;
					info.Color = parent.Attribute("color")?.Value;
				}
				else if (parent.Name.LocalName == "Notebook")
				{
					info.NotebookId = parent.Attribute("ID").Value;
				}

				id = GetParent(id);
			}

			info.Path = builder.ToString();
			return info;
		}


		/// <summary>
		/// Gets the ID of the parent hierachy object that owns the specified object; used when
		/// copying/moving pages from section to section
		/// </summary>
		/// <param name="objectId">The ID of the object whose parent you want to find</param>
		/// <returns>The ID of the parent or null if not found (e.g. parent of a notebook)</returns>
		public string GetParent(string objectId)
		{
			try
			{
				onenote.GetHierarchyParent(objectId, out var parentId);
				return parentId;
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		/// Gest the current section and its child page hierarchy
		/// </summary>
		/// <returns>A Section element with Page children</returns>
		public async Task<XElement> GetSection()
		{
			return await GetSection(CurrentSectionId);
		}


		/// <summary>
		/// Gest the specified section and its child page hierarchy
		/// </summary>
		/// <returns>A Section element with Page children</returns>
		public async Task<XElement> GetSection(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}

			try
			{
				string xml = null;
				await InvokeWithRetry(() =>
				{
					onenote.GetHierarchy(id, HierarchyScope.hsPages, out xml, XMLSchema.xs2013);
				});

				if (!string.IsNullOrEmpty(xml))
				{
					return XElement.Parse(xml);
				}
			}
			catch (Exception exc) when (exc is not ThreadAbortException)
			{
				if (FallThrough)
				{
					throw;
				}

				logger.WriteLine($"error getting section {id}", exc);
			}

			return null;
		}


		/// <summary>
		/// Gets the name, file path, and OneNote hyperlink to the current section;
		/// used to build up Favorites
		/// </summary>
		/// <returns></returns>
		public async Task<HierarchyInfo> GetSectionInfo(string sectionID = null)
		{
			var secID = sectionID ?? CurrentSectionId;
			var section = await GetSection(secID);
			if (section == null)
			{
				return null;
			}

			var info = new HierarchyInfo
			{
				SectionId = secID,
				Name = section.Attribute("name")?.Value
			};

			// path
			var builder = new StringBuilder();
			builder.Append($"/{info.Name}");

			var id = info.NotebookId = GetParent(secID);
			while (!string.IsNullOrEmpty(id))
			{
				onenote.GetHierarchy(id, HierarchyScope.hsSelf, out var xml, XMLSchema.xs2013);
				var x = XElement.Parse(xml);
				var n = x.Attribute("name")?.Value;

				if (n != null)
					builder.Insert(0, $"/{n}");

				id = GetParent(id);
			}

			info.Path = builder.ToString();

			var sectionId = section.Attribute("ID")?.Value;
			if (sectionId != null)
			{
				info.Link = GetHyperlink(sectionId, string.Empty);
			}

			return info;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Update...

		/// <summary>
		/// Deletes the given object from the specified page
		/// </summary>
		/// <param name="pageId">The ID of the page to modify</param>
		/// <param name="objectId">The ID of the object to remove from the page</param>
		public void DeleteContent(string pageId, string objectId)
		{
			try
			{
				onenote.DeletePageContent(pageId, objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error deleting page object {objectId}", exc);
			}
		}


		/// <summary>
		/// Deletes the given object(s) from the hierarchy; used for merging
		/// </summary>
		/// <param name="element"></param>
		public void DeleteHierarchy(string objectId)
		{
			try
			{
				onenote.DeleteHierarchy(objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error deleting hierarchy object {objectId}", exc);
			}
		}


		/// <summary>
		/// Sync updates to storage
		/// </summary>
		/// <param name="id">
		/// The ID of the notebook to sync or the current notebook by default
		/// </param>
		public async Task Sync(string id = null)
		{
			await InvokeWithRetry(() =>
			{
				onenote.SyncHierarchy(id ?? CurrentNotebookId);
			});
		}


		/// <summary>
		/// Update the current page.
		/// </summary>
		/// <param name="page">A Page</param>
		/// <param name="force">Keep all Outlines to force full page update</param>
		public async Task Update(Page page, bool force = false)
		{
			if (page.HasActiveMedia())
			{
				UI.MoreMessageBox.Show(Window, Resx.HasActiveMedia);
				return;
			}

			// must optimize before we can validate schema...
			page.OptimizeForSave(force);

			if (!ValidateSchema(page.Root))
			{
				return;
			}

			// dateExpectedLastModified is merely a pessimistic-locking safeguard to prevent
			// updating parts of a shared page that have since been updated
			//
			//var lastModTime = element.Attribute("lastModifiedTime") is XAttribute att
			//	? DateTime.Parse(att.Value).ToUniversalTime()
			//	: DateTime.MinValue;

			//logger.WriteLine(page.Root);
			var xml = page.Root.ToString(SaveOptions.DisableFormatting);

			await InvokeWithRetry(() =>
			{
				onenote.UpdatePageContent(xml, DateTime.MinValue, XMLSchema.xs2013, true);
			});
		}


		public static bool ValidateSchema(XElement root)
		{
			var document = new XDocument(root);
			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			var schemas = new XmlSchemaSet();
			schemas.Add(ns.ToString(),
				XmlReader.Create(new StringReader(Resx._0336_OneNoteApplication_2013)));

			var valid = true;
			document.Validate(schemas, (o, e) =>
			{
				Logger.Current.WriteLine($"schema error [{o}] ({o.GetType().FullName})");
				Logger.Current.WriteLine(e.Exception);

				if (o is XAttribute attribute &&
					e.Exception.InnerException?.HResult == MaxInclusiveHResult)
				{
					var exp = attribute.Value.IndexOf('E');
					if (exp > 0)
					{
						var dot = attribute.Value.IndexOf('.');
						if (dot < exp - 1)
						{
							var fix = attribute.Value.Substring(0, dot + 2);
							Logger.Current.WriteLine($"schema error, correcting [{o}] -> adjusted [{fix}]");
							attribute.Value = fix;
							return;
						}
					}
				}

				Logger.Current.WriteLine("schema error, unrecognized");
				valid = false;
			}
			// uncomment this parameter to collect schema validation info for GetSchemaInfo()
			// Note that validation info is not available until after Validate() returns so
			// would need to collect suspect nodes in a List<> and then attempt to correct
			// outside of the Validate callback...
			//, true
			);

			return valid;
		}


		/// <summary>
		/// Update the hierarchy info with the given XML; used for sorting
		/// </summary>
		/// <param name="element"></param>
		public void UpdateHierarchy(XElement element)
		{
			string xml = element.ToString(SaveOptions.DisableFormatting);

			try
			{
				onenote.UpdateHierarchy(xml);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating hierarchy", exc);
				logger.WriteLine(element);
				logger.WriteLine();
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// QuickFiling...

		/// <summary>
		/// A callback delegate for consumers who want to know the selected item in the
		/// QuickFiling dialog.
		/// </summary>
		/// <param name="nodeId"></param>
		public delegate Task SelectLocationCallback(string nodeId);


		/// <summary>
		/// Presents the QuickFiling dialog to the user and invokes the given callback
		/// when completed.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="scope"></param>
		/// <param name="callback"></param>
		public void SelectLocation(
			string title, string description, Scope scope, SelectLocationCallback callback)
		{
			var dialog = onenote.QuickFiling();
			dialog.Title = title;
			dialog.Description = description;
			dialog.ParentWindowHandle = onenote.Windows.CurrentWindow.WindowHandle;

			var restriction = HierarchyElement.heNotebooks;

			switch (scope)
			{
				case Scope.Notebooks:
					dialog.TreeDepth = HierarchyElement.heNotebooks;
					break;
				case Scope.SectionGroups:
					dialog.TreeDepth = HierarchyElement.heSectionGroups;
					dialog.TreeCollapsedState = TreeCollapsedStateType.tcsExpanded;
					dialog.ShowCreateNewNotebook();
					restriction = HierarchyElement.heSectionGroups | HierarchyElement.heNotebooks;
					break;
				case Scope.Sections:
					dialog.TreeDepth = HierarchyElement.heSections;
					restriction = HierarchyElement.heSections;
					break;
				case Scope.Pages:
					dialog.TreeDepth = HierarchyElement.hePages;
					restriction = HierarchyElement.heSectionGroups | HierarchyElement.heNotebooks |
						HierarchyElement.heSections | HierarchyElement.hePages;
					break;
			}

			dialog.AddButton(Resx.word_OK, restriction, restriction, false);

			dialog.Run(new FilingCallback(callback));
		}


		private sealed class FilingCallback : IQuickFilingDialogCallback
		{
			private readonly SelectLocationCallback userCallback;

			public FilingCallback(SelectLocationCallback usercb)
			{
				userCallback = usercb;
			}

			public void OnDialogClosed(IQuickFilingDialog dialog)
			{
				try
				{
					userCallback(dialog.SelectedItem);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error returned from FilingCallback", exc);
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Utilities...

		/// <summary>
		/// Exports the specified page to a file using the given format
		/// </summary>
		/// <param name="pageId">The page ID</param>
		/// <param name="path">The output file path</param>
		/// <param name="format">The format</param>
		public bool Export(string pageId, string path, ExportFormat format)
		{
			try
			{
				onenote.Publish(pageId, path, (PublishFormat)format);
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot publish page {pageId}", exc);
				return false;
			}
		}


		/// <summary>
		/// Imports the specified file under the current section.
		/// </summary>
		/// <param name="path">The path to a .one file</param>
		/// <returns>The ID of the new hierarchy object (pageId)</returns>
		public async Task<string> Import(string path)
		{
			var start = await GetSection();

			// Opening a .one file places its content in the transient OpenSections area
			// with its own notebook structure; need to dive down to find the page...
			//
			// OpenHierarchy, followed by SyncHierarchy should load the .one file and then
			// GetSection would load the hierarchy down to the page level. But the one:Section
			// has an attribute areAllPagesAvailable=false which means the page isn't loaded...
			// Instead use MergeSections to force loading the pages and merge into current section

			string openSectionId = null;

			await InvokeWithRetry(() =>
			{
				onenote.OpenHierarchy(path, null, out openSectionId, CreateFileType.cftSection);
			});

			if (string.IsNullOrEmpty(openSectionId))
			{
				return null;
			}

			await InvokeWithRetry(() =>
			{
				onenote.MergeSections(openSectionId, CurrentSectionId);
			});

			var section = await GetSection();
			var ns = GetNamespace(section);

			// determine newly added pageId by comparing new section against what we started with
			var pageId = section.Descendants(ns + "Page")
				.Select(e => e.Attribute("ID").Value)
				.Except(start.Descendants(ns + "Page").Select(e => e.Attribute("ID").Value))
				.FirstOrDefault();

			return pageId;
		}


		/// <summary>
		/// Forces OneNote to jump to the specified object, onenote Uri, or Web Uri
		/// </summary>
		/// <param name="uri">A pageId, sectionId, notebookId, onenote:URL, or Web URL</param>
		public async Task<bool> NavigateTo(string uri)
		{
			if (uri.StartsWith("onenote:") || uri.StartsWith("http"))
			{
				return await InvokeWithRetry(() =>
				{
					onenote.NavigateToUrl(uri);
				});
			}
			else
			{
				// must be an ID
				return await NavigateTo(uri, string.Empty);
			}
		}


		/// <summary>
		/// Forces OneNote to jump to a specific object on a given page.
		/// </summary>
		/// <param name="pageId">The page ID</param>
		/// <param name="objectId">The object ID</param>
		/// <returns></returns>
		public async Task<bool> NavigateTo(string pageId, string objectId)
		{
			return await InvokeWithRetry(() =>
			{
				onenote.NavigateTo(pageId, objectId);
			});
		}


		/// <summary>
		/// Search pages under the specified hierarchy node using the given query.
		/// </summary>
		/// <param name="nodeId">
		/// Can be String.Empty for all notebooks, CurrentNotebookId, CurrentSectionId,
		/// or CurrentPageId
		/// </param>
		/// <param name="query">The search string</param>
		/// <param name="unindexed">True to include unindexed pages in query</param>
		/// <returns>An hierarchy of pages whose content matches the search string</returns>
		public XElement Search(string nodeId, string query, bool unindexed = false)
		{
			// Windows Search doesn't handle symbols well
			query = Regex.Replace(query, @"[\p{P}<>$^=+]", String.Empty).Trim();

			if (query.IsNullOrEmpty())
			{
				logger.WriteLine("search string is empty after filtering symbols");
				return new XElement("empty");
			}

			try
			{
				onenote.FindPages(nodeId, query, out var xml, unindexed, false, XMLSchema.xs2013);

				var results = XElement.Parse(xml);

				// remove recyclebin nodes
				results.Descendants()
					.Where(n => n.Name.LocalName == "UnfiledNotes" ||
								n.Attribute("isRecycleBin") != null ||
								n.Attribute("isInRecycleBin") != null)
					.Remove();

				return results;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error searching for /{query}/", exc);
				return new XElement("empty");
			}
		}


		/// <summary>
		/// Search the hierarchy node for the specified meta tag
		/// </summary>
		/// <param name="nodeId">The root node: notebook, section, or page</param>
		/// <param name="name">The search string, meta key name</param>
		/// <param name="includeRecycleBin">True to include recycle bin section groups</param>
		/// <returns>A hierarchy XML starting at the given node.</returns>
		public async Task<XElement> SearchMeta(
			string nodeId, string name, bool includeRecycleBin = false)
		{
			string xml = null;

			/*
			 * FindMeta is hardly 100% accurate. It's also not much more efficient than getting
			 * the hierarchy and then filtering on meta elements. So let's do that instead!
			 * 
			await InvokeWithRetry(() =>
			{
				onenote.FindMeta(nodeId, name, out xml, true, XMLSchema.xs2013);
			});
			 */

			await InvokeWithRetry(() =>
			{
				onenote.GetHierarchy(nodeId, HierarchyScope.hsPages, out xml, XMLSchema.xs2013);
			});

			if (string.IsNullOrWhiteSpace(xml))
			{
				// only case was immediately after an Office upgrade but...
				return null;
			}

			XElement hierarchy;
			try
			{
				hierarchy = XElement.Parse(xml);
			}
			catch (Exception exc)
			{
				// possible problem with older schema of serialized Reminder
				logger.WriteLine("error parsing hierarchy XML in SearchMeta()", exc);
				logger.WriteLine($"XML to parse is [{xml}]");
				return null;
			}

			var ns = hierarchy.GetNamespaceOfPrefix(Prefix);

			// prune tree, leaving only pages with named meta element

			hierarchy.Descendants(ns + "Page")
				.Where(e => !e.Elements(ns + "Meta").Attributes("name").Any(a => a.Value == name))
				.Remove();

			hierarchy.Descendants(ns + "Section")
				.Where(e => !e.HasElements)
				.Remove();

			hierarchy.Descendants(ns + "SectionGroup")
				.Where(e => !e.Descendants(ns + "Page").Any() ||
					// ignore recycle bins
					(!includeRecycleBin && e.Attributes("isRecycleBin").Any())
					)
				.Remove();

			hierarchy.Elements(ns + "Notebook")
				.Where(e => !e.HasElements)
				.Remove();

			return hierarchy;
		}


		/// <summary>
		/// Special helper for DiagnosticsCommand
		/// </summary>
		/// <param name="builder"></param>
		public void ReportWindowDiagnostics(ILogger logger)
		{
			var win = onenote.Windows.CurrentWindow;

			logger.WriteLine($"CurrentNotebookId: {win.CurrentNotebookId}");
			logger.WriteLine($"CurrentPageId....: {win.CurrentPageId}");
			logger.WriteLine($"CurrentSectionId.: {win.CurrentSectionId}");
			logger.WriteLine($"CurrentSecGrpId..: {win.CurrentSectionGroupId}");
			logger.WriteLine($"DockedLocation...: {win.DockedLocation}");
			logger.WriteLine($"IsFullPageView...: {win.FullPageView}");
			logger.WriteLine($"IsSideNote.......: {win.SideNote}");

			var bounds = new Native.Rectangle();
			Native.GetWindowRect((IntPtr)win.WindowHandle, ref bounds);
			logger.WriteLine($"bounds...........: {bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");

			logger.WriteLine();

			logger.WriteLine($"Windows ({onenote.Windows.Count})");

			var e = onenote.Windows.GetEnumerator();
			while (e.MoveNext())
			{
				var window = e.Current as Window;

				var threadId = Native.GetWindowThreadProcessId(
					(IntPtr)window.WindowHandle, out var processId);

				logger.Write(window.Active ? "*" : "-");
				logger.Write($" window PID:{processId}, TID:{threadId}");
				logger.Write($" handle:{window.WindowHandle:x}");

				Native.GetWindowRect((IntPtr)window.WindowHandle, ref bounds);
				logger.WriteLine($" bounds:{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}");
			}
		}
	}
}
