//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

#pragma warning disable S3881 // IDisposable should be implemented correctly

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System;


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
			return type == null
				? new Application()
				: Activator.CreateInstance(type) as IApplication;
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