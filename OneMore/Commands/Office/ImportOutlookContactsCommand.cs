//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Microsoft.Office.Interop.Outlook;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using Resx = Properties.Resources;


	[CommandService]
	internal class ImportOutlookContactsCommand : Command
	{

		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ImportOutlookContactsCommand()
		{
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
				//await UpdateTableReport(args[1] as string);
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
				System.Windows.Forms.Application.DoEvents();

				using (var outlook = new Outlook())
				{
					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingCategories);
					categories = outlook.GetCategories().ToList();
					progress.Increment();
					System.Windows.Forms.Application.DoEvents();

					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingFolders);
					folders = outlook.GetContactFolders().ToList();
					progress.Increment();
					System.Windows.Forms.Application.DoEvents();

					progress.SetMessage(Resx.ImportOutlookContactsCommand_loadingContacts);
					contacts = CollectContacts(folders).ToList();
					progress.Increment();
					System.Windows.Forms.Application.DoEvents();
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
				// TODO: write selected contacts into the current page
			}
		}


		private IEnumerable<OutlookContact> CollectContacts(IEnumerable<OutlookFolder> folders)
		{
			foreach (var folder in folders)
			{
				foreach (var item in folder.Folder.Items)
				{
					if (item is ContactItem contact)
					{
						yield return new OutlookContact(contact);
					}
				}
			}
		}
	}
}
