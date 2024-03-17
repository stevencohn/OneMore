﻿//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using Resx = Properties.Resources;


	internal partial class ScheduleScanDialog : MoreForm
	{
		public ScheduleScanDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ScheduleScanDialog_Title;

				Localize(new string[]
				{
					"introBox",
					"laterRadio",
					"hintLabel",
					"scheduleLabel=word_Schedule",
					"nowRadio",
					"warningLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			dateTimePicker.Value = DateTime.Today.AddDays(1);
		}


		public ScheduleScanDialog(DateTime time)
			: this()
		{
			dateTimePicker.Value = time;
		}


		public DateTime StartTime => nowRadio.Checked
			? DateTime.Now
			// DateTimePicker returns dates with Unspecified Kind, so force Local
			: DateTime.SpecifyKind(dateTimePicker.Value, DateTimeKind.Local);

		public void SetIntroText(string text)
		{
			introBox.Text = text;
		}
	}
}
