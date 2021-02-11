//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
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
				await new FootnoteEditor(one).RefreshLabels(true);
			}
		}
	}
}
