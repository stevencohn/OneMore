//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// 
	/// </summary>
	internal abstract class TocGenerator : Loggable
	{
		protected readonly IList<string> parameters;
		protected readonly bool refreshing;


		/// <summary>
		/// Initialize a new generator with the given parameters. Inheritors should
		/// override and chain to this constructor.
		/// </summary>
		/// <param name="parameters"></param>
		protected TocGenerator(TocParameters parameters)
		{
			this.parameters = parameters;
			refreshing = parameters.Exists(p => p.StartsWith("refresh"));
		}


		protected abstract string RefreshCmd { get; }


		/// <summary>
		/// Build the TOC, either on the current page or on a new/existing page based on
		/// the implmentor's context. The implementor decides whether an existing TOC on
		/// the current page is replaced or a new TOC is appended
		/// </summary>
		/// <returns>True if the build is successful</returns>
		public abstract Task<bool> Build();


		/// <summary>
		/// Refresh the TOC on the current page. Implementors may deal with this differently.
		/// For example, PageToc may try to find one of many TOCs that exist on the page
		/// across Outlines, whereas section and notebooks TOCs are singletons on the page.
		/// </summary>
		/// <returns>True if the refresh is successful</returns>
		public abstract Task<bool> Refresh();


		/// <summary>
		/// Parse the style`n parameter, returning n
		/// </summary>
		/// <returns></returns>
		protected int GetTitleStyleIndex()
		{
			var index = (int)TocTitleStyles.StandardPageTitle;
			if (parameters.FirstOrDefault(p => p.StartsWith("style")) is string style)
			{
				index = int.Parse(style.Substring(5));
			}

			return index;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="page"></param>
		/// <param name="ns"></param>
		/// <param name="top"></param>
		/// <returns></returns>
		protected virtual XElement LocateInsertionPoint(Page page, XNamespace ns, XElement top)
		{
			XElement container;

			var meta = (refreshing ? page.Root : top)
				.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name") is XAttribute attr && attr.Value == Toc.MetaName);

			if (meta == null)
			{
				// make new and add to page...

				container = new XElement(ns + "OE");

				if (parameters.Contains("here"))
				{
					page.AddNextParagraph(container);
				}
				else
				{
					top.AddFirst(container);
				}
			}
			else
			{
				// reuse old and clear out to prepare for new table...

				container = meta.Parent;
				container.Elements().Remove();

				if (!refreshing)
				{
					var insertHere = parameters.Contains("here");

					// if user wants it at top of page, make sure that's where it is
					if (!insertHere && container.ElementsBeforeSelf(ns + "OE").Any())
					{
						container.Remove();
						top.AddFirst(container);
					}
					else if (insertHere)
					{
						if (page.GetSelectedElements() != null &&
							page.SelectionScope != SelectionScope.Unknown)
						{
							container.Remove();
							page.AddNextParagraph(container);
						}
					}
				}
			}

			return container;
		}


		/// <summary>
		/// Generate a title line for the on-page TOC. This should be common for all inheritors.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		protected XElement MakeTitle(Page page)
		{
			// note that RefreshCmd is per-inheritor
			var cmd = $"{Toc.RefreshUri}{RefreshCmd}";

			if (parameters.Contains("links")) cmd = $"{cmd}/links";
			if (parameters.Contains("align")) cmd = $"{cmd}/align";
			if (parameters.Contains("here")) cmd = $"{cmd}/here";

			var titleIndex = GetTitleStyleIndex();
			cmd = $"{cmd}/style{titleIndex}";

			var refresh = $"<a href=\"{cmd}\"><span style='{Toc.RefreshStyle}'>{Resx.word_Refresh}</span></a>";

			var title = new Paragraph(
				$"{Resx.InsertTocCommand_TOC} <span style='{Toc.RefreshStyle}'>[{refresh}]</span>"
				);

			var titleStyle = (TocTitleStyles)titleIndex;
			if (titleStyle < TocTitleStyles.CustomPageTitle)
			{
				var standard = titleStyle switch
				{
					TocTitleStyles.StandardHeading1 => StandardStyles.Heading1,
					TocTitleStyles.StandardHeading2 => StandardStyles.Heading2,
					TocTitleStyles.StandardHeading3 => StandardStyles.Heading3,
					_ => StandardStyles.PageTitle
				};

				var style = page.GetQuickStyle(standard);
				title.SetQuickStyle(style.Index);
			}
			else
			{
				var provider = new ThemeProvider();
				var styles = provider.Theme.GetStyles();
				Style style = null;
				if (titleStyle == TocTitleStyles.CustomPageTitle)
				{
					// NOTE this will only work for English names
					style = styles.Find(s => s.Name.EqualsICIC("Page Title"));
				}
				else
				{
					var index = titleStyle switch
					{
						TocTitleStyles.CustomHeading2 => 1,
						TocTitleStyles.CustomHeading3 => 2,
						_ => 0
					};

					var heads = styles.Where(s => s.StyleType == StyleType.Heading).ToArray();
					if (index < heads.Length)
					{
						style = styles[index];
					}
				}

				if (style != null)
				{
					title.SetStyle(style.ToCss());
				}
				else
				{
					// be sure to emit this with ToRBGHtml() otherwise OneNote may normalize White/Black
					// color names, removing them against Dark/Light backgrounds respectively
					var bestColor = page.GetBestTextColor();

					title.SetStyle($"font-size:16.0pt;color:{bestColor.ToRGBHtml()}");
				}
			}


			return title;
		}
	}
}
