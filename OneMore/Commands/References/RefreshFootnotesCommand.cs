//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class RefreshFootnotesCommand : Command
	{

		public RefreshFootnotesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var editor = new FootnoteEditor(one);
				if (editor.ValidContext())
				{
					await editor.RefreshLabels(true);
				}
			}
		}
	}
}
