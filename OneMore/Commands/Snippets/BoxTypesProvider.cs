//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Loads, saves, and resolves the box-type catalog (built-in info/note/warn plus any
	/// user-defined custom types) used by InsertInfoBoxCommand and the Snippets settings sheet.
	/// </summary>
	internal class BoxTypesProvider
	{
		/// <summary>
		/// Fixed symbol font for all box types; not user-configurable.
		/// </summary>
		public const string SymbolFont = "Segoe UI Emoji";

		private const float DefaultBoxWidth = 600f;

		// display order of the built-in types, matching the design mockup
		private static readonly string[] BuiltinIds = { "info", "warn", "note" };

		// parsed once per process rather than on every GetDefault() call
		private static readonly JObject Themes = JObject.Parse(Resx.InfoBoxThemes);

		private readonly SettingsProvider provider;


		/// <summary>
		/// Initialize a new provider backed by its own SettingsProvider; use this for one-off
		/// reads/writes outside the Settings dialog (e.g. from a ribbon command).
		/// </summary>
		public BoxTypesProvider() : this(new SettingsProvider())
		{
		}


		/// <summary>
		/// Initialize a new provider backed by the given SettingsProvider, so edits share the
		/// same in-memory settings root as the rest of a SettingsDialog session (the dialog
		/// itself calls provider.Save() once, after every sheet's CollectSettings() runs).
		/// </summary>
		public BoxTypesProvider(SettingsProvider provider)
		{
			this.provider = provider;
		}


		/// <summary>
		/// Loads the full working list of box types for editing in the Settings sheet:
		/// the three built-ins (customized values if saved, otherwise shipped defaults)
		/// followed by any custom types, in creation order.
		/// </summary>
		public List<BoxType> LoadAll()
		{
			var element = GetSettings().Get<XElement>("boxTypes");

			var saved = element?.Elements("boxType")
				.Select(ParseElement)
				.ToDictionary(b => b.Id);

			var list = new List<BoxType>();

			foreach (var id in BuiltinIds)
			{
				list.Add(saved != null && saved.TryGetValue(id, out var box)
					? box
					: GetDefault(id));
			}

			if (saved != null)
			{
				list.AddRange(saved.Values.Where(b => !b.IsBuiltin));
			}

			return list;
		}


		/// <summary>
		/// Returns the shipped (as-installed) values for one of the three built-in types,
		/// used to seed the catalog and to implement "Reset to default".
		/// </summary>
		public BoxType GetDefault(string id)
		{
			var theme = Themes[id];
			if (theme is null)
			{
				return null;
			}

			return new BoxType
			{
				Id = id,
				Title = Resx.ResourceManager.GetString(theme["titlex"].ToString(), AddIn.Culture),
				Shading = theme["shading"].ToString(),
				Symbol = theme["symbol"].ToString(),
				SymbolColor = theme["symbolColor"].ToString(),
				SymbolSize = int.Parse(theme["symbolSize"].ToString(), CultureInfo.InvariantCulture),
				TitleColor = theme["titleColor"].ToString(),
				TextColor = theme["textColor"].ToString(),
				IsBuiltin = true
			};
		}


		/// <summary>
		/// Runtime lookup used by InsertInfoBoxCommand, independent of whether the Settings
		/// sheet has ever been opened: returns the saved customization for the given id if
		/// one exists, otherwise falls back to the shipped default. Works for custom ids too.
		/// </summary>
		public BoxType GetTheme(string id)
		{
			var element = GetSettings().Get<XElement>("boxTypes");

			var found = element?.Elements("boxType")
				.FirstOrDefault(e => e.Attribute("id")?.Value == id);

			return found != null ? ParseElement(found) : GetDefault(id);
		}


		/// <summary>
		/// Gets the global box width (in points) applied to all info/note/warn/custom boxes.
		/// </summary>
		public float GetBoxWidth()
		{
			return GetSettings().Get("boxWidth", DefaultBoxWidth);
		}


		/// <summary>
		/// Persists the box types and the given box width. A built-in type that still
		/// matches its shipped default (unmodified) is not written at all - it keeps
		/// resolving through GetDefault() at load time, so it automatically picks up any
		/// future change to the shipped defaults; each built-in is judged independently, so
		/// customizing one doesn't force the other two into the settings file. Custom types
		/// have no "default" to fall back to, so they're always written. Does not save the
		/// settings file itself — the caller (typically SettingsDialog.OK) is responsible for
		/// calling provider.Save() once, after every sheet has collected its settings.
		/// </summary>
		public void SaveAll(IEnumerable<BoxType> types, float boxWidth)
		{
			var settings = GetSettings();

			var toSave = types.Where(t => !t.IsBuiltin || !MatchesDefault(t)).ToList();

			if (toSave.Count > 0)
			{
				var element = new XElement("boxTypes",
					toSave.Select(t => new XElement("boxType",
						new XAttribute("id", t.Id),
						new XAttribute("title", t.Title),
						new XAttribute("shading", t.Shading),
						new XAttribute("symbol", t.Symbol),
						new XAttribute("symbolColor", t.SymbolColor),
						new XAttribute("symbolSize", t.SymbolSize),
						new XAttribute("titleColor", t.TitleColor),
						new XAttribute("textColor", t.TextColor),
						new XAttribute("builtin", t.IsBuiltin)
						)));

				settings.Add("boxTypes", element);
			}
			else
			{
				settings.Remove("boxTypes");
			}

			settings.Add("boxWidth", (int)boxWidth);
			provider.SetCollection(settings);
		}


		/// <summary>
		/// True if every field of a built-in box type still matches its shipped default.
		/// </summary>
		private bool MatchesDefault(BoxType t)
		{
			var d = GetDefault(t.Id);
			return d != null
				&& t.Title == d.Title
				&& t.Shading == d.Shading
				&& t.Symbol == d.Symbol
				&& t.SymbolColor == d.SymbolColor
				&& t.SymbolSize == d.SymbolSize
				&& t.TitleColor == d.TitleColor
				&& t.TextColor == d.TextColor;
		}


		/// <summary>
		/// Builds ribbon menu buttons for every custom (non-builtin) box type, to be merged
		/// into the "My Snippets" dynamic menu content.
		/// </summary>
		public IEnumerable<XElement> MakeCustomBoxMenuItems(XNamespace ns)
		{
			foreach (var box in LoadAll().Where(b => !b.IsBuiltin))
			{
				yield return new XElement(ns + "button",
					new XAttribute("id", $"ribCustomBox{box.Id}"),
					new XAttribute("label", box.Title),
					new XAttribute("imageMso", "Info"),
					new XAttribute("tag", box.Id),
					new XAttribute("onAction", "InsertCustomBoxCmd")
					);
			}
		}


		private SettingsCollection GetSettings()
		{
			return provider.GetCollection(nameof(SnippetsSheet));
		}


		private static BoxType ParseElement(XElement e)
		{
			return new BoxType
			{
				Id = e.Attribute("id")?.Value,
				Title = e.Attribute("title")?.Value,
				Shading = e.Attribute("shading")?.Value,
				Symbol = e.Attribute("symbol")?.Value,
				SymbolColor = e.Attribute("symbolColor")?.Value,
				SymbolSize = int.Parse(
					e.Attribute("symbolSize")?.Value ?? "22", CultureInfo.InvariantCulture),
				TitleColor = e.Attribute("titleColor")?.Value,
				TextColor = e.Attribute("textColor")?.Value,
				IsBuiltin = bool.TryParse(e.Attribute("builtin")?.Value, out var builtin) && builtin
			};
		}
	}
}
