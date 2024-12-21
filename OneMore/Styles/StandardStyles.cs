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


	/// <summary>
	/// Defines the standard built-in OneNote styles as of v15
	/// </summary>
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
					style.SpaceAfter = "0.5";
					style.SpaceBefore = "0.8";
					style.Color = "#1e4e79";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading2:
					style.FontSize = "14.0";
					style.SpaceAfter = "0.5";
					style.SpaceBefore = "0.8";
					style.Color = "#2e75b5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading3:
					style.FontSize = "12.0";
					style.SpaceAfter = "0.3";
					style.SpaceBefore = "0.3";
					style.Color = "#5b9bd5";
					style.StyleType = StyleType.Heading;
					break;

				case StandardStyles.Heading4:
					style.FontSize = "12.0";
					style.SpaceAfter = "0.3";
					style.SpaceBefore = "0.3";
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
					// this FontFamily is a customization
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
					// this FontFamily is a customization
					style.FontFamily = "Consolas";
					style.Ignored = true;
					break;
			}

			return style;
		}


		public static string ToName(this StandardStyles key)
		{
			string name = key switch
			{
				StandardStyles.Heading1 => "h1",
				StandardStyles.Heading2 => "h2",
				StandardStyles.Heading3 => "h3",
				StandardStyles.Heading4 => "h4",
				StandardStyles.Heading5 => "h5",
				StandardStyles.Heading6 => "h6",
				StandardStyles.PageTitle => "PageTitle",
				StandardStyles.Citation => "cite",
				StandardStyles.Quote => "blockquote",
				StandardStyles.Code => "code",
				_ => "p",
			};
			return name;
		}
	}
}
