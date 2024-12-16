//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;


	internal class UpdatePageTimeCommand : Command
	{
		public UpdatePageTimeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _, OneNote.PageDetail.Basic);
			page.Root.SetAttributeValue("dateTime", DateTime.Now.ToZuluString());
			await one.Update(page);
		}
	}
}
