//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class NewStyleCommand : Command
	{
		private readonly Dictionary<string, string> properties;


		public NewStyleCommand() : base()
		{
			properties = new Dictionary<string, string>();
		}


		public void Execute()
		{
			try
			{
				CollectStyleFromContext();

				var style = new Style(properties)
				{
					Name = "Style-" + new Random().Next(1000, 9999).ToString()
				};

				DialogResult result;
				using (var dialog = new StyleDialog(style))
				{
					result = dialog.ShowDialog(owner);
					if (result == DialogResult.OK)
					{
						style = dialog.Style;
						if (style != null)
						{
							new StyleProvider().Save(style);
							ribbon.Invalidate();
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing NewStyleCommand", exc);
			}
		}


		/*
      <one:OE >
        <one:T><![CDATA[<span
style='font-family:Calibri'>This is the third </span><span style='font-weight:
bold;font-style:italic;text-decoration:underline line-through;font-family:Consolas;
color:#70AD47'>li</span>]]></one:T>
        <one:T selected="all" style="font-family:Consolas;font-size:12.0pt;color:#70AD47"><![CDATA[]]></one:T>
        <one:T style="font-family:Consolas;font-size:12.0pt;color:#70AD47"><![CDATA[<span style='font-weight:bold;font-style:italic;
text-decoration:underline line-through'>ne </span>]]></one:T>
      </one:OE>
		 */

		/// <summary>
		/// Infer styles from the context at the position of the text cursor on the current page.
		/// </summary>
		private void CollectStyleFromContext()
		{
			// infer contextual style

			XElement page = null;
			using (var manager = new ApplicationManager())
			{
				page = manager.CurrentPage();
			}

			if (page == null)
			{
				return;
			}

			var ns = page.GetNamespaceOfPrefix("one");

			var selection =
				(from e in page.Descendants(ns + "T")
				 where e.Attributes("selected").Any(a => a.Value.Equals("all"))
				 select e).FirstOrDefault();

			if (selection != null)
			{
				var cdata = selection.GetCData();
				if (cdata.IsEmpty())
				{
					//System.Diagnostics.Debugger.Launch();

					// inside a word, adjacent to a word, or somewhere in whitespace?

					var prev = selection.PreviousNode as XElement;
					if ((prev != null) && prev.GetCData().EndsWithWhitespace())
					{
						prev = null;
					}

					var next = selection.NextNode as XElement;
					if ((next != null) && next.GetCData().StartsWithWhitespace())
					{
						next = null;
					}

					if (prev != null)
					{
						selection = prev;

						if ((selection.DescendantNodes()?
							.Where(e => e.NodeType == XmlNodeType.CDATA)
							.LastOrDefault() is XCData data) && !string.IsNullOrEmpty(data?.Value))
						{
							var wrapper = data.GetWrapper();

							if (wrapper.Elements("span")
								.Where(e => e.Attribute("style") != null)
								.LastOrDefault() is XElement wspan)
							{
								CollectStyleProperties(wspan);
							}
						}

					}
					else if (next != null)
					{
						selection = next;

						if ((selection.DescendantNodes()?
							.Where(e => e.NodeType == XmlNodeType.CDATA)
							.FirstOrDefault() is XCData data) && !string.IsNullOrEmpty(data?.Value))
						{
							var wrapper = data.GetWrapper();

							if (wrapper.Elements("span")
								.Where(e => e.Attribute("style") != null)
								.FirstOrDefault() is XElement wspan)
							{
								CollectStyleProperties(wspan);
							}
						}
					}
				}

				// walk up the hierarchy from T, to OE, to page.QuickStyle

				// T
				CollectStyleProperties(selection);
				// OE
				CollectStyleProperties(selection.Parent);
				// Page/QuickStyleDef
				CollectQuickStyleProperties(selection.Parent, page);
			}
		}


		private void CollectStyleProperties (XElement element)
		{
			var props = element.CollectStyleProperties();

			if (props?.Any() == true)
			{
				var e = props.GetEnumerator();
				while (e.MoveNext())
				{
					properties.Add(e.Current.Key, e.Current.Value);
				}
			}
		}


		// one:QuickStyleDef possibles:
		// fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="11.0" spaceBefore="0.0" spaceAfter="0.0" />
		private void CollectQuickStyleProperties (XElement element, XElement page)
		{
			var quickStyleIndex = element.Attribute("quickStyleIndex")?.Value;
			if (quickStyleIndex != null)
			{
				var ns = page.GetNamespaceOfPrefix("one");

				var quick = (page.Elements(ns + "QuickStyleDef")
					.Where(e => e.Attribute("index").Value.Equals(quickStyleIndex))).FirstOrDefault();

				if (quick != null)
				{
					XAttribute a;

					a = quick.Attribute("font");
					if (a != null && !properties.ContainsKey("font-family"))
						properties.Add("font-family", a.Value);

					a = quick.Attribute("fontSize");
					if (a != null && !properties.ContainsKey("font-size"))
						properties.Add("font-size", a.Value);

					a = quick.Attribute("fontColor");
					if (a != null && !a.Value.Equals("automatic") && !properties.ContainsKey("color"))
						properties.Add("color", a.Value);

					a = quick.Attribute("highlightColor");
					if (a != null && !a.Value.Equals("automatic") && !properties.ContainsKey("background"))
						properties.Add("background", a.Value);

					a = quick.Attribute("spaceBefore");
					if (a != null && !properties.ContainsKey("spaceBefore"))
						properties.Add("spaceBefore", a.Value);

					a = quick.Attribute("spaceAfter");
					if (a != null && !properties.ContainsKey("spaceAfter"))
						properties.Add("spaceAfter", a.Value);
				}
			}
		}
	}
}
