//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Provides awaitable helper methods to invoke a Func or Action on an STA thread.
	/// This is required, for example, to access the Windows clipboard
	/// </summary>
	internal static class SingleThreaded
	{

		public static Task<T> Invoke<T>(Func<T> func)
		{
			var source = new TaskCompletionSource<T>();
			var thread = new Thread(() =>
			{
				try
				{
					source.SetResult(func());
				}
				catch (Exception e)
				{
					source.SetException(e);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			return source.Task;
		}


		public static Task Invoke(Action action)
		{
			var source = new TaskCompletionSource<object>();
			var thread = new Thread(() =>
			{
				try
				{
					action();
					source.SetResult(null);
				}
				catch (Exception e)
				{
					source.SetException(e);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			return source.Task;
		}
	}
}
