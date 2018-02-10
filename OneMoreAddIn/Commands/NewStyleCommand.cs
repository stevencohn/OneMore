//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class NewStyleCommand : Command
	{
		private Dictionary<string, string> attributes;


		public NewStyleCommand () : base()
		{
			attributes = new Dictionary<string, string>();
		}


		public void Execute ()
		{
			try
			{
				_Execute();
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing NewStyleCommand", exc);
			}
		}


		private void _Execute ()
		{
			// infer contextual style

			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				if (page != null)
				{
					var ns = page.GetNamespaceOfPrefix("one");

					var selection =
						(from e in page.Descendants(ns + "T")
						 where e.Attributes("selected").Any(a => a.Value.Equals("all"))
						 select e).FirstOrDefault();

					if (selection != null)
					{
						var phrase = new Phrase(selection);

						if (phrase.IsEmpty)
						{
							// infer selected word by looking to the left and to the right for
							// an adjacent non-whitespace character

							var found = false;
							if ((selection.PreviousNode != null) && (selection.PreviousNode is XElement))
							{
								var prev = new Phrase(selection.PreviousNode as XElement);
								if (!prev.EndsWithSpace)
								{
									selection = selection.PreviousNode as XElement;
									found = true;
								}
							}

							if (!found)
							{
								selection = selection.NextNode as XElement;
							}
						}

						if (selection != null)
						{
							GetTextStyle(selection);

							// one:OE possibles::
							// font-family:Calibri;font-size:11.0pt;color:red
							ReadSpanStyles(selection.Parent);

							GetQuickStyle(selection.Parent, page);
						}
					}
				}
			}

			// save contextual style

			var custom = MakeCustomFromAttributes();

			DialogResult result;
			using (var dialog = new StyleDialog(custom))
			{
				result = dialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					custom = dialog.CustomStyle;
					if (custom != null)
					{
						new StylesProvider().SaveStyle(custom);
						ribbon.Invalidate();
					}
				}
			}
		}


		// one:T possibles::
		// font-family:Calibri;font-size:11.0pt;color:#C0504D
		private void GetTextStyle (XElement element)
		{
			GetDataStyle(element);

			ReadSpanStyles(element);
		}


		// CDATA possibles::
		// font-weigth:bold;font-style:italic;text-decoration:underline;background:#000000
		private void GetDataStyle (XElement element)
		{
			var cdata = element.DescendantNodes()?
				.Where(e => e.NodeType == XmlNodeType.CDATA)
				.FirstOrDefault() as XCData;

			if (!string.IsNullOrEmpty(cdata?.Value))
			{
				// whether or not data contains XML, we're wrapping it so we can parse it as xml
				var wrapper = XElement.Parse("<x>" + cdata.Value + "</x>");
				var ns = wrapper.GetDefaultNamespace();

				var spans = wrapper.Elements(ns + "span").Where(e => e.Attribute("style") != null);
				if (spans != null)
				{
					foreach (var span in spans)
					{
						ReadSpanStyles(span);
					}
				}
			}
		}


		private void ReadSpanStyles (XElement element)
		{
			var ns = element.GetDefaultNamespace();

			var csss = element.Attributes(ns + "style").Select(a => a.Value);

			if (csss?.Count() == 0) return;

			foreach (var css in csss.ToList())
			{
				var parts = css.Split(';');
				if (parts.Length == 0) continue;

				foreach (var part in parts)
				{
					var pair = part.Split(':');
					if (pair.Length < 2) continue;

					var key = pair[0].Trim();
					if (!attributes.ContainsKey(key))
					{
						attributes.Add(key, pair[1].Replace("'", string.Empty).Trim());
					}
				}
			}
		}


		// one:QuickStyleDef possibles:
		// fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="11.0" spaceBefore="0.0" spaceAfter="0.0" />
		private void GetQuickStyle (XElement element, XElement page)
		{
			var quickStyleIndex = element.Attribute("quickStyleIndex")?.Value;
			if (quickStyleIndex != null)
			{
				var ns = page.GetNamespaceOfPrefix("one");

				var quick = (page.Elements(ns + "QuickStyleDef")
					.Where(e => e.Attribute("index").Value.Equals(quickStyleIndex))).FirstOrDefault();

				if (quick != null)
				{
					var a = quick.Attribute("highlightColor");
					if (a != null && !a.Value.Equals("automatic") && !attributes.ContainsKey("background"))
						attributes.Add("background", a.Value);

					a = quick.Attribute("font");
					if (a != null && !attributes.ContainsKey("font-family"))
						attributes.Add("font-family", a.Value);

					a = quick.Attribute("fontColor");
					if (a != null && !a.Value.Equals("automatic") && !attributes.ContainsKey("color"))
						attributes.Add("color", a.Value);

					a = quick.Attribute("fontSize");
					if (a != null && !attributes.ContainsKey("font-size"))
						attributes.Add("font-size", a.Value);

					a = quick.Attribute("spaceBefore");
					if (a != null && !attributes.ContainsKey("spaceBefore"))
						attributes.Add("spaceBefore", a.Value);

					a = quick.Attribute("spaceAfter");
					if (a != null && !attributes.ContainsKey("spaceAfter"))
						attributes.Add("spaceAfter", a.Value);
				}
			}
		}


		private CustomStyle MakeCustomFromAttributes ()
		{
			var family = attributes.ContainsKey("font-family") ? attributes["font-family"] : "Calibri";

			var size = 11;
			if (attributes.ContainsKey("font-size"))
			{
				if (float.TryParse(attributes["font-size"].Replace("pt", string.Empty), out var fs))
					size = (int)fs;
			}

			FontStyle style = FontStyle.Regular;
			if (attributes.ContainsKey("font-weight") && attributes["font-weight"].Equals("bold"))
				style |= FontStyle.Bold;

			if (attributes.ContainsKey("font-style") && attributes["font-style"].Equals("italic"))
				style |= FontStyle.Italic;

			if (attributes.ContainsKey("text-decoration") && attributes["text-decoration"].Equals("underline"))
				style |= FontStyle.Underline;

			Color color = Color.Black;
			if (attributes.ContainsKey("color"))
				color = ColorTranslator.FromHtml(attributes["color"]);

			Color background = Color.Transparent;
			if (attributes.ContainsKey("background"))
				background = ColorTranslator.FromHtml(attributes["background"]);

			int spaceBefore = 0;
			if (attributes.ContainsKey("spaceBefore"))
				int.TryParse(attributes["spaceBefore"], out spaceBefore);

			int spaceAfter = 0;
			if (attributes.ContainsKey("spaceAfter"))
				int.TryParse(attributes["spaceAfter"], out spaceAfter);

			var custom = new CustomStyle(
				"Style-" + new Random().Next(1000, 9999).ToString(),
				new Font(family, size, style), color, background, spaceBefore, spaceAfter);

			return custom;
		}
	}
}
