//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Specialized TreeView to display OneNote hierarchies, starting at any level, with icons for
	/// notebooks, section groups, and sections, and possibly checkboxes for pages. Pages can also
	/// be hyperlinked.
	/// </summary>
	internal class HierarchyView : TreeView, ILoadControl
	{
		private const int CheckboxMargin = 4;   // space between checkbox and label
		private const int ImageWidth = 28;      // width of image plus margins

		private readonly IntPtr hcursor;
		private readonly IntPtr pcursor;
		private Color backColor;
		private Color foreColor;
		private Color hotColor;


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

			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			pcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_ARROW);
		}


		/// <summary>
		/// Set this when updating the text of a node, otherwise a stack overflow will occur
		/// </summary>
		public bool Suspend { get; set; } = false;


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Populate...

		/// <summary>
		/// Provides a default mechanism for populating the hiearchy view given search results XML
		/// </summary>
		/// <param name="results">
		/// XML returned from one.Search or one.SearchMeta
		/// </param>
		/// <param name="ns"></param>
		public void Populate(XElement results, XNamespace ns)
		{
			BeginUpdate();

			DisplayResults(results, ns, Nodes);

			if (Nodes.Count > 0)
			{
				ExpandAll();
				Nodes[0].EnsureVisible();
			}

			EndUpdate();
		}


		private void DisplayResults(XElement root, XNamespace ns, TreeNodeCollection nodes)
		{
			TreeNode node;

			if (root.Name.LocalName == "Page")
			{
				node = new HierarchyNode(root.Attribute("name").Value, root)
				{
					Hyperlinked = true,
					BackColor = backColor,
					ForeColor = foreColor
				};

				nodes.Add(node);
				return;
			}

			if (root.Name.LocalName == "Notebooks")
			{
				foreach (var element in root.Elements())
				{
					DisplayResults(element, ns, nodes);
				}
				return;
			}

			node = new HierarchyNode(root.Attribute("name")?.Value, root)
			{
				BackColor = backColor,
				ForeColor = foreColor
			};

			nodes.Add(node);

			foreach (var element in root.Elements())
			{
				DisplayResults(element, ns, node.Nodes);
			}
		}


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			backColor = manager.GetColor("ControlLight");
			foreColor = manager.GetColor("ControlText");
			hotColor = manager.GetColor("HotTrack");
		}


		#region Results XML
		/*
		<one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
			<one:Notebook name="Personal" nickname="Personal" ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/" lastModifiedTime="2020-11-25T18:12:56.000Z" color="#F6B078">
			<one:Section name="OneMore" ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/OneMore.one" lastModifiedTime="2020-11-25T18:12:56.000Z" color="#B49EDE">
				<one:Page ID="{B49560C5-7EEF-41CB-9B62-A1999A367EC1}{1}{E1949357495378378527691939956701555580463441}" name="OneNoteTaggingKit" dateTime="2020-11-03T22:18:37.000Z" lastModifiedTime="2020-11-25T18:12:56.000Z" pageLevel="2">
					<one:Meta name="omTaggingLabels" content="OneNoteTaggingKit" />
					<one:Meta name="" content="" />
				</one:Page>
			</one:Section>
			</one:Notebook>
			<one:Notebook name="Flux" nickname="Flux" ID="{853348FB-82D9-44E4-93FE-60EBFBE72E9C}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/" lastModifiedTime="2020-11-25T18:13:39.000Z" color="#F5F96F" isCurrentlyViewed="true">
			<one:Section name="King" ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{B0}" path="https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/King.one" lastModifiedTime="2020-11-25T18:13:34.000Z" color="#8AA8E4" isCurrentlyViewed="true">
				<one:Page ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{E1811519588813354923720177303100143448474111}" name="OTK at Title" dateTime="2020-03-03T23:35:47.000Z" lastModifiedTime="2020-11-25T18:13:28.000Z" pageLevel="1">
					<one:Meta name="omTaggingLabels" content="cheese,apples,cream" />
					<one:Meta name="TaggingKit.PageTags" content="#Cheese, #cheese, #Grape, #grape, #Orange, #orange, -âœ©-" />
				</one:Page>
				<one:Page ID="{D20A5AAE-3E10-08C5-0E4F-6806D7C9C11A}{1}{E1823918871045154751520113206231786064788801}" name="Splitter" dateTime="2020-03-03T23:36:27.000Z" lastModifiedTime="2020-11-25T14:52:58.000Z" pageLevel="1" isCurrentlyViewed="true">
					<one:Meta name="TaggingKit.PageTags" content="" />
					<one:Meta name="omLabels" content="fish;orange;" />
					<one:Meta name="omTaggingLabels" content="Wonder Woman, Apples, orange" />
					<one:Meta name="omHighlightIndex" content="1" />
				</one:Page>
			</one:Section>
			</one:Notebook>
		</one:Notebooks>
		*/
		#endregion Results XML


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Manage node checkboxes...

		/// <summary>
		/// Intercepts the SETITEM message and modifies the TVITEM as appropriate for the desired
		/// state of the node
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero && Cursor == Cursors.Hand)
			{
				var point = PointToClient(Cursor.Position);
				var info = HitTest(point);
				if (info.Node is HierarchyNode node)
				{
					var rec = AdjustBounds(node, node.ShowCheckBox);
					Native.SetCursor(rec.Contains(point) ? hcursor : pcursor);
					m.Result = IntPtr.Zero; // indicate handled
					return;
				}
			}

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
			using var fillBrush = new SolidBrush(e.Node.BackColor);
			e.Graphics.FillRectangle(fillBrush,
				bounds.Left - CheckboxMargin, bounds.Top,
				bounds.Width + CheckboxMargin, bounds.Height);

			// notebooks, sectiongroups, and sections have icons...

			switch (node?.HierarchyLevel)
			{
				case HierarchyLevels.SectionGroup:
					e.Graphics.DrawImage(Resx.SectionGroup, bounds.Left, bounds.Top);
					break;

				case HierarchyLevels.Notebook:
					{
						var image = Resx.NotebookMask;
						image.MapColor(Color.Black, ColorHelper.FromHtml(node.Root.Attribute("color").Value));
						e.Graphics.DrawImage(image, bounds.Left, bounds.Top);
					}
					break;

				case HierarchyLevels.Section:
					{
						var image = Resx.SectionMask;
						image.MapColor(Color.Black, ColorHelper.FromHtml(node.Root.Attribute("color").Value));
						e.Graphics.DrawImage(image, bounds.Left, bounds.Top);
					}
					break;
			}

			// draw the label...

			Brush brush = null;
			Font font = null;
			var hot = ((e.State & TreeNodeStates.Hot) != 0) && (node?.Hyperlinked == true);

			try
			{
				if (hot)
				{
					brush = new SolidBrush(hotColor);
					font = new Font(e.Node.NodeFont ?? Font, FontStyle.Underline);
				}
				else
				{
					brush = new SolidBrush(e.Node.ForeColor);
					font = e.Node.NodeFont ?? Font;
				}

				var left = node?.HierarchyLevel < HierarchyLevels.Page ? bounds.Left + 20 : bounds.Left;
				e.Graphics.DrawString(e.Node.Text, font, brush, left, bounds.Top);
			}
			finally
			{
				brush?.Dispose();
				if (hot)
				{
					font?.Dispose();
				}
			}
		}


		private Rectangle AdjustBounds(TreeNode node, bool checkboxed)
		{
			if (checkboxed)
			{
				// shift bounds to right of checkbox, plus margin
				return new Rectangle(
					node.Bounds.Left + CheckboxMargin,
					node.Bounds.Top,
					node.Bounds.Width + 2,
					node.Bounds.Height);
			}

			// expand to include image
			return new Rectangle(
				node.Bounds.Left,
				node.Bounds.Top,
				node.Bounds.Width + ImageWidth,
				node.Bounds.Height);
		}
	}
}