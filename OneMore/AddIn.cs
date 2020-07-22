//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Extensibility;
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Management;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Timers;
	using Forms = System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// This is the OneNote addin component for OneMore functionality
	/// </summary>

	[ComVisible(true)]
	[Guid("88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61")]
	[ProgId("River.OneMoreAddin")]
	public partial class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		private const uint ReasonableClockSpeed = 1800;

		private IRibbonUI ribbon;                   // the ribbon control
		private ILogger logger;                     // our diagnostic logger
		private CommandFactory factory;
		private readonly Process process;           // current process, to kill if necessary
		private List<IDisposable> trash;            // track disposables
		private uint clockSpeed;                    // Mhz of CPU


		// Lifecycle...

		/// <summary>
		/// Initializes the addin
		/// </summary>
		public AddIn()
		{
			//System.Diagnostics.Debugger.Launch();

			logger = Logger.Current;
			trash = new List<IDisposable>();
			process = Process.GetCurrentProcess();

			UIHelper.PrepareUI();

			GetCurrentClockSpeed();

			logger.WriteLine();
			logger.WriteLine($"Starting {process.ProcessName}, process PID={process.Id}, CPU={clockSpeed}Mhz");
		}


		private void GetCurrentClockSpeed()
		{
			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant

			clockSpeed = ReasonableClockSpeed;
			using (var searcher = new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor"))
			{
				foreach (var item in searcher.Get())
				{
					clockSpeed = (uint)item["CurrentClockSpeed"];
					item.Dispose();
				}
			}
		}


		//========================================================================================
		// IDTExtensibility2

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

		public void OnConnection(
				object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			// do not grab a reference to Application here as it tends to prevent OneNote
			// from shutting down. Instead, use our ApplicationManager only as needed.

			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnConnection(ConnectionMode:{ConnectMode},{count})");
		}


		public void OnAddInsUpdate(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OneAddInsUpdate({count})");
		}


		public void OnStartupComplete(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnStartupComplete({count})");
			RegisterHotkeys();
		}

		#region RegisterHotkeys

		private void RegisterHotkeys()
		{
			HotkeyManager.RegisterHotKey(Forms.Keys.F, Hotmods.ControlAlt);
			HotkeyManager.RegisterHotKey(Forms.Keys.F, Hotmods.ControlShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.OemMinus, Hotmods.AltShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.Oemplus, Hotmods.AltShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.F4);
			HotkeyManager.RegisterHotKey(Forms.Keys.V, Hotmods.ControlAlt);
			HotkeyManager.RegisterHotKey(Forms.Keys.H, Hotmods.Control);
			HotkeyManager.RegisterHotKey(Forms.Keys.U, Hotmods.ControlShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.U, Hotmods.ControlAltShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.Oemplus, Hotmods.ControlAlt);
			HotkeyManager.RegisterHotKey(Forms.Keys.OemMinus, Hotmods.ControlAlt);
			HotkeyManager.RegisterHotKey(Forms.Keys.X, Hotmods.ControlAltShift);
			HotkeyManager.RegisterHotKey(Forms.Keys.F8);

			HotkeyManager.HotKeyPressed += HotkeyHandler;
		}


		private void HotkeyHandler(object sender, EventArgs args)
		{
			var a = args as HotkeyEventArgs;
			logger.WriteLine($"HOTKEY called {a.Modifiers}+{a.Key} value:{a.Value:x}");

			switch (a.Value)
			{
				case 0x460003: AddFootnoteCmd(null); break;
				case 0x460006: RemoveFootnoteCmd(null); break;
				case 0xbd0005: InsertHorizontalLineCmd(null); break;
				case 0xbb0005: InsertDoubleHorizontalLineCmd(null); break;
				case 0x730000: NoSpellCheckCmd(null); break;
				case 0x560003: PasteRtfCmd(null); break;
				case 0x480002: SearchAndReplaceCmd(null); break;
				case 0x550007: ToUppercaseCmd(null); break;
				case 0x550006: ToLowercaseCmd(null); break;
				case 0xbb0003: IncreaseFontSizeCmd(null); break;
				case 0xbd0003: DecreaseFontSizeCmd(null); break;
				case 0x580007: ShowXmlCmd(null); break;

				case 0x770000:
					factory.GetCommand<DiagnosticsCommand>().Execute();
					break;
			}
		}

		#endregion RegisterHotkeys


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/* Shutdown functions are called in this order:
		 *
		 * 1. OnBeginShutdown
		 * 2. OnDisconnection
		 */

		public void OnBeginShutdown(ref Array custom)
		{
			int count = custom == null ? 0 : custom.Length;
			logger.WriteLine($"OnBeginShutdown({count})");

			try
			{
				logger.WriteLine("Shutting down UI");

				HotkeyManager.Unregister();

				UIHelper.Shutdown();
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
		}


		public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
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


		private void StopTimer_Elapsed(object sender, ElapsedEventArgs e)
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


		//========================================================================================
		// Ribbon handlers

		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// IRibbonExtensibility...

		/// <summary>
		/// Return XML that describes the Ribbon customizations.  This is called by OneNote
		/// when initializing the addin.
		/// </summary>
		/// <param name="RibbonID"></param>
		/// <returns></returns>

		public string GetCustomUI(string RibbonID)
		{
			//logger.WriteLine($"GetCustomUI({RibbonID})");
			var ribbon = Resx.Ribbon;
			//logger.WriteLine("ribbon=[" + ribbon + "]");
			return ribbon;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Ribbon handlers...

		/// <summary>
		/// Specified in Ribbon.xml, this method is called once the custom ribbon UI is loaded
		/// allowing us to store a reference to the ribbon control.
		/// </summary>
		/// <param name="ribbon"></param>

		public void RibbonLoaded(IRibbonUI ribbon)
		{
			//logger.WriteLine("RibbonLoaded()");
			this.ribbon = ribbon;

			using (var manager = new ApplicationManager())
			{
				var (backupFolder, defaultFolder, unfiledFolder) = manager.GetLocations();
				logger.WriteLine("OneNote backup folder:: " + backupFolder);
				logger.WriteLine("OneNote default folder: " + defaultFolder);
				logger.WriteLine("OneNote unfiled folder: " + unfiledFolder);

				factory = new CommandFactory(logger, ribbon, trash,
					// looks complicated but necessary for this to work
					new Win32WindowHandle(new IntPtr((Int64)manager.WindowHandle)));
			}
		}


		/// <summary>
		/// Specified in Ribbon.xml, this method returns the image to display on the ribbon button
		/// </summary>
		/// <param name="imageName"></param>
		/// <returns></returns>

		public IStream GetImage(string imageName)
		{
			//logger.WriteLine($"GetImage({imageName})");
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

		public string GetItemContent(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemContent({control.Id})");
			return null;
		}


		/// <summary>
		/// getEnabled="GetItemEnabled"
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>

		public bool GetItemEnabled(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemEnabled({control.Id})");
			return true;
		}


		/// <summary>
		/// Ribbon handler called for items with getLabel="GetButtonLabel" attributes.
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns></returns>

		public string GetItemLabel(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemLabel({control.Id})");

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

		public string GetItemScreentip(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemScreentip({control.Id})");
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

		public bool GetItemVisible(IRibbonControl control)
		{
			//logger.WriteLine($"GetItemVisible({control.Id})");
			return true;
		}


		/*
		 * Note this is called when the OneNote window opens and when a new
		 * OneNote window is opened from there, so we can use this as a hook
		 * to know when a new window is opened
		 */

		public IStream GetOneMoreMenuImage(IRibbonControl control)
		{
			//logger.WriteLine($"GetOneMoreMenuImage({control.Id})");

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


		//========================================================================================
		// Addin menu behaviors

		public IStream GetDoubleLineImage(IRibbonControl control)
		{
			//logger.WriteLine($"GetDoubleLineImage({control.Id})");
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
	}
}
