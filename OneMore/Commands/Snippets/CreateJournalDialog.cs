//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Resx = Properties.Resources;


	internal partial class CreateJournalDialog : MoreForm
	{
		private const string SettingsKey = "Journal";

		private static readonly char[] InvalidSectionChars 
			= { '\\', '/', '*', '?', '"', '<', '>', '|', ':', '%' };

		private static readonly string[] DateFormats =
		{
			"dddd, MMMM d, yyyy",
			"MMMM d, yyyy",
			"MMM d, yyyy",
			"yyyy-MM-dd"
		};

		private readonly List<string> existingSectionNames;
		private bool settingSectionName;
		private bool sectionNameEdited;
		private bool loaded;


		public CreateJournalDialog(List<string> existingSectionNames)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CreateJournalDialog_Text;

				Localize(new string[]
				{
					"titleLabel",
					"yearLabel",
					"monthLabel",
					"dayRangeLabel",
					"weekdaysOnlyRadio",
					"weekdaysWeekendsRadio",
					"dateFormatLabel",
					"destinationLabel",
					"currentSectionRadio",
					"newSectionRadio",
					"sectionNameLabel",
					"hashtagsLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			this.existingSectionNames = existingSectionNames;

			titleBox.Items.AddRange(new object[] { Resx.word_Journal, Resx.word_Diary });

			var now = DateTime.Now;
			yearBox.Value = now.Year;

			for (var i = 1; i <= 12; i++)
			{
				monthBox.Items.Add(AddIn.Culture.DateTimeFormat.GetMonthName(i));
			}
			monthBox.SelectedIndex = now.Month - 1;

			foreach (var format in DateFormats)
			{
				dateFormatBox.Items.Add(now.ToString(format, AddIn.Culture));
			}

			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsKey);

			var title = settings["title"];
			titleBox.Text = string.IsNullOrWhiteSpace(title) ? Resx.word_Journal : title;

			weekdaysOnlyRadio.Checked = settings.Get("weekdaysOnly", false);
			weekdaysWeekendsRadio.Checked = !weekdaysOnlyRadio.Checked;

			var dateFormat = settings["dateFormat"];
			var formatIndex = dateFormat is null ? -1 : Array.IndexOf(DateFormats, dateFormat);
			dateFormatBox.SelectedIndex = formatIndex >= 0 ? formatIndex : 0;

			// SettingsProvider.GetCollection misclassifies a saved empty string as an
			// XElement (its "does this look like a child element" check treats an empty
			// Value as a tell for that, when it's equally true of an empty plain string).
			// settings["hashtags"] would then hard-cast that XElement to string and throw.
			// Hashtags is the only field here that's ever legitimately saved empty, so
			// guard specifically here rather than patching the shared indexer.
			hashtagsBox.Text = settings.IsElement("hashtags") 
				? string.Empty 
				: settings["hashtags"] ?? string.Empty;

			errorLabel.Visible = false;

			// controls fire their *Changed events as they're configured above (and even
			// during InitializeComponent(), e.g. NumericUpDown.EndInit() clamping yearBox's
			// default Value into range before monthBox has any items) so ValidateFields must
			// not do real work until construction has actually finished
			loaded = true;
			ValidateFields(this, EventArgs.Empty);
		}


		public string Title => titleBox.Text.Trim();

		public int Year => (int)yearBox.Value;

		public int Month => monthBox.SelectedIndex + 1;

		public bool WeekdaysOnly => weekdaysOnlyRadio.Checked;

		public string DateFormat => DateFormats[dateFormatBox.SelectedIndex];

		public bool CreateNewSection => newSectionRadio.Checked;

		public string SectionName => sectionNameBox.Text.Trim();

		public string Hashtags => hashtagsBox.Text.Trim();


		/// <summary>
		/// Persists the fields identified by the spec (Title, Day range, Date format,
		/// Hashtags) to Settings.xml under the "Journal" collection. Called by the command
		/// after the dialog returns DialogResult.OK.
		/// </summary>
		public void SaveSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsKey);

			settings.Add("title", Title);
			settings.Add("weekdaysOnly", WeekdaysOnly);
			settings.Add("dateFormat", DateFormat);

			// an empty string value round-trips through GetCollection as an XElement, not a
			// string (see the read-side comment in the constructor), so avoid persisting one
			if (string.IsNullOrEmpty(Hashtags))
			{
				settings.Remove("hashtags");
			}
			else
			{
				settings.Add("hashtags", Hashtags);
			}

			provider.SetCollection(settings);
			provider.Save();
		}


		private void ChangeDestination(object sender, EventArgs e)
		{
			sectionNameBox.Enabled = newSectionRadio.Checked;
			ValidateFields(sender, e);
		}


		private void EditSectionName(object sender, EventArgs e)
		{
			if (!settingSectionName)
			{
				sectionNameEdited = true;
			}

			ValidateFields(sender, e);
		}


		private void UpdateSectionNameTemplate()
		{
			if (sectionNameEdited)
			{
				return;
			}

			settingSectionName = true;
			sectionNameBox.Text = $"{monthBox.Text} {Year} {Title}".Trim();
			settingSectionName = false;
		}


		private void ValidateFields(object sender, EventArgs e)
		{
			if (!loaded)
			{
				return;
			}

			UpdateSectionNameTemplate();

			errorLabel.Visible = false;
			var ok = Title.Length > 0;

			if (ok && LooksLikeDate(Title))
			{
				ShowValidationError(Resx.CreateJournalDialog_titleLooksLikeDate);
				ok = false;
			}

			var dates = CreateJournalCommand.ComputeDateRange(Year, Month, WeekdaysOnly);
			var dayRangeText = WeekdaysOnly
				? Resx.CreateJournalDialog_weekdaysOnly
				: Resx.CreateJournalDialog_weekdaysWeekends;

			if (CreateNewSection)
			{
				var name = SectionName;

				if (name.Length == 0 || name.Length > 50 || name.IndexOfAny(InvalidSectionChars) >= 0)
				{
					if (ok)
					{
						ShowValidationError(Resx.CreateJournalDialog_invalidSectionName);
					}

					ok = false;
				}
				else if (existingSectionNames.Contains(name, StringComparer.CurrentCultureIgnoreCase))
				{
					if (ok)
					{
						ShowValidationError(string.Format(Resx.CreateJournalDialog_sectionCollision, name));
					}

					ok = false;
				}

				previewLabel.Text = string.Format(
					Resx.CreateJournalDialog_previewNew, dates.Count, dayRangeText,
					name.Length > 0 ? name : sectionNameBox.Text);
			}
			else
			{
				previewLabel.Text = string.Format(
					Resx.CreateJournalDialog_previewCurrent, dates.Count, dayRangeText);
			}

			okButton.Enabled = ok;
		}


		private void ShowValidationError(string message)
		{
			errorLabel.Text = message;
			errorLabel.Visible = true;
		}


		private static bool LooksLikeDate(string text)
		{
			return DateTime.TryParse(
				text, AddIn.Culture, DateTimeStyles.None, out _);
		}
	}
}
