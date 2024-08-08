//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S125        // Sections of code should not be commented out

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System.Drawing;
	using System.Runtime.InteropServices.ComTypes;
	using System.Threading.Tasks;

	public partial class AddIn
	{
		private TableThemeProvider tableThemeProvider;
		private Color tableGalleryBackground = Color.Empty;


		/*
		 * When Ribbon button is invalid, first calls:
		 *		GetTableGalleryItemCount(tableGallery)
		 *
		 * Then for each item, these are called in this order:
		 *     GetTableGalleryItemScreentip(tableGallery, 0) = "OneMore Table Theme 1-1"
		 *     GetTableGalleryItemImage(tableGallery, 0)
		 *     GetTableGalleryItemId(tableGallery, 0)
		 */

		/// <summary>
		/// Called by ribbon getItemCount, once when the ribbon is shown after it is invalidated
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public int GetTableGalleryItemCount(IRibbonControl control)
		{
			var background = Task.Run(async () =>
			{
				await using var one = new OneNote();

				// ribbon handlers apparently cannot be async so we need to do this
				var section = await one.GetSection();
				if (section.Attribute("locked") == null)
				{
					// ribbon handlers apparently cannot be async so we need to do this
					var page = await one.GetPage(OneNote.PageDetail.Basic);
					var color = page.GetPageColor(out _, out var black);
					return black
						? ColorTranslator.FromHtml("#201F1E")
						: color;
				}

				return Color.White;

			}).Result;


			if (tableGalleryBackground != background)
			{
				tableGalleryBackground = background;
				//logger.WriteLine($"GetTableGalleryItemCount({control.Id}) background:{tableGalleryBackground}");
				ribbon.Invalidate();
			}

			// load/reload cached theme
			tableThemeProvider = new TableThemeProvider();

			//logger.WriteLine($"GetTableGalleryItemCount() count:{tableThemeProvider.Count}");
			return tableThemeProvider.Count;
		}


		/// <summary>
		/// Called by ribbon getItemID, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public string GetTableGalleryItemId(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetTableGalleryItemId({control.Id}, {itemIndex})");
			return "table_" + itemIndex;
		}


		/// <summary>
		/// Called by ribbon getItemImage, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public IStream GetTableGalleryItemImage(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetTableGalleryItemImage({control.Id}, {itemIndex})");
			return TileFactory.MakeTableTile(
				tableThemeProvider.GetTheme(itemIndex), tableGalleryBackground);
		}


		/// <summary>
		/// Called by ribbon getItemScreentip, for each item only after invalidation
		/// </summary>
		/// <param name="control"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public string GetTableGalleryItemScreentip(IRibbonControl control, int itemIndex)
		{
			var tip = tableThemeProvider.GetName(itemIndex);
			//logger.WriteLine($"GetTableGalleryItemScreentip({control.Id}, {itemIndex}) = \"{tip}\"");
			return tip;
		}
	}
}
