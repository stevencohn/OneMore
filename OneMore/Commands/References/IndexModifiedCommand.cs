//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Finds all pages modified on a given date across all loaded notebooks and creates a new
	/// page, in the specified notebook/section, listing hyperlinks to each of them.
	/// </summary>
	internal class IndexModifiedCommand : Command, ICliCommand
	{
		public IndexModifiedCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "IndexModified";


		public string Description =>
			"List pages modified on a given date, across all notebooks, into a new page";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook where the index page will be created",
				required: true)
			.AddString("section", "Path of section where the index page will be created",
				required: true)
			.AddString("date", "Date to search for, in any recognizable format, e.g. 2026-07-24",
				required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			await using var one = new OneNote();

			string sectionId;
			DateTime target;

			if (cliParams != null)
			{
				cliParams.TryGet("notebook", out string notebookName);
				cliParams.TryGet("section", out string sectionPath);
				cliParams.TryGet("date", out string dateText);

				sectionId = await one.FindSectionIdByPath(notebookName, sectionPath);
				if (string.IsNullOrEmpty(sectionId))
				{
					CliOutput = $"Section not found: {notebookName}/{sectionPath}";
					return;
				}

				if (!TryParseDate(dateText, out target))
				{
					CliOutput = $"Could not parse date: {dateText}";
					return;
				}
			}
			else
			{
				// palette invocation, no dialog; create the index page in the current section
				sectionId = one.CurrentSectionId;
				target = DateTime.Today;

				if (string.IsNullOrEmpty(sectionId))
				{
					ShowError(Resx.IndexModifiedCommand_NoSection);
					return;
				}
			}

			// hsPages scope rooted at "" returns the full recursive hierarchy of every loaded
			// notebook - section groups, sections, and pages - in a single call
			var notebooks = await one.GetNotebooks(OneNote.Scope.Pages);
			if (notebooks is null || !notebooks.HasElements)
			{
				CliOutput = "No notebooks found";
				return;
			}

			var ns = one.GetNamespace(notebooks);

			// exclude recycle bin section groups/sections (and their pages) from the search;
			// same attributes OneNote.Search() filters on
			notebooks.Descendants()
				.Where(n => n.Attribute("isRecycleBin") != null || n.Attribute("isInRecycleBin") != null)
				.Remove();

			var matches = notebooks.Descendants(ns + "Page")
				.Where(p =>
				{
					var attr = p.Attribute("lastModifiedTime")?.Value;
					return attr != null &&
						DateTime.Parse(attr, CultureInfo.InvariantCulture).ToLocalTime().Date == target.Date;
				})
				.Select(p => (
					id: p.Attribute("ID")?.Value,
					name: p.Attribute("name")?.Value,
					notebook: p.Ancestors(ns + "Notebook").FirstOrDefault()?.Attribute("name")?.Value,
					section: p.Parent?.Attribute("name")?.Value))
				.Where(p => p.id != null)
				.ToList();

			var title = string.Format(Resx.IndexModifiedCommand_Title, $"{target:yyyy-MM-dd}");

			if (matches.Count == 0)
			{
				var message = $"No pages modified on {target:yyyy-MM-dd}";
				if (cliParams != null)
				{
					CliOutput = message;
				}
				else
				{
					ShowInfo(message);
				}

				return;
			}

			one.CreatePage(sectionId, out var newPageId);
			var page = await one.GetPage(newPageId);
			var pns = page.Namespace;
			PageNamespace.Set(pns);

			page.Title = title;

			var h1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
			var h2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
			var container = page.EnsureContentContainer();

			foreach (var notebookGroup in matches
				.GroupBy(p => p.notebook)
				.OrderBy(g => g.Key))
			{
				container.Add(new Paragraph(notebookGroup.Key).SetQuickStyle(h1Index));

				foreach (var sectionGroup in notebookGroup
					.GroupBy(p => p.section)
					.OrderBy(g => g.Key))
				{
					container.Add(new Paragraph(string.Empty));

					// nest the bullets directly inside the heading OE (after its own text)
					// rather than wrapping them in a separate, textless OE; a textless OE
					// gets an empty <one:T/> synthesized into it on save, which renders as
					// a blank paragraph between the heading and its page list
					var heading = new Paragraph(pns, sectionGroup.Key).SetQuickStyle(h2Index);

					var bullets = new ContentList(pns);
					foreach (var m in sectionGroup.OrderBy(p => p.name))
					{
						var link = one.GetHyperlink(m.id, string.Empty);
						bullets.Add(new Bullet(pns,
							$"<a href=\"{link}\">{WebUtility.HtmlEncode(m.name)}</a>"));
					}

					heading.Add(bullets);
					container.Add(heading);
				}

				container.Add(new Paragraph(string.Empty));
			}

			await one.Update(page);

			if (cliParams != null)
			{
				CliOutput = $"Created \"{title}\" with {matches.Count} page(s)";
			}
			else
			{
				await one.NavigateTo(newPageId);
			}
		}


		private static bool TryParseDate(string text, out DateTime date)
		{
			// accept any recognizable date string/format, not just yyyy-MM-dd;
			// try the user's culture first, then invariant/current culture as fallbacks
			// since a CLI --date value may not match the running culture

			return
				DateTime.TryParse(text, AddIn.Culture, DateTimeStyles.None, out date) ||
				DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
				DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out date);
		}
	}
}
