//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Generates a set of dated pages for a chosen month, one page per day, optionally
	/// skipping weekends, either in the current section or in a new section created for
	/// the purpose.
	/// </summary>
	internal class CreateJournalCommand : Command
	{
		public CreateJournalCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			if (string.IsNullOrEmpty(one.CurrentSectionId))
			{
				ShowError(Resx.CreateJournalCommand_noNotebook);
				return;
			}

			var notebook = await one.GetNotebook();
			var nns = notebook.GetNamespaceOfPrefix(OneNote.Prefix);
			var existingSectionNames = notebook.Descendants(nns + "Section")
				.Attributes("name")
				.Select(a => a.Value)
				.ToList();

			string title, sectionName, hashtags, dateFormat;
			int year, month;
			bool weekdaysOnly, createNewSection;

			using (var dialog = new CreateJournalDialog(existingSectionNames))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				dialog.SaveSettings();

				title = dialog.Title;
				year = dialog.Year;
				month = dialog.Month;
				weekdaysOnly = dialog.WeekdaysOnly;
				dateFormat = dialog.DateFormat;
				createNewSection = dialog.CreateNewSection;
				sectionName = dialog.SectionName;
				hashtags = dialog.Hashtags;
			}

			var dates = ComputeDateRange(year, month, weekdaysOnly);

			logger.WriteLine(
				$"creating journal for {year}/{month}, {dates.Count} pages, " +
				$"weekdaysOnly:{weekdaysOnly}, newSection:{createNewSection}");

			string sectionId;

			if (createNewSection)
			{
				try
				{
					var section = await one.CreateSection(sectionName);
					sectionId = section.Attribute("ID").Value;
				}
				catch (COMException exc)
				{
					logger.WriteLine("error creating journal section", exc);
					ShowError(Resx.CreateJournalCommand_syncError);
					return;
				}
			}
			else
			{
				sectionId = one.CurrentSectionId;
			}

			var sectionXml = await one.GetSection(sectionId);
			var sns = sectionXml.GetNamespaceOfPrefix(OneNote.Prefix);
			var existingTitles = new HashSet<string>(
				sectionXml.Elements(sns + "Page").Attributes("name").Select(a => a.Value),
				StringComparer.CurrentCultureIgnoreCase);

			var progress = new UI.ProgressDialog(async (self, token) =>
			{
				self.SetMaximum(dates.Count);

				string firstPageId = null;
				var created = 0;

				try
				{
					foreach (var date in dates)
					{
						if (token.IsCancellationRequested)
						{
							break;
						}

						var pageTitle = $"{date.ToString(dateFormat, AddIn.Culture)} - {title}";
						self.SetMessage(pageTitle);

						try
						{
							one.CreatePage(sectionId, out var pageId);
							var newpage = await one.GetPage(pageId);

							pageTitle = DeduplicateTitle(pageTitle, existingTitles);
							existingTitles.Add(pageTitle);
							newpage.Title = pageTitle;

							newpage.Root.SetAttributeValue("dateTime", date.ToZuluString());

							if (!string.IsNullOrWhiteSpace(hashtags))
							{
								var tags = AddHashtagCommand.NormalizeTags(hashtags);
								AddHashtagCommand.AddTagsToBank(newpage, tags);
							}

							await one.Update(newpage);

							firstPageId ??= pageId;
							created++;
						}
						catch (COMException exc)
						{
							logger.WriteLine("error creating journal page", exc);
							ShowError(Resx.CreateJournalCommand_syncError);
							break;
						}

						self.Increment();
					}

					if (firstPageId != null)
					{
						await one.NavigateTo(firstPageId);
					}

					if (token.IsCancellationRequested)
					{
						ShowInfo(string.Format(
							Resx.CreateJournalCommand_cancelled, created, dates.Count));
					}
				}
				finally
				{
					self.Close();
				}

				logger.WriteLine($"created {created} of {dates.Count} journal pages");
			});

			progress.RunModeless();
		}


		/// <summary>
		/// Computes the ordered list of calendar dates for the given month, optionally
		/// excluding Saturdays and Sundays. Shared by CreateJournalDialog (live page-count
		/// preview) and this command (actual page creation) so the two always agree.
		/// </summary>
		internal static List<DateTime> ComputeDateRange(int year, int month, bool weekdaysOnly)
		{
			var days = DateTime.DaysInMonth(year, month);
			var dates = new List<DateTime>(days);

			for (var day = 1; day <= days; day++)
			{
				var date = new DateTime(year, month, day);
				if (!weekdaysOnly ||
					(date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
				{
					dates.Add(date);
				}
			}

			return dates;
		}


		private static string DeduplicateTitle(string title, HashSet<string> existingTitles)
		{
			if (!existingTitles.Contains(title))
			{
				return title;
			}

			var n = 2;
			string candidate;
			do
			{
				candidate = $"{title} ({n})";
				n++;
			}
			while (existingTitles.Contains(candidate));

			return candidate;
		}
	}
}
