//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.Office.Interop.OneNote;
	using System.Collections;
	using System.Collections.Generic;


	public class MockWindows : Windows
	{
		private readonly List<MockWindow> windows;

		public MockWindows(IApplication application)
		{
			windows = new List<MockWindow>
			{
				new MockWindow(application)
			};
		}

		public IEnumerator GetEnumerator()
		{
			return windows.GetEnumerator();
		}

		public Window this[uint Index] => windows[(int)Index];

		public uint Count => (uint)windows.Count;

		public Window CurrentWindow => windows[0];
	}
}