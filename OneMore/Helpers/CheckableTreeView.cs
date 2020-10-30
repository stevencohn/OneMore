//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.Serialization;
	using System.Windows.Forms;


	// http://dotnetfollower.com/wordpress/2011/05/winforms-treeview-hide-checkbox-of-treenode/

	/// <summary>
	/// Represents a node with hidden checkbox
	/// </summary>
	internal class UncheckableTreeNode : TreeNode
	{
		public UncheckableTreeNode() { }
		public UncheckableTreeNode(string text) : base(text) { }
		public UncheckableTreeNode(string text, TreeNode[] children) : base(text, children) { }
		public UncheckableTreeNode(
			string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex) { }
		public UncheckableTreeNode(
			string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
			: base(text, imageIndex, selectedImageIndex, children) { }
		protected UncheckableTreeNode(
			SerializationInfo serializationInfo, StreamingContext context)
			: base(serializationInfo, context) { }
	}

	internal class CheckableTreeView : TreeView
	{

		/// <summary>
		/// Set this when updating the text of a node, otherwise a stack overflow will occur
		/// </summary>
		public bool Suspend { get; set; } = false;


		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			// trap TVM_SETITEM message
			if (m.Msg == Native.TVM_SETITEM ||
				m.Msg == Native.TVM_SETITEMA ||
				m.Msg == Native.TVM_SETITEMW)
			{
				// is TreeView.CheckBoxes enabled?
				if (CheckBoxes)
				{
					// get information about the node
					var props = (Native.TreeViewItemProps)m.GetLParam(typeof(Native.TreeViewItemProps));
					HideCheckBox(props);
				}
			}
		}

		protected void HideCheckBox(Native.TreeViewItemProps props)
		{
			if (props.ItemHandle != IntPtr.Zero)
			{
				// get the referenced node and test if it's an Uncheckable...
				if (!(TreeNode.FromHandle(this, props.ItemHandle) is UncheckableTreeNode _))
				{
					return;
				}

				// get the item's properties...

				var treehref = new HandleRef(this, Handle);
				var currentProps = new Native.TreeViewItemProps
				{
					ItemHandle = props.ItemHandle,
					StateMask = Native.TVIS_STATEIMAGEMASK,
					State = 0
				};

				var result = Native.SendMessage(treehref, Native.TVM_GETITEM, 0, ref currentProps);

				// check if item checkbox needs to be hidden (may already be)...

				bool needToHide = result.ToInt32() > 0 && currentProps.State != 0;
				if (needToHide)
				{
					var updatedProps = new Native.TreeViewItemProps
					{
						ItemHandle = props.ItemHandle,
						Mask = Native.TVIF_STATE,
						StateMask = Native.TVIS_STATEIMAGEMASK,
						State = 0
					};

					Native.SendMessage(treehref, Native.TVM_SETITEM, 0, ref updatedProps);
				}
			}
		}

		protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			base.OnBeforeCheck(e);

			if (Suspend)
			{
				// renaming node so ignore this message
				return;
			}

			// items with hidden checkboxes can still be double-clicked to toggle state
			// so if this is an Uncheckable then cancel that action
			if (e.Node is UncheckableTreeNode)
			{
				e.Cancel = true;
			}
		}
	}
}