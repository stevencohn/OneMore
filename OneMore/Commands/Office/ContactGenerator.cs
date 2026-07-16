//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************ 

#pragma warning disable S1075 // URI

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using Resx = Properties.Resources;


	internal class ContactGenerator
	{
		private const string RefreshMeta = "omOutlookContactRefresh";
		private const string TableMeta = "omOutlookContact";

		private const string ControlRowShading = "#C5E0B3";
		private const string DeetsHeaderShading = "#9CC3E5";
		private const string BackgroundShading = "#1E4E79";
		private const string LabelShading = "#BDD7EE";
		private const string ValueShading = "#DEEBF6";
		private const string GrayShading = "#E7E6E6";

		private const string CategoryBar = "\u258C"; // ▌ Left Half Block

		private const string RefreshUri = "onemore://ImportOutlookContactsCommand/refresh";

		private XNamespace ns;

		public ContactGenerator()
		{
		}


		public void GenerateReport(
			Page page,
			OutlookContact contact,
			ImportOutlookContactsDialog.TemplateOption template,
			List<OutlookCategory> categories)
		{
			PageNamespace.Set(ns = page.Namespace);

			var guid = Guid.NewGuid().ToString("b").ToUpper();
			var nowf = DateTime.Now.ToShortFriendlyString();

			var controlTable = new Table(ns, 1, 3)
			{
				HasHeaderRow = true
			};

			controlTable.SetColumnWidth(0, 70);
			controlTable.SetColumnWidth(1, 605);
			controlTable.SetColumnWidth(2, 40);

			var row = controlTable[0];
			row.SetShading(ControlRowShading);

			row[0].SetContent(new Paragraph(string.Empty));

			row[1].SetContent(new Paragraph(
				$"{Resx.ReminderReport_LastUpdated} {nowf} " +
				$"(<a href=\"{RefreshUri}/{guid}/{template}\">{Resx.word_Refresh}</a>)")
				.SetAlignment("right")
				.SetMeta(RefreshMeta, guid));

			row[2].SetContent(new Paragraph(string.Empty));

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


		private Table BuildDetailsTable(
			OutlookContact contact,
			ImportOutlookContactsDialog.TemplateOption template,
			List<OutlookCategory> categories)
		{
			var table = new Table(ns, 3, 2)
			{
				HasHeaderRow = true
			};

			table.SetColumnWidth(0, 70);
			table.SetColumnWidth(1, 650);

			var nameTable = BuildNameTable(
				contact, template == ImportOutlookContactsDialog.TemplateOption.Personal);

			var deetTable = BuildDeetsTable(contact, template);
			var miscTable = BuildMiscTable(contact);

			table[0].SetShading(BackgroundShading);

			table[0][1].SetContent(
				new ContentList(
					new Paragraph(string.Empty), new Paragraph(nameTable.Root),
					new Paragraph(string.Empty), new Paragraph(deetTable.Root),
					new Paragraph(string.Empty), new Paragraph(miscTable.Root),
					new Paragraph(string.Empty)
					)
				);

			BuildCategoriesRow(table, contact, categories);
			BuildNotesRow(table, contact);

			return table;
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

			for (int i = 0; i < table.RowCount; i++)
			{
				table[i][0].SetShading(LabelShading);
				table[i][1].SetShading(ValueShading);
			}

			table[0][0].SetContent(new Paragraph("First").SetItalic());
			table[0][1].SetContent(new Paragraph(contact.FirstName ?? string.Empty).SetBold());
			table[1][0].SetContent(new Paragraph("Middle").SetItalic());
			table[1][1].SetContent(new Paragraph(contact.MiddleName ?? string.Empty).SetBold());
			table[2][0].SetContent(new Paragraph("Last").SetItalic());
			table[2][1].SetContent(new Paragraph(contact.LastName ?? string.Empty).SetBold());

			if (!personal)
			{
				table[3][0].SetContent(new Paragraph("Title").SetItalic());
				table[3][1].SetContent(new Paragraph(contact.Title ?? string.Empty));
				table[4][0].SetContent(new Paragraph("Company").SetItalic());
				table[4][1].SetContent(new Paragraph(contact.CompanyName ?? string.Empty));
				table[5][0].SetContent(new Paragraph("Department").SetItalic());
				table[5][1].SetContent(new Paragraph(contact.Department ?? string.Empty));
				table[6][0].SetContent(new Paragraph("Customer ID").SetItalic());
				table[6][1].SetContent(new Paragraph(contact.CustomerID ?? string.Empty));
			}

			return table;
		}


		private Table BuildDeetsTable(
			OutlookContact contact, ImportOutlookContactsDialog.TemplateOption template)
		{
			var both = template == ImportOutlookContactsDialog.TemplateOption.Both;
			var table = new Table(ns, 9, both ? 3 : 2)
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

			var label = template == ImportOutlookContactsDialog.TemplateOption.Business
				? "Business"
				: "Personal";

			table[0].SetShading(DeetsHeaderShading);
			table[0][1].SetContent(new Paragraph(label).SetItalic());

			if (both)
			{
				table[0][2].SetContent(new Paragraph("Business").SetItalic());
			}

			for (int i=1; i < 9; i++)
			{
				table[i][0].ShadingColor = LabelShading;
				table[i][1].ShadingColor = ValueShading;
				if (both)
				{
					table[i][2].ShadingColor = ValueShading;
				}
			}

			table[1][0].SetContent(new Paragraph("Telephone").SetItalic());
			table[2][0].SetContent(new Paragraph("Mobile").SetItalic());
			table[3][0].SetContent(new Paragraph("Email").SetItalic());
			table[4][0].SetContent(new Paragraph("Street").SetItalic());
			table[5][0].SetContent(new Paragraph("City").SetItalic());
			table[6][0].SetContent(new Paragraph("State/Province").SetItalic());
			table[7][0].SetContent(new Paragraph("ZIP/Postal").SetItalic());
			table[8][0].SetContent(new Paragraph("Country/Region").SetItalic());

			if (template == ImportOutlookContactsDialog.TemplateOption.Personal || both)
			{
				table[1][1].SetContent(contact.HomeTelephoneNumber ?? string.Empty);
				table[2][1].SetContent(contact.MobileTelephoneNumber ?? string.Empty);
				table[3][1].SetContent(contact.Email1Address ?? string.Empty);
				table[4][1].SetContent(contact.HomeAddressStreet ?? string.Empty);
				table[5][1].SetContent(contact.HomeAddressCity ?? string.Empty);
				table[6][1].SetContent(contact.HomeAddressState ?? string.Empty);
				table[7][1].SetContent(contact.HomeAddressPostalCode ?? string.Empty);
				table[8][1].SetContent(contact.HomeAddressCountry ?? string.Empty);
			}

			if (template == ImportOutlookContactsDialog.TemplateOption.Business || both)
			{
				var c = template == ImportOutlookContactsDialog.TemplateOption.Business ? 1 : 2;

				table[1][c].SetContent(contact.BusinessTelephoneNumber ?? string.Empty);
				table[3][c].SetContent(contact.Email2Address ?? string.Empty);
				table[4][c].SetContent(contact.BusinessAddressStreet ?? string.Empty);
				table[5][c].SetContent(contact.BusinessAddressCity ?? string.Empty);
				table[6][c].SetContent(contact.BusinessAddressState ?? string.Empty);
				table[7][c].SetContent(contact.BusinessAddressPostalCode ?? string.Empty);
				table[8][c].SetContent(contact.BusinessAddressCountry ?? string.Empty);
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

			var birthday = contact.Birthday == DateTime.MinValue || contact.Birthday == DateTime.MaxValue
				? string.Empty
				: contact.Birthday.ToString(AddIn.Culture.DateTimeFormat.LongDatePattern);

			table[0][0]
				.SetShading(LabelShading)
				.SetContent(new Paragraph("Birthday").SetItalic());

			table[0][1]
				.SetShading(ValueShading)
				.SetContent(new Paragraph(birthday));

			var anniversary = contact.Anniversary == DateTime.MinValue || contact.Anniversary <= DateTime.MaxValue
				? string.Empty
				: contact.Anniversary.ToString(AddIn.Culture.DateTimeFormat.LongDatePattern);

			table[1][0]
				.SetShading(LabelShading)
				.SetContent(new Paragraph("Anniversary").SetItalic());

			table[1][1]
				.SetShading(ValueShading)
				.SetContent(new Paragraph(anniversary));

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
					span.Append($"<span style='color:{egory.ColorName}'>{CategoryBar}</span>{cat} ");
				}
			}

			var run = span.ToString();
			if (!string.IsNullOrWhiteSpace(run))
			{
				table[1][1].SetContent(new Paragraph(new TextRun(run)));
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
				.SetContent(new ContentList(
					new Paragraph(contact.Body ?? string.Empty),
					new Paragraph(string.Empty)
					));
		}
	}
}