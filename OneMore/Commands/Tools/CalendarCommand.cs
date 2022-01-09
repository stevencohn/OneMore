//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Threading.Tasks;


	internal class CalendarCommand : Command
	{
		public CalendarCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// presume same location as executing addin assembly

			var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			// special override for my own debugging
			if (location.StartsWith(@"C:\Github\OneMore"))
			{
				location = @"C:\Github\OneMore\OneMoreCalendar\bin\Debug";
			}

			var path = Path.Combine(location, "OneMoreCalendar.exe");

			try
			{
				Process.Start(path);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error starting calendar at {path}", exc);
			}

			await Task.Yield();
		}
	}
}
