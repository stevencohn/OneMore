//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Windows.Forms;


	internal class EditStylesCommand : Command
	{
		public EditStylesCommand() : base()
		{
		}


		public void Execute()
		{
			var provider = new StyleProvider();

			var styles = provider.GetStyles();
			DialogResult result;

			using (var dialog = new StyleDialog(styles))
			{
				result = dialog.ShowDialog(owner);

				if (result == DialogResult.OK)
				{
					// save styles to remove delete items and preserve ordering
					styles = dialog.GetStyles();
					provider.Save(styles);
					ribbon.Invalidate();
				}
			}
		}
	}
}
