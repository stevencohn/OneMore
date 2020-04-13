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


		public ApplyStylesCommand() : base()
		{
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
