//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ToggleDttmDialog : UI.LocalizableForm
	{
		public ToggleDttmDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ToggleDttmDialog_Text;

				Localize(new string[]
				{
					"toggleGroup",
					"hideRadio",
					"showRadio",
					"scopeGroup",
					"pageRadio",
					"sectionRadio",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		/// <summary>
		/// Gets a Boolean value indicating if the timestamps should be shown. Otherwise,
		/// the timestamps are hidden.
		/// </summary>
		public bool ShowTimestamps => showRadio.Checked;


		/// <summary>
		/// Gets a Boolean value indicating if the scope should be constrained to the current
		/// page. Otherwise, the scope is all pages in the current section
		/// </summary>
		public bool PageOnly => pageRadio.Checked;
	}
}
