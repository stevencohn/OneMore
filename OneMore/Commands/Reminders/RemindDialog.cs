//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemindDialog : UI.MoreForm
	{
		private readonly Reminder reminder;
		private string symbol;


		public RemindDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Reminder;

				Localize(new string[]
				{
					"subjectLabel",
					"startDateLabel",
					"startedLabel=word_Started",
					"dueDateLabel",
					"completedLabel=word_Completed",
					"statusLabel=word_Status",
					"statusBox",
					"priorityLabel",
					"priorityBox",
					"percentLabel=phrase_PctComplete",
					"optionsBox=word_Options",
					"silentBox",
					"snoozeLabel",
					"snoozeBox",
					"snoozeButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			startDateBox.CustomFormat = DateTimeExtensions.FriendlyPattern;
			dueDateBox.CustomFormat = DateTimeExtensions.FriendlyPattern;
			snoozeBox.SelectedIndex = 0;
		}


		public RemindDialog(Reminder reminder, bool canSnooze)
			: this()
		{
			this.reminder = reminder;

			subjectBox.Text = reminder.Subject;
			startDateBox.Value = reminder.Start.ToLocalTime();

			if (reminder.Status == ReminderStatus.InProgress ||
				reminder.Status == ReminderStatus.Completed)
			{
				startedBox.Text = reminder.Started.ToFriendlyString();
			}

			dueDateBox.Value = reminder.Due.ToLocalTime();

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
				symbol = reminder.Symbol;
				var symval = int.Parse(reminder.Symbol);
				using var dialog = new UI.TagPickerDialog(0, 0);
				var glyph = dialog.GetGlyph(symval);
				if (glyph != null)
				{
					tagButton.Text = null;
					tagButton.Image = glyph;
				}
			}

			snoozeTimeLabel.Text = string.Empty;
			var snoozed = reminder.Snooze != SnoozeRange.None &&
				reminder.SnoozeTime.CompareTo(DateTime.UtcNow) > 0;

			if (snoozed)
			{
				snoozeBox.SelectedIndex = (int)reminder.Snooze;
			}

			snoozeBox.Enabled = snoozeButton.Enabled = canSnooze || snoozed;
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

			using var dialog = new UI.TagPickerDialog(0, 0);

			// TagPickerDialog customizes Left and Top in its constructor because most
			// consumers place the tagButton in a groupBox and that offsets its true
			// position, whereas here we need the actual location of tagButton
			dialog.Left = location.X + (tagButton.Width / 2);
			dialog.Top = location.Y + tagButton.Height - 3;

			if (symbol is string sym && sym != "0")
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


		private void ChangeDate(object sender, EventArgs e)
		{
			if (sender == startDateBox)
			{
				if (startDateBox.Value.CompareTo(dueDateBox.Value) > 0)
				{
					dueDateBox.Value = startDateBox.Value;
				}
			}
			else
			{
				if (dueDateBox.Value.CompareTo(startDateBox.Value) < 0)
				{
					startDateBox.Value = dueDateBox.Value;
				}
			}
		}


		private void SelectSnooze(object sender, EventArgs e)
		{
			// dialog not yet initialized
			if (reminder == null) return;

			if (snoozeBox.SelectedIndex == 0)
			{
				if (reminder.Snooze == SnoozeRange.None ||
					reminder.SnoozeTime.CompareTo(DateTime.UtcNow) < 0)
				{
					snoozeTimeLabel.Text = string.Empty;
				}
				else
				{
					snoozeTimeLabel.Text = $"({reminder.SnoozeTime.ToShortFriendlyString()})";
				}
			}
			else
			{
				var time = DateTime.UtcNow.Add(GetSnoozeSpan((SnoozeRange)snoozeBox.SelectedIndex));
				snoozeTimeLabel.Text = $"({time.ToShortFriendlyString()})";
			}
		}


		private TimeSpan GetSnoozeSpan(SnoozeRange range)
		{
			return range switch
			{
				SnoozeRange.S5minutes => TimeSpan.FromMinutes(5),
				SnoozeRange.S10minutes => TimeSpan.FromMinutes(10),
				SnoozeRange.S15minutes => TimeSpan.FromMinutes(15),
				SnoozeRange.S30minutes => TimeSpan.FromMinutes(30),
				SnoozeRange.S1hour => TimeSpan.FromHours(1),
				SnoozeRange.S2hours => TimeSpan.FromHours(2),
				SnoozeRange.S4hours => TimeSpan.FromHours(4),
				SnoozeRange.S1day => TimeSpan.FromDays(1),
				SnoozeRange.S2days => TimeSpan.FromDays(2),
				SnoozeRange.S3days => TimeSpan.FromDays(3),
				SnoozeRange.S1week => TimeSpan.FromDays(7),
				SnoozeRange.S2weeks => TimeSpan.FromDays(14),
				_ => TimeSpan.Zero,
			};
		}


		private void OK(object sender, System.EventArgs e)
		{
			reminder.Subject = subjectBox.Text.Trim();
			reminder.Start = startDateBox.Value.ToUniversalTime();
			reminder.Due = dueDateBox.Value.ToUniversalTime();
			reminder.Status = (ReminderStatus)statusBox.SelectedIndex;
			reminder.Priority = (ReminderPriority)priorityBox.SelectedIndex;
			reminder.Percent = (int)percentBox.Value;
			reminder.Silent = silentBox.Checked;

			if (!string.IsNullOrEmpty(symbol) && symbol != "0")
			{
				reminder.Symbol = symbol;
			}

			if (sender == snoozeButton)
			{
				if (snoozeBox.SelectedIndex == 0)
				{
					reminder.Snooze = SnoozeRange.None;
					reminder.SnoozeTime = reminder.Start;
				}
				else
				{
					reminder.Snooze = (SnoozeRange)snoozeBox.SelectedIndex;
					reminder.SnoozeTime = DateTime.UtcNow.Add(GetSnoozeSpan(reminder.Snooze));
				}

				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
