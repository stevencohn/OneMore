//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;
	using River.OneMoreAddIn.Helpers.Office;
	using Resx = Properties.Resources;


	[CommandService]
	internal class OutlookContactCommand : Command
	{

		public override async Task Execute(params object[] args)
		{
			if (args.Length > 1 && args[0] is string action)
			{
				if (action == "refresh" && args.Length > 2)
				{
					if (!Office.IsInstalled("Outlook"))
					{
						ShowInfo(Resx.ImportOutlookContactsCommand_outlookRequired);
						return;
					}

					await Refresh(args);
				}
				else if (action == "save" && args.Length > 2)
				{
					if (!Office.IsInstalled("Outlook"))
					{
						ShowInfo(Resx.ImportOutlookContactsCommand_outlookRequired);
						return;
					}

					await Save(args);
				}
			}

			await Task.Yield();
		}


		private static async Task Refresh(object[] args)
		{
			var guid = args[1] as string;

			if (!Enum.TryParse(args[2] as string, out ContactTemplateOption template))
			{
				template = ContactTemplateOption.Both;
			}

			await new ContactGenerator().UpdateReport(guid, template);
		}


		private static async Task Save(object[] args)
		{
			var guid = args[1] as string;

			if (!Enum.TryParse(args[2] as string, out ContactTemplateOption template))
			{
				template = ContactTemplateOption.Both;
			}

			await new ContactGenerator().SaveReport(guid, template);
		}
	}
}
