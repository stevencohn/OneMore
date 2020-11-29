//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemoveSpacingDialog : Dialogs.LocalizableForm
	{
		public RemoveSpacingDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.RemoveSpacingDialog_Text;

				Localize(new string[]
				{
					"beforeBox",
					"afterBox",
					"betweenBox",
					"headingsBox",
					"okButton",
					"cancelButton"
				});
			}
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - Height);
			UIHelper.SetForegroundWindow(this);
		}


		public bool SpaceAfter => afterBox.Checked;

		public bool SpaceBefore => beforeBox.Checked;

		public bool SpaceBetween => betweenBox.Checked;

		public bool IncludeHeadings => headingsBox.Checked;
	}
}
