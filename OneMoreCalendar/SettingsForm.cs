//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window for user settings
	/// </summary>
	internal partial class SettingsForm : RoundedForm
	{

		private const float DesignDpi = 144f;

		private bool validate = true;
		private Font ambientFont;


		public SettingsForm()
			: base()
		{
			InitializeComponent();

			Translator.Localize(this, new[]
			{
				"optionsLabel",
				"notebooksLabel",
				"themeLabel",
				"userModeButton",
				"darkModeButton",
				"lightModeButton",
				"systemModeButton",
				"emptyBox",
				"deletedBox",
				"createdBox",
				"modifiedBox",
				"okButton",
				"cancelButton",
				"logLink",
				"aboutLink",
				"selectAllLink",
				"selectNoneLink",
				"barLabel"
			});
		}


		protected override async void OnLoad(EventArgs e)
		{
			var logger = River.OneMoreAddIn.Logger.Current;
			logger.Debug($"settings DPI={DeviceDpi} ambient font={Font.Name} {Font.SizeInPoints}pt " +
				$"unit={Font.Unit} height={Font.Height}px");

			// the ambient font comes from the system, which is not per-monitor DPI aware and
			// isn't rescaled with autoscaling off, so text can be too large on a lower-DPI
			// monitor; pin it to the point size the layout was designed with (Segoe UI 9pt)
			ambientFont = new Font("Segoe UI", 9F);
			Font = ambientFont;

			// the designer layout was authored at 150% (144 DPI) with autoscaling off; scale it
			// to the actual DPI before calling base.OnLoad so RoundedForm's rounded region uses
			// the final size
			this.ScaleLayout(DesignDpi);

			logger.Debug($"settings scaled size={Size} font height={Font.Height}px " +
				$"radio={darkModeButton.Size} check={createdBox.Size}");

			// call RoundForm.base to draw background
			base.OnLoad(e);

			// TODO: why is emptyBox getting truncated?
			emptyBox.AutoSize = false;
			emptyBox.Width += this.Scaled(17);

			if (!DesignMode)
			{
				var provider = SettingsProvider.Current;

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


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			ambientFont?.Dispose();
			ambientFont = null;
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
			var provider = SettingsProvider.Current;

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
