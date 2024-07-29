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
	/// Base class for TOC generators
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


		/// <summary>
		/// Gets the main part of the title inserted before the TOC on the page.
		/// </summary>
		protected abstract string PrimaryTitle { get; }


		/// <summary>
		/// Gets the URI segment string specifying the inheritors scope
		/// such as refresh, refreshs, and refreshn
		/// </summary>
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
		/// Generate a title line for the on-page TOC. This should be common for all inheritors.
		/// </summary>
		/// <param name="page">The page from which styles are taken to apply to the title</param>
		/// <param name="segments">
		/// An string specifying the inheritor-specific URI segments to append to the refresh
		/// link; it should NOT start with a forward slash.
		/// </param>
		/// <returns></returns>
		protected XElement MakeTitle(Page page, string segments)
		{
			// note that RefreshCmd is per-inheritor
			var cmd = $"{Toc.RefreshUri}{RefreshCmd}{segments}";

			var titleIndex = GetTitleStyleIndex();
			cmd = $"{cmd}/style{titleIndex}";

			var refresh = $"<a href=\"{cmd}\"><span style='" +
				$"{Toc.RefreshStyle}'>{Resx.word_Refresh}</span></a>";

			var title = new Paragraph(
				$"{PrimaryTitle} <span style='{Toc.RefreshStyle}'>[{refresh}]</span>"
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

				if (style is not null)
				{
					title.SetStyle(style.ToCss());
				}
				else
				{
					// be sure to emit this with ToRBGHtml() otherwise OneNote may normalize
					// White/Black color names, removing them against Dark/Light backgrounds
					// respectively
					var bestColor = page.GetBestTextColor();

					title.SetStyle($"font-size:16.0pt;color:{bestColor.ToRGBHtml()}");
				}
			}


			return title;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract Task<RefreshOption> RefreshExistingPage();
	}
}
