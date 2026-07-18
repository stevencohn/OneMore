//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************ 

#pragma warning disable S1075 // URI

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.Globalization;
	using System.Linq;
	using System.Security;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class ContactGenerator
	{
		private const string RefreshMeta = "omOutlookContactRefresh";
		private const string TableMeta = "omOutlookContact";
		private const string FieldMeta = "omField";
		private const string TemplateMeta = "omTemplate";

		private const string ControlRowShading = "#C5E0B3";
		private const string DeetsHeaderShading = "#9CC3E5";
		private const string BackgroundShading = "#1E4E79";
		private const string LabelShading = "#BDD7EE";
		private const string ValueShading = "#DEEBF6";
		private const string GrayShading = "#E7E6E6";

		private const string CategoryBar = "\u258C"; // ▌ Left Half Block

		private const string SpacerFontSize = "font-size:8.0pt";

		private const string RefreshUri = "onemore://OutlookContactCommand/refresh";
		private const string SaveUri = "onemore://OutlookContactCommand/save";

		// Outlook's sentinel for an unset Birthday/Anniversary; matches the Year >= 4500
		// threshold already used by BuildMiscTable to detect "no date"
		private static readonly DateTime NoDateValue = new(4501, 1, 1);

		// intentionally permissive so valid addresses from any locale/TLD aren't rejected
		private static readonly Regex EmailPattern =
			new(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled);

		// locale-agnostic: digits plus common separators/punctuation and an optional
		// leading +; requires at least 7 digits so short garbage strings are rejected
		private static readonly Regex PhonePattern =
			new(@"^\+?[0-9()\-.\s]{7,}$", RegexOptions.Compiled);

		private XNamespace ns;

		public ContactGenerator()
		{
		}


		public void GenerateReport(
			Page page,
			OutlookContact contact,
			ContactTemplateOption template,
			List<OutlookCategory> categories)
		{
			ns = page.Namespace;
			PageNamespace.Set(ns);

			var guid = Guid.NewGuid().ToString("b").ToUpper();

			var controlTable = BuildControlTable(contact, template, guid);
			var table = BuildDetailsTable(contact, template, categories);

			// this is a brand-new page that has never been displayed, so it has no cursor/
			// selection state for PageEditor.AddNextParagraph to anchor on; append directly
			// to the page's content container instead
			var container = page.EnsureContentContainer();
			container.Add(
				new Paragraph(controlTable.Root),
				new Paragraph(table.Root).SetMeta(TableMeta, guid)
				);
		}


		/// <summary>
		/// Reloads the given contact from Outlook and refreshes the details table and
		/// control line of a previously generated contact page in place
		/// </summary>
		/// <param name="guid">The unique ID of the report to refresh</param>
		/// <param name="template">The template style to apply</param>
		public async Task UpdateReport(
			string guid, ContactTemplateOption template)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return;
			}

			await using var one = new OneNote(out var page, out ns, OneNote.PageDetail.Basic);
			PageNamespace.Set(ns);

			var tableElement = ReportControlHelper.FindReportTable(page, ns, TableMeta, guid);

			var stampMeta = page.Root.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name").Value == RefreshMeta &&
					e.Attribute("content").Value == guid);

			var entryID = stampMeta?.Parent.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == "EntryID")
				?.Attribute("content").Value;

			if (tableElement == null || string.IsNullOrEmpty(entryID))
			{
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
				return;
			}

			List<OutlookContact> contacts;
			List<OutlookCategory> categories;

			using (var outlook = new Outlook())
			{
				contacts = outlook.LoadContactsByID(new[] { entryID }).ToList();
				categories = outlook.GetCategories().ToList();
			}

			try
			{
				var contact = contacts.FirstOrDefault();
				if (contact == null)
				{
					UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
					return;
				}

				var table = BuildDetailsTable(contact, template, categories);
				tableElement.ReplaceWith(table.Root);

				var stamp = stampMeta.Parent.Elements(ns + "T").FirstOrDefault();
				if (stamp != null)
				{
					stamp.GetCData().Value = BuildControlLine(contact, template, guid);
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


		/// <summary>
		/// Reads the (possibly edited) values out of a previously generated contact page's
		/// details table, applies them to the live Outlook contact, saves it, and refreshes
		/// the page in place to reflect exactly what was saved.
		/// </summary>
		/// <param name="guid">The unique ID of the report to save</param>
		/// <param name="template">The template style to apply</param>
		public async Task SaveReport(string guid, ContactTemplateOption template)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return;
			}

			await using var one = new OneNote(out var page, out ns, OneNote.PageDetail.Basic);
			PageNamespace.Set(ns);

			var tableElement = ReportControlHelper.FindReportTable(page, ns, TableMeta, guid);

			var stampMeta = page.Root.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name").Value == RefreshMeta &&
					e.Attribute("content").Value == guid);

			var entryID = stampMeta?.Parent.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == "EntryID")
				?.Attribute("content").Value;

			if (tableElement == null || string.IsNullOrEmpty(entryID))
			{
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
				return;
			}

			using var outlook = new Outlook();
			var contact = outlook.LoadContact(entryID);

			if (contact == null)
			{
				UI.MoreMessageBox.Show(one.OwnerWindow, Resx.ImportOutlookContactsCommand_reportNotFound);
				return;
			}

			try
			{
				var warnings = ApplyFields(tableElement, contact);

				contact.Contact.Save();

				var categories = outlook.GetCategories().ToList();
				var refreshed = BuildDetailsTable(contact, template, categories);
				tableElement.ReplaceWith(refreshed.Root);

				var stamp = stampMeta.Parent.Elements(ns + "T").FirstOrDefault();
				if (stamp != null)
				{
					stamp.GetCData().Value = BuildControlLine(contact, template, guid);
				}

				await one.Update(page);

				if (warnings.Count > 0)
				{
					UI.MoreMessageBox.Show(one.OwnerWindow, string.Format(
						Resx.OutlookContactCommand_saveWarnings, string.Join(", ", warnings)));
				}
			}
			finally
			{
				contact.Dispose();
			}
		}


		/// <summary>
		/// Walks every sub-table of a contact details table, reading each field-tagged
		/// (omField) row and applying its current value to the given contact. Fields that
		/// fail validation are left unchanged and their labels are returned so the caller
		/// can warn the user.
		/// </summary>
		private List<string> ApplyFields(XElement detailsRoot, OutlookContact contact)
		{
			var warnings = new List<string>();

			foreach (var subTableElement in detailsRoot.Descendants(ns + "Table"))
			{
				var table = new Table(subTableElement);
				var templateMeta = table.Rows.FirstOrDefault()?.Cells.FirstOrDefault()?.GetMeta(TemplateMeta);

				foreach (var row in table.Rows)
				{
					var cells = row.Cells.ToList();
					if (cells.Count < 2)
					{
						continue;
					}

					var field = cells[0].GetMeta(FieldMeta);
					if (string.IsNullOrEmpty(field))
					{
						// header row or otherwise not a field row
						continue;
					}

					if (templateMeta != null)
					{
						ApplyDeetsField(field, templateMeta, cells, contact, warnings);
					}
					else
					{
						ApplyDirectField(field, cells[1].GetText(), contact, warnings);
					}
				}
			}

			ApplyCategoriesAndNotes(detailsRoot, contact, warnings);

			return warnings;
		}


		/// <summary>
		/// Applies a row from the "Deets" table, whose field names are generic (no
		/// Home/Business prefix); the omTemplate meta on that table's header cell says
		/// which value column(s) map to which Home/Business contact property.
		/// </summary>
		private static void ApplyDeetsField(
			string field, string templateMeta, List<TableCell> cells,
			OutlookContact contact, List<string> warnings)
		{
			// Outlook has only one Mobile number (no Home/Business split); in the "Both"
			// template its second column is a static "N/A" placeholder, not a live field
			if (field == "MobileTelephoneNumber")
			{
				ApplyDirectField(field, cells[1].GetText(), contact, warnings);
				return;
			}

			if (!Enum.TryParse<ContactTemplateOption>(templateMeta, out var template))
			{
				return;
			}

			var both = template == ContactTemplateOption.Both;

			if (template == ContactTemplateOption.Personal || both)
			{
				ApplyDirectField($"Home{field}", cells[1].GetText(), contact, warnings);
			}

			if (template == ContactTemplateOption.Business || both)
			{
				var c = both ? 2 : 1;
				ApplyDirectField($"Business{field}", cells[c].GetText(), contact, warnings);
			}
		}


		/// <summary>
		/// Applies a single value, identified by its fully qualified OutlookContact
		/// property name, to the contact - validating phone numbers, emails, and dates
		/// where reasonably possible.
		/// </summary>
		private static void ApplyDirectField(
			string field, string text, OutlookContact contact, List<string> warnings)
		{
			var value = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();

			switch (field)
			{
				case "FirstName": contact.FirstName = value; break;
				case "MiddleName": contact.MiddleName = value; break;
				case "LastName": contact.LastName = value; break;
				case "JobTitle": contact.JobTitle = value; break;
				case "CompanyName": contact.CompanyName = value; break;
				case "Department": contact.Department = value; break;
				case "CustomerID": contact.CustomerID = value; break;

				case "Email1Address": SetIfValidEmail(v => contact.Email1Address = v, value, Resx.OutlookContactCommand_email1, warnings); break;
				case "Email2Address": SetIfValidEmail(v => contact.Email2Address = v, value, Resx.OutlookContactCommand_email2, warnings); break;
				case "Email3Address": SetIfValidEmail(v => contact.Email3Address = v, value, Resx.OutlookContactCommand_email3, warnings); break;

				case "HomeTelephoneNumber": SetIfValidPhone(v => contact.HomeTelephoneNumber = v, value, Resx.OutlookContactCommand_telephone, warnings); break;
				case "BusinessTelephoneNumber": SetIfValidPhone(v => contact.BusinessTelephoneNumber = v, value, Resx.OutlookContactCommand_telephone, warnings); break;
				case "MobileTelephoneNumber": SetIfValidPhone(v => contact.MobileTelephoneNumber = v, value, Resx.OutlookContactCommand_mobile, warnings); break;

				case "HomeAddressStreet": contact.HomeAddressStreet = value; break;
				case "HomeAddressCity": contact.HomeAddressCity = value; break;
				case "HomeAddressState": contact.HomeAddressState = value; break;
				case "HomeAddressPostalCode": contact.HomeAddressPostalCode = value; break;
				case "HomeAddressCountry": contact.HomeAddressCountry = value; break;

				case "BusinessAddressStreet": contact.BusinessAddressStreet = value; break;
				case "BusinessAddressCity": contact.BusinessAddressCity = value; break;
				case "BusinessAddressState": contact.BusinessAddressState = value; break;
				case "BusinessAddressPostalCode": contact.BusinessAddressPostalCode = value; break;
				case "BusinessAddressCountry": contact.BusinessAddressCountry = value; break;

				case "Birthday": SetIfValidDate(v => contact.Birthday = v, value, contact.Birthday, Resx.OutlookContactCommand_birthday, warnings); break;
				case "Anniversary": SetIfValidDate(v => contact.Anniversary = v, value, contact.Anniversary, Resx.OutlookContactCommand_anniversary, warnings); break;
			}
		}


		private static void SetIfValidEmail(
			Action<string> setter, string value, string label, List<string> warnings)
		{
			if (value.Length == 0 || EmailPattern.IsMatch(value))
			{
				setter(value);
			}
			else
			{
				warnings.Add(label);
			}
		}


		private static void SetIfValidPhone(
			Action<string> setter, string value, string label, List<string> warnings)
		{
			if (value.Length == 0 || PhonePattern.IsMatch(value))
			{
				setter(value);
			}
			else
			{
				warnings.Add(label);
			}
		}


		/// <summary>
		/// Accepts any date string the current culture can parse, even though BuildMiscTable
		/// only ever displays "MMMM d". If the text includes an explicit year, that year is
		/// stored; otherwise the contact's existing year is kept (or the current year, if the
		/// date was previously unset) and only the month/day are overwritten.
		/// </summary>
		private static void SetIfValidDate(
			Action<DateTime> setter, string value, DateTime existing, string label, List<string> warnings)
		{
			if (value.Length == 0)
			{
				setter(NoDateValue);
				return;
			}

			if (!TryParseDate(value, out var parsed, out var hasYear))
			{
				warnings.Add(label);
				return;
			}

			if (hasYear)
			{
				setter(parsed);
				return;
			}

			var year = existing.Year < 4500 ? existing.Year : DateTime.Now.Year;
			setter(new DateTime(year, parsed.Month, parsed.Day));
		}


		/// <summary>
		/// Parses any date string recognized by the current culture, additionally reporting
		/// whether the text explicitly included a year. Detected by comparing a normal parse
		/// (which defaults a missing year to the current year) against a parse using
		/// NoCurrentDateDefault (which defaults a missing year to year 1 instead) - if the
		/// two disagree on year, the input didn't actually specify one.
		/// </summary>
		private static bool TryParseDate(string text, out DateTime date, out bool hasYear)
		{
			hasYear = false;

			if (!DateTime.TryParse(
				text, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
			{
				return false;
			}

			if (DateTime.TryParse(
				text, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out var probe))
			{
				hasYear = probe.Year != 1;
			}

			return true;
		}


		/// <summary>
		/// The Categories and Notes rows live directly on the outer details table rather
		/// than in one of its nested sub-tables, so they're handled separately from
		/// ApplyFields' sub-table walk.
		/// </summary>
		private void ApplyCategoriesAndNotes(
			XElement detailsRoot, OutlookContact contact, List<string> warnings)
		{
			var table = new Table(detailsRoot);
			var rows = table.Rows.ToList();
			if (rows.Count < 3)
			{
				return;
			}

			//// USER IS CURRENTLY NOT ALLOWED TO CHANGE CATEGORIES
			//
			//var categoriesRow = rows[1].Cells.ToList();
			//if (categoriesRow[0].GetMeta(FieldMeta) == "Categories")
			//{
			//	var text = categoriesRow[1].GetText() ?? string.Empty;
			//	contact.Categories = string.Join(";",
			//		text.Split(CategoryBar[0])
			//			.Select(s => s.Trim())
			//			.Where(s => s.Length > 0));
			//}

			var notesRow = rows[2].Cells.ToList();
			if (notesRow[0].GetMeta(FieldMeta) == "Body")
			{
				// one line per OE; the last OE is always the trailing spacer paragraph
				// written by BuildNotesRow, not content, so drop it before joining
				var lines = notesRow[1].GetTextLines().ToList();
				if (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
				{
					lines.RemoveAt(lines.Count - 1);
				}

				contact.Body = string.Join(Environment.NewLine, lines);
			}
		}


		private Table BuildControlTable(
			OutlookContact contact,
			ContactTemplateOption template,
			string guid)
		{
			var content = new Paragraph(BuildControlLine(contact, template, guid))
				.SetAlignment("right")
				.SetMeta("EntryID", contact.EntryID)
				.SetMeta(RefreshMeta, guid);

			return ReportControlHelper.BuildControlTable(ns, ControlRowShading, 70, 605, 40, content);
		}


		private static string BuildControlLine(
			OutlookContact contact,
			ContactTemplateOption template,
			string guid)
		{
			var nowf = DateTime.Now.ToShortFriendlyString();
			var updated = $"{Resx.ReminderReport_LastUpdated} {nowf}";
			// seems like folder should always be Contacts regardless of contact's actual .Folder
			var openin = $"<a href=\"onenote:outlook?folder=Contacts&amp;entryid={contact.EntryID}\">{Resx.OutlookContactCommand_openInOutlook}</a>";
			var refresh = $"<a href=\"{RefreshUri}/{guid}/{template}\">{Resx.word_Refresh}</a>";
			var save = $"<a href=\"{SaveUri}/{guid}/{template}\">{Resx.word_Save}</a>";

			return $"{updated} | {openin} | {refresh} | {save}";
		}


		private Table BuildDetailsTable(
			OutlookContact contact,
			ContactTemplateOption template,
			List<OutlookCategory> categories)
		{
			var table = new Table(ns, 3, 2)
			{
				HasHeaderRow = true
			};

			table.SetColumnWidth(0, 70);
			table.SetColumnWidth(1, 650);
			table[0].SetShading(BackgroundShading);

			// picture

			SetPicture(table[0][0], contact);

			// details

			var nameTable = BuildNameTable(
				contact, template == ContactTemplateOption.Personal);

			var deetTable = BuildDeetsTable(contact, template);
			var mailTable = BuildEmailTable(contact);
			var miscTable = BuildMiscTable(contact);

			table[0][1].SetContent(
				new ContentList(
					new Paragraph(string.Empty).SetStyle(SpacerFontSize), new Paragraph(nameTable.Root),
					new Paragraph(string.Empty).SetStyle(SpacerFontSize), new Paragraph(mailTable.Root),
					new Paragraph(string.Empty).SetStyle(SpacerFontSize), new Paragraph(deetTable.Root),
					new Paragraph(string.Empty).SetStyle(SpacerFontSize), new Paragraph(miscTable.Root),
					new Paragraph(string.Empty).SetStyle(SpacerFontSize)
					)
				);

			BuildCategoriesRow(table, contact, categories);
			BuildNotesRow(table, contact);

			return table;
		}


		private void SetPicture(TableCell cell, OutlookContact contact)
		{
			const int diameter = 60;
			const string ContactGlyph = "\ue2af"; //  Contact

			using var picture = contact.Picture;
			if (picture is not null)
			{
				// resize
				using var resized = new Bitmap(diameter, diameter, PixelFormat.Format32bppArgb);
				using (var g = Graphics.FromImage(resized))
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawImage(picture, new Rectangle(0, 0, diameter, diameter));
				}

				// create thumbnail with transparency
				using var output = new Bitmap(diameter, diameter, PixelFormat.Format32bppArgb);
				using (var g = Graphics.FromImage(output))
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;

					// centered circular mask
					var circle = new GraphicsPath();
					circle.AddEllipse(0, 0, diameter, diameter);

					// clip to circle
					g.SetClip(circle);

					// draw resized image inside the circular clip
					g.DrawImage(resized, new Rectangle(0, 0, diameter, diameter));
				}

				// output
				var image = new OneImage(output, ns);

				cell.SetContent(new ContentList(
					new Paragraph(string.Empty),
					new Paragraph(image).SetAlignment("center")
					));
			}
			else
			{
				cell.SetContent(new ContentList(
					new Paragraph(string.Empty),
					new Paragraph(ContactGlyph)
						.SetStyle("font-family:'Segoe UI Symbol';font-size:48.0pt;color:#DEEBF6")
						.SetAlignment("center")
					));
			}
		}


		private static void WriteField(
			TableRow row, string label, string value, string name, int valCol = 1)
		{
			WriteField(row, label, new Paragraph(value ?? string.Empty), name, valCol);
		}

		private static void WriteField(
			TableRow row, string label, Paragraph content, string name, int valCol = 1)
		{
			row[0]
				.SetShading(LabelShading)
				.SetContent(new Paragraph(label).SetItalic().SetMeta(FieldMeta, name));

			row[valCol]
				.SetShading(ValueShading)
				.SetContent(content);
		}

		private static void SetLabel(TableCell cell, string label, string name)
		{
			cell.SetContent(new Paragraph(label).SetItalic().SetMeta(FieldMeta, name));
		}

		private static void WriteValue(TableCell cell, string value)
		{
			cell.SetShading(ValueShading)
				.SetContent(new Paragraph(value ?? string.Empty));
		}


		private Table BuildNameTable(OutlookContact contact, bool personal)
		{
			var table = new Table(ns, personal ? 3 : 7, 2)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 120);
			table.SetColumnWidth(1, 505);

			WriteField(table[0], Resx.OutlookContactCommand_first, new Paragraph(contact.FirstName ?? string.Empty).SetBold(), "FirstName");
			WriteField(table[1], Resx.OutlookContactCommand_middle, new Paragraph(contact.MiddleName ?? string.Empty).SetBold(), "MiddleName");
			WriteField(table[2], Resx.OutlookContactCommand_last, new Paragraph(contact.LastName ?? string.Empty).SetBold(), "LastName");

			if (!personal)
			{
				WriteField(table[3], Resx.OutlookContactCommand_jobTitle, contact.JobTitle, "JobTitle");
				WriteField(table[4], Resx.OutlookContactCommand_company, contact.CompanyName, "CompanyName");
				WriteField(table[5], Resx.OutlookContactCommand_department, contact.Department, "Department");
				WriteField(table[6], Resx.OutlookContactCommand_customerID, contact.CustomerID, "CustomerID");
			}

			return table;
		}


		private Table BuildEmailTable(OutlookContact contact)
		{
			var table = new Table(ns, 3, 2)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 120);
			table.SetColumnWidth(1, 505);

			FillValue(contact.Email1Address, 0, Resx.OutlookContactCommand_email1, "Email1Address");
			FillValue(contact.Email2Address, 1, Resx.OutlookContactCommand_email2, "Email2Address");
			FillValue(contact.Email3Address, 2, Resx.OutlookContactCommand_email3, "Email3Address");

			return table;

			void FillValue(string email, int r, string label, string name)
			{
				table[r][0]
					.SetShading(LabelShading)
					.SetContent(new Paragraph(label).SetItalic().SetMeta(FieldMeta, name));

				table[r][1].SetShading(ValueShading);

				var content = string.IsNullOrWhiteSpace(email)
					? new Paragraph(string.Empty)
					: new Paragraph(new TextRun($"<a href='mailto:{email}'>{email}</a>"));

				table[r][1].SetContent(content);
			}
		}


		private Table BuildDeetsTable(
			OutlookContact contact,
			ContactTemplateOption template)
		{
			var both = template == ContactTemplateOption.Both;
			var table = new Table(ns, 8, both ? 3 : 2)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 120);
			table.SetColumnWidth(1, both ? 250 : 505);
			if (both)
			{
				table.SetColumnWidth(2, 250);
			}

			var label = template == ContactTemplateOption.Business
				? Resx.OutlookContactCommand_business
				: Resx.OutlookContactCommand_personal;

			table[0].SetShading(DeetsHeaderShading);
			table[0][0].SetMeta(TemplateMeta, template.ToString());
			table[0][1].SetContent(new Paragraph(label).SetItalic());

			if (both)
			{
				table[0][2].SetContent(new Paragraph(Resx.OutlookContactCommand_business).SetItalic());
			}

			for (int i = 1; i < 8; i++)
			{
				table[i][0].ShadingColor = LabelShading;
			}

			// field names are generic (no Home/Business prefix); omTemplate on (0,0)
			// tells the save operation which value column(s) map to which contact property
			SetLabel(table[1][0], Resx.OutlookContactCommand_telephone, "TelephoneNumber");
			SetLabel(table[2][0], Resx.OutlookContactCommand_mobile, "MobileTelephoneNumber");
			SetLabel(table[3][0], Resx.OutlookContactCommand_street, "AddressStreet");
			SetLabel(table[4][0], Resx.OutlookContactCommand_city, "AddressCity");
			SetLabel(table[5][0], Resx.OutlookContactCommand_stateProvince, "AddressState");
			SetLabel(table[6][0], Resx.OutlookContactCommand_zipPostal, "AddressPostalCode");
			SetLabel(table[7][0], Resx.OutlookContactCommand_countryRegion, "AddressCountry");

			if (template == ContactTemplateOption.Personal || both)
			{
				WriteValue(table[1][1], contact.HomeTelephoneNumber);
				WriteValue(table[2][1], contact.MobileTelephoneNumber);
				WriteValue(table[3][1], contact.HomeAddressStreet);
				WriteValue(table[4][1], contact.HomeAddressCity);
				WriteValue(table[5][1], contact.HomeAddressState);
				WriteValue(table[6][1], contact.HomeAddressPostalCode);
				WriteValue(table[7][1], contact.HomeAddressCountry);
			}

			if (template == ContactTemplateOption.Business || both)
			{
				var c = template == ContactTemplateOption.Business ? 1 : 2;

				WriteValue(table[1][c], contact.BusinessTelephoneNumber);

				if (template == ContactTemplateOption.Business)
				{
					WriteValue(table[2][c], contact.MobileTelephoneNumber);
				}
				else
				{
					table[2][c].SetShading(ValueShading);
					table[2][c].SetContent(new Paragraph(Resx.OutlookContactCommand_notApplicable)
						.SetStyle("font-size:8.0pt;color:#7F7F7F"));
				}

				WriteValue(table[3][c], contact.BusinessAddressStreet);
				WriteValue(table[4][c], contact.BusinessAddressCity);
				WriteValue(table[5][c], contact.BusinessAddressState);
				WriteValue(table[6][c], contact.BusinessAddressPostalCode);
				WriteValue(table[7][c], contact.BusinessAddressCountry);
			}

			return table;
		}


		private Table BuildMiscTable(OutlookContact contact)
		{
			var table = new Table(ns, 2, 2)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 120);
			table.SetColumnWidth(1, 505);

			// Year ≤ 1604 → treat as “no year / invalid birthday”
			// Year ≥ 4501 → treat as “no year / invalid anniversary”

			var birthday = contact.Birthday.Year < 4500
				? contact.Birthday.ToString("MMMM d")
				: string.Empty;

			WriteField(table[0], Resx.OutlookContactCommand_birthday, birthday, "Birthday");

			var anniversary = contact.Anniversary.Year < 4500
				? contact.Anniversary.ToString("MMMM d")
				: string.Empty;

			WriteField(table[1], Resx.OutlookContactCommand_anniversary, anniversary, "Anniversary");

			return table;
		}


		private static void BuildCategoriesRow(
			Table table, OutlookContact contact, List<OutlookCategory> categories)
		{
			// label...

			table[1][0]
				.SetShading(GrayShading)
				.SetContent(new ContentList(
					new Paragraph(Resx.OutlookContactCommand_categories).SetBold().SetItalic().SetMeta(FieldMeta, "Categories"),
					new Paragraph(string.Empty)
					));

			if (string.IsNullOrWhiteSpace(contact.Categories))
			{
				return;
			}

			// categories...

			var cats = contact.Categories.Split(';');
			var span = new StringBuilder();

			foreach (var cat in cats)
			{
				var egory = categories.FirstOrDefault(c => c.Name.EqualsICIC(cat));
				if (egory is not null)
				{
					span.Append(
						$"<span style='color:{egory.ColorName}'>{CategoryBar}</span>{SecurityElement.Escape(cat)} ");
				}
			}

			var run = span.ToString();
			if (!string.IsNullOrWhiteSpace(run))
			{
				table[1][1]
					.SetContent(new Paragraph(new TextRun(run)));
			}
		}


		private static void BuildNotesRow(Table table, OutlookContact contact)
		{
			table[2][0]
				.SetShading(LabelShading)
				.SetContent(new ContentList(
					new Paragraph(Resx.word_Notes).SetBold().SetItalic().SetMeta(FieldMeta, "Body"),
					new Paragraph(string.Empty)
					));

			// one OE per line so multi-line notes round-trip through ApplyCategoriesAndNotes;
			// the trailing empty paragraph is a spacer, not content, and is dropped on read
			var lines = (contact.Body ?? string.Empty)
				.Replace("\r\n", "\n")
				.Split('\n')
				.Select(line => new Paragraph(line))
				.Append(new Paragraph(string.Empty))
				.ToArray();

			table[2][1]
				.SetShading(ValueShading)
				.SetContent(new ContentList(lines));
		}
	}
}