//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class HierarchyView : TreeView
	{
		private const int CheckboxMargin = 4;	// space between checkbox and label
		private const int ImageWidth = 28;      // width of image plus margins


		/// <summary>
		/// Initializes a new instance
		/// </summary>
		public HierarchyView()
			: base()
		{
			CheckBoxes = true;
			DoubleBuffered = true;
			DrawMode = TreeViewDrawMode.OwnerDrawText;
			HotTracking = true;
			ShowLines = false;
			ShowRootLines = false;
		}


		/// <summary>
		/// Set this when updating the text of a node, otherwise a stack overflow will occur
		/// </summary>
		public bool Suspend { get; set; } = false;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Manage node checkboxes...

		#region Checkboxes
		/// <summary>
		/// Intercepts the SETITEM message and modifies the TVITEM as appropriate for the desired
		/// state of the node
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			// trap TVM_SETITEM message
			if (m.Msg == Native.TVM_SETITEM ||
				m.Msg == Native.TVM_SETITEMA ||
				m.Msg == Native.TVM_SETITEMW)
			{
				// check if checkboxes are even possible
				if (CheckBoxes)
				{
					// get incoming item properties; we'll modify these
					var item = (Native.TreeItem)m.GetLParam(typeof(Native.TreeItem));
					if (!IntPtr.Zero.Equals(item.ItemHandle))
					{
						// check if the the node wants to hide its checkbox
						if (TreeNode.FromHandle(this, item.ItemHandle) is HierarchyNode node)
						{
							if (!node.ShowCheckBox)
							{
								HideCheckBox(item);
							}
						}
					}
				}
			}
		}

		private void HideCheckBox(Native.TreeItem item)
		{
			// get the item's properties...

			var href = new HandleRef(this, Handle);
			var current = new Native.TreeItem
			{
				ItemHandle = item.ItemHandle,
				StateMask = Native.TVIS_STATEIMAGEMASK,
				State = 0
			};

			var result = Native.SendMessage(href, Native.TVM_GETITEM, 0, ref current);

			// check if item checkbox needs to be hidden; might be already...

			if (result.ToInt32() > 0 && current.State != 0)
			{
				var updated = new Native.TreeItem
				{
					ItemHandle = item.ItemHandle,
					Mask = Native.TVIF_STATE,
					StateMask = Native.TVIS_STATEIMAGEMASK,
					State = 0
				};

				Native.SendMessage(href, Native.TVM_SETITEM, 0, ref updated);
			}
		}


		protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			base.OnBeforeCheck(e);

			// while edit the label, must ignore this message or a stack overflow
			// will occur when e.Cancel is set to true
			if (e.Node.IsEditing || Suspend)
			{
				return;
			}

			// items with hidden checkboxes can still be double-clicked to toggle state
			// so if this is an Uncheckable then cancel that action
			if (e.Node is HierarchyNode node)
			{
				if (!node.ShowCheckBox)
				{
					e.Cancel = true;
				}
			}
		}
		#endregion Checkboxes


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Drawing...

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			//base.OnDrawNode(e);

			var node = e.Node as HierarchyNode;
			var bounds = AdjustBounds(e.Node, node?.ShowCheckBox == true);

			// hide selection background by drawing it using the Window background color; this must be
			// done regardless of selection since mousedown on unselected will still draw background

			// expand bounds to fill gap between checkbox and label
			e.Graphics.FillRectangle(SystemBrushes.Window,
				bounds.Left - CheckboxMargin, bounds.Top,
				bounds.Width + CheckboxMargin, bounds.Height);

			// notebooks, sectiongroups, and sections have icons
			switch (node?.HierarchyLevel)
			{
				case HierarchyLevels.SectionGroup:
					e.Graphics.DrawImage(Resx.SectionGroup, bounds.Left, bounds.Top);
					break;

				case HierarchyLevels.Notebook:
					{
						var image = Resx.NotebookMask;
						image.MapColor(Color.Black,
							ColorTranslator.FromHtml(node.Root.Attribute("color").Value));
						e.Graphics.DrawImage(image, bounds.Left, bounds.Top);
					}
					break;

				case HierarchyLevels.Section:
					{
						var image = Resx.SectionMask;
						image.MapColor(Color.Black,
							ColorTranslator.FromHtml(node.Root.Attribute("color").Value));
						e.Graphics.DrawImage(image, bounds.Left, bounds.Top);
					}
					break;
			}

			// draw the label
			Brush brush;
			Font font;

			if (((e.State & TreeNodeStates.Hot) != 0) && (node?.Hyperlinked == true))
			{
				brush = SystemBrushes.HotTrack;
				font = new Font(e.Node.NodeFont ?? Font, FontStyle.Underline);
			}
			else
			{
				brush = SystemBrushes.ControlText;
				font = e.Node.NodeFont ?? Font;
			}

			var left = node?.HierarchyLevel < HierarchyLevels.Page ? bounds.Left + 20 : bounds.Left;
			e.Graphics.DrawString(e.Node.Text, font, brush, left, bounds.Top);
		}


		private Rectangle AdjustBounds(TreeNode node, bool checkboxed)
		{
			if (checkboxed)
			{
				return new Rectangle(
					node.Bounds.Left + CheckboxMargin,
					node.Bounds.Top,
					node.Bounds.Width + 2,
					node.Bounds.Height);
			}

			return new Rectangle(
				node.Bounds.Left,
				node.Bounds.Top,
				node.Bounds.Width + ImageWidth,     // include space for image
				node.Bounds.Height);
		}
	}
}