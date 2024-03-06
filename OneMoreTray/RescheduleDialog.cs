//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn.UI;
	using System;
	using Resx = Properties.Resources;


	internal partial class RescheduleDialog : MoreForm
	{
		public RescheduleDialog()
		{
			InitializeComponent();


			if (NeedsLocalizing())
			{
				Text = Resx.RescheduleDialog_Title;

				Localize(new string[]
				{
					"newLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public RescheduleDialog(DateTime time)
			: this()
		{
			currentLabel.Text = string.Format(
				Resx.RescheduleDialog_currentLabel_Text,
				time.ToString(Resx.ScheduleTimeFormat));

			dateTimePicker.Value = time;
		}


		public DateTime Time => dateTimePicker.Value;
	}
}
