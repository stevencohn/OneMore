//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using OLK = Microsoft.Office.Interop.Outlook;
	using Resx = Properties.Resources;


	[CommandService]
	internal class ImportOutlookContactsCommand : Command
	{
		private const string TableMeta = "omOutlookContacts";
		private const string RefreshMeta = "omOutlookContactsRefresh";
		private const string ContactMeta = "omOutlookContact";

		private const string ControlRowShading = "#C5E0B3";
		private const string OddRowShading = "#F2F2F2";
		private const string HeaderRowShading = "#1E4E79";
		private const string ContactGlyph = "\ue2af"; // "Contact" glyph in Segoe UI Symbol
		private const string ArrowGlyph = "\u2197"; // "NE Arrow" glyph in Segoe UI Symbol

		private const string RefreshUri = "onemore://ImportOutlookContactsCommand/refresh";

		private readonly Regex emailPattern;


		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ImportOutlookContactsCommand()
		{
			emailPattern = new Regex(@"([A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,})");
		}


		public override async Task Execute(params object[] args)
		{
			if (!Office.IsInstalled("Outlook"))
			{
				ShowInfo(Resx.ImportOutlookContactsCommand_outlookRequired);
				return;
			}

			if (args.Length > 1 && args[0] is string refreshArg && refreshArg == "refresh")
			{
				var guid = args[1] as string;
				var atemp = ImportOutlookContactsDialog.TemplateOption.Both;
				var asort = ImportOutlookContactsDialog.SortByOption.LastName;

				Enum.TryParse(args[2] as string, out atemp);
				Enum.TryParse(args[3] as string, out asort);

				await UpdateReport(args[1] as string, atemp, asort);
				return;
			}

			// select...

			List<OutlookCategory> categories;
			List<OutlookFolder> folders;
			List<OutlookContact> contacts;

			// Outlook COM objects (ContactItem, Folder, ...) are apartment-bound to whichever
			// thread creates them, and this dialog reads their properties throughout its whole
			// lifetime - so the load must happen synchronously on this same thread rather than
			// on a background thread (e.g. ProgressDialog's Func-based execute mode), otherwise
			// later property access from this thread against COM objects created elsewhere can
			// hang indefinitely rather than throw. A simple progress window plus manual
			// Increment/DoEvents between steps keeps everything on one thread while still
			// giving the user feedback during the multi-second Outlook enumeration.
			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(3);
				progress.Show();
				Application.DoEvents();

				using (var outlook = new Outlook())
				{
					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingCategories);
					categories = outlook.GetCategories().ToList();
					progress.Increment();
					Application.DoEvents();

					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingFolders);
					folders = outlook.GetContactFolders().ToList();
					progress.Increment();
					Application.DoEvents();

					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingContacts);
					contacts = CollectContacts(folders).ToList();
					progress.Increment();
					Application.DoEvents();
				}

				progress.Close();
			}

			using var dialog = new ImportOutlookContactsDialog(folders, categories, contacts);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			var selected = dialog.SelectedContacts.ToList();
			if (selected.Count == 0)
			{
				return;
			}

			var template = dialog.Template;
			var sortBy = dialog.SortBy;

			// import...

			await using (one = new OneNote(out page, out ns))
			{
				await GenerateReport(selected, template, sortBy);
			}
		}


		private static IEnumerable<OutlookContact> CollectContacts(
			IEnumerable<OutlookFolder> folders)
		{
			foreach (var folder in folders)
			{
				foreach (var item in folder.Folder.Items)
				{
					if (item is OLK.ContactItem contact)
					{
						yield return new OutlookContact(contact);
					}
				}
			}
		}


		private async Task GenerateReport(
			IEnumerable<OutlookContact> contacts,
			ImportOutlookContactsDialog.TemplateOption template,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			PageNamespace.Set(ns);
			var editor = new PageEditor(page);

			var guid = Guid.NewGuid().ToString("b").ToUpper();
			var nowf = DateTime.Now.ToShortFriendlyString();

			var controlTable = new Table(ns, 1, 3)
			{
				HasHeaderRow = true
			};

			var personal = template == ImportOutlookContactsDialog.TemplateOption.Personal;

			controlTable.SetColumnWidth(0, 70);
			controlTable.SetColumnWidth(1, personal ? 525 : 680);
			controlTable.SetColumnWidth(2, 40);

			var row = controlTable[0];
			row.SetShading(ControlRowShading);
			row[0].SetContent(new Paragraph(string.Empty));
			row[1].SetContent(new Paragraph(
				$"{Resx.ReminderReport_LastUpdated} {nowf} " +
				$"(<a href=\"{RefreshUri}/{guid}/{template}/{sortBy}\">{Resx.word_Refresh}</a>)")
				.SetAlignment("right")
				.SetMeta(RefreshMeta, guid));
			row[2].SetContent(new Paragraph(string.Empty));

			var table = BuildDataTable(contacts, template, sortBy);

			editor.AddNextParagraph(
				new Paragraph(controlTable.Root),
				new Paragraph(table.Root).SetMeta(TableMeta, guid)
				);

			await one.Update(page);
		}


		private async Task UpdateReport(string guid,
			ImportOutlookContactsDialog.TemplateOption template,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return;
			}

			await using (one = new OneNote(out page, out ns, OneNote.PageDetail.Basic))
			{
				PageNamespace.Set(ns);

				var meta = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e =>
						e.Attribute("name").Value == TableMeta &&
						e.Attribute("content").Value == guid &&
						e.Parent.Elements(ns + "Table").Any());

				if (meta == null)
				{
					ShowInfo(Resx.ImportOutlookContactsCommand_reportNotFound);
					return;
				}

				var tableElement = meta.Parent.Elements(ns + "Table").First();

				var ids = tableElement.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == ContactMeta)
					.Select(e => e.Attribute("content").Value)
					// ref by list so they're not disposed when we discard the old table
					.ToList();

				if (ids.Count == 0)
				{
					ShowInfo(Resx.ImportOutlookContactsCommand_reportNotFound);
					return;
				}

				using var outlook = new Outlook();
				var contacts = outlook.LoadContactsByID(ids).ToList();

				var table = BuildDataTable(contacts, template, sortBy);
				tableElement.ReplaceWith(table.Root);

				// update "Last updated..." line...

				var stamp = page.Root.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value == RefreshMeta &&
						e.Attribute("content").Value == guid)
					.Select(e => e.Parent.Elements(ns + "T").FirstOrDefault())
					.FirstOrDefault();

				if (stamp != null)
				{
					stamp.GetCData().Value =
						$"{Resx.ReminderReport_LastUpdated} {DateTime.Now.ToShortFriendlyString()} " +
						$"(<a href=\"{RefreshUri}/{guid}/{template}/{sortBy}\">{Resx.word_Refresh}</a>)";
				}

				await one.Update(page);
			}
		}


		private Table BuildDataTable(
			IEnumerable<OutlookContact> contacts,
			ImportOutlookContactsDialog.TemplateOption template,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			var personal = template == ImportOutlookContactsDialog.TemplateOption.Personal;

			var table = new Table(ns, 1, personal ? 6 : 7)
			{
				HasHeaderRow = true
			};

			int i = 0;
			table.SetColumnWidth(i++, 45);
			table.SetColumnWidth(i++, 90);
			table.SetColumnWidth(i++, 85);

			if (!personal)
			{
				table.SetColumnWidth(i++, 150);
			}

			table.SetColumnWidth(i++, 150);
			table.SetColumnWidth(i++, 100);
			table.SetColumnWidth(i++, 150);

			var row = table[0];
			PopulateHeader(row, personal);

			var shaded = false;

			var list = sortBy switch
			{
				ImportOutlookContactsDialog.SortByOption.FirstName =>
					contacts.OrderBy(t => t.FirstName).ThenBy(t => t.LastName).ToList(),

				ImportOutlookContactsDialog.SortByOption.Company =>
					contacts.OrderBy(t => t.CompanyName)
					.ThenBy(t => t.LastName).ThenBy(t => t.FirstName).ToList(),

				_ => contacts.OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToList()
			};

			foreach (var contact in list)
			{
				row = table.AddRow();

				if (shaded)
				{
					row.SetShading(OddRowShading);
				}

				PopulateRow(row, contact, personal);

				shaded = !shaded;
			}

			return table;
		}


		private static void PopulateHeader(TableRow row, bool personal)
		{
			const string css = "color:#DEEBF6;font-weight:bold";

			row.SetShading(HeaderRowShading);
			var i = 0;

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty).SetStyle("font-size:8.0pt"),
				new Paragraph(ArrowGlyph).SetStyle($"font-size:16.0pt;{css}").SetAlignment("center"),
				new Paragraph(string.Empty)
				));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("First Name").SetStyle(css)));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Last Name").SetStyle(css)));

			if (!personal)
			{
				row[i++].SetContent(new ContentList(
					new Paragraph(string.Empty),
					new Paragraph("Company/Title").SetStyle(css)));
			}

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Email").SetStyle(css)));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Phone").SetStyle(css)));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Address").SetStyle(css)));
		}


		private void PopulateRow(TableRow row, OutlookContact contact, bool personal)
		{
			var i = 0;
			row[i++].SetContent(new Paragraph(ContactGlyph)
				.SetStyle("font-family:'Segoe UI Symbol';font-size:24.0pt;color:#0070C0")
				.SetAlignment("center")
				.SetMeta(ContactMeta, contact.EntryID)
				);

			row[i++].SetContent(new ContentList(
				new Paragraph(contact.FirstName ?? string.Empty).SetStyle("font-weight:bold")));

			row[i++].SetContent(
				new Paragraph(contact.LastName ?? string.Empty).SetStyle("font-weight:bold"));

			ContentList content;

			// Company/Title
			if (!personal)
			{
				content = new ContentList();
				if (!string.IsNullOrWhiteSpace(contact.CompanyName))
				{
					content.Add(new Paragraph(contact.CompanyName));
				}
				if (!string.IsNullOrWhiteSpace(contact.Title))
				{
					content.Add(new Paragraph(contact.Title).SetStyle("font-style:italic"));
				}
				if (!content.HasElements)
				{
					content.Add(new Paragraph(string.Empty));
				}

				row[i++].SetContent(content);
			}

			// Emails

			content = new ContentList();
			var e = MakeEmailLink(contact.Email1Address);
			if (e is not null)
			{
				content.Add(new Paragraph(new TextRun(e)));
			}

			e = MakeEmailLink(contact.Email2Address);
			if (e is not null)
			{
				content.Add(new Paragraph(new TextRun(e)));
			}

			if (!content.HasElements)
			{
				content.Add(new Paragraph(string.Empty));
			}

			row[i++].SetContent(content);

			// Phones

			content = new ContentList();
			if (!string.IsNullOrWhiteSpace(contact.BusinessTelephoneNumber))
			{
				content.Add(new Paragraph(contact.BusinessTelephoneNumber).SetStyle("font-weight:bold"));
			}
			if (!string.IsNullOrWhiteSpace(contact.HomeTelephoneNumber))
			{
				content.Add(new Paragraph(contact.HomeTelephoneNumber).SetStyle("font-weight:bold"));
			}
			if (!content.HasElements)
			{
				content.Add(new Paragraph(string.Empty));
			}

			row[i++].SetContent(content);

			// Address

			content = new ContentList();
			content.Add(new Paragraph(new TextRun(MakeBestAddress(contact, personal))));

			row[i++].SetContent(content);
		}


		private string MakeEmailLink(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return null;
			}

			var match = emailPattern.Match(email);
			if (match.Success)
			{
				email = match.Groups[1].Value;
				return $"<a href=\"{email}\">{email}</a>";
			}

			return null;
		}


		private static string MakeBestAddress(OutlookContact contact, bool personal)
		{
			string BR = "<br>" + Environment.NewLine;

			var address = string.Empty;

			if (!string.IsNullOrWhiteSpace(contact.HomeAddressStreet) ||
				!string.IsNullOrWhiteSpace(contact.HomeAddressCity) ||
				!string.IsNullOrWhiteSpace(contact.HomeAddressState) ||
				!string.IsNullOrWhiteSpace(contact.HomeAddressPostalCode) ||
				!string.IsNullOrWhiteSpace(contact.HomeAddressCountry))
			{
				if (!string.IsNullOrWhiteSpace(contact.HomeAddressCity))
				{
					address += contact.HomeAddressCity + ", ";
				}
				if (!string.IsNullOrWhiteSpace(contact.HomeAddressState))
				{
					address += contact.HomeAddressState + " ";
				}
				if (!string.IsNullOrWhiteSpace(contact.HomeAddressPostalCode))
				{
					address += contact.HomeAddressPostalCode + BR;
				}
				if (!string.IsNullOrWhiteSpace(contact.HomeAddressCountry))
				{
					address += contact.HomeAddressCountry;
				}
				if (!string.IsNullOrWhiteSpace(contact.HomeAddressStreet))
				{
					address = contact.HomeAddressStreet + BR + address;
				}
			}

			if (string.IsNullOrWhiteSpace(address) && (
				!string.IsNullOrWhiteSpace(contact.BusinessAddressStreet) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressCity) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressState) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressPostalCode) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressCountry)))
			{
				if (!string.IsNullOrWhiteSpace(contact.BusinessAddressCity))
				{
					address += contact.BusinessAddressCity + ", ";
				}
				if (!string.IsNullOrWhiteSpace(contact.BusinessAddressState))
				{
					address += contact.BusinessAddressState + " ";
				}
				if (!string.IsNullOrWhiteSpace(contact.BusinessAddressPostalCode))
				{
					address += contact.BusinessAddressPostalCode + BR;
				}
				if (!string.IsNullOrWhiteSpace(contact.BusinessAddressCountry))
				{
					address += contact.BusinessAddressCountry;
				}
				if (!string.IsNullOrWhiteSpace(contact.BusinessAddressStreet))
				{
					address = contact.BusinessAddressStreet + BR + address;
				}
			}

			return address;
		}
	}
}
