//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Updater
{
	internal interface IUpdateReport
	{
		bool IsUpToDate { get; }

		string InstalledDate { get; }

		string InstalledUrl { get; }

		string InstalledVersion { get; }

		string UpdateDate { get; }

		string UpdateDescription { get; }

		string UpdateUrl { get; }

		string UpdateVersion { get; }
	}
}
