//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// Inspects and manages the selection ranges within an element
	/// </summary>
	/// <remarks>
	/// Typically instantiated with a page Outline or an OE on a page. Doesn't specifically
	/// check the root element name so can be instantiated with any element with T descendants.
	/// </remarks>
	internal class SelectionRange
	{
		public XElement root;
		public XNamespace ns;


		public SelectionRange(XElement element)
		{
			root = element;
			ns = element.GetNamespaceOfPrefix(OneNote.Prefix);
		}


		public string ObjectId
		{
			get
			{
				root.GetAttributeValue("objectID", out var objectId);
				return objectId;
			}
		}


		/// <summary>
		/// Get the root XElement of the outline element
		/// </summary>
		public XElement Root => root;


		/// <summary>
		/// Get the selection scope based on the results of GetSelection or MergeFromCursor
		/// </summary>
		public SelectionScope SelectionScope { get; private set; }


		/// <summary>
		/// Gets the singly selected text run.
		/// </summary>
		/// <returns>
		/// The one outline element or null if there are multiple runs selected or the selected
		/// region is unknonwn. This also sets the SelectionScope property
		/// </returns>
		/// <remarks>
		/// If there is exactly one selected text run and its width is zero then this visually
		/// appears as the current cursor insertion point and can be used to infer the current
		/// word or phrase in context.
		/// 
		/// If there is exactly one selected text run and its width is greater than zero then
		/// this visually appears as a selected region within one paragraph (outline element)
		/// </remarks>
		public XElement GetSelection()
		{
			SelectionScope = SelectionScope.Unknown;

			var selections = GetSelections();
			var count = selections.Count();

			if (count == 0)
			{
				return null;
			}

			if (count > 1)
			{
				SelectionScope = SelectionScope.Region;
				return null;
			}

			var cursor = selections.First();
			if (!(cursor.FirstNode is XCData cdata))
			{
				// shouldn't happen?
				return null;
			}

			// empty or link or xml-comment because we can't tell the difference between
			// a zero-selection zero-selection link and a partial or fully selected link.
			// Note that XML comments are used to wrap mathML equations
			if (cdata.Value.Length == 0 ||
				Regex.IsMatch(cdata.Value, @"<a href.+?</a>") ||
				Regex.IsMatch(cdata.Value, @"<!--.+?-->"))
			{
				SelectionScope = SelectionScope.Empty;
				return cursor;
			}

			// the entire current non-empty run is selected
			SelectionScope = SelectionScope.Run;
			return cursor;
		}


		/// <summary>
		/// Return a collection of all selected text runs
		/// </summary>
		/// <returns>An IEnumerable of XElements</returns>
		/// <remarks>
		/// Sets SelectionScope by making a basic assumption that if all selectioned runs are
		/// under the same parent then it must be a Run, otherwise it must be a Region.
		/// </remarks>
		public IEnumerable<XElement> GetSelections()
		{
			var selections = root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (selections.Any())
			{
				var count = selections.GroupBy(e => e.Parent).Count();
				SelectionScope = count == 1 ? SelectionScope.Run : SelectionScope.Region;
			}
			else
			{
				SelectionScope = SelectionScope.Unknown;
			}

			return selections;
		}


		/// <summary>
		/// Gets the T element of a zero-width selection. Visually, this appears as the current
		/// cursor insertion point and can be used to infer the current word or phrase in text.
		/// </summary>
		/// <param name="merge">
		/// If true then merge the runs around the empty cursor and return that merged element
		/// otherwise return the empty cursor
		/// </param>
		/// <returns>
		/// The one:T XElement or null if there is a selected range greater than zero
		/// </returns>
		public XElement MergeFromCursor()
		{
			SelectionScope = SelectionScope.Unknown;

			var selections = GetSelections();
			var count = selections.Count();

			if (count == 0)
			{
				return null;
			}

			if (count > 1)
			{
				// empty cursor run should be the only selected run
				SelectionScope = SelectionScope.Region;
				return null;
			}

			var cursor = selections.First();
			if (!(cursor.FirstNode is XCData cdata))
			{
				// shouldn't happen?
				return null;
			}

			// empty or link or xml-comment because we can't tell the difference between
			// a zero-selection zero-selection link and a partial or fully selected link.
			// Note that XML comments are used to wrap mathML equations
			if (cdata.Value.Length == 0 ||
				Regex.IsMatch(cdata.Value, @"<a href.+?</a>") ||
				Regex.IsMatch(cdata.Value, @"<!--.+?-->"))
			{
				cursor = MergeRuns(cursor);
				SelectionScope = SelectionScope.Empty;
				return cursor;
			}

			// the entire current non-empty run is selected
			SelectionScope = SelectionScope.Run;
			return cursor;
		}


		// remove the empty CDATA[] cursor, combining the previous and next runs into one
		private XElement MergeRuns(XElement cursor)
		{
			XElement merged = null;

			if (cursor.PreviousNode is XElement prev)
			{
				if (cursor.NextNode is XElement next)
				{
					var cprev = prev.GetCData();
					var cnext = next.GetCData();
					cprev.Value = $"{cprev.Value}{cnext.Value}";
					next.Remove();
					merged = prev;
				}
			}

			cursor.Remove();
			return merged;
		}


		/// <summary>
		/// Return the combined text value of the outline element
		/// </summary>
		/// <returns>
		/// A string of the concatenation of text from all runs in the outline element
		/// </returns>
		public string TextValue()
		{
			return root.TextValue();
		}


		/// <summary>
		/// Return a string representation of the XML of the outline element
		/// </summary>
		/// <returns>An XML string</returns>
		public override string ToString()
		{
			return root.ToString();
		}
	}
}
