//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading;


	/// <summary>
	/// Process-wide reference-counted pause signal for the HashtagService scanner.
	/// Foreground commands that make heavy OneNote COM calls hold a pause token via Hold()
	/// to signal the background scanner to extend its inter-page throttle delay.
	/// </summary>
	internal static class HashtagServicePause
	{
		private static volatile int count;


		/// <summary>
		/// Returns true when at least one caller is holding a pause token.
		/// </summary>
		public static bool IsPaused => count > 0;


		/// <summary>
		/// Signals the scanner to back off while the returned handle is alive.
		/// The caller must dispose the handle when it no longer needs the pause.
		/// </summary>
		public static IDisposable Hold()
		{
			Interlocked.Increment(ref count);
			return new PauseHandle();
		}


		private sealed class PauseHandle : IDisposable
		{
			private int disposed;

			public void Dispose()
			{
				if (Interlocked.Exchange(ref disposed, 1) == 0)
				{
					Interlocked.Decrement(ref count);
				}
			}
		}
	}
}
