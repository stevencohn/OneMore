//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Threading.Tasks;

	internal class InsertDateCommand : Command
	{

		public InsertDateCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out _);

			var includeTime = (bool)args[0];
			var text = DateTime.Now.ToString(includeTime ? "yyy-MM-dd hh:mm tt" : "yyy-MM-dd");

			var editor = new PageEditor(page);
			editor.InsertOrReplace(text);

			await one.Update(page);
		}
	}
}
