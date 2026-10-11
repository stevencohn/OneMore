//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.TextIndex;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// The options of the text index that makes the Search command fast.
	/// </summary>
	internal partial class IndexSheet : SheetBase
	{
		private const double Megabyte = 1024d * 1024d;

		private readonly bool available;
		private bool hasIndex;


		public IndexSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(IndexSheet);
			Title = Resx.IndexSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"enabledBox",
					"allBox",
					"selectLink",
					"buildLink",
					"buildNote",
					"deleteLink"
				});
			}

			var settings = provider.GetCollection(Name);

			// what the installer was told if the user has not chosen in Settings
			enabledBox.Checked = settings.Get("enabled", IndexConsumer.InstalledDefault());
			allBox.Checked = settings.Get("all", IndexConsumer.InstalledDefault());

			available = IndexProvider.IsAvailable;
			if (!available)
			{
				// the index needs a component that could not be loaded in this installation
				enabledBox.Checked = false;
				enabledBox.Enabled = false;
				statusLabel.Text = Resx.IndexSheet_unavailable;
			}
			else
			{
				RefreshStatus();
			}

			UpdateControls();
		}


		private void UpdateControls()
		{
			var enabled = available && enabledBox.Checked;

			allBox.Enabled = enabled;
			selectLink.Enabled = enabled && !allBox.Checked;
			buildLink.Enabled = enabled;

			// only when the index is off; while it is on, the next background pass would just
			// start building it again
			deleteLink.Enabled = available && !enabledBox.Checked && hasIndex;
		}


		// says what the index holds, if anything has been indexed
		private void RefreshStatus()
		{
			try
			{
				hasIndex = IndexProvider.CatalogExists();
				if (hasIndex)
				{
					using var index = new IndexProvider();

					// what the index is for is the notebooks that are chosen, as the check boxes
					// show them now, even if their pages are not yet gone from the index
					var chosen = new HashSet<string>(index.ReadNotebooks()
						.Where(n => n.Included)
						.Select(n => n.NotebookKey));

					var all = allBox.Checked;
					var stats = index.ReadStatistics(key => all || chosen.Contains(key));

					if (stats.Pages > 0)
					{
						statusLabel.Text = string.Format(
							Resx.IndexSheet_status,
							stats.Pages, stats.Paragraphs, stats.Bytes / Megabyte);

						return;
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error reading the text index status", exc);
			}

			statusLabel.Text = Resx.IndexSheet_statusNone;
		}


		private void ToggleEnabled(object sender, EventArgs e)
		{
			UpdateControls();

			// what counts as indexed depends on whether every notebook is chosen
			if (available)
			{
				RefreshStatus();
			}
		}


		private void SelectNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using var dialog = new NotebooksDialog(forIndex: true);
			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				RefreshStatus();
			}
		}


		private void DeleteIndex(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!available || enabledBox.Checked)
			{
				return;
			}

			if (MoreMessageBox.ShowQuestion(this, Resx.IndexSheet_deleteConfirm) != DialogResult.Yes)
			{
				return;
			}

			// The setting that is saved is what the background indexing goes by. Save the choice
			// now, so that cancelling the dialog does not leave indexing on to refill the index.
			CollectSettings();

			var pages = 0;
			var cursor = Cursor;
			Cursor = Cursors.WaitCursor;

			try
			{
				// a pass already in progress is not working through pages while this runs
				using (IndexConsumer.SuspendBackground())
				using (var index = new IndexProvider())
				{
					pages = index.Clear();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deleting the text index", exc);
				Cursor = cursor;
				MoreMessageBox.ShowError(this, exc.Message);
				return;
			}

			Cursor = cursor;

			RefreshStatus();
			UpdateControls();

			MoreMessageBox.Show(this, string.Format(Resx.IndexSheet_deleteDone, pages));
		}


		private void BuildNow(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!available || !enabledBox.Checked)
			{
				return;
			}

			// the check box is what the user sees, even if OK has not been clicked yet
			var all = allBox.Checked;
			IndexBuildResult result = null;
			Exception error = null;
			var maximum = 0;

			using (var progress = new ProgressDialog())
			{
				progress.SetMessage(Resx.IndexSheet_buildMessage);

				progress.ShowDialogWithCancel(async (dialog, token) =>
				{
					try
					{
						result = await IndexBuilder.Build((done, expected) =>
						{
							if (expected != maximum)
							{
								maximum = expected;
								dialog.SetMaximum(Math.Max(expected, 1));
							}

							dialog.SetMessage(string.Format(Resx.IndexSheet_buildProgress, done, expected));
							dialog.Increment();
						},
						token, all);

						return true;
					}
					catch (OperationCanceledException)
					{
						return false;
					}
					catch (Exception exc)
					{
						error = exc;
						return false;
					}
				}, cancelable: true);
			}

			RefreshStatus();

			if (error is not null)
			{
				logger.WriteLine("error building the text index", error);
				MoreMessageBox.ShowError(this, error.Message);
			}
			else if (result is not null && result.Failed)
			{
				MoreMessageBox.ShowError(this, Resx.IndexSheet_buildFailed);
			}
			else if (result is not null)
			{
				MoreMessageBox.Show(this, result.Cancelled
					? string.Format(Resx.IndexSheet_buildCancelled, result.Indexed)
					: string.Format(Resx.IndexSheet_buildDone, result.Indexed));
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			// neither requires a restart; the pipeline reads them again every cycle
			// The choice is only kept when it differs from what the installer set up, so that the
			// installer's default is still followed otherwise. Nothing is changed when indexing
			// cannot be used here, as the box was forced off.
			var save = false;
			if (available)
			{
				save = enabledBox.Checked == IndexConsumer.InstalledDefault()
					? settings.Remove("enabled")
					: settings.Add("enabled", enabledBox.Checked);
			}

			// kept only when it differs from the installer's default, as "enabled" is
			if (available)
			{
				save = allBox.Checked == IndexConsumer.InstalledDefault()
					? settings.Remove("all") || save
					: settings.Add("all", allBox.Checked) || save;
			}

			if (save)
			{
				provider.SetCollection(settings);
			}

			return false;
		}
	}
}
