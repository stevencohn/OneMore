//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S125        // Sections of code should not be commented out

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Runtime.InteropServices.ComTypes;


	public partial class AddIn
	{
		private static Color pageColor;
		private static Theme theme;


		/*
		 * When Ribbon button is invalid, first calls:
		 *		GetStyleGalleryItemCount(styleGallery)
		 *
		 * Then for each item, these are called in this order:
		 *     GetStyleGalleryItemScreentip(styleGallery, 0) = "Heading 1"
		 *     GetStyleGalleryItemImage(styleGallery, 0)
		 *     GetStyleGalleryItemId(styleGallery, 0)
		 */


		/// <summary>
		/// Called by ribbon getItemCount, once when the ribbon is shown after it is invalidated
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public int GetStyleGalleryItemCount(IRibbonControl control)
		{
			using (var one = new OneNote(out var page, out _))
			{
				pageColor = page.GetPageColor(out _, out var black);
				if (black)
				{
					// translate Black into a custom black smoke
					pageColor = ColorTranslator.FromHtml("#201F1E");
				}
			}

			// load/reload cached theme
			theme = new ThemeProvider().Theme;

			var count = theme.GetCount();
			//logger.WriteLine($"GetStyleGalleryItemCount() count:{count}");
			return count;
		}


		/// <summary>
		/// Called by ribbon getItemID, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public string GetStyleGalleryItemId(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetStyleGalleryItemId({control.Id}, {itemIndex})");
			return "style_" + itemIndex;
		}


		/// <summary>
		/// Called by ribbon getItemImage, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public IStream GetStyleGalleryItemImage(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetStyleGalleryItemImage({control.Id}, {itemIndex})");
			return new TileFactory().MakeTile(theme.GetStyle(itemIndex), pageColor);
		}


		/// <summary>
		/// Called by ribbon getItemScreentip, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public string GetStyleGalleryItemScreentip(IRibbonControl control, int itemIndex)
		{
			var tip = theme.GetName(itemIndex);

			if (itemIndex < 9)
			{
				tip = string.Format(Properties.Resources.CustomStyle_Screentip, tip, itemIndex + 1);
			}

			//logger.WriteLine($"GetStyleGalleryItemScreentip({control.Id}, {itemIndex}) = \"{tip}\"");
			return tip;
		}
	}
}
