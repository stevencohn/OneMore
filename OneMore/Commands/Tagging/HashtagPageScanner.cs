//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;
	using Models;


	/// <summary>
	/// Scans a page for all occurances of ##hashtags
	/// </summary>
	internal class HashtagPageScanner
	{
		private readonly XElement root;
		private readonly XNamespace ns;
		private readonly Regex hashPattern;
		private readonly string pageID;


		/// <summary>
		/// Do not call directly; use the HashtagPageScannerFactory class to create a new scanner.
		/// </summary>
		/// <param name="root">The root element of the page</param>
		/// <param name="pattern">The compiled regular express used to find ##hashtags</param>
		public HashtagPageScanner(XElement root, Regex pattern)
		{
			this.root = root;
			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
			pageID = root.Attribute("ID").Value;

			MoreID = root.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.PageID)?
				.Attribute("content").Value;

			if (string.IsNullOrWhiteSpace(MoreID))
			{
				MoreID = Guid.NewGuid().ToString("N");
				UpdateMeta = true;
			}

			hashPattern = pattern;
		}


		/// <summary>
		/// Gets the unique ID assigned to the page
		/// </summary>
		public string MoreID { get; private set; }


		/// <summary>
		/// Gest an indication of whether the MoreID needs updating
		/// </summary>
		public bool UpdateMeta { get; private set; }


		/// <summary>
		/// Finds ##hashtags on the page
		/// </summary>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags Scan()
		{
			var tags = new Hashtags();

			var paragraphs = root
				.Elements(ns + "Outline")
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
					ScanParagraph(paragraph, tags);
				}
			}

			return tags;
		}


		private void ScanParagraph(XElement paragraph, Hashtags tags)
		{
			// where all the magic happens...

			var text = paragraph.Elements(ns + "T")?
				.DescendantNodes().OfType<XCData>()
				.Select(c => GetPlainText(c.Value))
				.Aggregate(string.Empty, (x, y) => $"{x} {y}");

			if (!string.IsNullOrWhiteSpace(text))
			{
				var matches = hashPattern.Matches(text);
				if (matches.Count > 0)
				{
					var objectID = paragraph.Attribute("objectID").Value;

					foreach (Match match in matches)
					{
						if (match.Success)
						{
							var name = match.Captures[0].Value;

							if (!tags.Exists(t => t.Tag == name && t.ObjectID == objectID))
							{
								tags.Add(new Hashtag
								{
									Tag = name,
									MoreID = MoreID,
									PageID = pageID,
									ObjectID = objectID
								});
							}
						}
					}
				}
			}

			var children = paragraph
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (children.Any())
			{
				foreach (var child in children)
				{
					ScanParagraph(child, tags);
				}
			}

			var tables = paragraph.Elements(ns + "Table");
			if (tables.Any())
			{
				foreach (var table in tables)
				{
					var toes = table
						.Elements(ns + "Row")
						.Elements(ns + "Cell")
						.Elements(ns + "OEChildren")
						.Elements(ns + "OE");

					if (toes.Any())
					{
						foreach (var toe in toes)
						{
							ScanParagraph(toe, tags);
						}
					}
				}
			}
		}


		private string GetPlainText(string text)
		{
			// normalize the text to be XML compliant...
			var value = text.Replace("&nbsp;", " ");
			value = Regex.Replace(value, @"\<\s*br\s*\>", "\n");

			var plain = Regex.Replace(value, @"\<[^>]+>", "");
			plain = HttpUtility.HtmlDecode(plain);

			return plain;
		}
	}
}
