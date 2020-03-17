//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class StylesProvider
	{
		private XElement root;
		private XNamespace ns;


		public StylesProvider ()
		{
			var path = Path.Combine(PathFactory.GetAppDataPath(), Resx.CustomStylesFilename);

			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("Error reading " + path);
					Logger.Current.WriteLine(exc);
				}
			}

			if (root == null)
			{
				root = XElement.Parse(Resx.CustomStyles);
				Save(root, path);
			}

			ns = root.GetDefaultNamespace();
		}


		public IEnumerable<XElement> Filter (Func<XElement, bool> filter)
		{
			return root.Elements(ns + "Styles").Elements(ns + "Style").Where(e => filter(e));
		}


		public int GetCount ()
		{
			var styles = root.Elements(ns + "Styles").Elements(ns + "Style");
			if (styles != null)
			{
				return styles.Count();
			}

			return 0;
		}


		public string GetName (int index)
		{
			var template = root.Elements(ns + "Styles").Elements(ns + "Style")?.Skip(index).FirstOrDefault();
			if (template != null)
			{
				return template.Attribute("name")?.Value ?? null;
			}

			return null;
		}


		public CustomStyle GetStyle (int index, bool scaling = true)
		{
			CustomStyle style = null;

			var template = root.Elements(ns + "Styles").Elements(ns + "Style")?.Skip(index).FirstOrDefault();
			if (template != null)
			{
				style = ReadStyle(template, scaling);
			}

			return style;
		}


		public List<CustomStyle> GetStyles ()
		{
			var styles = new List<CustomStyle>();

			var templates = root.Elements(ns + "Styles").Elements(ns + "Style");
			if (templates != null)
			{
				foreach (var template in templates)
				{
					styles.Add(ReadStyle(template));
				}
			}

			return styles;
		}


		//========================================================================================
		// Load

		public List<CustomStyle> LoadTheme (string path)
		{
			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
					return GetStyles();
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("Error loading theme " + path);
					Logger.Current.WriteLine(exc);
				}
			}

			return null;
		}


		//========================================================================================
		// ReadStyle

		private CustomStyle ReadStyle (XElement template, bool scaling = false)
		{
			float scaleFactor = 1f;
			if (scaling)
			{
				using (var bitmap = new Bitmap(1, 1))
				{
					using (var graphics = Graphics.FromImage(bitmap))
					{
						scaleFactor = 96 / graphics.DpiY;
					}
				}
			}

			// name
			if (!template.ReadAttributeValue("name", out var name))
			{
				name = "Style-" + new Random().Next(1000, 9999).ToString();
			}

			// styleType
			if (!template.ReadAttributeValue("styleType", out var styleType, StyleType.Paragraph))
			{
				if (template.ReadAttributeValue("isHeading", out var isHeading, false))
				{
					styleType = isHeading ? StyleType.Heading : StyleType.Paragraph;
				}
				else
				{
					styleType = StyleType.Paragraph;
				}
			}

			// font size and style needed to build font...

			// fontSize
			template.ReadAttributeValue("fontSize", out var fontSize, CustomStyle.DefaultFontSize);

			// fontStyle
			var fontStyle = FontStyle.Regular;
			if (template.ReadAttributeValue("fontStyle", out var stylev, "Regular"))
			{
				var parts = stylev.Split('+');
				foreach (var part in parts)
				{
					if (Enum.TryParse<FontStyle>(part, out var flag))
					{
						fontStyle |= flag;
					}
				}
			}

			// font
			Font font = null;
			if (template.ReadAttributeValue("fontFamily", out var fontFamily, CustomStyle.DefaultFontFamily))
			{
				try
				{
					font = new Font(fontFamily, fontSize * scaleFactor, fontStyle);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine(
						$"Error creating font({fontFamily}, {fontSize}, {fontStyle.ToString()})",
						exc);

					font = new Font(
						CustomStyle.DefaultFontFamily, 
						CustomStyle.DefaultFontSize * scaleFactor, 
						fontStyle);
				}
			}

			// color
			Color color = Color.Black;
			if (template.ReadAttributeValue("color", out var colorv, "Black"))
			{
				try
				{
					color = ColorTranslator.FromHtml(colorv);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error translating color {colorv}", exc);
				}
			}

			// background
			Color background = Color.Empty;
			if (template.ReadAttributeValue("background", out var backgroundv, "Empty"))
			{
				try
				{
					background = ColorTranslator.FromHtml(backgroundv);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error translating background {backgroundv}", exc);
				}
			}

			// applyColors
			template.ReadAttributeValue("applyColors", out var applyColors, true);

			// spaceAfter
			template.ReadAttributeValue("spaceAfter", out var spaceAfter, 0);

			// spaceBefore
			template.ReadAttributeValue("spaceBefore", out var spaceBefore, 0);


			Logger.Current.WriteLine(
				$"StylesProvider() read (name:{name}, type:{styleType}, family:{font.FontFamily.Name}, size:{font.Size}, color:{color}, background:{background}, apply:{applyColors}, before:{spaceBefore}, after:{spaceAfter})");

			// make new CustomType

			return new CustomStyle
			{
				Name = name,
				StyleType = styleType,
				Font = font,
				Color = color,
				Background = background,
				ApplyColors = applyColors,
				SpaceAfter = spaceAfter,
				SpaceBefore = spaceBefore
			};
		}


		//========================================================================================
		// Save

		public void SaveStyles (List<CustomStyle> styles)
		{
			var all = new XElement("Styles");

			foreach (var style in styles)
			{
				all.Add(MakeElement(style));
			}

			root = new XElement(new XElement("CustomStyles", all));

			string path = PathFactory.GetAppDataPath();
			PathFactory.EnsurePathExists(path);

			Save(root, Path.Combine(path, Resx.CustomStylesFilename));
		}


		public void SaveStyle (CustomStyle style)
		{
			var candidate =
				(from e in root.Elements(ns + "Styles").Elements(ns + "Style")
				 where e.Attribute("name").Value.Equals(style.Name)
				 select e).FirstOrDefault();

			if (candidate != null)
			{
				candidate.ReplaceWith(MakeElement(style));
			}
			else
			{
				root.Element(ns + "Styles")?.Add(MakeElement(style));
			}

			string path = PathFactory.GetAppDataPath();
			PathFactory.EnsurePathExists(path);

			Save(root, Path.Combine(path, Resx.CustomStylesFilename));
		}


		private void Save (XElement root, string path)
		{
			var parent = Path.GetDirectoryName(path);
			if (!Directory.Exists(parent))
			{
				try
				{
					Directory.CreateDirectory(parent);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("Error creating " + parent);
					Logger.Current.WriteLine(exc);
					return;
				}
			}

			try
			{
				root.Save(path, SaveOptions.None);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("Error writing " + path);
				Logger.Current.WriteLine(exc);
			}
		}


		/// <summary>
		/// Build an XML Style element suitable for storing int the user preferences file.
		/// </summary>
		/// <returns></returns>

		private XElement MakeElement (CustomStyle custom)
		{
			var builder = new StringBuilder();
			if ((custom.Font.Style & FontStyle.Bold) > 0) builder.Append("Bold");
			if ((custom.Font.Style & FontStyle.Italic) > 0) { if (builder.Length > 0) builder.Append("+"); builder.Append("Italic"); }
			if ((custom.Font.Style & FontStyle.Underline) > 0) { if (builder.Length > 0) builder.Append("+"); builder.Append("Underline"); }

			var style = builder.Length == 0 ? "Regular" : builder.ToString();

			var colorHex = custom.Color.ToArgb().ToString("X6");
			if (colorHex.Length > 6) colorHex = colorHex.Substring(colorHex.Length - 6);

			var element = new XElement("Style",
				new XAttribute("name", custom.Name),
				new XAttribute("styleType", custom.StyleType.ToString()),
				new XAttribute("fontFamily", custom.Font.FontFamily.Name),
				new XAttribute("fontSize", custom.Font.Size.ToString("#.0")),
				new XAttribute("fontStyle", style),
				new XAttribute("color", "#ff" + colorHex),
				new XAttribute("applyColor", custom.ApplyColors.ToString()),
				new XAttribute("spaceBefore", custom.SpaceBefore),
				new XAttribute("spaceAfter", custom.SpaceAfter)
				);

			if (!custom.Background.IsEmpty && 
				!custom.Background.Equals(Color.Transparent) && 
				!custom.Background.Equals(custom.Color))
			{
				var backHex = custom.Background.ToArgb().ToString("X6");
				if (backHex.Length > 6) backHex = backHex.Substring(backHex.Length - 6);
				element.Add(new XAttribute("background", "#ff" + backHex));
			}

			return element;
		}
	}
}
