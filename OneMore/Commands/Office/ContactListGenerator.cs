//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URI

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using Resx = Properties.Resources;


	/// <summary>
	/// Builds or refreshes the Outlook contacts report table on a OneNote page
	/// </summary>
	internal class ContactListGenerator
	{
		private const string ContactMeta = "omOutlookContact";
		private const string RefreshMeta = "omOutlookContactsRefresh";
		private const string TableMeta = "omOutlookContacts";

		private const string ControlRowShading = "#C5E0B3";
		private const string HeaderRowShading = "#1E4E79";
		private const string OddRowShading = "#F2F2F2";

		// Segoe UI Symbols...
		private const string ArrowGlyph = "\u2197"; // ↗ North East Arrow
		private const string ContactGlyph = "\ue2af"; //  Contact
		private const string BusinessTelephoneGlyph = "\uD83E\uDDD1\u200D\uD83D\uDCBC"; // 🏢 Business
		private const string HomeTelephoneGlyph = "\ud83c\udfe0"; // 🏠 Home
		private const string MobileTelephoneGlyph = "\ud83d\udcf1"; // 📱 Mobile

		private const string RefreshUri = "onemore://ImportOutlookContactsCommand/refresh";

		private readonly Regex emailPattern;

		private Page page;
		private XNamespace ns;


		public ContactListGenerator()
		{
			emailPattern = new Regex(
				@"([A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,})", RegexOptions.Compiled);
		}


		public async Task GenerateReport(
			IEnumerable<OutlookContact> contacts,
			ImportOutlookContactsDialog.TemplateOption template,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			await using var one = new OneNote(out page, out ns);

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
			controlTable.SetColumnWidth(1, personal ? 543 : 700);
			controlTable.SetColumnWidth(2, 40);

			var row = controlTable[0];
			row.SetShading(ControlRowShading);
			row[0].SetContent(new Paragraph(string.Empty));
			row[1].SetContent(new Paragraph(
				$"{Resx.ReminderReport_LastUpdated} {nowf} | " +
				$"<a href=\"{RefreshUri}/{guid}/{template}/{sortBy}\">{Resx.word_Refresh}</a>")
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


		public async Task UpdateReport(string guid,
			ImportOutlookContactsDialog.TemplateOption template,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return;
			}

			await using var one = new OneNote(out page, out ns, OneNote.PageDetail.Basic);

			PageNamespace.Set(ns);

			var meta = page.Root.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name").Value == TableMeta &&
					e.Attribute("content").Value == guid &&
					e.Parent.Elements(ns + "Table").Any());

			if (meta == null)
			{
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
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
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
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


		internal static List<OutlookContact> SortContacts(
			IEnumerable<OutlookContact> contacts,
			ImportOutlookContactsDialog.SortByOption sortBy)
		{
			return sortBy switch
			{
				ImportOutlookContactsDialog.SortByOption.FirstName =>
					contacts.OrderBy(t => t.FirstName).ThenBy(t => t.LastName).ToList(),

				ImportOutlookContactsDialog.SortByOption.Company =>
					contacts.OrderBy(t => t.CompanyName)
					.ThenBy(t => t.LastName).ThenBy(t => t.FirstName).ToList(),

				_ => contacts.OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToList()
			};
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
			table.SetColumnWidth(i++, 120);
			table.SetColumnWidth(i, 150);

			var row = table[0];
			PopulateHeader(row, personal);

			var shaded = false;

			var list = SortContacts(contacts, sortBy);

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

			row[i].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Address").SetStyle(css)));
		}


		private void PopulateRow(TableRow row, OutlookContact contact, bool personal)
		{
			var i = 0;
			var glyph = string.IsNullOrEmpty(contact.PageUri)
				? ContactGlyph
				: $"<a href=\"{contact.PageUri}\">{ContactGlyph}</a>";

			row[i++].SetContent(new Paragraph(glyph)
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

			e = MakeEmailLink(contact.Email3Address);
			if (e is not null)
			{
				content.Add(new Paragraph(new TextRun(e)));
			}

			if (content.HasElements)
			{
				row[i++].SetContent(content);
			}
			else
			{
				row[i++].SetContent(string.Empty);
			}

			// Phones

			content = new ContentList();
			if (!string.IsNullOrWhiteSpace(contact.HomeTelephoneNumber))
			{
				content.Add(new Paragraph(
					new TextRun(HomeTelephoneGlyph),
					new TextRun(contact.HomeTelephoneNumber).SetBold()));
			}
			if (!string.IsNullOrWhiteSpace(contact.BusinessTelephoneNumber))
			{
				content.Add(new Paragraph(
					new TextRun(BusinessTelephoneGlyph),
					new TextRun(contact.BusinessTelephoneNumber).SetBold()));
			}
			if (!string.IsNullOrWhiteSpace(contact.MobileTelephoneNumber))
			{
				content.Add(new Paragraph(
					new TextRun(MobileTelephoneGlyph),
					new TextRun(contact.MobileTelephoneNumber).SetBold()));
			}
			if (!content.HasElements)
			{
				content.Add(new Paragraph(string.Empty));
			}

			row[i++].SetContent(content);

			// Address

			content = new ContentList();
			content.Add(new Paragraph(new TextRun(MakeBestAddress(contact))));

			row[i].SetContent(content);
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
				return $"<a href=\"mailto:{email}\">{email}</a>";
			}

			return null;
		}


		private static string MakeBestAddress(OutlookContact contact)
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
