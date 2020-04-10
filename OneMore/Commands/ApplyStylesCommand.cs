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

		private XElement page;
		private XNamespace ns;


		public ApplyStylesCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					page = manager.CurrentPage();
					if (page != null)
					{
						if (ApplyStyles(page))
						{
							manager.UpdatePageContent(page);
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(ApplyStylesCommand)}", exc);
			}
		}


		private bool ApplyStyles(XElement page)
		{
			var applied = false;

			ns = page.GetNamespaceOfPrefix("one");

			var quickStyles = page.Elements(ns + "QuickStyleDef");
			if (quickStyles?.Any() == true)
			{
				var styles = new StyleProvider().GetStyles();
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
	}
}
