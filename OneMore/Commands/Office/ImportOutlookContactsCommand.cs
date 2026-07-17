//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Helpers.Office;
	using OLK = Microsoft.Office.Interop.Outlook;
	using Resx = Properties.Resources;


	[CommandService]
	internal class ImportOutlookContactsCommand : Command
	{
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

				if (!Enum.TryParse(args[2] as string, out ImportOutlookContactsDialog.TemplateOption atemp))
				{
					atemp = ImportOutlookContactsDialog.TemplateOption.Both;
				}

				if (!Enum.TryParse(args[3] as string, out ImportOutlookContactsDialog.SortByOption asort))
				{
					asort = ImportOutlookContactsDialog.SortByOption.LastName;
				}

				await new ContactListGenerator().UpdateReport(guid, atemp, asort);
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

			await using (var one = new OneNote(out var page, out _))
			{
				if (page is null)
				{
					logger.WriteLine("could not load current page; skipping contact subpages");
				}
				else
				{
					var ordered = ContactListGenerator.SortContacts(selected, sortBy);
					ordered.Reverse(); // CreateChildPage always inserts right after the parent,
									   // so reverse-inserting yields ascending sortBy order

					foreach (var contact in ordered)
					{
						var title = ImportOutlookContactsDialog.GetContactDisplayName(contact);
						var child = await one.CreateChildPage(page, title);

						new ContactGenerator().GenerateReport(child, contact, template, categories);
						await one.Update(child);

						contact.PageUri = await one.GetHyperlinkWithRetry(child.PageId, string.Empty);
					}
				}
			}

			await new ContactListGenerator().GenerateReport(selected, template, sortBy);
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
	}
}
