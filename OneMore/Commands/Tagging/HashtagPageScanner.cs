//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Scans a page for all occurances of ##hashtags
	/// </summary>
	internal class HashtagPageScanner
	{
		private const int ContextLength = 100;
		private const string SkipRegion = "#skiphashtags";
		private const string KeepRegion = "#keephashtags";

		private readonly Page page;
		private readonly XNamespace ns;
		private readonly Regex hashPattern;
		private readonly SearchAndReplaceEditor editor;
		private readonly string pageID;
		private bool keepTags;
		private int documentOrder;


		/// <summary>
		/// Do not call directly; use the HashtagPageScannerFactory class to create a new scanner.
		/// </summary>
		/// <param name="root">The root element of the page</param>
		/// <param name="pattern">The compiled regular express used to find ##hashtags</param>
		/// <param name="styleTemplate">template used to stylize hashtags</param>
		public HashtagPageScanner(Page page, Regex pattern, XElement styleTemplate)
		{
			this.page = page;
			ns = page.Root.GetNamespaceOfPrefix(OneNote.Prefix);
			pageID = page.Root.Attribute("ID").Value;

			MoreID = page.GetMetaContent(MetaNames.PageID);
			if (string.IsNullOrWhiteSpace(MoreID))
			{
				SetMoreID();
			}

			hashPattern = pattern;

			if (styleTemplate != null)
			{
				editor = new SearchAndReplaceEditor(
					pattern.ToString(), styleTemplate, true, false);
			}

			keepTags = true;
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
		/// Gets an indication of whether hashtags styles were updated
		/// </summary>
		public bool UpdateStyle { get; private set; }


		/// <summary>
		/// Sets or updates the moreID of the page and marks UpdateMeta as true.
		/// </summary>
		public void SetMoreID()
		{
			MoreID = Guid.NewGuid().ToString("N");
			page.SetMeta(MetaNames.PageID, MoreID);
			UpdateMeta = true;
		}


		/// <summary>
		/// Finds ##hashtags on the page
		/// </summary>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags Scan()
		{
			var tags = new Hashtags();

			var paragraphs = page.Root
				.Elements(ns + "Outline")
				.Elements(ns + "OEChildren")
				.Elements(ns + "OE");

			if (paragraphs.Any())
			{
				documentOrder = 0;

				foreach (var paragraph in paragraphs)
				{
					var count = tags.Count;

					ScanParagraph(paragraph, tags);

					if (editor != null && tags.Count != count)
					{
						editor.SearchAndReplace(paragraph);
						UpdateStyle = true;
					}
				}
			}

			return tags;
		}


		private void ScanParagraph(XElement paragraph, Hashtags tags)
		{
			// where all the magic happens...

			var text = paragraph.Elements(ns + "T")?
				.DescendantNodes().OfType<XCData>()
				.Select(c => c.Value.PlainText())
				.Aggregate(string.Empty, (x, y) => $"{x} {y}");

			if (!string.IsNullOrWhiteSpace(text))
			{
				var matches = hashPattern.Matches(text);
				if (matches.Count > 0)
				{
					var objectID = paragraph.Attribute("objectID").Value;
					var lastModifiedTime = paragraph.Attribute("lastModifiedTime").Value;

					foreach (Match match in matches)
					{
						if (match.Success)
						{
							// hashPattern always has at least one capture group
							var capture = match.Groups[1].Captures[0];
							var name = capture.Value;
							var lower = name.ToLower();

							if (lower == SkipRegion)
							{
								keepTags = false;
							}
							else if (lower == KeepRegion)
							{
								keepTags = true;
							}
							// this "else" ensures the #Skip and #Keep tags are not captured
							else if (keepTags &&
								!tags.Exists(t => t.Tag == name && t.ObjectID == objectID))
							{
								var context = ExtractContext(text, capture.Index, capture.Length);

								tags.Add(new Hashtag
								{
									Tag = name,
									MoreID = MoreID,
									PageID = pageID,
									ObjectID = objectID,
									Snippet = context,
									DocumentOrder = documentOrder++,
									LastModified = lastModifiedTime
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


		private string ExtractContext(string text, int capIndex, int capLength)
		{
			var stublen = (ContextLength - capLength) / 2;
			int index, length;

			if (capIndex + capLength + stublen >= text.Length)
			{
				index = Math.Max(text.Length - ContextLength, 0);
				length = Math.Min(index + ContextLength, text.Length - index);
			}
			else
			{
				index = Math.Max(capIndex - stublen, 0);
				length = ContextLength;
				if (index + length > text.Length - 1) length = text.Length - index;
			}

			var context = text.Substring(index, length);

			if (index > 0) context = $"...{context}";
			if (index + length < text.Length - 1) context = $"{context}...";

			return context;
		}
	}
}
