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
		private const string ControlRowShading = "#C5E0B3";
		private const string OddRowShading = "#F2F2F2";
		private const string HeaderRowShading = "#1E4E79";
		private const string ContactGlyph = "\ue2af"; // "Contact" glyph in Segoe UI Symbol
		private const string ArrowGlyph = "\u2197"; // "NE Arrow" glyph in Segoe UI Symbol

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
				//await UpdateReport(args[1] as string);
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

			// import...

			await using (one = new OneNote(out page, out ns))
			{
				await GenerateReport(selected);
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


		private async Task GenerateReport(IEnumerable<OutlookContact> contacts)
		{
			PageNamespace.Set(ns);
			var editor = new PageEditor(page);

			var controlTable = new Table(ns, 1, 3)
			{
				HasHeaderRow = true
			};

			controlTable.SetColumnWidth(0, 70);
			controlTable.SetColumnWidth(1, 680);
			controlTable.SetColumnWidth(2, 40);

			var row = controlTable[0];
			row.SetShading(ControlRowShading);
			row[0].SetContent(new Paragraph(string.Empty));
			row[1].SetContent(new Paragraph(Resx.word_Refresh).SetAlignment("right"));
			row[2].SetContent(new Paragraph(string.Empty));

			var table = new Table(ns, 1, 7)
			{
				HasHeaderRow = true
			};

			table.SetColumnWidth(0, 45);
			table.SetColumnWidth(1, 90);
			table.SetColumnWidth(2, 85);
			table.SetColumnWidth(3, 150);
			table.SetColumnWidth(4, 150);
			table.SetColumnWidth(5, 100);
			table.SetColumnWidth(6, 150);

			row = table[0];
			PopulateHeader(row);

			var shaded = false;

			foreach (var contact in contacts
				.OrderBy(t => t.FirstName)
				.ThenBy(t => t.LastName))
			{
				row = table.AddRow();

				if (shaded)
				{
					row.SetShading(OddRowShading);
				}

				PopulateRow(row, contact);

				shaded = !shaded;
			}

			editor.AddNextParagraph(
				new Paragraph(controlTable.Root),
				new Paragraph(table.Root)
				);

			await one.Update(page);
		}


		private static void PopulateHeader(TableRow row)
		{
			const string css = "color:#DEEBF6;font-weight:bold";

			row.SetShading(HeaderRowShading);

			row[0].SetContent(new ContentList(
				new Paragraph(string.Empty).SetStyle("font-size:8.0pt"),
				new Paragraph(ArrowGlyph).SetStyle($"font-size:16.0pt;{css}").SetAlignment("center"),
				new Paragraph(string.Empty)
				));

			row[1].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("First Name").SetStyle(css)));

			row[2].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Last Name").SetStyle(css)));

			row[3].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Company/Title").SetStyle(css)));

			row[4].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Email").SetStyle(css)));

			row[5].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Phone").SetStyle(css)));

			row[6].SetContent(new ContentList(
				new Paragraph(string.Empty),
				new Paragraph("Address").SetStyle(css)));
		}


		private void PopulateRow(TableRow row, OutlookContact contact)
		{
			row[0].SetContent(new Paragraph(ContactGlyph)
				.SetStyle("font-family:'Segoe UI Symbol';font-size:24.0pt;color:#0070C0")
				.SetAlignment("center")
				);

			row[1].SetContent(new ContentList(
				new Paragraph(contact.FirstName ?? string.Empty).SetStyle("font-weight:bold")));

			row[2].SetContent(
				new Paragraph(contact.LastName ?? string.Empty).SetStyle("font-weight:bold"));

			// Company/Title

			var content = new ContentList();
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

			row[3].SetContent(content);

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

			row[4].SetContent(content);

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

			row[5].SetContent(content);

			// Address

			content = new ContentList();
			content.Add(new Paragraph(new TextRun(MakeBestAddress(contact))));

			row[6].SetContent(content);
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


		private static string MakeBestAddress(OutlookContact contact)
		{
			string BR = "<br>" + Environment.NewLine;

			var address = string.Empty;

			if (!string.IsNullOrWhiteSpace(contact.BusinessAddressStreet) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressCity) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressState) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressPostalCode) ||
				!string.IsNullOrWhiteSpace(contact.BusinessAddressCountry))
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
			else if (!string.IsNullOrWhiteSpace(contact.HomeAddressStreet) ||
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

			return address;
		}
	}
}
