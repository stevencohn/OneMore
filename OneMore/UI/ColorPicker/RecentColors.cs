//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Maintains the most recently picked colors, most recent first, in CustomColors.xml,
	/// the file formerly used by the ColorDialog-based picker so existing history carries over.
	/// </summary>
	/// <remarks>
	/// The file holds Win32 COLORREF values (0xBBGGRR), as written by ColorDialog.CustomColors,
	/// so values are stored the same way here and converted with ColorTranslator.
	/// </remarks>
	internal static class RecentColors
	{
		public const int Capacity = 10;


		public static List<Color> Load()
		{
			var colors = new List<Color>();

			try
			{
				var path = Path.Combine(PathHelper.GetAppDataPath(), Resx.CustomColorsFilesname);
				if (!File.Exists(path))
				{
					return colors;
				}

				var doc = XElement.Load(path, LoadOptions.None);
				var ns = doc.GetDefaultNamespace();

				foreach (var element in doc.Elements(ns + "color"))
				{
					var text = element.Value.Trim();
					if (text.Length > 0 && text[0] == '#')
					{
						text = text.Substring(1);
					}

					if (!int.TryParse(text, NumberStyles.HexNumber,
						CultureInfo.InvariantCulture, out var value) ||
						BasicColors.IsKnown(value))
					{
						continue;
					}

					var color = ColorTranslator.FromWin32(value);
					if (!colors.Exists(c => ColorMath.SameRgb(c, color)))
					{
						colors.Add(color);
					}

					if (colors.Count == Capacity)
					{
						break;
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error loading recent colors", exc);
			}

			return colors;
		}


		/// <summary>
		/// Moves the color to the front of the persisted list
		/// </summary>
		public static void Add(Color color)
		{
			if (color.IsEmpty || color.A == 0)
			{
				return;
			}

			var colors = Load();
			colors.RemoveAll(c => ColorMath.SameRgb(c, color));
			colors.Insert(0, Color.FromArgb(color.R, color.G, color.B));
			if (colors.Count > Capacity)
			{
				colors.RemoveRange(Capacity, colors.Count - Capacity);
			}

			try
			{
				var doc = new XElement("CustomColors");
				foreach (var c in colors)
				{
					doc.Add(new XElement("color", "#" + ColorTranslator.ToWin32(c).ToString("X6")));
				}

				var path = PathHelper.GetAppDataPath();
				if (PathHelper.EnsurePathExists(path))
				{
					doc.Save(Path.Combine(path, Resx.CustomColorsFilesname), SaveOptions.None);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error saving recent colors", exc);
			}
		}
	}
}
