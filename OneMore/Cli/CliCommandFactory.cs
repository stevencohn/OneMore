//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System.Collections.Generic;

	internal static class CliCommandFactory
	{
		public static CommandFactory Make()
		{
			var trash = new List<System.IDisposable>();

			return new CommandFactory(
				Logger.Current, null, trash,
				runningFromCli: true);
		}
	}
}
