//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;
	using Microsoft.Office.Interop.OneNote;


	public partial class SortDialog : Form, IOneMoreWindow
	{

		public enum Sortings
		{
			ByName,
			ByCreated,
			ByModified
		}


		public enum Directions
		{
			Ascending,
			Descending
		}


		public SortDialog()
		{
			InitializeComponent();

			scopeBox.SelectedIndex = 0;
		}


		public HierarchyScope Scope
		{
			get
			{
				switch (scopeBox.SelectedIndex)
				{
					case 0:
						return HierarchyScope.hsPages;

					case 1:
						return HierarchyScope.hsSections;

					default:
						return HierarchyScope.hsNotebooks;
				}
			}
		}


		public Directions Direction =>
			ascButton.Checked ? Directions.Ascending : Directions.Descending;


		public bool PinNotes => pinNotesBox.Checked;


		public Sortings Soring =>
			nameButton.Checked ? Sortings.ByName
			: (createdButton.Checked ? Sortings.ByCreated : Sortings.ByModified);


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		protected override void OnClosed(EventArgs e)
		{
		}


		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void ChangeSelection(object sender, EventArgs e)
		{
			if (!(createdButton.Enabled = (scopeBox.SelectedIndex == 0)))
			{
				if (createdButton.Checked)
				{
					nameButton.Checked = true;
				}
			}

			pinNotesBox.Enabled = (scopeBox.SelectedIndex == 1);
			quickLabel.Enabled = (scopeBox.SelectedIndex == 1);
		}
	}
}
