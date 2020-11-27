//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;


	internal class TaggedCommand : Command
	{

		public TaggedCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var dialog = new TaggedDialog();

			dialog.RunModeless((sender, e) =>
			{
				var d = sender as TaggedDialog;
				if (d?.DialogResult == DialogResult.OK)
				{
					//
				}
			}, 30);
		}
	}
}
