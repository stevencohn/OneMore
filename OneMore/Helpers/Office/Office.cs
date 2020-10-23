//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;


	internal static class Office
	{
		public static bool IsWordInstalled()
		{
			var type = Type.GetTypeFromProgID("Word.Application");
			return type != null;
		}


		public static bool IsPowerpointInstalled()
		{
			var type = Type.GetTypeFromProgID("Powerpoint.Application");
			return type != null;
		}
	}
}
