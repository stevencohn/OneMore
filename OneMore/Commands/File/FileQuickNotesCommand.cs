//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class FileQuickNotesCommand : Command
	{
		private OneNote one;
		private bool titled;
		private bool stamped;


		public FileQuickNotesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var collection = new SettingsProvider().GetCollection(nameof(QuickNotesSheet));
			titled = collection.Get("titled", false);
			stamped = collection.Get("stamped", false);

			var organization = collection.Get("organization", "notebook");
			if (organization == "notebook")
			{
				var notebookID = collection.Get<string>("notebookID");
				if (!string.IsNullOrWhiteSpace(notebookID))
				{
					var grouping = collection.Get("grouping", 0);
					await FileIntoNotebook(notebookID, grouping);
				}
			}
			else
			{
				var sectionID = collection.Get<string>("sectionID");
				if (!string.IsNullOrWhiteSpace(sectionID))
				{
					await FileIntoSection(sectionID);
				}
			}
		}


		private async Task FileIntoNotebook(string notebookID, int grouping)
		{
			var unfiled = await LoadQuickNotes();
			if (unfiled == null)
			{
				return;
			}
		}


		private async Task FileIntoSection(string sectionID)
		{
			one = new OneNote();

			var unfiled = await LoadQuickNotes();
			if (unfiled == null)
			{
				return;
			}

			var section = one.GetSection(sectionID);
			var ns = one.GetNamespace(section);
			var count = 0;

			unfiled.Descendants(ns + "Page").ForEach(async e =>
			{
				e.GetAttributeValue("name", out var name, string.Empty);
				e.GetAttributeValue("dateTime", out var dateTime);
				e.GetAttributeValue("lastModifiedTime", out var lastModifiedTime);

				logger.WriteLine($"moving quick note [{name}]");

				var page = one.GetPage(e.Attribute("ID").Value, OneNote.PageDetail.All);

				AddHeader(page, name, dateTime);
				var pageID = await CopyPage(page, sectionID);

				Timewarp(sectionID, pageID, dateTime, lastModifiedTime);
			});

			if (count > 0)
			{
				EmptyQuickNotes(unfiled);
			}
		}


		private async Task<XElement> LoadQuickNotes()
		{
			var books = await one.GetNotebooks();
			var ns = one.GetNamespace(books);

			// Quick Notes are stored in the singular one:UnfiledNotes notebook node
			var book = books.Elements(ns + "UnfiledNotes").FirstOrDefault();
			if (book == null)
			{
				return null;
			}

			// get the notebook with pages...
			var unfiled = await one.GetNotebook(book.Attribute("ID").Value, OneNote.Scope.Pages);
			if (unfiled == null || !unfiled.Elements().Any())
			{
				return null;
			}

			return unfiled;
		}


		private void AddHeader(Page page, string name, string dateTime)
		{
			if (!titled)
			{
				name = string.Empty;
			}

			if (stamped && DateTime.TryParse(dateTime, out var dttm))
			{
				name = $"{dttm.ToString("yyyy-MM-dd")} {name}";
			}

			PageNamespace.Set(page.Namespace);
			var ns = page.Namespace;
			var title = page.Root.Elements(ns + "Title").FirstOrDefault();
			if (title == null)
			{
				var style = page.GetQuickStyle(Styles.StandardStyles.PageTitle);
				title = new XElement(ns + "Title",
					new Paragraph(name).SetQuickStyle(style.Index));

				var outline = page.Root.Elements(ns + "Outline")
					.FirstOrDefault();

				if (outline != null)
				{
					outline.AddBeforeSelf(title);
				}
			}
			else
			{
				var para = new Paragraph(title.Elements(ns + "OE").First());
				para.GetCData().Value = name;
			}
		}


		private async Task<string> CopyPage(Page page, string sectionID)
		{
			one.CreatePage(sectionID, out var pageID);

			// set the page ID to the new page's ID
			page.Root.Attribute("ID").Value = pageID;
			// remove all objectID values and let OneNote generate new IDs
			page.Root.Descendants().Attributes("objectID").Remove();
			var copy = new Page(page.Root); // reparse to refresh PageId

			await one.Update(copy);

			return pageID;
		}


		private void Timewarp(
			string sectionID, string pageID, string dateTime, string lastModifiedTime)
		{
			var section = one.GetSection(sectionID);
			var ns = one.GetNamespace(section);

			var page = section.Descendants(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID")?.Value == pageID);

			if (page != null)
			{
				page.SetAttributeValue("dateTime", dateTime);
				page.SetAttributeValue("lastModifiedTime", lastModifiedTime);

				one.UpdateHierarchy(section);
			}
		}


		private void EmptyQuickNotes(XElement unfiled)
		{
			var ns = one.GetNamespace(unfiled);

			var sectionID = unfiled
				.Elements(ns + "Section")
				.Attributes("ID")
				.Select(a => a.Value)
				.FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(sectionID))
			{
				logger.WriteLine($"deleting section [{sectionID}]");
				//one.DeleteHierarchy(sectionID);
			}
		}
	}
}
/*
<one:UnfiledNotes xmlns:one="" ID="{}">
	<one:Section name="Quick Notes" ID="{}" path="C:\Users\..\OneDrive\Documents\OneNote Notebooks\Quick Notes.one" lastModifiedTime="" color="#B7C997">
	<one:Page ID="{}" name="This is a quick note" dateTime="" lastModifiedTime="" pageLevel="1" />
	<one:Page ID="{}" name="This is another quick note" dateTime="" lastModifiedTime="" pageLevel="1" />
	</one:Section>
</one:UnfiledNotes>
*/
