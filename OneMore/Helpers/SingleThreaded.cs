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

		/// <summary>
		/// Invoke the given Func on an STA thread or on the current thread if it is STA.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public static Task<T> Invoke<T>(Func<T> func)
		{
			var source = new TaskCompletionSource<T>();

			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				try
				{
					source.SetResult(func());
				}
				catch (Exception e)
				{
					source.SetException(e);
				}
			}
			else
			{
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
			}

			return source.Task;
		}


		/// <summary>
		/// Invoke the given Action on an STA thread or on the current thread if it is STA.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static Task Invoke(Action action)
		{
			var source = new TaskCompletionSource<object>();

			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
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
			}
			else
			{
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
			}

			return source.Task;
		}
	}
}
