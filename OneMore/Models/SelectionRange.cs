//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
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
		private readonly XElement root;
		private readonly XNamespace ns;


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
		/// Removes the selection run from a sequence of runs, merging content with the previous
		/// and next sibling runs.
		/// </summary>
		/// <returns>
		/// The merged run containing next, run, and previous content
		/// </returns>
		/// <remarks>
		/// The selection run may be either empty or contain content. An empty selection
		/// represents the insertion text cursor with no range; content represents a selection
		/// range of a word or phrase.
		/// 
		/// The SelectionScope is set based on the range type: Empty, Run, Special, or Region
		/// if there are multiple selections in context, e.g. the Root is higher than an OE.
		/// </remarks>
		public XElement Deselect()
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
				// empty cursor run should be the only selected run;
				// this can only happen if the given root is not an OE
				SelectionScope = SelectionScope.Region;
				return null;
			}

			var cursor = selections.First();
			if (!(cursor.FirstNode is XCData cdata))
			{
				// shouldn't happen?
				return null;
			}

			// A zero length insertion cursor (CDATA[]) is easy to recognize. But OneNote doesn't
			// provide enough information when the cursor is positioned on a partially or fully
			// selected hyperlink or XML comment so we can't tell the difference between these
			// three cases without looking at the CDATA value. Note that XML comments are used
			// to wrap mathML equations.

			if (cdata.Value.Length == 0)
			{
				cursor = MergeRuns(cursor);
				SelectionScope = SelectionScope.Empty;
			}
			else if (Regex.IsMatch(cdata.Value, @"<a href.+?</a>") ||
				Regex.IsMatch(cdata.Value, @"<!--.+?-->"))
			{
				SelectionScope = SelectionScope.Special;
			}
			else
			{
				// the entire current non-empty run is selected
				SelectionScope = SelectionScope.Run;
			}

			root.DescendantsAndSelf().Attributes("selected").Remove();

			return cursor;
		}


		// Remove an empty CDATA[] cursor or a selected=all T run, combining it with the previous
		// and next runs into a single run
		private XElement MergeRuns(XElement run)
		{
			if (run.PreviousNode is XElement prev)
			{
				var cprev = prev.GetCData();
				var builder = new StringBuilder(cprev.Value);
				builder.Append(run.GetCData().Value);

				if (run.NextNode is XElement next)
				{
					builder.Append(next.GetCData().Value);
					next.Remove();
				}

				cprev.Value = builder.ToString();
				run.Remove();
				return prev;
			}
			else if (run.NextNode is XElement next)
			{
				var cdata = run.GetCData();
				cdata.Value = $"{cdata.Value}{next.GetCData().Value}";
				next.Remove();
			}

			return run;
		}


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
			if (cdata.Value.Length == 0)
			{
				SelectionScope = SelectionScope.Empty;
			}
			else if (Regex.IsMatch(cdata.Value, @"<a href.+?</a>") ||
				Regex.IsMatch(cdata.Value, @"<!--.+?-->"))
			{
				SelectionScope = SelectionScope.Special;
			}
			else
			{
				// the entire current non-empty run is selected
				SelectionScope = SelectionScope.Run;
			}

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
			return root.ToString(SaveOptions.DisableFormatting);
		}
	}
}
