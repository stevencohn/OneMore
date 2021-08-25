//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class Theme
	{
		private readonly XElement root;
		private readonly XNamespace ns;
		private readonly string name;
		private readonly bool dark;


		/// <summary>
		/// Initialize a new instance by deserializing the given theme XML.
		/// </summary>
		public Theme(XElement root, string key)
		{
			Key = key;

			this.root = root;
			ns = root.GetDefaultNamespace();

			if (!root.GetAttributeValue("name", out name, key))
			{
				root.Add(new XAttribute("name", name));
			}

			root.GetAttributeValue("dark", out dark, false);
		}


		/// <summary>
		/// Initialize a new theme
		/// </summary>
		/// <param name="styles">The styles to store in this theme</param>
		/// <param name="key">The access key for the theme</param>
		/// <param name="name">The display name of the theme</param>
		/// <param name="dark">True if intended for a dark background</param>
		public Theme(List<Style> styles, string key, string name, bool dark)
		{
			Key = key;
			this.name = name;
			this.dark = dark;

			root = new XElement("Theme",
				new XAttribute("name", name),
				new XAttribute("dark", dark.ToString())
				);

			ns = root.GetDefaultNamespace();

			ReplaceStyles(styles);
		}


		/// <summary>
		/// Gets the key for this theme. Used as the file name
		/// </summary>
		public string Key { get; private set; }


		/// <summary>
		/// Gets the display name of the theme
		/// </summary>
		public string Name => name;


		/// <summary>
		/// Gets a Boolean value indicating if this theme is intended for
		/// use on dark background pages
		/// </summary>
		public bool Dark => dark;


		/// <summary>
		/// Gets the count of styles in this theme
		/// </summary>
		/// <returns>An integer specifying the count</returns>
		public int GetCount()
		{
			return root.Elements(ns + "Style").Count();
		}


		/// <summary>
		/// Gets the name of the indexed style
		/// </summary>
		/// <param name="index">The index of the style to name</param>
		/// <returns>The name of the specified style</returns>
		public string GetName(int index)
		{
			return root.Elements(ns + "Style")
				.ElementAt(index)
				.Attribute("name").Value;
		}


		/// <summary>
		/// Gets the list of style records ready for saving
		/// </summary>
		/// <returns>A List of StyleRecord items</returns>
		public List<StyleRecord> GetRecords()
		{
			return root.Elements(ns + "Style").ToList()
				.ConvertAll(e => new StyleRecord(e))
				.OrderBy(e => e.Index).ToList();
		}


		/// <summary>
		/// Gets one style definition by its index value; used for the Style Gallery
		/// </summary>
		/// <param name="index">The index of the style to retrieve</param>
		/// <returns>A Style</returns>
		public Style GetStyle(int index)
		{
			return new Style(new StyleRecord(
				root.Elements(ns + "Style").ElementAt(index)
				));
		}


		/// <summary>
		/// Gets the list of Styles described by this theme.
		/// </summary>
		/// <returns>A List of Style items</returns>
		public List<Style> GetStyles()
		{
			return root.Elements(ns + "Style")
				.ToList()
				.ConvertAll(e => new Style(new StyleRecord(e)));
		}


		/// <summary>
		/// Add a new style or repalce an existing style by its display name.
		/// Used when creating a single new style
		/// </summary>
		/// <param name="style">The style to add or update</param>
		public void ReplaceStyle(Style style)
		{
			var suspect = root.Elements(ns + "Style")
				.FirstOrDefault(e => e.Attribute("name").Value == style.Name);

			if (suspect != null)
			{
				// replace named style with new style, adopting its index value
				style.Index = int.Parse(suspect.Attribute("index").Value);
				suspect.ReplaceWith(new StyleRecord(style).ToXElement());
			}
			else
			{
				// calculate new index
				style.Index = root.Elements(ns + "Style")
					.Max(e => int.Parse(e.Attribute("index").Value)) + 1;

				root.Add(new StyleRecord(style).ToXElement());
			}
		}


		/// <summary>
		/// Replaces all styles in this theme with the given list of styles; used when
		/// editing an entire theme at once.
		/// </summary>
		/// <param name="styles"></param>
		public void ReplaceStyles(List<Style> styles)
		{
			var elements = root.Elements(ns + "Style");
			if (elements.Any())
			{
				elements.Remove();
			}

			var records = styles.ConvertAll(e => new StyleRecord(e)).OrderBy(e => e.Index);
			foreach (var record in records)
			{
				root.Add(record.ToXElement());
			}
		}
	}
}
