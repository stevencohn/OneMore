//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class UpdateGuardDialog : UI.MoreForm
	{
		public UpdateGuardDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.UpdateGuardDialog_Text;
				Localize(new string[]
				{
					"messageBox",
					"browseLink",
					"okButton=word_OK"
				});
			}

			iconBox.Image = SystemIcons.Warning.ToBitmap();
		}


		private void BrowseReleases(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start($"{Resx.OneMore_GitHub}/releases/latest");
			Close();
		}
	}
}
