﻿//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window for user settings
	/// </summary>
	internal partial class SettingsForm : RoundedForm
	{

		private bool validate = true;


		public SettingsForm()
			: base()
		{
			InitializeComponent();
		}


		protected override async void OnLoad(EventArgs e)
		{
			// call RoundForm.base to draw background
			base.OnLoad(e);

			// TODO: why is emptyBox getting truncated?
			emptyBox.AutoSize = false;
			emptyBox.Width += 25;

			if (!DesignMode)
			{
				var provider = new SettingsProvider();

				createdBox.Checked = provider.Created;
				modifiedBox.Checked = provider.Modified;
				deletedBox.Checked = provider.Deleted;
				emptyBox.Checked = !provider.Empty;

				var hasCustom = ThemeProvider.HasCustomTheme();

				if (provider.Theme == ThemeMode.User && hasCustom)
				{
					userModeButton.Checked = true;
				}
				else
				{
					userModeButton.Enabled = hasCustom;
					switch (provider.Theme)
					{
						case ThemeMode.Light: lightModeButton.Checked = true; break;
						case ThemeMode.Dark: darkModeButton.Checked = true; break;
						default: systemModeButton.Checked = true; break;
					}
				}

				var notebooks = await provider.GetNotebooks();
				notebooksBox.Items.Clear();
				foreach (var notebook in notebooks)
				{
					notebooksBox.Items.Add(notebook);
					notebooksBox.SetItemChecked(notebooksBox.Items.Count - 1, notebook.Checked);
				}
			}
		}


		public bool Busy { get; private set; }


		public bool ShowCreated => createdBox.Checked;


		public bool ShowEmpty => !emptyBox.Checked;


		public bool ShowDeleted => deletedBox.Checked;


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
			if (validate &&
				e.NewValue == CheckState.Unchecked && notebooksBox.CheckedItems.Count == 1)
			{
				e.NewValue = CheckState.Checked;
			}
		}


		private void ToggleAllNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var selected = sender == selectAllLink;

			validate = false;
			for (var i = 0; i < notebooksBox.Items.Count; i++)
            {
				notebooksBox.SetItemChecked(i, selected);
			}

			validate = true;
		}


		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(River.OneMoreAddIn.Logger.Current.LogPath);
		}


		private void ShowAbout(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Busy = true;

			using var dialog = new AboutDialog();
			dialog.ShowDialog(Program.MainForm);

			Busy = false;
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

			provider.SetFilter(
				createdBox.Checked, modifiedBox.Checked,
				deletedBox.Checked, !emptyBox.Checked);

			ThemeMode mode;
			if (lightModeButton.Checked) mode = ThemeMode.Light;
			else if (darkModeButton.Checked) mode = ThemeMode.Dark;
			else if (userModeButton.Checked) mode = ThemeMode.User;
			else mode = ThemeMode.System;

			provider.SetTheme(mode);

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
