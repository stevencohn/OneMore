//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ReportRemindersReuseDialog : UI.LocalizableForm
	{
		public ReportRemindersReuseDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ReportRemindersReuseDialog_Text;

				Localize(new string[]
				{
					"oldRadio",
					"newRadio",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public DialogResult Option =>
			oldRadio.Checked ? DialogResult.OK : DialogResult.Yes;
	}
}
