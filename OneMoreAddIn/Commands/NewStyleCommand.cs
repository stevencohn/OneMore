//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class NewStyleCommand : Command
	{

		public NewStyleCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				var style = CollectStyleFromContext();

				if (style != null)
				{
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
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(NewStyleCommand)}", exc);
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
		private Style CollectStyleFromContext()
		{
			// infer contextual style

			XElement page = null;
			using (var manager = new ApplicationManager())
			{
				page = manager.CurrentPage();
			}

			if (page == null)
			{
				return null;
			}

			var ns = page.GetNamespaceOfPrefix("one");

			var selection =
				(from e in page.Descendants(ns + "T")
				 where e.Attributes("selected").Any(a => a.Value.Equals("all"))
				 select e).FirstOrDefault();

			if (selection != null)
			{
				var analyzer = new StyleAnalyzer(page);

				var cdata = selection.GetCData();
				if (cdata.IsEmpty())
				{
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
								analyzer.CollectStyleProperties(wspan);
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
								analyzer.CollectStyleProperties(wspan);
							}
						}
					}
				}

				var properties = analyzer.CollectStyleProperties(selection);

				var style = new Style(properties)
				{
					Name = "Style-" + new Random().Next(1000, 9999).ToString()
				};

				return style;
			}

			return null;
		}
	}
}
