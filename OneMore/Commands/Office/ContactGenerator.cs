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
	using System.Linq;
	using System.Security;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class ContactGenerator
	{
		private const string RefreshMeta = "omOutlookContactRefresh";
		private const string TableMeta = "omOutlookContact";
		private const string FieldMeta = "omField";

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
			var openin = $"<a href=\"onenote:outlook?folder=Contacts&amp;entryid={contact.EntryID}\">Open in Outlook</a>";
			var refresh = $"<a href=\"{RefreshUri}/{guid}/{template}\">{Resx.word_Refresh}</a>";
			var save = $"<a href=\"{SaveUri}/{guid}/{template}\">Save</a>";

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
			content.SetMeta(FieldMeta, name);

			row[0]
				.SetShading(LabelShading)
				.SetContent(new Paragraph(label).SetItalic());

			row[valCol]
				.SetShading(ValueShading)
				.SetContent(content);
		}

		private static void WriteField(TableCell cell, string value, string name)
		{
			var content = new Paragraph(value ?? string.Empty).SetMeta(FieldMeta, name);

			cell.SetShading(ValueShading)
				.SetContent(content);
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

			WriteField(table[0], "First", new Paragraph(contact.FirstName ?? string.Empty).SetBold(), "FirstName");
			WriteField(table[1], "Middle", new Paragraph(contact.MiddleName ?? string.Empty).SetBold(), "MiddleName");
			WriteField(table[2], "Last", new Paragraph(contact.LastName ?? string.Empty).SetBold(), "LastName");

			if (!personal)
			{
				WriteField(table[3], "Title", contact.Title, "Title");
				WriteField(table[4], "Company", contact.CompanyName, "CompanyName");
				WriteField(table[5], "Department", contact.Department, "Department");
				WriteField(table[6], "Customer ID", contact.CustomerID, "CustomerID");
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

			FillValue(contact.Email1Address, 0, "Email 1", "Email1Address");
			FillValue(contact.Email2Address, 1, "Email 2", "Email2Address");
			FillValue(contact.Email3Address, 2, "Email 3", "Email3Address");

			return table;

			void FillValue(string email, int r, string label, string name)
			{
				table[r][0]
					.SetShading(LabelShading)
					.SetContent(new Paragraph(label).SetItalic());

				table[r][1].SetShading(ValueShading);

				var content = string.IsNullOrWhiteSpace(email)
					? new Paragraph(string.Empty)
					: new Paragraph(new TextRun($"<a href='mailto:{email}'>{email}</a>"));

				table[r][1].SetContent(content.SetMeta(FieldMeta, name));
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
				? "Business"
				: "Personal";

			table[0].SetShading(DeetsHeaderShading);
			table[0][1].SetContent(new Paragraph(label).SetItalic());

			if (both)
			{
				table[0][2].SetContent(new Paragraph("Business").SetItalic());
			}

			for (int i = 1; i < 8; i++)
			{
				table[i][0].ShadingColor = LabelShading;
			}

			table[1][0].SetContent(new Paragraph("Telephone").SetItalic());
			table[2][0].SetContent(new Paragraph("Mobile").SetItalic());
			table[3][0].SetContent(new Paragraph("Street").SetItalic());
			table[4][0].SetContent(new Paragraph("City").SetItalic());
			table[5][0].SetContent(new Paragraph("State/Province").SetItalic());
			table[6][0].SetContent(new Paragraph("ZIP/Postal").SetItalic());
			table[7][0].SetContent(new Paragraph("Country/Region").SetItalic());

			if (template == ContactTemplateOption.Personal || both)
			{
				WriteField(table[1][1], contact.HomeTelephoneNumber, "HomeTelephoneNumber");
				WriteField(table[2][1], contact.MobileTelephoneNumber, "MobileTelephoneNumber");
				WriteField(table[3][1], contact.HomeAddressStreet, "HomeAddressStreet");
				WriteField(table[4][1], contact.HomeAddressCity, "HomeAddressCity");
				WriteField(table[5][1], contact.HomeAddressState, "HomeAddressState");
				WriteField(table[6][1], contact.HomeAddressPostalCode, "HomeAddressPostalCode");
				WriteField(table[7][1], contact.HomeAddressCountry, "HomeAddressCountry");
			}

			if (template == ContactTemplateOption.Business || both)
			{
				var c = template == ContactTemplateOption.Business ? 1 : 2;

				WriteField(table[1][c], contact.BusinessTelephoneNumber, "BusinessTelephoneNumber");

				if (template == ContactTemplateOption.Business)
				{
					WriteField(table[2][c], contact.MobileTelephoneNumber, "MobileTelephoneNumber");
				}
				else
				{
					table[2][c].SetShading(ValueShading);
					table[2][c].SetContent(new Paragraph("N/A")
						.SetStyle("font-size:8.0pt;color:#7F7F7F"));
				}

				WriteField(table[3][c], contact.BusinessAddressStreet, "BusinessAddressStreet");
				WriteField(table[4][c], contact.BusinessAddressCity, "BusinessAddressCity");
				WriteField(table[5][c], contact.BusinessAddressState, "BusinessAddressState");
				WriteField(table[6][c], contact.BusinessAddressPostalCode, "BusinessAddressPostalCode");
				WriteField(table[7][c], contact.BusinessAddressCountry, "BusinessAddressCountry");
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

			WriteField(table[0], "Birthday", birthday, "Birthday");

			var anniversary = contact.Anniversary.Year < 4500
				? contact.Anniversary.ToString("MMMM d")
				: string.Empty;

			WriteField(table[1], "Anniversary", anniversary, "Anniversary");

			return table;
		}


		private static void BuildCategoriesRow(
			Table table, OutlookContact contact, List<OutlookCategory> categories)
		{
			// label...

			table[1][0]
				.SetShading(GrayShading)
				.SetContent(new ContentList(
					new Paragraph("Categories").SetBold().SetItalic(),
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
					.SetMeta(FieldMeta, "Categories")
					.SetContent(new Paragraph(new TextRun(run)));
			}
		}


		private static void BuildNotesRow(Table table, OutlookContact contact)
		{
			table[2][0]
				.SetShading(LabelShading)
				.SetContent(new ContentList(
					new Paragraph("Notes").SetBold().SetItalic(),
					new Paragraph(string.Empty)
					));

			table[2][1]
				.SetShading(ValueShading)
				.SetMeta(FieldMeta, "Body")
				.SetContent(new ContentList(
					new Paragraph(contact.Body ?? string.Empty),
					new Paragraph(string.Empty)
					));
		}
	}
}