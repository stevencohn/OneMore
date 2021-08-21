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
				cursor = JoinCursorContext(cursor);
				NormalizeRuns();
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
				NormalizeRuns();
				SelectionScope = SelectionScope.Run;
			}

			root.DescendantsAndSelf().Attributes("selected").Remove();

			return cursor;
		}


		// Remove an empty CDATA[] cursor or a selected=all T run, combining it with the previous
		// and next runs into a single run
		private XElement JoinCursorContext(XElement run)
		{
			var cdata = run.GetCData();

			if (run.PreviousNode is XElement prev)
			{
				var word = prev.ExtractLastWord(true);
				cdata.Value = $"{word}{cdata.Value}";

				if (prev.GetCData().Value.Length == 0)
				{
					prev.Remove();
				}
			}

			if (run.NextNode is XElement next)
			{
				var word = next.ExtractFirstWord(true);
				cdata.Value = $"{cdata.Value}{word}";

				if (next.GetCData().Value.Length == 0)
				{
					next.Remove();
				}
			}

			return run;
		}


		// Merge consecutive runs with equal styling. OneNote does this after navigating away
		// from a selection range within a T by combining similar Ts back into one
		private void NormalizeRuns()
		{
			var runs = root.Elements(ns + "T").ToList();

			for (int i = 0; i < runs.Count; i++)
			{
				var cdata = runs[i].GetCData();
				var wrapper = cdata.GetWrapper();
				var nodes = wrapper.Nodes().ToList();
				var updated = false;
				for (int n = 0, m = 1; m < nodes.Count; m++)
				{
					if (nodes[n] is XElement noden && nodes[m] is XElement nodem)
					{
						var sn = new Style(noden.CollectStyleProperties());
						var sm = new Style(nodem.CollectStyleProperties());

						if (sn.Equals(sm))
						{
							noden.Value = $"{noden.Value}{nodem.Value}";
							nodes[m].Remove();
							updated = true;
						}
						else
						{
							n = m;
						}
					}
					else
					{
						n = m;
					}
				}

				if (updated)
				{
					cdata.Value = wrapper.GetInnerXml();
				}
			}

			runs = root.Elements(ns + "T").ToList();

			for (int i = 0, j = 1; j < runs.Count; j++)
			{
				var si = new Style(runs[i].CollectStyleProperties());
				var sj = new Style(runs[j].CollectStyleProperties());

				if (si.Equals(sj))
				{
					var ci = runs[i].GetCData();
					var cj = runs[j].GetCData();

					var spani = ci.Value.StartsWith("<span");
					var spanj = cj.Value.StartsWith("<span");

					if (spani && spanj)
					{
						var wi = ci.GetWrapper();
						var wj = cj.GetWrapper();

						var ei = wi.FirstNode as XElement;
						var ej = wj.FirstNode as XElement;

						ei.Value = $"{ei.Value}{ej.Value}";
						ci.Value = wi.GetInnerXml(true);

						runs[j].Remove();
					}
					else if (!spani && !spanj)
					{
						ci.Value = $"{ci.Value}{cj.Value}";
						runs[j].Remove();
					}
				}
				else
				{
					i = j;
				}
			}
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
		/// Return a string representation of the XML of the outline element
		/// </summary>
		/// <returns>An XML string</returns>
		public override string ToString()
		{
			return root.ToString(SaveOptions.DisableFormatting);
		}
	}
}
