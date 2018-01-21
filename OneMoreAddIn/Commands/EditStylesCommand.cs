//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Windows.Forms;


	internal class EditStylesCommand : Command
	{
		public EditStylesCommand () : base()
		{
		}


		public void Execute ()
		{
			var provider = new StylesProvider();

			var styles = provider.GetStyles();
			DialogResult result;

			using (var dialog = new StyleDialog(styles))
			{
				result = dialog.ShowDialog(owner);
			}

			if (result == DialogResult.OK)
			{
				// strip out styles marked for deletion
				styles.RemoveAll(s => s.Name.Length == 0);

				provider.SaveStyles(styles);

				ribbon.Invalidate();
			}
		}
	}
}
