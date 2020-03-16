//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Maintains a CDATA value with zero or more spans and text
	/// </summary>

	internal class Phrase
	{

		// <![CDATA[One<span style='font-weight:bold'>two</span> thre]]>

		public Phrase (XCData cdata)
		{
			this.CData = cdata;
		}

		public Phrase (XElement textRange)
		{
			CData = textRange.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA)
				.FirstOrDefault() as XCData;
		}

		public XCData CData { get; }

		public bool ContainsMultipleWords => (CData.Value.Length > 1) && (CData.Value.IndexOf(' ') >= 0);

		public bool IsEmpty => CData.Value.Length == 0;

		public bool EndsWithSpace => !IsEmpty && (CData.Value[CData.Value.Length - 1] == ' ');

		public bool StartsWithSpace => !IsEmpty && (CData.Value[0] == ' ');


		public void ClearFormatting ()
		{
			if (IsEmpty)
			{
				return;
			}

			var data = string.Empty;
			var wrap = GetSanitizedWrapper();

			foreach (var node in wrap.Nodes())
			{
				if (node.NodeType == XmlNodeType.Text)
					data += node.ToString();
				else
					data += ((XElement)node).Value;
			}

			CData.Value = data;
		}


		public string ExtractFirstWord ()
		{
			if (IsEmpty)
			{
				return string.Empty;
			}

			// copy the first word and remove it from the phrase
			var wrap = GetSanitizedWrapper();
			var first = wrap.Nodes().First();
			first.Remove();

			using (var reader = wrap.CreateReader())
			{
				reader.MoveToContent();
				CData.Value = reader.ReadInnerXml();
			}

			if (first.NodeType == XmlNodeType.Text)
				return first.ToString();

			return (first as XElement).Value;
		}


		public string ExtractLastWord ()
		{
			if (IsEmpty)
			{
				return string.Empty;
			}

			// copy the last worrd and remove it from the phrase
			var wrap = GetSanitizedWrapper();
			var last = wrap.Nodes().Last();
			last.Remove();

			using (var reader = wrap.CreateReader())
			{
				reader.MoveToContent();
				CData.Value = reader.ReadInnerXml();
			}

			if (last.NodeType == XmlNodeType.Text)
				return last.ToString();

			return (last as XElement).Value;
		}


		public CssInfo GetStyleInfo ()
		{
			if (IsEmpty)
			{
				return null;
			}

			var wrap = GetSanitizedWrapper();
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


		public XElement GetSanitizedWrapper ()
		{
			// ensure proper XML

			var ctext = CData.Value
				.Replace("<br>", "<br/>")
				.Replace("&nbsp;", " ");

			// convert lang=yo to lang="yo"
			var pattern = @"(\s)lang=([\w\-]+)([\s/>])";
			ctext = Regex.Replace(ctext, pattern, "$1lang=\"$2\"$3");

			return XElement.Parse("<w>" + ctext + "</w>");
		}
	}
}
