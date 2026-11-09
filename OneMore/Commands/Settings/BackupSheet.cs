//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Backups;
	using System;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Configures scheduled backups of the OneMore database.
	/// </summary>
	internal partial class BackupSheet : SheetBase
	{
		private bool refreshing;
		private string oneNoteFolder;
		private bool oneNoteMissing;


		public BackupSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(BackupSheet);
			Title = Resx.BackupSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"enableBox",
					"oneNoteRadio",
					"customRadio",
					"intervalLabel",
					"keepLabel",
					"backupNowButton"
				});
			}

			// order must match the BackupInterval enum
			intervalBox.Items.AddRange(new object[]
			{
				Resx.BackupSheet_Daily,
				Resx.BackupSheet_Weekly,
				Resx.BackupSheet_Monthly
			});

			var options = BackupOptions.Load(provider.GetCollection(Name));

			enableBox.Checked = options.Enabled;
			oneNoteRadio.Checked = options.UseOneNote;
			customRadio.Checked = !options.UseOneNote;
			folderBox.Text = options.Folder;
			intervalBox.SelectedIndex = (int)options.Interval;
			keepBox.Value = options.Keep;

			ShowLastBackup();
			ChangeState(null, null);

			folderBox.Leave += (s, e) => _ = ShowNote();

			_ = ShowOneNoteFolder();
		}


		// the user can change OneNote's backup folder in OneNote at any time, so read it
		// afresh every time this sheet is displayed
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				_ = ShowOneNoteFolder();
			}
		}


		private async Task ShowOneNoteFolder()
		{
			if (refreshing)
			{
				return;
			}

			refreshing = true;
			try
			{
				var folder = await DatabaseBackup.GetOneNoteFolder();
				if (!IsDisposed)
				{
					// show the path just below the radio button only if there is one
					oneNoteFolder = folder;
					oneNoteLabel.Text = folder;
					oneNoteLabel.Visible = !string.IsNullOrEmpty(folder);

					// with no OneNote backup folder the only choice is a custom folder
					oneNoteMissing = string.IsNullOrEmpty(folder);
					if (oneNoteMissing)
					{
						customRadio.Checked = true;
					}

					ChangeState(null, null);
				}
			}
			finally
			{
				refreshing = false;
			}

			await ShowNote();
		}


		// backups may have been moved off this disk or machine; if a backup time was recorded
		// but the chosen folder holds no backup files, say so
		private async Task ShowNote()
		{
			if (oneNoteRadio.Checked && oneNoteFolder is null)
			{
				// not known yet; ShowOneNoteFolder calls back when it is
				return;
			}

			var last = BackupOptions.Load().LastBackup;
			var folder = oneNoteRadio.Checked ? oneNoteFolder : folderBox.Text.Trim();

			var missing = last is not null &&
				!await Task.Run(() => DatabaseBackup.HasBackups(folder));

			if (!IsDisposed)
			{
				noteLabel.Text = missing ? Resx.BackupSheet_NoBackupFiles : string.Empty;
				noteLabel.Visible = missing;
			}
		}


		private void ChangeState(object sender, EventArgs e)
		{
			var enabled = enableBox.Checked;

			oneNoteRadio.Enabled = enabled && !oneNoteMissing;
			customRadio.Enabled = enabled;

			if (sender == oneNoteRadio || sender == customRadio)
			{
				_ = ShowNote();
			}

			folderBox.Enabled = enabled && customRadio.Checked;
			folderButton.Enabled = enabled && customRadio.Checked;
			intervalBox.Enabled = enabled;
			keepBox.Enabled = enabled;
			backupNowButton.Enabled = enabled;
		}


		private void BrowseFolder(object sender, EventArgs e)
		{
			try
			{
				var path = folderBox.Text;

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using var dialog = new FolderBrowserDialog()
					{
						Description = Resx.BackupSheet_SelectFolderText,
						SelectedPath = path
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						path = dialog.SelectedPath;
					}
				})
				{
					Name = $"{nameof(BackupSheet)}Thread"
				};

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				thread.Join();

				folderBox.Text = path;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running FolderBrowserDialog", exc);
			}
		}


		private async void BackupNow(object sender, EventArgs e)
		{
			var options = GetOptions();

			if (!options.UseOneNote &&
				(string.IsNullOrWhiteSpace(options.Folder) || !Directory.Exists(options.Folder)))
			{
				UI.MoreMessageBox.ShowWarning(this, Resx.BackupSheet_NoFolder);
				return;
			}

			backupNowButton.Enabled = false;
			try
			{
				// uses the settings as currently shown, even if they haven't been saved yet
				var outcome = await DatabaseBackup.Execute(options, this, force: true);

				if (outcome == BackupOutcome.Success)
				{
					// keep the in-memory copy current so that OK doesn't revert the time
					var settings = provider.GetCollection(Name);
					var last = BackupOptions.Load().LastBackup;
					if (last is not null)
					{
						settings.Add("lastBackup",
							last.Value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
						provider.SetCollection(settings);
					}

					ShowLastBackup();
					await ShowNote();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running backup", exc);
			}
			finally
			{
				backupNowButton.Enabled = enableBox.Checked;
			}
		}


		private BackupOptions GetOptions()
		{
			return new BackupOptions
			{
				Enabled = enableBox.Checked,
				UseOneNote = oneNoteRadio.Checked,
				Folder = folderBox.Text.Trim(),
				Interval = (BackupInterval)Math.Max(0, intervalBox.SelectedIndex),
				Keep = (int)keepBox.Value
			};
		}


		private void ShowLastBackup()
		{
			var last = BackupOptions.Load().LastBackup;
			statusLabel.Text = last is null
				? Resx.BackupSheet_NeverBackedUp
				: string.Format(Resx.BackupSheet_LastBackup, last.Value.ToLocalTime().ToString("g"));
		}


		public override bool CollectSettings()
		{
			var options = GetOptions();
			var settings = provider.GetCollection(Name);

			if (options.Enabled)
			{
				settings.Add("enabled", true);
			}
			else
			{
				settings.Remove("enabled");
			}

			settings.Add("useOneNote", options.UseOneNote);
			settings.Add("folder", options.Folder);
			settings.Add("interval", (int)options.Interval);
			settings.Add("keep", options.Keep);

			// lastBackup is written by the backup itself; take the latest so that saving
			// this sheet doesn't overwrite it with a stale value
			var last = new SettingsProvider().GetCollection(Name).Get<string>("lastBackup");
			if (!string.IsNullOrEmpty(last))
			{
				settings.Add("lastBackup", last);
			}

			provider.SetCollection(settings);

			// restart not required; the pipeline stage reads these settings every cycle
			return false;
		}
	}
}
