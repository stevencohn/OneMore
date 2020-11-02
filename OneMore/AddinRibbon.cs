//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S125        // Sections of code should not be commented out

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices.ComTypes;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// IRibbonExtensibility and ribbon handlers
	/// </summary>
	public partial class AddIn
	{
		private XElement engines;


		/// <summary>
		/// IRibbonExtensibility method, returns XML describing the Ribbon customizations.
		/// Called directly by OneNote when initializing the addin.
		/// </summary>
		/// <param name="RibbonID">The ID of the ribbon</param>
		/// <returns>XML starting at the customUI root element</returns>
		public string GetCustomUI(string RibbonID)
		{
			var provider = new SettingsProvider();

			var ccommands = provider.GetCollection("ContextMenuSheet");
			var searchers = provider.GetCollection("SearchEngineSheet");

			if (ccommands.Count == 0 && searchers.Count == 0)
			{
				return Resx.Ribbon;
			}

			try
			{
				var root = XElement.Parse(Resx.Ribbon);
				var ns = root.GetDefaultNamespace();

				// construct context menu UI
				var menu = new XElement(ns + "contextMenu",
					new XAttribute("idMso", "ContextMenuText"));

				if (ccommands.Count > 0)
				{
					AddContextMenuCommands(ccommands, root, menu);
				}

				if (searchers.Count > 0)
				{
					AddContextMenuSearchers(searchers, menu, ns);
				}

				// add separator before Cut
				menu.Add(new XElement(ns + "menuSeparator",
					new XAttribute("id", "omContextMenuSeparator"),
					new XAttribute("insertBeforeMso", "Cut")
					));

				root.Add(new XElement(ns + "contextMenus", menu));

				logger.WriteLine(menu.ToString());

				return root.ToString(SaveOptions.DisableFormatting);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error extending context menu", exc);
				return Resx.Ribbon;
			}
		}

		private void AddContextMenuCommands(
			SettingsCollection ccommands, XElement root, XElement menu)
		{
			foreach (var key in ccommands.Keys)
			{
				if (ccommands.Get<bool>(key))
				{
					var element = root.Descendants()
						.FirstOrDefault(e => e.Attribute("id")?.Value == key);

					if (element != null)
					{
						// deep clone item but must change id and remove getEnabled...

						var item = new XElement(element);

						// cleanup the item itself
						XAttribute id = item.Attribute("id");
						if (id != null && id.Value.StartsWith("rib"))
						{
							id.Value = $"ctx{id.Value.Substring(3)}";
						}

						var enabled = item.Attribute("getEnabled");
						if (enabled != null) enabled.Remove();

						// cleanup all children below the item
						foreach (var node in item.Descendants()
							.Where(e => e.Attributes().Any(a => a.Name == "id")))
						{
							id = node.Attribute("id");
							if (id != null && id.Value.StartsWith("rib"))
							{
								id.Value = $"ctx{id.Value.Substring(3)}";
							}

							enabled = node.Attribute("getEnabled");
							if (enabled != null) enabled.Remove();
						}

						item.Add(new XAttribute("insertBeforeMso", "Cut"));

						menu.Add(item);
					}
				}
			}
		}


		private void AddContextMenuSearchers(
			SettingsCollection ccommands, XElement menu, XNamespace ns)
		{
			engines = ccommands.Get<XElement>("engines");
			var elements = engines.Elements("engine");
			var count = elements.Count();

			XElement content = null;
			if (count == 1)
			{
				content = MakeSearchButton(elements.First(), ns, 0);
			}
			else if (count > 1)
			{
				content = new XElement(ns + "menu",
					new XAttribute("id", "ctxSearchMenu"),
					new XAttribute("label", "Search"),
					new XAttribute("imageMso", "WebPagePreview"),
					new XAttribute("insertBeforeMso", "Cut")
					);

				var id = 0;
				foreach (var engine in engines.Elements("engine"))
				{
					content.Add(MakeSearchButton(engine, ns, id++));
				}
			}

			if (content != null)
			{
				menu.Add(content);
			}
		}


		private XElement MakeSearchButton(XElement engine, XNamespace ns, int id)
		{
			return new XElement(ns + "button",
				new XAttribute("id", $"ctxSearch{id}"),
				new XAttribute("insertBeforeMso", "Cut"),
				new XAttribute("label", engine.Element("name").Value),
				new XAttribute("getImage", "GetRibbonSearchImage"),
				new XAttribute("tag", engine.Element("uri").Value),
				new XAttribute("onAction", "SearchEngineCmd")
				);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

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
		 * Note, while very similar to GetRibbonImage, this is called when the OneNote opens and
		 * when a new OneNote window is opened from there, so we can use this as a hook to be
		 * informed when a new window is opened. Specified in /ribOneMoreMenu@getImage attribute
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
			// convert ctx items to rib items so they share the same label
			var id = control.Id;
			if (id.StartsWith("ctx")) id = $"rib{id.Substring(3)}";

			return ReadString(id + "_Label");
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


		/// <summary>
		/// Specified as the value of the @getImage attribute for the context menu Search items,
		/// loads an image associated with the given search engine
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns>A steam of the Image to display</returns>
		public IStream GetRibbonSearchImage(IRibbonControl control)
		{
			if (engines.HasElements)
			{
				var engine = engines.Elements("engine")
					.FirstOrDefault(e => e.Element("uri").Value == control.Tag);

				var img = engine.Element("image")?.Value;
				if (!string.IsNullOrEmpty(img))
				{
					var bytes = Convert.FromBase64String(img);
					using (var stream = new MemoryStream(bytes, 0, bytes.Length))
					{
						return ((Bitmap)(Image.FromStream(stream))).GetReadOnlyStream();
					}
				}
			}

			return Resx.Smiley.GetReadOnlyStream();
		}
	}
}
