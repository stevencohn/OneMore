//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
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

			if (args.Length > 3 && args[0] is string refreshArg && refreshArg == "refresh")
			{
				var guid = args[1] as string;

				if (!Enum.TryParse(args[2] as string, out ContactTemplateOption atemp))
				{
					atemp = ContactTemplateOption.Both;
				}

				if (!Enum.TryParse(args[3] as string, out ContactSortByOption asort))
				{
					asort = ContactSortByOption.LastName;
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

			var ordered = ContactListGenerator.SortContacts(selected, sortBy);

			// CreateChildPages always inserts right after the parent,
			// so reverse-inserting yields ascending sortBy order
			ordered.Reverse();

			// importing can take a minute or two for a large number of contacts, so run it
			// through a cancelable progress dialog rather than blocking the UI silently
			var importProgress = new UI.ProgressDialog(async (self, token) =>
			{
				try
				{
					await using var one = new OneNote();

					self.SetMaximum(ordered.Count + 3);

					self.SetMessage(Resx.ImportOutlookContactsCommand_creatingPages);
					var listPage = await one.CreateChildPage(null, Resx.ImportOutlookContactsCommand_pageTitle);
					self.Increment();

					var titles = ordered.Select(ImportOutlookContactsDialog.GetContactDisplayName).ToList();
					var children = await one.CreateChildPages(listPage, titles);
					self.Increment();

					var indexUri = await one.GetHyperlinkWithRetry(listPage.PageId, string.Empty);

					for (var i = 0; i < ordered.Count; i++)
					{
						if (token.IsCancellationRequested)
						{
							return;
						}

						var contact = ordered[i];
						var child = children[i];

						self.SetMessage(string.Format(
							Resx.ImportOutlookContactsCommand_importing,
							ImportOutlookContactsDialog.GetContactDisplayName(contact)));

						new ContactGenerator().GenerateReport(child, contact, template, categories, indexUri);
						await one.Update(child);

						contact.PageUri = await one.GetHyperlinkWithRetry(child.PageId, string.Empty);

						self.Increment();
					}

					if (token.IsCancellationRequested)
					{
						return;
					}

					self.SetMessage(Resx.ImportOutlookContactsCommand_finalizing);
					new ContactListGenerator().GenerateReport(listPage, selected, template, sortBy);
					await one.Update(listPage);
					self.Increment();

					await one.NavigateTo(listPage.PageId);
				}
				finally
				{
					self.Close();
				}
			});

			importProgress.RunModeless();
		}


		private static IEnumerable<OutlookContact> CollectContacts(
			IEnumerable<OutlookFolder> folders)
		{
			foreach (var folder in folders)
			{
				var items = folder.Folder.Items;
				try
				{
					foreach (var item in items)
					{
						if (item is OLK.ContactItem contact)
						{
							// ownership transfers to the wrapping OutlookContact
							yield return new OutlookContact(contact);
						}
						else
						{
							Marshal.ReleaseComObject(item);
						}
					}
				}
				finally
				{
					Marshal.ReleaseComObject(items);
				}
			}
		}
	}
}
