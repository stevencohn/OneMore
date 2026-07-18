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


		public void GenerateReport(
			Page page,
			IEnumerable<OutlookContact> contacts,
			ContactTemplateOption template,
			ContactSortByOption sortBy)
		{
			this.page = page;
			ns = page.Namespace;
			PageNamespace.Set(ns);

			var guid = Guid.NewGuid().ToString("b").ToUpper();
			var nowf = DateTime.Now.ToShortFriendlyString();

			var personal = template == ContactTemplateOption.Personal;

			var content = new Paragraph(
				$"{Resx.ReminderReport_LastUpdated} {nowf} | " +
				$"<a href=\"{RefreshUri}/{guid}/{template}/{sortBy}\">{Resx.word_Refresh}</a>")
				.SetAlignment("right")
				.SetMeta(RefreshMeta, guid);

			var controlTable = ReportControlHelper.BuildControlTable(
				ns, ControlRowShading, 70, personal ? 543 : 700, 40, content);

			var table = BuildDataTable(contacts, template, sortBy);

			// this is a brand-new page that has never been displayed, so it has no cursor/
			// selection state for PageEditor.AddNextParagraph to anchor on; append directly
			// to the page's content container instead
			var container = page.EnsureContentContainer();
			container.Add(
				new Paragraph(controlTable.Root),
				new Paragraph(table.Root).SetMeta(TableMeta, guid)
				);
		}


		public async Task UpdateReport(string guid,
			ContactTemplateOption template,
			ContactSortByOption sortBy)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return;
			}

			await using var one = new OneNote(out page, out ns, OneNote.PageDetail.Basic);

			PageNamespace.Set(ns);

			var tableElement = ReportControlHelper.FindReportTable(page, ns, TableMeta, guid);

			if (tableElement == null)
			{
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
				return;
			}

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

			try
			{
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
						$"{Resx.ReminderReport_LastUpdated} {DateTime.Now.ToShortFriendlyString()} | " +
						$"<a href=\"{RefreshUri}/{guid}/{template}/{sortBy}\">{Resx.word_Refresh}</a>";
				}

				await one.Update(page);
			}
			finally
			{
				foreach (var contact in contacts)
				{
					contact.Dispose();
				}
			}
		}


		internal static List<OutlookContact> SortContacts(
			IEnumerable<OutlookContact> contacts,
			ContactSortByOption sortBy)
		{
			return sortBy switch
			{
				ContactSortByOption.FirstName =>
					contacts.OrderBy(t => t.FirstName).ThenBy(t => t.LastName).ToList(),

				ContactSortByOption.Company =>
					contacts.OrderBy(t => t.CompanyName)
					.ThenBy(t => t.LastName).ThenBy(t => t.FirstName).ToList(),

				_ => contacts.OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToList()
			};
		}


		private Table BuildDataTable(
			IEnumerable<OutlookContact> contacts,
			ContactTemplateOption template,
			ContactSortByOption sortBy)
		{
			var personal = template == ContactTemplateOption.Personal;

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
				new Paragraph(Resx.ImportOutlookContactsCommand_firstName).SetStyle(css)));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph(Resx.ImportOutlookContactsCommand_lastName).SetStyle(css)));

			if (!personal)
			{
				row[i++].SetContent(new ContentList(
					new Paragraph(string.Empty),
					new Paragraph(Resx.ImportOutlookContactsCommand_companyTitle).SetStyle(css)));
			}

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph(Resx.ImportOutlookContactsCommand_email).SetStyle(css)));

			row[i++].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph(Resx.ImportOutlookContactsCommand_phone).SetStyle(css)));

			row[i].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph(Resx.ImportOutlookContactsCommand_address).SetStyle(css)));
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
				if (!string.IsNullOrWhiteSpace(contact.JobTitle))
				{
					content.Add(new Paragraph(contact.JobTitle).SetStyle("font-style:italic"));
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
			var address = BuildAddress(
				contact.HomeAddressStreet, contact.HomeAddressCity,
				contact.HomeAddressState, contact.HomeAddressPostalCode, contact.HomeAddressCountry);

			return string.IsNullOrWhiteSpace(address)
				? BuildAddress(
					contact.BusinessAddressStreet, contact.BusinessAddressCity,
					contact.BusinessAddressState, contact.BusinessAddressPostalCode, contact.BusinessAddressCountry)
				: address;
		}


		private static string BuildAddress(
			string street, string city, string state, string postalCode, string country)
		{
			if (string.IsNullOrWhiteSpace(street) &&
				string.IsNullOrWhiteSpace(city) &&
				string.IsNullOrWhiteSpace(state) &&
				string.IsNullOrWhiteSpace(postalCode) &&
				string.IsNullOrWhiteSpace(country))
			{
				return string.Empty;
			}

			string BR = "<br>" + Environment.NewLine;

			var address = string.Empty;

			if (!string.IsNullOrWhiteSpace(city))
			{
				address += city + ", ";
			}
			if (!string.IsNullOrWhiteSpace(state))
			{
				address += state + " ";
			}
			if (!string.IsNullOrWhiteSpace(postalCode))
			{
				address += postalCode + BR;
			}
			if (!string.IsNullOrWhiteSpace(country))
			{
				address += country;
			}
			if (!string.IsNullOrWhiteSpace(street))
			{
				address = street + BR + address;
			}

			return address;
		}
	}
}
