//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemindDialog : UI.LocalizableForm
	{
		private readonly Reminder reminder;
		private string symbol;


		public RemindDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.RemindDialog_Text;

				Localize(new string[]
				{
					"subjectLabel",
					"startDateLabel",
					"startedLabel",
					"dueDateLabel",
					"completedLabel",
					"statusLabel",
					"statusBox",
					"priorityLabel",
					"priorityBox",
					"percentLabel",
					"silentBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			startDateBox.CustomFormat = DateTimeExtensions.FriendlyPattern;
			dueDateBox.CustomFormat = DateTimeExtensions.FriendlyPattern;
		}


		public RemindDialog(Reminder reminder)
			: this()
		{
			this.reminder = reminder;

			subjectBox.Text = reminder.Subject;
			startDateBox.Value = reminder.Start;

			if (reminder.Status == ReminderStatus.InProgress ||
				reminder.Status == ReminderStatus.Completed)
			{
				startedBox.Text = reminder.Started.ToFriendlyString();
			}

			dueDateBox.Value = reminder.Due;

			if (reminder.Status == ReminderStatus.Completed)
			{
				completedBox.Text = reminder.Completed.ToFriendlyString();
			}

			statusBox.SelectedIndex = (int)reminder.Status;
			priorityBox.SelectedIndex = (int)reminder.Priority;
			percentBox.Value = reminder.Percent;
			silentBox.Checked = reminder.Silent;

			if (!string.IsNullOrEmpty(reminder.Symbol))
			{
				var symval = int.Parse(reminder.Symbol);
				using (var dialog = new UI.TagPickerDialog(0, 0))
				{
					var glyph = dialog.GetGlyph(symval);
					if (glyph != null)
					{
						tagButton.Text = null;
						tagButton.Image = glyph;
					}
				}
			}
		}


		public Reminder Reminder => reminder;


		private void ChangeSubject(object sender, System.EventArgs e)
		{
			okButton.Enabled = subjectBox.Text.Trim().Length > 0;
		}

		private void ChangePercent(object sender, System.EventArgs e)
		{
			if (percentBox.Value == 0) statusBox.SelectedIndex = (int)ReminderStatus.NotStarted;
			else if (percentBox.Value == 100) statusBox.SelectedIndex = (int)ReminderStatus.Completed;
			else statusBox.SelectedIndex = (int)ReminderStatus.InProgress;
		}

		private void ChangeStatus(object sender, System.EventArgs e)
		{
			if (statusBox.SelectedIndex == (int)ReminderStatus.NotStarted) percentBox.Value = 0;
			else if (statusBox.SelectedIndex == (int)ReminderStatus.Completed) percentBox.Value = 100;
			else if (statusBox.SelectedIndex == (int)ReminderStatus.InProgress && percentBox.Value == 100) percentBox.Value = 0;
		}

		private void SelectTag(object sender, System.EventArgs e)
		{
			var location = PointToScreen(tagButton.Location);

			using (var dialog = new UI.TagPickerDialog(0, 0))
			{
				// TagPickerDialog customizes Left and Top in its constructor because most
				// consumers place the tagButton in a groupBox and that offsets its true
				// position, whereas here we need the actual location of tagButton
				dialog.Left = location.X + (tagButton.Width / 2);
				dialog.Top = location.Y + tagButton.Height - 3;

				if (reminder?.Symbol is string sym && sym != "0")
				{
					dialog.Select(int.Parse(sym));
				}

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					var glyph = dialog.GetGlyph();
					if (glyph != null)
					{
						tagButton.Text = null;
						tagButton.Image = glyph;
					}
					else
					{
						tagButton.Text = "?";
					}

					symbol = dialog.Symbol.ToString();
				}
			}
		}

		private void OK(object sender, System.EventArgs e)
		{
			reminder.Subject = subjectBox.Text.Trim();
			reminder.Start = startDateBox.Value;
			reminder.Due = dueDateBox.Value;
			reminder.Status = (ReminderStatus)statusBox.SelectedIndex;
			reminder.Priority = (ReminderPriority)priorityBox.SelectedIndex;
			reminder.Percent = (int)percentBox.Value;
			reminder.Silent = silentBox.Checked;

			if (!string.IsNullOrEmpty(symbol) && symbol != "0")
			{
				reminder.Symbol = symbol;
			}
		}
	}
}
