//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.TextIndex
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// What a deliberate build of the text index did.
	/// </summary>
	internal sealed class IndexBuildResult
	{
		/// <summary>Gets the number of pages read and indexed.</summary>
		public int Indexed { get; set; }

		/// <summary>Gets whether the build was cut short by the user.</summary>
		public bool Cancelled { get; set; }

		/// <summary>Gets whether OneNote could not be read, so that nothing was done.</summary>
		public bool Failed { get; set; }
	}


	/// <summary>
	/// Builds the text index now, as the user asked, instead of a little at a time in the
	/// background. It is the same work as the background consumer's, with no time budget; the
	/// progress is reported so the user can follow it and cancel. Cancelling is safe: every page
	/// is indexed in its own transaction, and the background work resumes from where this left off.
	/// </summary>
	internal static class IndexBuilder
	{
		/// <summary>
		/// Indexes every page of the chosen notebooks that is not up to date.
		/// </summary>
		/// <param name="progress">Called after each page with the number indexed so far and the
		/// number expected</param>
		/// <param name="token">Cancels the build between pages</param>
		/// <param name="indexAll">True to index every notebook that has not been explicitly excluded,
		/// instead of only those the user chose</param>
		public static async Task<IndexBuildResult> Build(
			Action<int, int> progress, CancellationToken token, bool indexAll)
		{
			var result = new IndexBuildResult();

			// the background consumer is working through the same pages; it waits for this
			using var hold = IndexConsumer.SuspendBackground();

			var snapshot = await HashtagScanner.ReadIdentities(token, fillGuids: false);
			if (snapshot is null)
			{
				result.Failed = true;
				return result;
			}

			using var consumer = IndexConsumer.ForBuild(progress, indexAll);

			// no throttle of the user's own: the build is a deliberate burst, held to the small gap
			// that the consumer asks for between reads
			var pass = new PageContentPass(new IPageConsumer[] { consumer }, throttle: 0);
			await pass.Run(snapshot, token);

			result.Indexed = consumer.Indexed;
			result.Cancelled = token.IsCancellationRequested;
			return result;
		}
	}
}
