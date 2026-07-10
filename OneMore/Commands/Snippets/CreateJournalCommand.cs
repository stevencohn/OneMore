//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
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
	internal class CreateJournalCommand : Command, ICliCommand
	{
		public CreateJournalCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "CreateJournal";


		public string Description => "Generate a set of dated journal pages for a month";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the journal notebook", required: true)
			.AddString("sectiongroup",
				"Name of the section group in which to create the new section", required: false)
			.AddString("section", "Name of the section to append pages to", required: false)
			.AddString("title", "Title to append to each page",
				required: false, defaultValue: Resx.word_Journal)
			.AddInteger("year", "Year of journal entries", 1900, 2200,
				required: false, defaultValue: DateTime.Now.Year)
			.AddString("month", "Month of journal entries, by name or number",
				required: false, defaultValue: AddIn.Culture.DateTimeFormat.GetMonthName(DateTime.Now.Month))
			.AddString("format", "Date format string applied to each page title",
				required: false, defaultValue: AddIn.Culture.DateTimeFormat.LongDatePattern)
			.AddBoolean("weekdays", "Restrict journal entries to weekdays only",
				required: false, defaultValue: false)
			.AddString("name",
				"Customized name of a new section to create; default is to append to --section",
				required: false)
			.AddString("tags", "Comma-separated list of hashtags to add to every page",
				required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (runningFromCli)
			{
				await ExecuteCli(args);
				return;
			}

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


		private async Task ExecuteCli(object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			string notebookName = null;
			string sectionGroupName = null;
			string sectionName = null;
			string title = null;
			string month = null;
			string dateFormat = null;
			string newSectionName = null;
			string tags = null;
			var year = 0;
			var weekdaysOnly = false;

			if (cliParams != null)
			{
				cliParams.TryGet("notebook", out notebookName);
				cliParams.TryGet("sectiongroup", out sectionGroupName);
				cliParams.TryGet("section", out sectionName);
				cliParams.TryGet("title", out title);
				cliParams.TryGet("year", out year);
				cliParams.TryGet("month", out month);
				cliParams.TryGet("format", out dateFormat);
				cliParams.TryGet("weekdays", out weekdaysOnly);
				cliParams.TryGet("name", out newSectionName);
				cliParams.TryGet("tags", out tags);
			}

			if (string.IsNullOrWhiteSpace(notebookName))
			{
				CliOutput = "The --notebook argument is required.";
				return;
			}

			if (string.IsNullOrWhiteSpace(newSectionName) &&
				string.IsNullOrWhiteSpace(sectionGroupName) &&
				string.IsNullOrWhiteSpace(sectionName))
			{
				CliOutput = "Either --name, --sectiongroup, or --section must be specified.";
				return;
			}

			if (string.IsNullOrWhiteSpace(title))
			{
				title = Resx.word_Journal;
			}

			if (year <= 0)
			{
				year = DateTime.Now.Year;
			}

			if (string.IsNullOrWhiteSpace(month))
			{
				month = AddIn.Culture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
			}

			if (!TryParseMonth(month, out var monthIndex))
			{
				CliOutput = $"Invalid --month value: {month}";
				return;
			}

			if (string.IsNullOrWhiteSpace(dateFormat))
			{
				dateFormat = AddIn.Culture.DateTimeFormat.LongDatePattern;
			}

			await using var one = new OneNote();

			var notebook = await FindNotebookTree(one, notebookName);
			if (notebook == null)
			{
				CliOutput = $"Notebook not found: {notebookName}";
				return;
			}

			var ns = one.GetNamespace(notebook);

			string sectionId;
			string effectiveSectionName;

			try
			{
				if (!string.IsNullOrWhiteSpace(newSectionName))
				{
					// -name specified: append the new section directly to the notebook
					var section = await CreateSectionUnder(one, notebook, newSectionName);
					if (section == null)
					{
						CliOutput = $"Failed to create section: {newSectionName}";
						return;
					}

					sectionId = section.Attribute("ID").Value;
					effectiveSectionName = newSectionName;
				}
				else if (!string.IsNullOrWhiteSpace(sectionGroupName))
				{
					// -name not specified, -sectiongroup specified: append the new section,
					// named by -section (or a generated default), to the named section group
					var group = FindDescendant(notebook, ns, "SectionGroup", sectionGroupName);
					if (group == null)
					{
						CliOutput = $"Section group not found: {sectionGroupName}";
						return;
					}

					var name = string.IsNullOrWhiteSpace(sectionName)
						? $"{AddIn.Culture.DateTimeFormat.GetMonthName(monthIndex)} {year} {title}"
						: sectionName;

					var section = await CreateSectionUnder(one, group, name);
					if (section == null)
					{
						CliOutput = $"Failed to create section: {name}";
						return;
					}

					sectionId = section.Attribute("ID").Value;
					effectiveSectionName = name;
				}
				else
				{
					// neither -name nor -sectiongroup specified: append pages to the
					// existing section named by -section
					var section = FindDescendant(notebook, ns, "Section", sectionName);
					if (section == null)
					{
						CliOutput = $"Section not found: {sectionName}";
						return;
					}

					sectionId = section.Attribute("ID").Value;
					effectiveSectionName = sectionName;
				}
			}
			catch (COMException exc)
			{
				logger.WriteLine("error creating journal section", exc);
				CliOutput = "Error synchronizing with OneNote while creating the section.";
				return;
			}

			var dates = ComputeDateRange(year, monthIndex, weekdaysOnly);

			var sectionXml = await one.GetSection(sectionId);
			var sns = sectionXml.GetNamespaceOfPrefix(OneNote.Prefix);
			var existingTitles = new HashSet<string>(
				sectionXml.Elements(sns + "Page").Attributes("name").Select(a => a.Value),
				StringComparer.CurrentCultureIgnoreCase);

			string normalizedTags = null;
			if (!string.IsNullOrWhiteSpace(tags))
			{
				var tokens = tags.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0);
				normalizedTags = AddHashtagCommand.NormalizeTags(string.Join(" ", tokens));
			}

			logger.WriteLine(
				$"creating journal for {year}/{monthIndex}, {dates.Count} pages, " +
				$"weekdaysOnly:{weekdaysOnly}, section:{effectiveSectionName}");

			var created = 0;

			foreach (var date in dates)
			{
				if (Cancellation.IsCancellationRequested)
				{
					break;
				}

				var pageTitle = $"{date.ToString(dateFormat, AddIn.Culture)} - {title}";

				try
				{
					one.CreatePage(sectionId, out var pageId);
					var newpage = await one.GetPage(pageId);

					pageTitle = DeduplicateTitle(pageTitle, existingTitles);
					existingTitles.Add(pageTitle);
					newpage.Title = pageTitle;

					newpage.Root.SetAttributeValue("dateTime", date.ToZuluString());

					if (!string.IsNullOrEmpty(normalizedTags))
					{
						AddHashtagCommand.AddTagsToBank(newpage, normalizedTags);
					}

					await one.Update(newpage);

					created++;
				}
				catch (COMException exc)
				{
					logger.WriteLine("error creating journal page", exc);
					CliOutput = $"Error synchronizing with OneNote after creating {created} of {dates.Count} pages.";
					return;
				}
			}

			logger.WriteLine($"created {created} of {dates.Count} journal pages in section '{effectiveSectionName}'");

			CliOutput = $"Created {created} of {dates.Count} pages in section '{effectiveSectionName}'.";
		}


		private static bool TryParseMonth(string month, out int monthIndex)
		{
			if (int.TryParse(month, out var n) && n is >= 1 and <= 12)
			{
				monthIndex = n;
				return true;
			}

			for (var i = 1; i <= 12; i++)
			{
				if (string.Equals(AddIn.Culture.DateTimeFormat.GetMonthName(i), month,
						StringComparison.CurrentCultureIgnoreCase) ||
					string.Equals(AddIn.Culture.DateTimeFormat.GetAbbreviatedMonthName(i), month,
						StringComparison.CurrentCultureIgnoreCase))
				{
					monthIndex = i;
					return true;
				}
			}

			monthIndex = 0;
			return false;
		}


		/// <summary>
		/// Finds a notebook by name and returns its full section/section-group hierarchy
		/// (no pages).
		/// </summary>
		private static async Task<XElement> FindNotebookTree(OneNote one, string name)
		{
			var notebooks = await one.GetNotebooks();
			for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebooks = await one.GetNotebooks();
			}

			if (notebooks == null)
			{
				return null;
			}

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, name, StringComparison.CurrentCultureIgnoreCase));

			if (notebook == null)
			{
				return null;
			}

			return await one.GetNotebook(notebook.Attribute("ID").Value, OneNote.Scope.Sections);
		}


		/// <summary>
		/// Finds the first descendant of <paramref name="root"/> with the given local name
		/// (Section or SectionGroup) and display name, skipping recycle bins.
		/// </summary>
		private static XElement FindDescendant(
			XElement root, XNamespace ns, string localName, string name)
		{
			return root.Descendants(ns + localName)
				.Where(e => e.Attribute("isRecycleBin") == null && e.Attribute("isInRecycleBin") == null)
				.FirstOrDefault(e => string.Equals(
					e.Attribute("name")?.Value, name, StringComparison.CurrentCultureIgnoreCase));
		}


		/// <summary>
		/// Creates a new section under the given parent (a Notebook or SectionGroup element)
		/// and returns the newly created Section element with its assigned ID.
		/// </summary>
		private static async Task<XElement> CreateSectionUnder(OneNote one, XElement parent, string name)
		{
			var ns = one.GetNamespace(parent);
			parent.Add(new XElement(ns + "Section", new XAttribute("name", name)));
			one.UpdateHierarchy(parent);

			// GetNotebook simply wraps GetHierarchy(id, scope); it works for any hierarchy
			// node, not just notebooks, so it can refresh a SectionGroup parent too
			var refreshed = await one.GetNotebook(parent.Attribute("ID").Value, OneNote.Scope.Sections);

			return refreshed?.Elements(ns + "Section")
				.FirstOrDefault(e => string.Equals(
					e.Attribute("name")?.Value, name, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
