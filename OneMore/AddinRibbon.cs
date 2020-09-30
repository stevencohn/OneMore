//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Helpers.Settings;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices.ComTypes;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// IRibbonExtensibility and ribbon handlers
	/// </summary>
	public partial class AddIn
	{

		/// <summary>
		/// IRibbonExtensibility method, returns XML describing the Ribbon customizations.
		/// Called directly by OneNote when initializing the addin.
		/// </summary>
		/// <param name="RibbonID">The ID of the ribbon</param>
		/// <returns>XML starting at the customUI root element</returns>
		public string GetCustomUI(string RibbonID)
		{
			var engines = new SearchEngineProvider().LoadEngines();
			if (engines?.Count == 0)
			{
				return Resx.Ribbon;
			}

			try
			{
				var root = XElement.Parse(Resx.Ribbon);
				var ns = root.GetDefaultNamespace();
				var menu = root.Element(ns + "contextMenus")?.Element(ns + "contextMenu");
				if (menu != null)
				{
					var separator = menu.Elements(ns + "menuSeparator")
						.Where(e => e.Attribute("id").Value == "ctxSeparator")
						.FirstOrDefault();

					XElement content = null;

					if (engines.Count == 1)
					{
						content = MakeSearchButton(engines[0], ns, 0);
					}
					else
					{
						content = new XElement(ns + "menu",
							new XAttribute("id", "ctxSearchMenu"),
							new XAttribute("label", "Search"),
							new XAttribute("imageMso", "RmsInvokeBrowser"),
							new XAttribute("insertBeforeMso", "Cut")
							);

						var count = 0;
						foreach (var engine in engines)
						{
							content.Add(MakeSearchButton(engine, ns, count++));
						}
					}

					if (separator != null)
					{
						separator.AddBeforeSelf(content);
					}
					else
					{
						menu.Add(content);
					}
				}

				//logger.WriteLine(root.ToString());

				return root.ToString(SaveOptions.DisableFormatting);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error extending context menu", exc);
				return Resx.Ribbon;
			}
		}


		private XElement MakeSearchButton(SearchEngine engine, XNamespace ns, int id)
		{
			return new XElement(ns + "button",
				new XAttribute("id", $"ctxSearch{id}"),
				new XAttribute("insertBeforeMso", "Cut"),
				new XAttribute("label", engine.Name),
				new XAttribute("imageMso", "RmsInvokeBrowser"),
				new XAttribute("tag", engine.Uri),
				new XAttribute("onAction", "SearchEngineCmd")
				);
		}


		/// <summary>
		/// Specified as the value of the /customUI@onLoad attribute, called immediately after the
		/// custom ribbon UI is initialized, allowing us to store a reference to the ribbon control.
		/// </summary>
		/// <param name="ribbon">The Ribbon</param>
		public void RibbonLoaded(IRibbonUI ribbon)
		{
			//logger.WriteLine("RibbonLoaded()");
			this.ribbon = ribbon;
		}


		/// <summary>
		/// Specified as the value of the /customUI@loadImage attribute, returns the named image for
		/// a ribbon item; typically a button on the ribbon or one of its sub-menus
		/// </summary>
		/// <param name="imageName">The name of the image to return</param>
		/// <returns>A Bitmap image</returns>
		public IStream GetRibbonImage(string imageName)
		{
			//logger.WriteLine($"GetRibbonImage({imageName})");
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
		 * Note, while very similar to GetRibbonImage, this is called when the OneNote window opens and
		 * when a new OneNote window is opened from there, so we can use this as a hook to be informed
		 * when a new window is opened. Specified in the /ribOneMoreMenu@getImage attribute
		 */
		public IStream GetOneMoreRibbonImage(IRibbonControl control)
		{
			//logger.WriteLine($"GetOneMoreRibbonImage({control.Id})");
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
		/// Not used? getContent="GetItemContent", per item
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public string GetRibbonContent(IRibbonControl control)
		{
			//logger.WriteLine($"GetRibbonContent({control.Id})");
			return null;
		}


		/// <summary>
		/// Not used? getEnabled="GetItemEnabled", per item
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public bool GetRibbonEnabled(IRibbonControl control)
		{
			//logger.WriteLine($"GetRibbonEnabled({control.Id})");
			return true;
		}


		/// <summary>
		/// Specified as the value of the @getLabel attribute for each item to retrieve the
		/// localized text of the item
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns>A string specifying the text of the element</returns>
		public string GetRibbonLabel(IRibbonControl control)
		{
			return ReadString(control.Id + "_Label");
		}


		/// <summary>
		/// Specified as the value of the @getScreentip attribute for each item to retrieve the
		/// localized text of the screentip of the item
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns>A string specifying the screentip text of the element</returns>
		public string GetRibbonScreentip(IRibbonControl control)
		{
			return ReadString(control.Id + "_Screentip");
		}


		private string ReadString(string resId)
		{
			try
			{
				//logger.WriteLine($"GetString({resId})");
				return Resx.ResourceManager.GetString(resId);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
				return $"*{resId}*";
			}
		}
	}
}
