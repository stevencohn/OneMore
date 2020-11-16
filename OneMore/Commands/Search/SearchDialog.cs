//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Search
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SearchDialog : LocalizableForm
	{

		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchDialog_Title;

				Localize(new string[]
				{
					"introLabel",
					"findLabel",
					"allButton",
					"notebookButton",
					"sectionButton",
					"moveButton",
					"copyButton",
					"cancelButton"
				});
			}

			SelectedPages = new List<string>();
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 5));
			UIHelper.SetForegroundWindow(this);
		}


		private void ChangeQuery(object sender, EventArgs e)
		{
			searchButton.Enabled = findBox.Text.Trim().Length > 0;
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter &&
				findBox.Text.Trim().Length > 0)
			{
				Search(sender, e);
			}
		}


		private void Search(object sender, EventArgs e)
		{
			resultTree.Nodes.Clear();

			using (var one = new OneNote())
			{
				string startId = string.Empty;
				if (notebookButton.Checked)
					startId = one.CurrentNotebookId;
				else if (sectionButton.Checked)
					startId = one.CurrentSectionId;

				var xml = one.Search(startId, findBox.Text);
				var results = XElement.Parse(xml);

				// remove recyclebin nodes
				results.Descendants()
					.Where(n => n.Name.LocalName == "UnfiledNotes" ||
								n.Attribute("isRecycleBin") != null ||
								n.Attribute("isInRecycleBin") != null)
					.Remove();

				if (results.HasElements)
				{
					resultTree.BeginUpdate();
					DisplayResults(results, one.GetNamespace(results), resultTree.Nodes);
					if (resultTree.Nodes.Count > 0)
					{
						resultTree.ExpandAll();
						resultTree.Nodes[0].EnsureVisible();
					}
					resultTree.EndUpdate();
				}
			}
		}


		private void DisplayResults(XElement root, XNamespace ns, TreeNodeCollection nodes)
		{
			TreeNode node;

			if (root.Name.LocalName == "Page")
			{
				node = new TreeNode(root.Attribute("name").Value) { Tag = root };
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

			node = new UncheckableTreeNode(root.Attribute("name")?.Value) { Tag = root };
			nodes.Add(node);

			foreach (var element in root.Elements())
			{
				DisplayResults(element, ns, node.Nodes);
			}
		}


		private void TreeDrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			var element = e.Node.Tag as XElement;
			var isContainer = element.Name.LocalName != "Page";

			var color = SystemBrushes.ControlText;
			var bounds = MakeNodeBounds(e.Node);

			// draw the selection background
			if ((e.State & TreeNodeStates.Selected) != 0)
			{
				e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
				color = SystemBrushes.HighlightText;
			}

			if (isContainer)
			{
				var image = Resx.SectionGroup;
				if (element.Name.LocalName == "Notebook" || element.Name.LocalName == "Section")
				{
					image = element.Name.LocalName == "Notebook" ? Resx.NotebookMask : Resx.SectionMask;
					image.MapColor(Color.Black, ColorTranslator.FromHtml(element.Attribute("color").Value));
				}

				e.Graphics.DrawImage(image, bounds.Left, bounds.Top);
			}

			// draw the node text
			var font = e.Node.NodeFont ?? ((TreeView)sender).Font;
			var left = isContainer ? bounds.Left + 20 : bounds.Left;
			e.Graphics.DrawString(e.Node.Text, font, color, left, bounds.Top);

			if (isContainer)
			{
				// draw the focus rectangle
				if ((e.State & TreeNodeStates.Focused) != 0)
				{
					using (var pen = new Pen(Color.Black))
					{
						pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
						bounds.Size = new Size(bounds.Width - 1, bounds.Height - 1);
						e.Graphics.DrawRectangle(pen, bounds);
					}
				}
			}
		}

		private Rectangle MakeNodeBounds(TreeNode node)
		{
			var element = node.Tag as XElement;
			if (element.Name.LocalName == "Page")
			{
				return new Rectangle(
					node.Bounds.Left + 4,   // offset from checkbox
					node.Bounds.Top,
					node.Bounds.Width + 2,
					node.Bounds.Height);
			}

			return new Rectangle(
				node.Bounds.Left,
				node.Bounds.Top,
				node.Bounds.Width + 24,     // include space for image
				node.Bounds.Height);
		}


		private void TreeMouseDown(object sender, MouseEventArgs e)
		{
			var node = resultTree.GetNodeAt(e.X, e.Y);
			if (node != null)
			{
				if (MakeNodeBounds(node).Contains(e.X, e.Y))
				{
					resultTree.SelectedNode = node;
				}
			}
		}


		private void TreeDoubleClick(object sender, EventArgs e)
		{
			var node = resultTree.SelectedNode;
			if (node != null)
			{
				var element = node.Tag as XElement;
				if (element?.Name.LocalName == "Page")
				{
					var pageId = element.Attribute("ID").Value;

					using (var one = new OneNote())
					{
						// TODO: there must be a better way to do it than this...
						// Invoke or SynchronizationContext???

						var link = one.GetHyperlink(pageId, string.Empty);
						Process.Start(new ProcessStartInfo(link));
					}
				}
			}
		}


		private void TreeAfterCheck(object sender, TreeViewEventArgs e)
		{
			var element = e.Node.Tag as XElement;
			var id = element.Attribute("ID").Value;

			if (e.Node.Checked)
			{
				if (!SelectedPages.Contains(id))
				{
					SelectedPages.Add(id);
				}
			}
			else if (SelectedPages.Contains(id))
			{
				SelectedPages.Remove(id);
			}

			copyButton.Enabled = moveButton.Enabled = SelectedPages.Count > 0;
		}


		private void CopyPressed(object sender, EventArgs e)
		{
			CopySelections = true;
			DialogResult = DialogResult.OK;
			Close();
		}


		private void MovePressed(object sender, EventArgs e)
		{
			CopySelections = false;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
