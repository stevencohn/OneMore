//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using River.OneMoreAddIn.Styles;
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


		/// <summary>
		/// Initialize a new instance for a given scoped context.
		/// </summary>
		/// <param name="element">
		/// The XElement possibly containig a selection range. This could be the page Root
		/// or any descendant container such as Outline, OE, or even Table.
		/// </param>
		public SelectionRange(XElement element)
		{
			root = element;
			ns = element.GetNamespaceOfPrefix(OneNote.Prefix);
		}


		/// <summary>
		/// Initialize a new instance, scoped to a given page.
		/// </summary>
		/// <param name="page"></param>
		public SelectionRange(Page page)
			: this(page.Root)
		{
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
		public SelectionScope Scope { get; private set; }


		/// <summary>
		/// Gets a Boolean indicating whether the Scope is confined to a single paragraph.
		/// </summary>
		public bool SingleParagraph { get; private set; }


		/// <summary>
		/// Deselects all selected runs within the root context and merges/optimizes consecutive
		/// runs where possible, such as removing the empty cursor run and merging its previous
		/// and next siblings - presuming they have the same styling - to create one run.
		/// </summary>
		public void Deselect()
		{
			var selections = GetSelections();
			if (!selections.Any())
			{
				return;
			}

			if (Scope == SelectionScope.TextCursor)
			{
				JoinCursorContext(selections.First());
			}

			NormalizeRuns(root);

			root.DescendantsAndSelf().Attributes("selected").Remove();
		}


		// Remove an empty CDATA[] cursor and, if possible, combine previous and next runs
		private void JoinCursorContext(XElement run)
		{
			// should only be for SelectionScope.TextCursor...

			if (run.PreviousNode is XElement prev && prev.Name.LocalName == "T" &&
				run.NextNode is XElement next && next.Name.LocalName == "T")
			{
				run.Remove();
				NormalizeRuns(prev.Parent);
			}
			else
			{
				run.Remove();
			}

			//var cdata = run.GetCData();

			//if (run.PreviousNode is XElement prev && prev.Name.LocalName == "T")
			//{
			//	var word = prev.ExtractLastWord(true);
			//	cdata.Value = $"{word}{cdata.Value}";

			//	if (prev.GetCData().Value.Length == 0)
			//	{
			//		prev.Remove();
			//	}
			//}

			//if (run.NextNode is XElement next && next.Name.LocalName == "T")
			//{
			//	var word = next.ExtractFirstWord(true);
			//	cdata.Value = $"{cdata.Value}{word}";

			//	if (next.GetCData().Value.Length == 0)
			//	{
			//		next.Remove();
			//	}
			//}
		}


		// Merge consecutive runs with equal styling. OneNote does this after navigating away
		// from a selection range within a T by combining similar Ts back into one
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell",
			"S127:\"for\" loop stop conditions should be invariant", Justification = "<Pending>")]
		private void NormalizeRuns(XElement parent)
		{
			var runs = parent.Elements(ns + "T").ToList();

			// Within a single CDATA, combine back-to-back SPANS with exactly the same styles.
			// Note that OneNote normalizes these automatically when page is saved, so this
			// handles spans added by OneMore before saving.

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
							// merge with first and drop second
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

			// compare back-to-back T runs and merge if we can...

			runs = parent.Elements(ns + "T").ToList();

			for (int i = 0, j = 1; j < runs.Count; j++)
			{
				// both runs must be owned by the same OE
				if (runs[i].Parent != runs[j].Parent)
				{
					i = j;
					continue;
				}

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

			// final pass, concat all remaining T runs...

			if (parent.Name.LocalName == "OE")
			{
				runs = parent.Elements(ns + "T").ToList();
				if (runs.Count > 1)
				{
					var first = runs[0];
					var cdata = first.GetCData();

					for (int i = 1; i < runs.Count; i++)
					{
						var cd = runs[i].GetCData();
						cdata.Value = $"{cdata.Value}{cd.Value}";
						runs[i].Remove();
					}
				}
			}
		}


		/// <summary>
		/// Finds the singly-selected text run
		/// </summary>
		/// <param name="allowNonEmpty">
		/// True to allow a non-empty text run; default is to prefer the empty text cursor
		/// element - the caret position.
		/// </param>
		/// <returns>
		/// The one outline element or null if there are multiple runs selected or the selected
		/// range is unknonwn. This method also sets the SelectionScope property
		/// </returns>
		/// <remarks>
		/// If there is exactly one selected text run and its width is zero then this visually
		/// appears as the current cursor insertion point and can be used to infer the current
		/// word or phrase in context.
		/// 
		/// If there is exactly one selected text run and its width is greater than zero then
		/// this visually appears as a selected range within one paragraph (outline element)
		/// </remarks>
		public XElement GetSelection(bool allowNonEmpty = false)
		{
			var selections = GetSelections(true);
			if (Scope == SelectionScope.None)
			{
				return null;
			}

			if (Scope == SelectionScope.Range)
			{
				return null;
			}

			var run = selections.First();
			if (run.FirstNode is not XCData)
			{
				// shouldn't happen? should it fail?
				Logger.Current.WriteLine("found invalid schema, one:T does not contain CDATA");
				Scope = SelectionScope.None;
				return null;
			}

			// at this point, Scope will be TextCursor, SpecialCursor, or Run...

			// are we forcing empty text cursor or allowing non-empty T selection?
			if (Scope == SelectionScope.Run && !allowNonEmpty)
			{
				Scope = SelectionScope.None;
				return null;
			}

			// must have valid empty text cursor
			return run;
		}


		/// <summary>
		/// Return a collection of all selected text runs
		/// </summary>
		/// <param name="allowPageTitle">
		/// True to include the page title, otherwise just the body of the which would be
		/// all regular Outlines including the tag bank
		/// </param>
		/// <param name="defaultToAnyIfNoRange">
		/// True to fallback and return all elements within scope if no selected range or
		/// run found.
		/// </param>
		/// <param name="anyElement">
		/// True to collect any XElement/@select=all from page, not just T runs
		/// </param>
		/// <returns>An IEnumerable of XElements, which may be empty</returns>
		public IEnumerable<XElement> GetSelections(
			bool allowPageTitle = false,
			bool defaulToAnytIfNoRange = false,
			bool anyElement = false)
		{
			IEnumerable<XElement> start = new List<XElement>() { root };

			// allowPageTitle only makes sense for an entire Page
			if (root.Name.LocalName == "Page")
			{
				start = allowPageTitle
					? Root.Elements()
					: Root.Elements(ns + "Outline");
			}

			var selections = GetSelections(start, anyElement: anyElement);

			if ((
				Scope == SelectionScope.None ||
				Scope == SelectionScope.TextCursor ||
				Scope == SelectionScope.SpecialCursor) &&
				defaulToAnytIfNoRange)
			{
				selections = Root.Descendants(ns + "T");
			}

			return selections;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="roots"></param>
		/// <param name="anyElement">
		/// True to collect any XElement/@select=all from page, not just T runs
		/// </param>
		/// <returns></returns>
		public IEnumerable<XElement> GetSelections(
			IEnumerable<XElement> roots, bool anyElement = false)
		{
			IEnumerable<XElement> selections;

			var start = anyElement
				? root.Descendants()
				: root.Descendants(ns + "T");

			selections = start
				.Where(e => e.Attribute("selected") is XAttribute a && a.Value == "all");

			if (!selections.Any())
			{
				Scope = SelectionScope.None;
				SingleParagraph = false;
				return Enumerable.Empty<XElement>();
			}

			if (selections.Count() > 1 && selections.All(e => e.Name.LocalName == "T"))
			{
				Scope = SelectionScope.Range;

				/// indicate when all selectied runs share the same parent
				SingleParagraph = selections.GroupBy(e => e.Parent).Count() == 1;

				return selections;
			}

			// single element selected...

			var element = selections.First();
			if (element.Name.LocalName != "T" && anyElement)
			{
				Scope = SelectionScope.Block;
				return selections;
			}

			if (element.FirstNode is not XCData cdata)
			{
				// shouldn't happen?
				Logger.Current.WriteLine("found invalid schema, one:T does not contain CDATA");
				// throw? ...
				Scope = SelectionScope.None;
				return Enumerable.Empty<XElement>();
			}

			SingleParagraph = true;

			// empty or link or xml-comment because we can't tell the difference between
			// a zero-selection zero-selection link and a partial or fully selected link.
			// Note that XML comments are used to wrap mathML equations

			if (cdata.Value.Length == 0)
			{
				// variant of Run, indicates an empty selection, the text cursor 'caret'
				Scope = SelectionScope.TextCursor;
			}
			else if (
				Regex.IsMatch(cdata.Value, @"<a\s+href.+?</a>", RegexOptions.Singleline) ||
				Regex.IsMatch(cdata.Value, @"<!--.+?-->", RegexOptions.Singleline))
			{
				Scope = SelectionScope.SpecialCursor;
			}
			else
			{
				// the entire current non-empty run is selected
				Scope = SelectionScope.Run;
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
