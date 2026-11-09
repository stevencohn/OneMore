//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Backups
{
	using River.OneMoreAddIn.Pipeline;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The stage of the background pipeline that backs up the database when the schedule
	/// chosen in Settings says a backup is due.
	/// </summary>
	internal sealed class BackupStage : Loggable, IPipelineStage
	{
		// the schedule is measured in days so checking hourly is plenty
		private const int CheckInterval = 60 * 60 * 1000;


		public string Name => "backup";

		public int Interval => CheckInterval;

		// read every cycle so that changes in Settings take effect without a restart
		public bool IsEnabled => BackupOptions.Load().Enabled;


		public void Initialize()
		{
		}


		// not-ready stages are retried every few seconds, so "not due" is handled in Run
		public Task<bool> IsReady(CancellationToken token)
		{
			return Task.FromResult(true);
		}


		public async Task Run(PipelineContext context, CancellationToken token)
		{
			var options = BackupOptions.Load();
			if (!options.Enabled || !DatabaseBackup.IsDue(options, System.DateTime.UtcNow))
			{
				return;
			}

			var outcome = await DatabaseBackup.Execute(options, null, false);
			logger.WriteLine($"database backup: {outcome}");
		}


		public void Dispose()
		{
		}
	}
}
