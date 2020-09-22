//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
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
				var text = Root.Elements(Namespace + "Title")
					.Elements(Namespace + "OE")
					.Elements(Namespace + "T").FirstOrDefault();

				return text?.GetCData().Value;
			}

			set
			{
				var text = Root.Elements(Namespace + "Title")
					.Elements(Namespace + "OE")
					.Elements(Namespace + "T").FirstOrDefault();

				if (text != null)
				{
					text.GetCData().Value = value;
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
				.Where(e => e.Elements(Namespace + "T").Attributes("selected").Any(a => a.Value == "all"))
				.LastOrDefault();

			if (current != null)
			{
				if (content.Name.LocalName != "OE")
				{
					content = new XElement(Namespace + "OE", content);
				}

				current.AddAfterSelf(content);
			}
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
