//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Search
{
	using System;
	using System.Windows.Forms;
	using System.Xml.Linq;


	public partial class SearchDialog : Form
	{
		public SearchDialog()
		{
			InitializeComponent();
		}


		public bool CopySelections { get; private set; }


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
			using (var one = new OneNote())
			{
				string startId = string.Empty;
				if (notebookButton.Checked)
					startId = one.CurrentNotebookId;
				else if (sectionButton.Checked)
					startId = one.CurrentSectionId;

				var xml = one.Search(startId, findBox.Text);
				var results = XElement.Parse(xml);

				resultTree.Nodes.Clear();

				if (results.HasElements)
				{
					DisplayResults(results, one.GetNamespace(results), resultTree.Nodes);
				}

				copyButton.Enabled = true;
				moveButton.Enabled = true;
			}
		}


		private void DisplayResults(XElement results, XNamespace ns, TreeNodeCollection nodes)
		{
			
		}
		/*
<one:Notebooks xmlns>
  <one:Notebook name="Personal" nickname="Personal" ID="" color="#F6B078">
    <one:SectionGroup name="Jesters" ID="">
      <one:Section name="OneMore" ID="" color="#B49EDE">
      <one:Page ID="" name="" />
    </one:Section>
  </one:Notebook>
</one:Notebooks>
		*/

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


	internal class NotebookNode : TreeNode
	{
		public NotebookNode()
		{
		}
	}
}
