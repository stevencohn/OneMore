//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System.Drawing;
	using System.Runtime.InteropServices.ComTypes;


	public partial class AddIn
	{

		/*
		 * called once, when the ribbon is shown after it is invalidated
		 */

		public int GetStyleGalleryItemCount(IRibbonControl control)
		{
			var count = new StyleProvider().Count;
			//logger.WriteLine($"GetStyleGalleryItemCount({control.Id}) = {count}");
			return count;
		}

		/*
		 * Foreach... in this order...
		 * 
		 *     GetStyleGalleryItemScreentip(styleGallery, 0) = "Heading 1"
		 *     GetStyleGalleryItemImage(styleGallery, 0)
		 *     GetStyleGalleryItemId(styleGallery, 0)
		 */

		public string GetStyleGalleryItemId(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetStyleGalleryItemId({control.Id}, {itemIndex})");
			return "style_" + itemIndex;
		}


		public IStream GetStyleGalleryItemImage(IRibbonControl control, int itemIndex)
		{
			//logger.WriteLine($"GetStyleGalleryItemImage({control.Id}, {itemIndex})");

			Color pageColor;
			using (var manager = new ApplicationManager())
			{
				pageColor = new Page(manager.CurrentPage()).GetPageColor(out _, out var black);
				if (black)
				{
					pageColor = ColorTranslator.FromHtml("#201F1E");
				}
			}

			return factory.GetCommand<GalleryTileFactory>().MakeTile(itemIndex, pageColor);
		}


		public string GetStyleGalleryItemScreentip(IRibbonControl control, int itemIndex)
		{
			var name = new StyleProvider().GetName(itemIndex);
			//logger.WriteLine($"GetStyleGalleryItemScreentip({control.Id}, {itemIndex}) = \"{name}\"");
			return name;
		}
	}
}
