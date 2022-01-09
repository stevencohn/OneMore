//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

#pragma warning disable S3881 // IDisposable should be implemented correctly

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System;
	using System.Runtime.InteropServices;


	/// <summary>
	/// Provides a poor-man's IOC registration where we can replace the OneNote Application
	/// object with a mock instance for testing
	/// </summary>
	internal static class ApplicationFactory
	{
		private static Type type = null;


		/// <summary>
		/// Instantiate a new IApplication object
		/// </summary>
		/// <returns>An IApplication</returns>
		public static IApplication CreateApplication()
		{
			IApplication app = null;
			int retries = 0;

			try
			{
				while (retries < 3)
				{
					try
					{
						app = type == null
							? new Application()
							: Activator.CreateInstance(type) as IApplication;

						if (retries > 0)
						{
							Logger.Current.WriteLine(
								$"completed successfully after {retries} retries");
						}

						retries = int.MaxValue;
					}
					catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrCOMBusy)
					{
						retries++;
						var ms = 250 * retries;

						Logger.Current.WriteLine($"OneNote is busy, retyring in {ms}ms");
						System.Threading.Thread.Sleep(ms);
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error instantiating OneNote IApplication after {retries} retries", exc);
			}

			return app;
		}


		/// <summary>
		/// Registers an alternate implementation of the IApplication interface
		/// </summary>
		/// <param name="type">The type of the alternate implementation</param>
		public static void RegisterApplication(Type type)
		{
			ApplicationFactory.type = type;
		}
	}
}