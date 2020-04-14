//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Apply the user-defined custom styles to the page; this is done by apply the styles
	/// directly to the QuickStyleDefs declarations at the top of the page XML
	/// </summary>
	internal class ApplyStylesCommand : Command
	{

		private Page page;
		private Stylizer stylizer;


		public ApplyStylesCommand() : base()
		{
			// using blank Style just so we have a valid Stylizer
			stylizer = new Stylizer(new Style());
		}


		public void Execute()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					page = new Page(manager.CurrentPage());
					if (page != null)
					{
						var styles = new StyleProvider().GetStyles();

						if (ApplyStyles(styles))
						{
							ApplyToLists(styles);

							manager.UpdatePageContent(page.Root);
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(ApplyStylesCommand)}", exc);
			}
		}


		private bool ApplyStyles(List<Style> styles)
		{
			var applied = false;

			var ns = page.Namespace;

			var quickStyles = page.Root.Elements(ns + "QuickStyleDef");
			if (quickStyles?.Any() == true)
			{
				var foundP = false;

				foreach (var quick in quickStyles)
				{
					var name = quick.Attribute("name").Value;

					// only affect the first QuickStyleDef[@name='p']
					if (!foundP || name != "p")
					{
						var style = FindStyle(styles, name);
						if (style != null)
						{
							//logger.WriteLine(
							//	$"~ name:{quick.Attribute("name").Value} style:{style.Name}");

							// could use QuickStyleDef class here but this is faster
							// that replacing the element...

							quick.Attribute("font").Value = style.FontFamily;

							// TODO: why does OneNote screw these up?
							//quick.Attribute("spaceBefore").Value = style.SpaceBefore;
							//quick.Attribute("spaceAfter").Value = style.SpaceAfter;

							quick.Attribute("fontColor").Value = style.Color;
							quick.Attribute("highlightColor").Value = style.Highlight;

							if (name == "p")
							{
								if (!foundP)
								{
									quick.Attribute("fontSize").Value = style.FontSize;
									foundP = true;
								}
							}
							else
							{
								quick.Attribute("fontSize").Value = style.FontSize;
							}

							applied = true;
						}

						var index = quick.Attribute("index").Value;
						ClearInlineStyles(index, name == "p");
					}
				}
			}

			return applied;
		}

		private Style FindStyle(List<Style> styles, string name)
		{
			Style style = null;

			switch (name)
			{
				case "p":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "normal")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "body")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "p");
					break;

				case "cite":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "citation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "cite");
					break;

				case "blockquote":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "quote")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "quotation")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "blockquote");
					break;

				case "code":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "code")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "source code");
					break;

				case "PageTitle":
					style = styles.SingleOrDefault(s => s.Name.ToLower() == "page title")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "pagetitle")
						?? styles.SingleOrDefault(s => s.Name.ToLower() == "title");
					break;

				default:
					var nmatch = Regex.Match(name, @"^h(\d)$");
					if (nmatch.Success)
					{
						var index = nmatch.Groups[1].Captures[0].Value;

						// match any of (h1, head1, head 1, heading1, heading 1)
						style = styles.SingleOrDefault(s => 
							Regex.IsMatch(s.Name, $@"^[Hh](?:ead)?.*?{index}$"));
					}
					break;
			}

			return style;
		}


		// when applying styles, we want to preserve any special treatments to paragraphs
		// because that would be tedious for the user to restore manually if their intention 
		// was to keep those colors, but we want to clear all color styling from every other
		// element type like headings, et al.
		private void ClearInlineStyles(string index, bool paragraph)
		{
			var elements = page.Root.Descendants()
				.Where(e => e.Attribute("quickStyleIndex")?.Value == index);

			if (elements != null)
			{
				foreach (var element in elements)
				{
					stylizer.Clear(element, paragraph ? Stylizer.Clearing.Gray : Stylizer.Clearing.All);
				}
			}
		}


		private void ApplyToLists(List<Style> styles)
		{
			var style = styles.SingleOrDefault(s =>
				s.Name.ToLower() == "normal" ||
				s.Name.ToLower() == "body" ||
				s.Name.ToLower() == "p");

			string color;
			if (style != null)
			{
				color = style.Color;
			}
			else
			{
				color = page.GetPageColor().GetBrightness() < 0.5
					? "#FFFFFF"
					: "#000000";
			}

			var ns = page.Namespace;
			var elements = page.Root.Descendants(ns + "Bullet");
			if (elements?.Any() == true)
			{
				ApplyToListItems(elements, color);
			}

			elements = page.Root.Descendants(ns + "Number");
			if (elements?.Any() == true)
			{
				ApplyToListItems(elements, color);
			}
		}

		private void ApplyToListItems(IEnumerable<XElement> elements, string color)
		{
			foreach (var element in elements)
			{
				var attr = element.Attribute("fontColor");
				if (attr != null)
				{
					attr.Value = color;
				}
				else
				{
					element.Add(new XAttribute("fontColor", color));
				}
			}
		}
	}
}
