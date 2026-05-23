//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:OEChildren element — the container for a group of outline elements at one
	/// indent level. Implements IEnumerable&lt;OENode&gt; so standard LINQ works directly on it.
	/// </summary>
	internal sealed class OEChildrenNode : OneNoteNode, IEnumerable<OENode>
	{
		internal OEChildrenNode(XElement el) : base(el) { }


		/// <summary>Direct OE children (one indent level only).</summary>
		public IEnumerable<OENode> Items
			=> el.Elements(NS + "OE").Select(e => new OENode(e));


		/// <summary>Number of direct OE children.</summary>
		public int Count => el.Elements(NS + "OE").Count();


		// IEnumerable<OENode> — enables direct LINQ on the node
		public IEnumerator<OENode> GetEnumerator() => Items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		/// <summary>
		/// Depth-first sequence of all OE nodes at this level and all nested levels.
		/// Includes OEs inside table cells.
		/// </summary>
		public IEnumerable<OENode> AllDescendants()
		{
			foreach (var oe in Items)
			{
				yield return oe;

				// recurse into nested OEChildren (indented sub-items)
				if (oe.Children is not null)
					foreach (var d in oe.Children.AllDescendants())
						yield return d;

				// recurse into table cells
				if (oe.Table is not null)
					foreach (var cell in oe.Table.AllCells())
						foreach (var d in cell.Content.AllDescendants())
							yield return d;
			}
		}


		/// <summary>
		/// Depth-first sequence of all nodes in this subtree, including table and image nodes.
		/// Use .OfType&lt;T&gt;() or Descendants&lt;T&gt;() for type-filtered access.
		/// </summary>
		public IEnumerable<OneNoteNode> AllNodes()
		{
			foreach (var oe in Items)
			{
				yield return oe;

				if (oe.Children is not null)
					foreach (var node in oe.Children.AllNodes())
						yield return node;

				if (oe.Table is not null)
				{
					yield return oe.Table;
					foreach (var row in oe.Table.Rows)
					{
						yield return row;
						foreach (var cell in row.Cells)
						{
							yield return cell;
							foreach (var node in cell.Content.AllNodes())
								yield return node;
						}
					}
				}

				if (oe.Image is not null)
					yield return oe.Image;
			}
		}


		/// <summary>Returns all nodes of type T in DFS order.</summary>
		public IEnumerable<T> Descendants<T>() where T : OneNoteNode
			=> AllNodes().OfType<T>();


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Mutation

		/// <summary>Appends a new plain-text OE paragraph and returns it.</summary>
		public OENode AppendItem(string text = "")
		{
			var oe = OENode.Create(text);
			el.Add(oe.Element);
			return oe;
		}


		/// <summary>Inserts a new paragraph immediately after the given sibling.</summary>
		public OENode InsertItemAfter(OENode sibling, string text = "")
		{
			var oe = OENode.Create(text);
			sibling.Element.AddAfterSelf(oe.Element);
			return oe;
		}


		/// <summary>Removes an OE from this container.</summary>
		public void RemoveItem(OENode oe) => oe.Element.Remove();


		/// <summary>
		/// Indents the given item by wrapping it in a nested OEChildren inside its preceding
		/// sibling. If there is no preceding sibling the call is a no-op.
		/// </summary>
		public void Indent(OENode item)
		{
			var prev = item.Element.ElementsBeforeSelf(NS + "OE").LastOrDefault();
			if (prev is null) return;

			item.Element.Remove();
			var children = prev.Elements(NS + "OEChildren").FirstOrDefault();
			if (children is null)
			{
				children = E("OEChildren");
				prev.Add(children);
			}
			children.Add(item.Element);
		}


		/// <summary>
		/// Outdents the given item by moving it to the parent OEChildren level.
		/// If already at the top level the call is a no-op.
		/// </summary>
		public void Outdent(OENode item)
		{
			var parentOE = item.Element.Parent?.Parent; // OEChildren → OE
			if (parentOE is null || parentOE.Name != NS + "OE") return;

			var grandparent = parentOE.Parent; // outer OEChildren
			if (grandparent is null) return;

			item.Element.Remove();
			parentOE.AddAfterSelf(item.Element);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// List helpers

		/// <summary>Applies a bullet list marker to all direct OE children.</summary>
		public void MakeBulletList(string bullet = "2", string fontSize = "11.0")
		{
			foreach (var oe in Items)
				oe.SetBullet(bullet, fontSize);
		}


		/// <summary>Applies a numbered list format to all direct OE children.</summary>
		public void MakeNumberedList(string format = "##.", string font = "Calibri")
		{
			int seq = 0;
			foreach (var oe in Items)
				oe.SetNumber(seq++, format, font);
		}
	}
}
