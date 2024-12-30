//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

//#define DEBUGRIB // uncomment to enable the DebugRibbon() conditional method

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Ribbon;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices.ComTypes;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// IRibbonExtensibility and ribbon handlers
	/// </summary>
	/// <remarks>
	/// idMso, context menus and button identifiers are here
	/// https://github.com/OfficeDev/office-fluent-ui-command-identifiers
	/// and available in the References folder under this sln
	/// </remarks>
	public partial class AddIn
	{
		private XElement engines;
		private XNamespace ns;


		/// <summary>
		/// IRibbonExtensibility method, returns XML describing the Ribbon customizations.
		/// Called directly by OneNote when initializing the addin.
		/// </summary>
		/// <param name="RibbonID">The ID of the ribbon</param>
		/// <returns>XML starting at the customUI root element</returns>
		public string GetCustomUI(string RibbonID)
		{
			logger.WriteLine("building ribbon");

			try
			{
				var provider = new SettingsProvider();

				var root = XElement.Parse(Resx.Ribbon);
				ns = root.GetDefaultNamespace();

				var layout = provider
					.GetCollection(nameof(RibbonBarSheet))
					.Get("layout", "group");

				var tab = XElement.Parse(layout == "tab"
					? Resx.RibbonTabOneMore
					: Resx.RibbonTabHome);

				root.Element(ns + "ribbon").Element(ns + "tabs").Add(tab);

				SetPosition(layout, root, ns, provider);

				AddColorizerCommands(root, provider.GetCollection(nameof(ColorizerSheet)));
				AddProofingCommands(root);

				var contextMenus = root.Element(ns + "contextMenus");
				if (contextMenus == null)
				{
					contextMenus = new XElement(ns + "contextMenus");
					root.Add(contextMenus);
				}

				if (layout == "group")
				{
					var ribbonbar = provider.GetCollection(nameof(RibbonBarSheet));
					if (ribbonbar.Count > 0)
					{
						AddRibbonBarCommands(ribbonbar, root);
					}
				}

				var ccommands = provider.GetCollection(nameof(ContextMenuSheet));
				var searchers = provider.GetCollection(nameof(SearchEngineSheet));

				if (ccommands.Count == 0 && searchers.Count == 0)
				{
					return root.ToString(SaveOptions.DisableFormatting);
				}

				// construct context menu UI...

				var menu = new XElement(ns + "contextMenu",
					new XAttribute("idMso", "ContextMenuText"));

				if (ccommands.Count > 0)
				{
					AddContextMenuCommands(ccommands, root, menu);
				}

				if (searchers.Count > 0)
				{
					AddContextMenuSearchers(searchers, menu);
				}

				// add separator before Cut
				menu.Add(new XElement(ns + "menuSeparator",
					new XAttribute("id", "omContextMenuSeparator"),
					new XAttribute("insertBeforeMso", "Cut")
					));

				contextMenus.Add(menu);

				//logger.WriteLine(root);
				return root.ToString(SaveOptions.DisableFormatting);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building custom UI", exc);
				return XElement.Parse(Resx.Ribbon).ToString(SaveOptions.DisableFormatting);
			}
		}


		private static void SetPosition(
			string layout, XElement root, XNamespace ns, SettingsProvider provider)
		{
			var position = provider
				.GetCollection(nameof(RibbonBarSheet))
				.Get("position", (int)RibbonPosiition.End);

			if (position < (int)RibbonPosiition.End)
			{
				XElement element;
				if (layout == "tab")
				{
					element = root.Elements(ns + "ribbon")
						.Elements(ns + "tabs")
						.Elements(ns + "tab")
						.FirstOrDefault(e => e.Attribute("id").Value == "TabOneMore");
				}
				else
				{
					element = root.Element(ns + "ribbon")
						.Element(ns + "tabs")
						.Elements(ns + "tab")
						.Where(e => e.Attribute("idMso").Value == "TabHome")
						.Elements(ns + "group")
						.FirstOrDefault(e => e.Attribute("id").Value == "ribOneMoreGroup");
				}

				element?.SetAttributeValue("insertAfterMso", ((RibbonPosiition)position).ToString());
			}
		}


		private void AddColorizerCommands(XElement root, SettingsCollection settings)
		{
			logger.WriteLine("building ribbon colorizer commands");

			try
			{
				var menu = root.Descendants(ns + "menu")
					.FirstOrDefault(e => e.Attribute("id").Value == "ribColorizeMenu");

				if (menu == null)
				{
					return;
				}

				var hidden = settings.Get(
					ColorizerSheet.HiddenKey, new XElement(ColorizerSheet.HiddenKey));

				var languages = Colorizer.Colorizer.LoadLanguageNames();
				foreach (var name in languages.Keys)
				{
					var tag = languages[name];

					if (hidden.Element(tag) == null)
					{
						menu.Add(new XElement(ns + "button",
							new XAttribute("id", $"ribColorize{tag}Button"),
							new XAttribute("getImage", "GetColorizeImage"),
							new XAttribute("label", name),
							new XAttribute("tag", tag),
							new XAttribute("onAction", "ColorizeCmd")
							));
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building colorize menu", exc);
			}
		}


		private void AddProofingCommands(XElement root)
		{
			var codes = Office.GetEditingLanguages();
			if (codes == null || codes.Length < 2)
			{
				return;
			}

			logger.WriteLine("building ribbon language proofing commands");

			try
			{
				var anchor = root.Descendants(ns + "menu")
					.FirstOrDefault(e => e.Attribute("id").Value == "ribEditMenu");

				if (anchor == null)
				{
					return;
				}

				var item = new XElement(ns + "menu",
					new XAttribute("id", "ribProofingMenu"),
					new XAttribute("imageMso", "SetLanguage"),
					new XAttribute("getLabel", "GetRibbonLabel")
					);

				foreach (var code in codes)
				{
					var name = CultureInfo.GetCultureInfo(code).DisplayName;
					var id = code.Replace("-", string.Empty);

					item.Add(new XElement(ns + "button",
						new XAttribute("id", $"ribProof{id}Button"),
						new XAttribute("imageMso", "Spelling"),
						new XAttribute("label", name),
						new XAttribute("tag", code),
						new XAttribute("onAction", "ProofingCmd")
						));
				}

				anchor.AddFirst(item);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building ribbon proofing menu", exc);
			}
		}


		private void AddRibbonBarCommands(SettingsCollection ribbonbar, XElement root)
		{
			void AddUsingTemplate(
				XElement group, string lookupID, bool showLabel)
			{
				var template = root.Descendants(ns + "button")
					.FirstOrDefault(e => e.Attribute("id")?.Value == lookupID);

				if (template is not null)
				{
					var button = XElement.Parse(template.ToString());
					button.Attribute("id").Value = $"bar{lookupID.Substring(3)}";

					if (!showLabel)
					{
						button.Add(new XAttribute("showLabel", "false"));
					}

					group.Add(button);
				}
			}


			logger.WriteLine("building ribbon groups");

			var group = root.Descendants(ns + "group")
				.FirstOrDefault(e => e.Attribute("id")?.Value == "ribOneMoreGroup");

			if (group == null)
			{
				return;
			}

			var hashtagCommands = ribbonbar.Get<bool>("hashtagCommands");
			var editCommands = ribbonbar.Get<bool>("editCommands");
			var formulaCommands = ribbonbar.Get<bool>("formulaCommands");

			if (hashtagCommands || editCommands || formulaCommands)
			{
				group.Add(new XElement(ns + "separator", new XAttribute("id", "omRibbonExtensions")));

				if (hashtagCommands)
				{
					var showLabel = !ribbonbar.Get<bool>("hashtagIconsOnly");
					AddUsingTemplate(group, "ribSearchHashtagsButton", showLabel);
					AddUsingTemplate(group, "ribHashtaggerButton", showLabel);
				}

				if (editCommands)
				{
					var showLabel = !ribbonbar.Get<bool>("editIconsOnly");
					AddUsingTemplate(group, "ribDisableSpellCheckButton", showLabel);
					AddUsingTemplate(group, "ribPastRtfButton", showLabel);
					AddUsingTemplate(group, "ribSearchAndReplaceButton", showLabel);
				}

				if (formulaCommands)
				{
					var showLabel = !ribbonbar.Get<bool>("formulaIconsOnly");
					AddUsingTemplate(group, "ribAddFormulaButton", showLabel);
					AddUsingTemplate(group, "ribHighlightFormulaButton", showLabel);
					AddUsingTemplate(group, "ribRecalculateFormulaButton", showLabel);
				}
			}
		}


		private void AddContextMenuCommands(
			SettingsCollection ccommands, XElement root, XElement menu)
		{
			logger.WriteLine("building context menu");

			var keysRoot = ccommands.Get<XElement>("items");
			if (keysRoot == null)
			{
				return;
			}

			var keys = keysRoot.Elements("item");

			foreach (var key in keys.Select(e => e.Value))
			{
				// special case to hide Proofing menu if language set is only 1; because it
				// may have changed since last time user added this to the context menu
				if (key == "ribProofingMenu")
				{
					var langs = Office.GetEditingLanguages();
					if (langs == null || langs.Length < 2)
					{
						continue;
					}
				}

				var element = root.Descendants()
					.FirstOrDefault(e => e.Attribute("id")?.Value == key);

				if (element == null)
				{
					logger.WriteLine($"cannot add {key} command to context menu, element not found");
					continue;
				}

				// deep clone item but must change id and remove getEnabled...

				var item = new XElement(element);

				// cleanup the item itself
				var id = item.Attribute("id");
				if (id == null)
				{
					logger.WriteLine($"cannot add {key} command to context menu, id not found");
					continue;
				}

				// special case to avoid collisions between a top menu and its submenu, such as
				// Edit/Colorize and Colorize both chosen; in this case the submenu, Colorize,
				// will be placed as an item in the context menu but removed from the copy of
				// Edit menu in the context menu...

				if (id.Value == "ribEditMenu")
				{
					if (keys.Any(k => k.Value == "ribColorizeMenu"))
					{
						item.Elements().Where(e => e.Attribute("id")?.Value == "ribColorizeMenu").Remove();
					}

					if (keys.Any(k => k.Value == "ribProofingMenu"))
					{
						item.Elements().Where(e => e.Attribute("id")?.Value == "ribProofingMenu").Remove();
					}
				}

				// fabricate unique IDs to avoid collisions with other items on the ribbon
				// pcm = page context menu menu item
				// pcs = page context menu sub item

				if (id.Value.StartsWith("rib"))
				{
					id.Value = $"pcm{id.Value.Substring(3)}";
				}

				item.Attributes().Where(a => a.Name == "getEnabled" || a.Name == "size").Remove();

				// cleanup all children below the item
				foreach (var node in item.Descendants()
					.Where(e => e.Attributes().Any(a => a.Name == "id")))
				{
					id = node.Attribute("id");
					if (id != null)
					{
						if (id.Value.StartsWith("rib") || id.Value.StartsWith("sep"))
						{
							id.Value = $"pcs{id.Value.Substring(3)}";
						}
					}

					node.Attributes().Where(a => a.Name == "getEnabled" || a.Name == "size").Remove();
				}

				item.Add(new XAttribute("insertBeforeMso", "Cut"));

				if (!menu.Elements().Any(e => e.Attribute("id").Value == item.Attribute("id").Value))
				{
					menu.Add(item);
				}
				else
				{
					logger.WriteLine($"duplicate context menu item {item.Attribute("id").Value}");
				}
			}
		}


		private void AddContextMenuSearchers(
			SettingsCollection ccommands, XElement menu)
		{
			logger.WriteLine("building context menu search engines");

			engines = ccommands.Get<XElement>("engines");

			if (engines == null || !engines.HasElements)
			{
				return;
			}

			var elements = engines.Elements("engine");
			var count = elements.Count();

			XElement content = null;
			if (count == 1)
			{
				content = MakeSearchButton(elements.First(), 0);
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
					content.Add(MakeSearchButton(engine, id++));
				}
			}

			if (content != null)
			{
				menu.Add(content);
			}
		}


		private XElement MakeSearchButton(XElement engine, int id)
		{
			return new XElement(ns + "button",
				new XAttribute("id", $"ctxSearch{id}"),
				new XAttribute("insertBeforeMso", "Cut"),
				new XAttribute("label", engine.Element("name").Value),
				new XAttribute("getImage", "GetRibbonSearchImage"),
				new XAttribute("tag", engine.Element("uri").Value),
				new XAttribute("onAction", "SearchWebCmd")
				);
		}



		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Populates the Favorites dynamic menu
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public string GetFavoritesContent(IRibbonControl control)
		{
			DebugRibbon($"GetFavoritesContent({control.Id}) culture:{AddIn.Culture.Name}");

			// TODO: this doesn't seem to work!
			System.Threading.Thread.CurrentThread.CurrentCulture = AddIn.Culture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = AddIn.Culture;

			return Task.Run(async () =>
			{
				await using var provider = new FavoritesProvider(ribbon);
				var favorites = provider.LoadFavoritesMenu();
				return favorites.ToString(SaveOptions.DisableFormatting);

			}).Result;
		}


		public string GetMyPluginsContent(IRibbonControl control)
		{
			return new PluginsProvider().MakePluginsMenu(ns).ToString(SaveOptions.DisableFormatting);
		}


		/// <summary>
		/// Populates the Snippets dynamic menu
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public string GetMySnippetsContent(IRibbonControl control)
		{
			var snippets = new SnippetsProvider().MakeSnippetsMenu(ns);
			return snippets.ToString(SaveOptions.DisableFormatting);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Specified as the value of the /customUI@onLoad attribute, called immediately after the
		/// custom ribbon UI is initialized, allowing us to store a reference to the ribbon control.
		/// </summary>
		/// <param name="ribbon">The Ribbon</param>
		public void RibbonLoaded(IRibbonUI ribbon)
		{
			DebugRibbon("RibbonLoaded()");
			this.ribbon = ribbon;
		}


		/// <summary>
		/// Loads the image associated with the tagged language.
		/// </summary>
		/// <param name="control">The ribbon button from the Colorize menu</param>
		/// <returns>A Bitmap image</returns>
		public IStream GetColorizeImage(IRibbonControl control)
		{
			DebugRibbon($"GetColorizeImage({control.Tag})");

			IStream stream = null;
			try
			{
				var path = Path.Combine(
					Colorizer.Colorizer.GetColorizerDirectory(),
					$@"Languages\{control.Tag}.png"
					);

				if (!File.Exists(path))
				{
					return null;
				}

				stream = ((Bitmap)Image.FromFile(path)).GetReadOnlyStream();
				trash.Add((IDisposable)stream);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return stream;
		}


		/// <summary>
		/// Specified as the value of the /customUI@loadImage attribute, returns the named image for
		/// a ribbon item; typically a button on the ribbon or one of its sub-menus
		/// </summary>
		/// <param name="imageName">The name of the image to return</param>
		/// <returns>A Bitmap image</returns>
		public IStream GetRibbonImage(string imageName)
		{
			if (Office.IsBlackThemeEnabled(true))
			{
				var darkName = $"Dark{imageName}";
				DebugRibbon($"GetRibbonImage({imageName})");

				try
				{
					if (Resx.ResourceManager.GetObject(darkName) is Bitmap res)
					{
						var stream = res.GetReadOnlyStream();
						trash.Add((IDisposable)stream);
						return stream;
					}
				}
				catch
				{
					logger.WriteLine($"{darkName} resource not found, trying light mode");
				}
			}

			try
			{
				DebugRibbon($"GetRibbonImage({imageName})");

				if (Resx.ResourceManager.GetObject(imageName) is Bitmap res)
				{
					var stream = res.GetReadOnlyStream();
					trash.Add((IDisposable)stream);
					return stream;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			return null;
		}

		public IStream GetRibbonImageByID(IRibbonControl control)
		{
			DebugRibbon($"GetRibbonImageByID({control.Id})");

			var id = control.Id;
			if (!id.StartsWith("rib"))
			{
				id = $"rib{id.Substring(3)}";
			}

			IStream stream = null;
			try
			{
				if (Resx.ResourceManager.GetObject(id) is Bitmap res)
				{
					stream = res.GetReadOnlyStream();
					trash.Add((IDisposable)stream);
					return stream;
				}
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
			DebugRibbon($"GetOneMoreRibbonImage({control.Id})");

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
			DebugRibbon($"GetRibbonContent({control.Id})");
			return null;
		}


		/// <summary>
		/// Not used? getEnabled="GetItemEnabled", per item
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public bool GetRibbonEnabled(IRibbonControl control)
		{
			DebugRibbon($"GetRibbonEnabled({control.Id})");
			return true;
		}


		/// <summary>
		/// Specified as the value of the @getLabel attribute for each item to retrieve the
		/// localized text of the item
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns>A string specifying the text of the element</returns>
		/// <remarks>
		/// This is called each time a menu is opened for each item in that menu.
		/// So we can inject our own customizations, such as the one for ribEditStylesButton
		/// </remarks>
		public string GetRibbonLabel(IRibbonControl control)
		{
			// convert ctx items to rib items so they share the same label
			var id = control.Id;
			var key = id.Substring(0, 3);

			if (key == "pcm" || key == "pcs" || // pcm/pcs - fabricated page context menu items
				key == "ctx" || key == "cts" || // ctx/cts - de-dupping from within ribbon.xml
				key == "bar" ||                 // bar - custom ribbon bar from settings
				key == "ct2")                   // ct2 - used?
			{
				id = $"rib{id.Substring(3)}";
			}

			string label = null;
			if (id == "ribEditStylesButton")
			{
				label = MakeEditStylesButtonLabel();
			}

			label ??= ReadString($"{id}_Label");

			DebugRibbon($"GetRibbonLabel({id}_Label) => [{label}]");
			return label;
		}


		private string MakeEditStylesButtonLabel()
		{
			try
			{
				var theme = new ThemeProvider().Theme;

				return theme == null
					? Resx.ribEditStylesButton_Label
					: string.Format(Resx.ribEditStylesButton_named, theme.Name);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"MakeEditStylesButtonLabel cannot load theme", exc);
				return null;
			}
		}


		/// <summary>
		/// Specified as the value of the @getScreentip attribute for each item to retrieve the
		/// localized text of the screentip of the item
		/// </summary>
		/// <param name="control">The control element with a unique Id.</param>
		/// <returns>A string specifying the screentip text of the element</returns>
		public string GetRibbonScreentip(IRibbonControl control)
		{
			// convert ctx items to rib items so they share the same label
			var id = control.Id;
			if (id.StartsWith("ctx") || id.StartsWith("ct2") || id.StartsWith("bar"))
			{
				id = $"rib{id.Substring(3)}";
			}

			var tip = ReadString($"{id}_Screentip");
			if (string.IsNullOrEmpty(tip))
			{
				tip = GetRibbonLabel(control);
			}

			DebugRibbon($"GetRibbonScreentip({id}_Screentip) => [{tip}]");
			return tip;
		}


		private string ReadString(string resId)
		{
			try
			{
				var s = Resx.ResourceManager.GetString(resId, AddIn.Culture);
				s ??= $"*{resId}*";
				return s.Trim();
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
			DebugRibbon($"GetRibbonSearchImage({control.Tag})");

			if (engines?.HasElements == true)
			{
				var engine = engines.Elements("engine")
					.FirstOrDefault(e => e.Element("uri").Value == control.Tag);

				if (engine is not null)
				{
					var img = engine.Element("image")?.Value;
					if (!string.IsNullOrEmpty(img))
					{
						var bytes = Convert.FromBase64String(img);
						using var stream = new MemoryStream(bytes, 0, bytes.Length);
						return ((Bitmap)(Image.FromStream(stream))).GetReadOnlyStream();
					}
				}
			}

			return Resx.e_Smiley.GetReadOnlyStream();
		}


		// #define DEBUGRIBBON to enable this method; otherwise compiler will remove it entirely
		[Conditional("DEBUGRIB")]
		private void DebugRibbon(string message)
		{
			Logger.Current.WriteLine(message);
		}
	}
}
