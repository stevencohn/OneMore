//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Outlook;
	using System;
	using System.Runtime.InteropServices;


	/// <summary>
	/// Slight abstraction of an Outlook folder.
	/// </summary>
	internal class OutlookFolder : IDisposable
	{
		private bool disposed;

		/// <summary>
		/// Initializes a new instance with the specified Outlook folder.
		/// </summary>
		/// <param name="folder">
		/// The Outlook folder to wrap.
		/// This is the actual Outlook folder that will be used to retrieve items.
		/// </param>
		public OutlookFolder(Folder folder)
		{
			Folder = folder;
		}


		/// <summary>
		/// Releases the wrapped Outlook Folder COM object.
		/// </summary>
		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			Marshal.ReleaseComObject(Folder);
			disposed = true;
		}


		/// <summary>
		/// Gets the name of the Outlook folder.
		/// This is the name that will be displayed in Outlook.
		/// </summary>
		public string Name => Folder.Name;


		/// <summary>
		/// Gets the underlying Outlook folder object. This is the actual Outlook folder that
		/// will be used to retrieve items.
		/// </summary>
		public Folder Folder { get; private set; }
	}
}
