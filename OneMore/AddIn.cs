//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001		// Type is not CLS-compliant
#pragma warning disable IDE0060		// remove unused parameter

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Timers;
	using Extensibility;
	using Microsoft.Office.Core;
	using Resx = Properties.Resources;


	//********************************************************************************************
	// class Addin
	//********************************************************************************************

	[ComVisible(true)]
	[Guid("88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61")]
	[ProgId("River.OneMoreAddin")]
	public class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		private IRibbonUI ribbon;					// reference to ribbon control

		private ILogger logger;						// diagnostic logger
		private CommandFactory factory;
		private readonly Process process;			// current process, to kill if necessary

		private List<IDisposable> trash;


		//========================================================================================
		// Lifecycle
		//========================================================================================

		public AddIn ()
		{
#if WTF
			Debugger.Launch();
#endif
			logger = Logger.Current;
			trash = new List<IDisposable>();
			process = Process.GetCurrentProcess();

			UIHelper.PrepareUI();

			logger.WriteLine();
			logger.WriteLine($"Starting {process.ProcessName}, process PID={process.Id}");
		}


		//========================================================================================
		// Realizations
		//========================================================================================

		#region IDTExtensibility2

		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/* Startup functions are called in this order:
		 *
		 * 1. OnConnection
		 * 2. OnAddInsUpdate
		 * 3. GetCustomUI
		 * 4. OnStartupComplete ?? - Haven't seen this called!
		 */

		/// <summary>
		/// Called upon startup; required to keep a reference to the OneNote application object.
		/// </summary>
		/// <param name="Application"></param>
		/// <param name="ConnectMode"></param>
		/// <param name="AddInInst"></param>
		/// <param name="custom"></param>

		public void OnConnection (
				object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			// do not grab a reference to Application here as it tends to prevent OneNote
			// from shutting down. Instead, use our ApplicationManager only as needed.

			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnConnection(ConnectionMode:{ConnectMode},{count})");
		}


		public void OnAddInsUpdate (ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OneAddInsUpdate({count})");
		}


		public void OnStartupComplete (ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnStartupComplete({count})");
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/* Shutdown functions are called in this order:
		 *
		 * 1. OnBeginShutdown
		 * 2. OnDisconnection
		 */

		public void OnBeginShutdown (ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnBeginShutdown({count})");

			try
			{
				logger.WriteLine("Shutting down UI");
				UIHelper.Shutdown();
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
		}


		public void OnDisconnection (ext_DisconnectMode RemoveMode, ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnDisconnection(RemoveMode:{RemoveMode},{count})");

			try
			{
				if (trash.Count > 0)
				{
					logger.WriteLine($"Disposing {trash.Count} streams");

					foreach (var item in trash)
					{
						item?.Dispose();
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			logger.WriteLine("Closing log");
			logger.Dispose();
			logger = null;

			ribbon = null;
			trash = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var stopTimer = new Timer();
			stopTimer.Elapsed += StopTimer_Elapsed;
			stopTimer.Interval = 2000;
			stopTimer.Start();
		}


		private void StopTimer_Elapsed (object sender, ElapsedEventArgs e)
		{
#if FORCEDKILL
			var procs = Process.GetProcessesByName("ONENOTE");
			if (procs.Length > 0)
			{
				foreach (var proc in procs)
				{
					// TODO: there must be a friendlier way to do this?!
					proc.Kill();
				}
			}
#endif
			try
			{
				if (process != null)
				{
					process.Kill();
					process.Dispose();
				}
			}
			catch
			{
			}
		}

		#endregion IDTExtensibility2

		#region IRibbonExtensibility

		/// <summary>
		/// Return XML that describes the Ribbon customizations.  This is called by OneNote
		/// when initializing the addin.
		/// </summary>
		/// <param name="RibbonID"></param>
		/// <returns></returns>

		public string GetCustomUI (string RibbonID)
		{
			logger.WriteLine($"GetCustomUI({RibbonID})");
			var ribbon = Resx.Ribbon;
			//logger.WriteLine("ribbon=[" + ribbon + "]");
			return ribbon;
		}

		#endregion IRibbonExtensibility


		//========================================================================================
		// Ribbon handlers
		//========================================================================================

		#region Ribbon handlers

		/// <summary>
		/// Specified in Ribbon.xml, this method is called once the custom ribbon UI is loaded
		/// allowing us to store a reference to the ribbon control.
		/// </summary>
		/// <param name="ribbon"></param>

		public void RibbonLoaded (IRibbonUI ribbon)
		{
			logger.WriteLine("RibbonLoaded()");
			this.ribbon = ribbon;

			using (var manager = new ApplicationManager())
			{
				var (backupFolder, defaultFolder, unfiledFolder) = manager.GetLocations();
				logger.WriteLine("OneNote backup folder:: " + backupFolder);
				logger.WriteLine("OneNote default folder: " + defaultFolder);
				logger.WriteLine("OneNote unfiled folder: " + unfiledFolder);

				factory = new CommandFactory(logger, ribbon, trash,
					new Win32WindowHandle(new IntPtr((Int64)manager.Application.Windows.CurrentWindow.WindowHandle)));
			}
		}


		/// <summary>
		/// Specified in Ribbon.xml, this method returns the image to display on the ribbon button
		/// </summary>
		/// <param name="imageName"></param>
		/// <returns></returns>

		public IStream GetImage (string imageName)
		{
			logger.WriteLine($"GetImage({imageName})");
			IStream stream = null;
			try
			{
				stream = ((Bitmap)Resx.ResourceManager.GetObject(imageName)).GetReadOnlyStream();
				trash.Add((IDisposable)stream);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return stream;
		}


		/// <summary>
		/// getContent="GetItemContent"
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>

		public string GetItemContent (IRibbonControl control)
		{
			logger.WriteLine($"GetItemContent({control.Id})");
			return null;
		}


		/// <summary>
		/// getEnabled="GetItemEnabled"
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>

		public bool GetItemEnabled (IRibbonControl control)
		{
			logger.WriteLine($"GetItemEnabled({control.Id})");
			return true;
		}


		/// <summary>
		/// Ribbon handler called for items with getLabel="GetButtonLabel" attributes.
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns></returns>

		public string GetItemLabel (IRibbonControl control)
		{
			logger.WriteLine($"GetItemLabel({control.Id})");

			string label;
			string resId = control.Id + "_Label";
			try
			{
				label = Resx.ResourceManager.GetString(resId);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
				label = "*" + resId;
			}

			return label;
		}


		/// <summary>
		/// Ribbon handler called for items with getScreentip="GetButtonScreentip" attributes.
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns></returns>

		public string GetItemScreentip (IRibbonControl control)
		{
			logger.WriteLine($"GetItemScreentip({control.Id})");
			string resId = control.Id + "_Screentip";

			string label;
			try
			{
				label = Resx.ResourceManager.GetString(resId);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
				label = "*" + resId;
			}

			return label;
		}


		/// <summary>
		/// getVisible="GetItemVisible"
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>

		public bool GetItemVisible (IRibbonControl control)
		{
			logger.WriteLine($"GetItemVisible({control.Id})");
			return true;
		}


		public IStream GetOneMoreMenuImage (IRibbonControl control)
		{
			logger.WriteLine($"GetOneMoreMenuImage({control.Id})");

			IStream stream = null;

			try
			{
				stream = Resx.Logo.GetReadOnlyStream();
				trash.Add((IDisposable)stream);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return stream;
		}

		#endregion Ribbon handlers

		#region Menu behaviors

		public bool EnsureBodyContext(IRibbonControl control)
		{
			return factory.GetCommand<InsertLineCommand>().IsBodyContext();
		}

		public IStream GetDoubleLineImage(IRibbonControl control)
		{
			logger.WriteLine($"GetDoubleLineImage({control.Id})");
			IStream stream = null;
			try
			{
				stream = ((Bitmap)Resx.ResourceManager.GetObject("DoubleLine")).GetReadOnlyStream();
				trash.Add((IDisposable)stream);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return stream;
		}

		#endregion Menu behaviors

		#region Style Gallery
		public int GetStyleGalleryItemCount (IRibbonControl control)
		{
			var count = new StyleProvider().Count;
			logger.WriteLine($"GetStyleGalleryItemCount({control.Id}) = {count}");
			return count;
		}

		public string GetStyleGalleryItemId (IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetStyleGalleryItemId({control.Id}, {itemIndex})");
			return "style_" + itemIndex;
		}

		public IStream GetStyleGalleryItemImage (IRibbonControl control, int itemIndex)
		{
			return factory.GetCommand<GalleryTileFactory>().MakeTile(itemIndex);
		}

		public string GetStyleGalleryItemScreentip (IRibbonControl control, int itemIndex)
		{
			var name = new StyleProvider().GetName(itemIndex);
			//logger.WriteLine($"GetStyleGalleryItemScreentip({control.Id}, {itemIndex}) = \"{name}\"");
			return name;
		}
		#endregion Style Gallery

		#region Favorites handlers

		public string GetFavoritesContent(IRibbonControl control)
		{
			logger.WriteLine($"GetFavoritesContent({control.Id})");
			return new FavoritesProvider(ribbon).GetMenuContent();
		}

		public void AddFavoritePage(IRibbonControl control)
		{
			logger.WriteLine($"AddFavoritePage({control.Id})");
			new FavoritesProvider(ribbon).AddFavorite();
		}

		public void NavigateToFavorite(IRibbonControl control)
		{
			logger.WriteLine($"NavigateToFavorite({control.Tag})");
			factory.GetCommand<NavigateCommand>().Execute(control.Tag);
		}

		public void RemoveFavorite(IRibbonControl control)
		{
			logger.WriteLine($"RemoveFavorite({control.Tag})");
			new FavoritesProvider(ribbon).RemoveFavorite(control.Tag);
		}

		#endregion Favorites handlers


		//========================================================================================
		// More menu handlers
		//========================================================================================

		public void AddFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<AddFootnoteCommand>().Execute();
		}

		public void AddTitleIconCmd (IRibbonControl control)
		{
			factory.GetCommand<AddTitleIconCommand>().Execute();
		}

		public void ApplyStyleCmd (IRibbonControl control, string selectedId, int selectedIndex)
		{
			//logger.WriteLine($"StyleGallerySelected2({control.Id}, {selectedId}, {selectedIndex})");
			factory.GetCommand<ApplyStyleCommand>().Execute(selectedIndex);
		}

		public void CollapseCmd (IRibbonControl control)
		{
			factory.GetCommand<CollapseCommand>().Execute();
		}

		public void DecreaseFontSizeCmd (IRibbonControl control)
		{
			factory.GetCommand<AlterSizeCommand>().Execute(-1);
		}

		public void EditStylesCmd (IRibbonControl control)
		{
			factory.GetCommand<EditStylesCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void NewStyleCmd (IRibbonControl control)
		{
			factory.GetCommand<NewStyleCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void IncreaseFontSizeCmd (IRibbonControl control)
		{
			factory.GetCommand<AlterSizeCommand>().Execute(1);
		}

		public void InsertDoubleHorizontalLineCmd (IRibbonControl control)
		{
			factory.GetCommand<InsertLineCommand>().Execute('═');
		}

		public void InsertHorizontalLineCmd (IRibbonControl control)
		{
			factory.GetCommand<InsertLineCommand>().Execute('─');
		}

		public void InsertTocCmd (IRibbonControl control)
		{
			factory.GetCommand<InsertTocCommand>().Execute();
		}

		public void NoSpellCheckCmd (IRibbonControl control)
		{
			factory.GetCommand<NoSpellCheckCommand>().Execute();
		}

		public void PasteRtfCmd(IRibbonControl control)
		{
			factory.GetCommand<PasteRtfCommand>().Execute();
		}

		public void SearchAndReplaceCmd (IRibbonControl control)
		{
			factory.GetCommand<SearchAndReplaceCommand>().Execute();
		}

		public void ShowAboutCmd (IRibbonControl control)
		{
			factory.GetCommand<ShowAboutCommand>().Execute();
		}

		public void ShowXmlCmd (IRibbonControl control)
		{
			factory.GetCommand<ShowXmlCommand>().Execute();
		}

		public void SortCmd(IRibbonControl control)
		{
			factory.GetCommand<SortCommand>().Execute();
		}

		public void ToLowercaseCmd (IRibbonControl control)
		{
			factory.GetCommand<ToCaseCommand>().Execute(false);
		}

		public void ToUppercaseCmd (IRibbonControl control)
		{
			factory.GetCommand<ToCaseCommand>().Execute(true);
		}

		public void TrimCmd(IRibbonControl control)
		{
			factory.GetCommand<TrimCommand>().Execute();
		}
	}
}
