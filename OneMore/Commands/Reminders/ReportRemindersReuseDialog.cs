//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ReportRemindersReuseDialog : UI.MoreForm
	{
		public ReportRemindersReuseDialog()
		{
			InitializeComponent();

			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ActiveControl = okButton;

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
