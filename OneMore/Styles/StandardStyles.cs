//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	internal enum StandardStyles
	{
		Heading1,
		Heading2,
		Heading3,
		Heading4,
		Heading5,
		Heading6,
		PageTitle,
		Citation,
		Quote,
		Code,
		Normal
	}


	internal static class StandardStylesExtensions
	{
		public static QuickStyleDef GetDefaults(this StandardStyles key)
		{
			var style = new QuickStyleDef
			{
				Name = key.ToName(),
				FontFamily = "Calibri",
				FontSize = "11.0",
				Color = "#000000"
			};

			switch (key)
			{
				case StandardStyles.Heading1:
					style.FontSize = "16.0";
					style.Color = "#1e4e79";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading2:
					style.FontSize = "14.0";
					style.Color = "#2e75b5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading3:
					style.FontSize = "12.0";
					style.Color = "#5b9bd5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading4:
					style.FontSize = "12.0";
					style.IsItalic = true;
					style.Color = "#5b9bd5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading5:
					style.Color = "#2e75b5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading6:
					style.IsItalic = true;
					style.Color = "#2e75b5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.PageTitle:
					style.FontFamily = "Calibri Light";
					style.FontSize = "20.0";
					break;

				case StandardStyles.Citation:
					style.FontSize = "9.0";
					style.Color = "#595959";
					break;

				case StandardStyles.Quote:
					style.IsItalic = true;
					style.Color = "#595959";
					break;

				case StandardStyles.Code:
					style.FontFamily = "Consolas";
					break;
			}

			return style;
		}


		public static string ToName(this StandardStyles key)
		{
			string name;
			switch (key)
			{
				case StandardStyles.Heading1: name = "h1"; break;
				case StandardStyles.Heading2: name = "h2"; break;
				case StandardStyles.Heading3: name = "h3"; break;
				case StandardStyles.Heading4: name = "h4"; break;
				case StandardStyles.Heading5: name = "h5"; break;
				case StandardStyles.Heading6: name = "h6"; break;
				case StandardStyles.PageTitle: name = "PageTitle"; break;
				case StandardStyles.Citation: name = "cite"; break;
				case StandardStyles.Quote: name = "blockquote"; break;
				case StandardStyles.Code: name = "code"; break;
				default: name = "p"; break;
			}

			return name;
		}
	}
}
