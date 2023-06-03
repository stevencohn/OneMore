//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Proxy to EmbedSubpageCommand that accepts string arguments from the
	/// CommandService named pipe and from OneNoteProtocolHandler
	/// </summary>
	[CommandService]
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

			var sourceId = args.Length > 1 ? args[1] as string : null;
			var linkId = args.Length > 2 ? args[2] as string : null;

			await factory.Run<EmbedSubpageCommand>(update, sourceId, linkId);
		}
	}
}