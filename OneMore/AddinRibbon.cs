//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices.ComTypes;
	using Resx = Properties.Resources;


	/// <summary>
	/// IRibbonExtensibility and ribbon handlers
	/// </summary>
	public partial class AddIn
	{

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
					new Win32WindowHandle(new IntPtr((long)manager.WindowHandle)));
			}
		}


		/// <summary>
		/// Specified in Ribbon.xml, this method returns the image to display on the ribbon button
		/// </summary>
		/// <param name="imageName"></param>
		/// <returns></returns>
		public IStream GetImage(string imageName)
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


		/*
		 * Note, while this is very similar to GetImage, this is called when the OneNote window
		 * opens and when a new OneNote window is opened from there, so we can use this as a hook
		 * to know when a new window is opened
		 */
		public IStream GetOneMoreMenuImage(IRibbonControl control)
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
	}
}
