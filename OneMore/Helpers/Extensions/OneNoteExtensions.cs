//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal static class OneNoteExtensions
	{
		private const int LargeContentThreshold = 512 * 1024; // 1/2 MB
		private const int LargeUpdateTimeoutSeconds = 300; // 5 minutes
		private const long MinReportableMs = 1000; // 1 second


		/// <summary>
		/// OneMore Extension >> Updates the given page, showing a timed progress dialog if the
		/// page content is large enough that the save is likely to take a noticeable amount of
		/// time; otherwise updates directly with no dialog.
		/// </summary>
		/// <param name="one">The OneNote wrapper used to persist the page</param>
		/// <param name="page">The page to save</param>
		/// <param name="force">Keep all Outlines to force full page update</param>
		/// <returns>True if the page content was updated successfully</returns>
		public static async Task<bool> UpdateWithProgress(
			this OneNote one, Page page, bool force = false)
		{
			var size = page.Root.ToString(SaveOptions.DisableFormatting).Length;
			if (size <= LargeContentThreshold)
			{
				return await one.Update(page, force);
			}

			Logger.Current.WriteLine($"page content is {size} bytes, showing progress while saving");

			using var progress = new UI.ProgressDialog(LargeUpdateTimeoutSeconds);
			progress.SetMessage(Resx.InsertTocCommand_Saving);

			var result = progress.ShowTimedDialog(async (dialog, token) =>
			{
				try
				{
					return await one.Update(page, force);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error saving page", exc);
					return false;
				}
			}, cancelable: false);

			return result == DialogResult.OK;
		}


		/// <summary>
		/// OneMore Extension >> Reports aggregated page-save timing, gathered across a single
		/// OneNote instance's lifetime (typically one command execution), as a single telemetry
		/// event; a no-op unless the user has opted into telemetry, or the slowest Update call
		/// didn't even reach MinReportableMs (not interesting enough to be worth a report). Used
		/// to inform the size threshold used by UpdateWithProgress.
		/// </summary>
		/// <param name="count">The number of Update calls made through this instance</param>
		/// <param name="elapsedMs">The total elapsed milliseconds spent saving</param>
		/// <param name="bytes">The total serialized content size, in bytes, saved</param>
		/// <param name="maxMs">The longest single Update call, in milliseconds</param>
		/// <param name="maxBytes">The content size, in bytes, of that longest Update call</param>
		internal static void ReportUpdateTelemetry(
			int count, long elapsedMs, long bytes, long maxMs, long maxBytes)
		{
			if (!AddIn.Telemetry || count == 0 || maxMs < MinReportableMs)
			{
				return;
			}

			var info =
				$"count={count};avgMs={elapsedMs / count};avgBytes={bytes / count};" +
				$"maxMs={maxMs};maxBytes={maxBytes}";

			_ = TelemetryClient.LogTiming("page-save", string.Empty, info);
		}
	}
}
