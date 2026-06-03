//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;

	internal static class CliCommandFactory
	{
		public static CommandFactory Make()
		{
			var trash = new List<System.IDisposable>();

			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			AddIn.Telemetry = settings.Get("telemetry", false);

			return new CommandFactory(
				Logger.Current, null, trash,
				runningFromCli: true);
		}
	}
}
