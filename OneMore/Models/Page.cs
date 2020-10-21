//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Media;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a page with helper methods
	/// </summary>
	internal partial class Page
	{

		/// <summary>
		/// Initialize a new instance with the given page
		/// </summary>
		/// <param name="page"></param>
		public Page(XElement page)
		{
			if (page != null)
			{
				Root = page;
				Namespace = page.GetNamespaceOfPrefix("one");

				PageId = page.Attribute("ID")?.Value;
			}
		}


		public bool IsValid => Root != null;


		public string PageId { get; private set; }


		/// <summary>
		/// Gets the root element of the page
		/// </summary>
		public XElement Root { get; private set; }


		/// <summary>
		/// Gest the namespace used to create new elements for the page
		/// </summary>
		public XNamespace Namespace { get; private set; }


		public string PageName
		{
			get
			{
				// account for multiple T runs, e.g., the cursor being in the title
				var titles = Root
					.Elements(Namespace + "Title")
					.Elements(Namespace + "OE")
					.Elements(Namespace + "T")
					.Select(e => e.GetCData().GetWrapper().Value);

				return titles == null ? null : string.Concat(titles);
			}

			set
			{
				// overwrite the entire title
				var title = Root.Elements(Namespace + "Title")
					.Elements(Namespace + "OE").FirstOrDefault();

				if (title != null)
				{
					title.ReplaceNodes(new XElement(Namespace + "T", new XCData(value)));
				}
			}
		}


		/// <summary>
		/// Adds the given content after the selected insertion point; this will not
		/// replace selected regions.
		/// </summary>
		/// <param name="content">The content to add</param>
		public void AddNextParagraph(XElement content)
		{
			var current = Root.Descendants(Namespace + "OE")
				.LastOrDefault(e => e.Elements(Namespace + "T").Attributes("selected").Any(a => a.Value == "all"));

			if (current != null)
			{
				if (content.Name.LocalName != "OE")
				{
					content = new XElement(Namespace + "OE", content);
				}

				current.AddAfterSelf(content);
			}
		}


		/// <summary>
		/// Adds a TagDef to the page and returns its index value. If the tag already exists
		/// the index is returned with no other changes to the page.
		/// </summary>
		/// <param name="symbol">The symbol of the tag</param>
		/// <param name="name">The name to apply to the new tag</param>
		/// <returns>The index of the new tag or of the existing tag with the same symbol</returns>
		public string AddTag(string symbol, string name)
		{
			//<one:TagDef index="0" type="0" symbol="140" fontColor="automatic" highlightColor="none" name="Calculated" />
			var tags = Root.Elements(Namespace + "TagDef");

			int index = 0;
			if (tags?.Any() == true)
			{
				var tag = tags.FirstOrDefault(e => e.Attribute("symbol").Value == symbol);
				if (tag != null)
				{
					return tag.Attribute("index").Value;
				}

				index = tags.Max(e => int.Parse(e.Attribute("index").Value)) + 1;
			}

			Root.AddFirst(new XElement(Namespace + "TagDef",
				new XAttribute("index", index.ToString()),
				new XAttribute("type", "0"),
				new XAttribute("symbol", symbol),
				new XAttribute("fontColor", "automatic"),
				new XAttribute("highlightColor", "none"),
				new XAttribute("name", name)
				));

			return index.ToString();
		}


		public bool ConfirmBodyContext(bool feedback = false)
		{
			var found = Root.Elements(Namespace + "Outline")?
				.Attributes("selected").Any(a => a.Value.Equals("all") || a.Value.Equals("partial"));

			if (found != true)
			{
				if (feedback)
				{
					Logger.Current.WriteLine("Could not confirm body context");
					SystemSounds.Exclamation.Play();
				}

				return false;
			}

			return true;
		}


		public bool ConfirmImageSelected(bool feedback = false)
		{
			var found = Root.Descendants(Namespace + "Image")?
				.Attributes("selected").Any(a => a.Value.Equals("all"));

			if (found != true)
			{
				if (feedback)
				{
					Logger.Current.WriteLine("Could not confirm image selections");
					SystemSounds.Exclamation.Play();
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Adjusts the width of the given page to accomodate the width of the specified
		/// string without wrapping.
		/// </summary>
		/// <param name="line">The string to measure</param>
		/// <param name="fontFamily">The font family name to apply</param>
		/// <param name="fontSize">The font size to apply</param>
		/// <param name="handle">
		/// A handle to the current window; should be: 
		/// (IntPtr)manager.Application.Windows.CurrentWindow.WindowHandle
		/// </param>
		public void EnsurePageWidth(
			string line, string fontFamily, float fontSize, IntPtr handle)
		{
			// detect page width

			var element = Root.Elements(Namespace + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Elements(Namespace + "Size")
				.FirstOrDefault();

			if (element != null)
			{
				var attr = element.Attribute("width");
				if (attr != null)
				{
					var outlinePoints = double.Parse(attr.Value);

					// measure line to ensure page width is sufficient

					using (var g = Graphics.FromHwnd(handle))
					{
						using (var font = new Font(fontFamily, fontSize))
						{
							var stringSize = g.MeasureString(line, font);
							var stringPoints = stringSize.Width * 72 / g.DpiX;

							if (stringPoints > outlinePoints)
							{
								attr.Value = stringPoints.ToString("#0.00");

								// must include isSetByUser or width doesn't take effect!
								if (element.Attribute("isSetByUser") == null)
								{
									element.Add(new XAttribute("isSetByUser", "true"));
								}
							}
						}
					}
				}
			}
		}


		public Style GetQuickStyle(StandardStyles key)
		{
			string name = key.ToName();

			var style = Root.Elements(Namespace + "QuickStyleDef")
				.Where(e => e.Attribute("name").Value == name)
				.Select(p => new Style(new QuickStyleDef(p)))
				.FirstOrDefault();

			if (style == null)
			{
				var quick = key.GetDefaults();

				var sibling = Root.Elements(Namespace + "QuickStyleDef").LastOrDefault();
				if (sibling == null)
				{
					quick.Index = 0;
					Root.AddFirst(quick.ToElement(Namespace));
				}
				else
				{
					quick.Index = int.Parse(sibling.Attribute("index").Value) + 1;
					sibling.AddAfterSelf(quick.ToElement(Namespace));
				}

				style = new Style(quick);
			}

			return style;
		}


		/// <summary>
		/// Construct a list of possible templates from both this page's quick styles
		/// and our own custom style definitions, choosing only Heading styles, all
		/// ordered by the internal Index.
		/// </summary>
		/// <returns>A List of Styles ordered by Index</returns>
		public List<Style> GetQuickStyles()
		{
			// collect all quick style defs

			// going to reference both heading and non-headings
			var styles = Root.Elements(Namespace + "QuickStyleDef")
				.Select(p => new Style(new QuickStyleDef(p)))
				.ToList();

			// tag the headings (h1, h2, h3, ...)
			foreach (var style in styles)
			{
				if (Regex.IsMatch(style.Name, @"h\d"))
				{
					style.StyleType = StyleType.Heading;
				}
			}

			return styles;
		}


		/// <summary>
		/// Gets a Color value specifying the background color of the page
		/// </summary>
		/// <returns></returns>
		public Color GetPageColor(out bool automatic, out bool black)
		{
			black = ApplicationManager.OfficeSetToBlackTheme();

			var color = Root.Element(Namespace + "PageSettings").Attribute("color")?.Value;
			if (string.IsNullOrEmpty(color) || color == "automatic")
			{
				automatic = true;
				return Color.White;
			}

			automatic = false;

			try
			{
				return ColorTranslator.FromHtml(color);
			}
			catch
			{
				return ApplicationManager.OfficeSetToBlackTheme() ? Color.Black : Color.White;
			}
		}


		/// <summary>
		/// Finds the index of the tag by its specified symbol
		/// </summary>
		/// <param name="symbol">The symbol of the tag to find</param>
		/// <returns>The index value or null if not found</returns>
		public string GetTagIndex(string symbol)
		{
			var tag = Root.Elements(Namespace + "TagDef")
				.FirstOrDefault(e => e.Attribute("symbol").Value == symbol);

			if (tag != null)
			{
				return tag.Attribute("index").Value;
			}

			return null;
		}


		/// <summary>
		/// Replaces the selected range on the page with the given content, keeping
		/// the cursor after the newly inserted content.
		/// <para>
		/// This attempts to replicate what Word might do when pasting content in a
		/// document with a selection range.
		/// </para>
		/// </summary>
		/// <param name="page">The page root node</param>
		/// <param name="content">The content to insert</param>
		public void ReplaceSelectedWithContent(XElement content)
		{
			var elements = Root.Descendants(Namespace + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if ((elements.Count() == 1) &&
				(elements.First().GetCData().Value.Length == 0))
			{
				// zero-width selection so insert just before cursor
				elements.First().AddBeforeSelf(content);
			}
			else
			{
				// replace one or more [one:T @select=all] with status, placing cursor after
				var element = elements.Last();
				element.AddAfterSelf(content);
				elements.Remove();

				content.AddAfterSelf(new XElement(Namespace + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)
					));
			}
		}
	}
}
