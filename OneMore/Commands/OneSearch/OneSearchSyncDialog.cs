//************************************************************************************************
// OneSearch sync progress dialog
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal sealed class OneSearchSyncDialog : Form
	{
		private readonly OneSearchService service;
		private readonly OneSearchSettings settings;
		private readonly CancellationTokenSource cts = new();
		private readonly ManualResetEventSlim pauseEvent = new(true);
		private readonly ProgressBar progressBar = new() { Dock = DockStyle.Top, Height = 24, Minimum = 0, Maximum = 100 };
		private readonly Label statusLabel = new() { Dock = DockStyle.Top, Height = 22, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
		private readonly Label detailLabel = new() { Dock = DockStyle.Top, Height = 48, TextAlign = System.Drawing.ContentAlignment.TopLeft, AutoEllipsis = true, AutoSize = false };
		private readonly Button pauseButton = new() { Text = "Pause", Width = 90 };
		private readonly Button cancelButton = new() { Text = "Cancel", Width = 90 };

		private int total;
		private bool paused;


		public OneSearchSyncDialog(OneSearchService service, OneSearchSettings settings)
		{
			this.service = service;
			this.settings = settings;

			Text = "OneSearch Sync";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			StartPosition = FormStartPosition.CenterParent;
			Width = 720;
			Height = 240;
			MinimumSize = new System.Drawing.Size(680, 220);

			var panel = new FlowLayoutPanel
			{
				Dock = DockStyle.Bottom,
				FlowDirection = FlowDirection.RightToLeft,
				Height = 40,
				Padding = new Padding(8)
			};

			pauseButton.Click += (_, _) => TogglePause();
			cancelButton.Click += (_, _) => CancelSync();
			panel.Controls.Add(cancelButton);
			panel.Controls.Add(pauseButton);

			Controls.Add(panel);
			Controls.Add(detailLabel);
			Controls.Add(statusLabel);
			Controls.Add(progressBar);

			Shown += async (_, _) => await StartSyncAsync();
			FormClosed += (_, _) => { if (!cts.IsCancellationRequested) cts.Cancel(); pauseEvent.Set(); };
		}


		private async Task StartSyncAsync()
		{
			statusLabel.Text = "Starting...";
			progressBar.Value = 0;
			pauseButton.Enabled = true;
			cancelButton.Enabled = true;

			var progress = new Progress<SyncProgress>(ReportProgress);

			try
			{
				var summary = await service.SyncAsync(settings.CacheRoot, progress, cts.Token, pauseEvent);
				statusLabel.Text = $"Done. Updated {summary.Exported}, skipped {summary.Skipped}, failed {summary.Failed}.";
				pauseButton.Enabled = false;
				cancelButton.Text = "Close";
			}
			catch (OperationCanceledException)
			{
				statusLabel.Text = "Cancelled.";
				pauseButton.Enabled = false;
				cancelButton.Text = "Close";
			}
			catch (Exception exc)
			{
				statusLabel.Text = exc.Message;
				pauseButton.Enabled = false;
				cancelButton.Text = "Close";
			}
		}


		private void ReportProgress(SyncProgress progress)
		{
			if (progress.Total <= 0)
			{
				progressBar.Style = ProgressBarStyle.Marquee;
				statusLabel.Text = $"Processing {progress.Processed}...";
			}
			else
			{
				progressBar.Style = ProgressBarStyle.Continuous;
				total = progress.Total;
				var percent = Math.Min(100, (int)((double)progress.Processed / total * 100));
				progressBar.Value = percent;
				statusLabel.Text = $"Processing {progress.Processed}/{progress.Total}";
			}

			detailLabel.Text = $"Notebook: {progress.CurrentNotebook ?? "(unknown)"}\r\nPage: {progress.CurrentTitle}";
		}


		private void TogglePause()
		{
			if (paused)
			{
				pauseEvent.Set();
				pauseButton.Text = "Pause";
				statusLabel.Text = "Resumed.";
				paused = false;
			}
			else
			{
				pauseEvent.Reset();
				pauseButton.Text = "Resume";
				statusLabel.Text = "Paused.";
				paused = true;
			}
		}


		private void CancelSync()
		{
			if (cancelButton.Text == "Close")
			{
				Close();
				return;
			}

			cancelButton.Enabled = false;
			cts.Cancel();
			pauseEvent.Set();
		}
	}
}
