//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Maintains a CDATA value with zero or more spans and text
	/// </summary>

	internal class Phrase
	{
		XCData cdata;


		// <![CDATA[One<span style='font-weight:bold'>two</span> thre]]>

		public Phrase (XCData cdata)
		{
			this.cdata = cdata;
		}

		public Phrase (XElement textRange)
		{
			cdata = textRange.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA)
				.FirstOrDefault() as XCData;
		}

		public XCData CData => cdata;

		public bool ContainsMultipleWords => (cdata.Value.Length > 1) && (cdata.Value.IndexOf(' ') >= 0);

		public bool IsEmpty => cdata.Value.Length == 0;

		public bool EndsWithSpace => !IsEmpty && (cdata.Value[cdata.Value.Length - 1] == ' ');

		public bool StartsWithSpace => !IsEmpty && (cdata.Value[0] == ' ');


		public void ClearFormatting ()
		{
			if (IsEmpty)
				return;

			var data = string.Empty;
			var ctext = cdata.Value.Replace("<br>", "<br/>");
			var wrap = XElement.Parse("<w>" + ctext + "</w>");
			foreach (var node in wrap.Nodes())
			{
				if (node.NodeType == XmlNodeType.Text)
					data += node.ToString();
				else
					data += ((XElement)node).Value;
			}

			cdata.Value = data;
		}


		public string ExtractFirstWord ()
		{
			if (IsEmpty)
				return string.Empty;

			var ctext = cdata.Value.Replace("<br>", "<br/>");
			var wrap = XElement.Parse("<w>" + ctext + "</w>");
			var first = wrap.Nodes().First();

			first.Remove();

			using (var reader = wrap.CreateReader())
			{
				reader.MoveToContent();
				cdata.Value = reader.ReadInnerXml();
			}

			if (first.NodeType == XmlNodeType.Text)
				return first.ToString();

			return (first as XElement).Value;
		}

		public string ExtractLastWord ()
		{
			if (IsEmpty)
				return string.Empty;

			var ctext = cdata.Value.Replace("<br>", "<br/>");
			var wrap = XElement.Parse("<w>" + ctext + "</w>");
			var last = wrap.Nodes().Last();

			last.Remove();

			using (var reader = wrap.CreateReader())
			{
				reader.MoveToContent();
				cdata.Value = reader.ReadInnerXml();
			}

			if (last.NodeType == XmlNodeType.Text)
				return last.ToString();

			return (last as XElement).Value;
		}


		public CssInfo GetStyleInfo ()
		{
			if (IsEmpty)
				return null;

			var ctext = cdata.Value.Replace("<br>", "<br/>"); // ensure proper XML
			var wrap = XElement.Parse("<w>" + ctext + "</w>");
			var ns = wrap.GetDefaultNamespace();

			CssInfo info = null;

			var spans = wrap.Elements(ns + "span").Select(e => e.Attribute("style").Value);
			if (spans?.Count() > 0)
			{
				foreach (var css in spans)
				{
					var next = new CssInfo(css);
					if (info == null)
					{
						info = next;
					}
					else if (!next.Matches(info))
					{
						// conflicts across multiple span elements
						// so a single overall style cannot be determined
						return null;
					}
				}
			}
			else
			{
				return new CssInfo();
			}

			return info;
		}
	}
}
