//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class TimestampDialog : LocalizableForm
	{
		public TimestampDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.TimestampDialog_Text;

				Localize(new string[]
				{
					"toggleGroup",
					"hideRadio",
					"showRadio",
					"scopeGroup",
					"pageRadio",
					"sectionRadio",
					"okButton",
					"cancelButton"
				});
			}
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
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
