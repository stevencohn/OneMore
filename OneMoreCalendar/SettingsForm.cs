//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal partial class SettingsForm : Form
	{
		private const int Radius = 8;


		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // width of ellipse
			int nHeightEllipse // height of ellipse
		); 
		

		public SettingsForm()
		{
			InitializeComponent();

			FormBorderStyle = FormBorderStyle.None;
			Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, Radius, Radius));

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
					notebooksBox.SetItemChecked(notebooksBox.Items.Count - 1, notebook.Checked);
				}
			}
		}


		public bool ShowCreated => createdBox.Checked;


		public bool ShowModified => modifiedBox.Checked;


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


		private void ChangeFilter(object sender, EventArgs e)
		{
			if (!createdBox.Checked && !modifiedBox.Checked)
			{
				modifiedBox.Checked = true;
			}
		}



		private void ValidateCheckedItems(object sender, ItemCheckEventArgs e)
		{
			// ensure that at least one notebook is checked
			if (e.NewValue == CheckState.Unchecked && notebooksBox.CheckedItems.Count == 1)
			{
				e.NewValue = CheckState.Checked;
			}
		}


		private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}


		private void Cancel(object sender, EventArgs e)
		{
			Close();
		}


		private void Apply(object sender, EventArgs e)
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


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			using (var pen = new Pen(AppColors.PressedBorder))
			{
				var r = new Rectangle(0, 0, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
				e.Graphics.DrawRoundedRectangle(pen, r, Radius);
			}
		}
	}
}
