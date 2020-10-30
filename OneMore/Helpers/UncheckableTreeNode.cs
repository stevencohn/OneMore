
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
		public UncheckableTreeNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex) { }
		public UncheckableTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children) : base(text, imageIndex, selectedImageIndex, children) { }
		protected UncheckableTreeNode(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) { }
	}

	internal class MixedCheckBoxesTreeView : TreeView
	{

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			// trap TVM_SETITEM message
			if (m.Msg == Native.TVM_SETITEM || 
				m.Msg == Native.TVM_SETITEMA || 
				m.Msg == Native.TVM_SETITEMW)
			{
				// check if CheckBoxes are turned on
				if (CheckBoxes)
				{
					// get information about the node
					var tv_item = (Native.TreeViewItem)m.GetLParam(typeof(Native.TreeViewItem));
					HideCheckBox(tv_item);
				}
			}
		}

		protected void HideCheckBox(Native.TreeViewItem tv_item)
		{
			if (tv_item.ItemHandle != IntPtr.Zero)
			{
				// get TreeNode-object, that corresponds to TV_ITEM-object
				var currentTN = TreeNode.FromHandle(this, tv_item.ItemHandle);

				var hiddenCheckBoxTreeNode = currentTN as UncheckableTreeNode;
				// check if it's HiddenCheckBoxTreeNode and
				// if its checkbox already has been hidden

				if (hiddenCheckBoxTreeNode != null)
				{
					var treeHandleRef = new HandleRef(this, Handle);

					// check if checkbox already has been hidden
					var currentTvItem = new Native.TreeViewItem
					{
						ItemHandle = tv_item.ItemHandle,
						StateMask = Native.TVIS_STATEIMAGEMASK,
						State = 0
					};

					var result = Native.SendMessage(treeHandleRef, Native.TVM_GETITEM, 0, ref currentTvItem);
					bool needToHide = result.ToInt32() > 0 && currentTvItem.State != 0;

					if (needToHide)
					{
						// specify attributes to update
						var updatedTvItem = new Native.TreeViewItem
						{
							ItemHandle = tv_item.ItemHandle,
							Mask = Native.TVIF_STATE,
							StateMask = Native.TVIS_STATEIMAGEMASK,
							State = 0
						};

						// send TVM_SETITEM message
						Native.SendMessage(treeHandleRef, Native.TVM_SETITEM, 0, ref updatedTvItem);
					}
				}
			}
		}

		protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			base.OnBeforeCheck(e);

			// prevent checking/unchecking of HiddenCheckBoxTreeNode,
			// otherwise, we will have to repeat checkbox hiding
			if (e.Node is UncheckableTreeNode)
				e.Cancel = true;
		}
	}
}