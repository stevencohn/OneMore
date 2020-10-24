//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S125		// Sections of code should not be commented out

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;

	public partial class AddIn
	{
		public string GetFavoritesContent(IRibbonControl control)
		{
			//logger.WriteLine($"GetFavoritesContent({control.Id})");
			return new FavoritesProvider(ribbon).GetMenuContent();
		}

		public void AddFavoritePage(IRibbonControl control)
		{
			//logger.WriteLine($"AddFavoritePage({control.Id})");
			new FavoritesProvider(ribbon).AddFavorite();
		}

		public void NavigateToFavorite(IRibbonControl control)
		{
			//logger.WriteLine($"NavigateToFavorite({control.Tag})");
			factory.Run<NavigateCommand>(control.Tag);
		}

		public void RemoveFavorite(IRibbonControl control)
		{
			//logger.WriteLine($"RemoveFavorite({control.Tag})");
			new FavoritesProvider(ribbon).RemoveFavorite(control.Tag);
		}
	}
}
