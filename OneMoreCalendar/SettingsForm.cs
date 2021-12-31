//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Collections.Generic;
	using System.Windows.Forms;


	internal partial class SettingsForm : Form
	{
		public SettingsForm()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				var provider = new SettingsProvider();

				createdBox.Checked = provider.ShowCreated;
				modifiedBox.Checked = provider.ShowModified;

				var notebooks = provider.GetNotebooks();
				notebooksBox.Items.Clear();
				foreach (var notebook in notebooks)
				{
					notebooksBox.Items.Add(notebook);
					notebooksBox.SetItemChecked(notebooksBox.Items.Count - 1, true);
				}
			}
		}


		public bool ShowCreated => createdBox.Checked;


		public bool ShowLastModified => modifiedBox.Checked;


		public IEnumerable<Notebook> Notebooks => GetNotebooks();


		private IEnumerable<Notebook> GetNotebooks()
		{
			var notebooks = new List<Notebook>();
			foreach (Notebook notebook in notebooksBox.CheckedItems)
			{
				notebooks.Add(notebook);
			}
			return notebooks;
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
			var provider = new SettingsProvider();
			provider.SetFilter(createdBox.Checked, modifiedBox.Checked);

			var ids = new List<string>();
			foreach (Notebook notebook in notebooksBox.CheckedItems)
			{
				ids.Add(notebook.ID);
			}
			provider.SetNotebookIDs(ids);
			provider.Save();

			Close();
		}
	}
}
