//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemoveSpacingDialog : UI.LocalizableForm
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
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool SpaceAfter => afterBox.Checked;

		public bool SpaceBefore => beforeBox.Checked;

		public bool SpaceBetween => betweenBox.Checked;

		public bool IncludeHeadings => headingsBox.Checked;
	}
}
