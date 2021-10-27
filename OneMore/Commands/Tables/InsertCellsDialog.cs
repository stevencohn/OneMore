//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class InsertCellsDialog : UI.LocalizableForm
	{
		public InsertCellsDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertCellsDialog_Text;

				Localize(new string[]
				{
					"shiftDownRadio",
					"shiftRightRadio",
					"numLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool ShiftDown => shiftDownRadio.Checked;


		public int ShiftCount => (int)numCellsBox.Value;
	}
}
