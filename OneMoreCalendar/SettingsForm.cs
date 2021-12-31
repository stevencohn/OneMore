//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Collections.Generic;
	using System.Windows.Forms;


	public partial class SettingsForm : Form
	{
		public SettingsForm()
		{
			InitializeComponent();
		}


		public bool ShowCreated => createdBox.Checked;


		public bool ShowLastModified => modifiedBox.Checked;


		public IEnumerable<string> NotebookIDs => GetSelectedNotebooks();


		private IEnumerable<string> GetSelectedNotebooks()
		{
			var list = new List<string>();
			foreach (var item in notebooksBox.CheckedItems)
			{
				list.Add(item.ToString());
			}
			return list;
		}


		private void ChangeFilter(object sender, System.EventArgs e)
		{
			if (!createdBox.Checked && !modifiedBox.Checked)
			{
				modifiedBox.Checked = true;
			}
		}


		private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}


		private void Cancel(object sender, System.EventArgs e)
		{
			Close();
		}


		private void Apply(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
