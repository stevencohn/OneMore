//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;


	internal static class PageHelper
	{

		/// <summary>
		/// Adjusts the width of the given page to accomodate the width of the specified
		/// string without wrapping.
		/// </summary>
		/// <param name="page">The page root node</param>
		/// <param name="line">The string to measure</param>
		/// <param name="fontFamily">The font family name to apply</param>
		/// <param name="fontSize">The font size to apply</param>
		/// <param name="handle">
		/// A handle to the current window; should be: 
		/// (IntPtr)manager.Application.Windows.CurrentWindow.WindowHandle
		/// </param>
		public static void EnsurePageWidth(
			XElement page, string line, string fontFamily, float fontSize, IntPtr handle)
		{
			// detect page width

			var ns = page.GetNamespaceOfPrefix("one");

			var element = page.Elements(ns + "Outline")
				.Where(e => e.Attributes("selected").Any())
				.Elements(ns + "Size")
				.FirstOrDefault();

			if (element != null)
			{
				var attr = element.Attribute("width");
				if (attr != null)
				{
					var outlineWidth = double.Parse(attr.Value);

					// measure line to ensure page width is sufficient

					using (var g = Graphics.FromHwnd(handle))
					{
						using (var font = new Font(fontFamily, fontSize))
						{
							var stringSize = g.MeasureString(line, font);
							var stringPoints = stringSize.Width * 72 / g.DpiX;

							if (stringPoints > outlineWidth)
							{
								attr.Value = stringPoints.ToString("#0.00");

								// must include isSetByUser or width doesn't take effect!
								if (element.Attribute("isSetByUser") == null)
								{
									element.Add(new XAttribute("isSetByUser", "true"));
								}
							}
						}
					}
				}
			}
		}
	}
}
