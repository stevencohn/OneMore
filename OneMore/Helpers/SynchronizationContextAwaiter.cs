//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1186 // Methods should not be empty

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading;

	/*
	 * based on https://thomaslevesque.com/2015/11/11/explicitly-switch-to-the-ui-thread-in-an-async-method/
	 */

	/// <summary>
	/// OneNote COM interop sucks. This solves the suck. async/await isn't guaranteed to return
	/// to the original awaiting thread because there is no default WinForm synchronization
	/// context in our add-in; the add-in runs on an interop thread in its own dllhost.exe
	/// process, not the OneNote UI thread, dummy!
	/// 
	/// This class, along with the SynchronizationContextExtension allows a pattern that returns
	/// to the original thread so we can continue to interact with the OneNote UI.
	/// </summary>
	/// <example>
	/// var context = SynchronizationContext.Current;
	/// await SomethingAsync().ConfigureAwait(false);
	/// await context;
	/// </example>
	public readonly struct SynchronizationContextAwaiter : INotifyCompletion
	{
		private static readonly SendOrPostCallback callback = state => ((Action)state)();

		private readonly SynchronizationContext context;

		public SynchronizationContextAwaiter(SynchronizationContext context)
		{
			this.context = context;
		}

		public bool IsCompleted => context == SynchronizationContext.Current;

		public void OnCompleted(Action continuation) => context.Post(callback, continuation);

		public void GetResult() { }
	}


	/// <summary>
	/// Allows the caller to 'await context'
	/// </summary>
	public static class SynchronizationContextExtensions
	{
		public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
		{
			return new SynchronizationContextAwaiter(context);
		}
	}
}