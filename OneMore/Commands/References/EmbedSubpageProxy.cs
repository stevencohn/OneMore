﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Proxy to EmbedSubpageCommand that accepts string arguments from the
	/// CommandService named pipe and from OneNoteProtocolHandler
	/// </summary>
	internal class EmbedSubpageProxy : Command
	{
		public EmbedSubpageProxy()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var update = true;
			if (args.Length > 0 && args[0] is string arg)
			{
				bool.TryParse(arg, out update);
			}

			await new EmbedSubpageCommand().Execute(update);
		}
	}
}