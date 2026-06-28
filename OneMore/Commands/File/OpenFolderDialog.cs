//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class OpenFolderDialog : UI.MoreForm
	{
		private readonly string backupFolder;


		public OpenFolderDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.OpenFolderDialog_Text;

				Localize(new string[]
				{
					"folderLabel=word_Folder",
					"editBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			using var one = new OneNote();
			(backupFolder, _, _) = one.GetFolders();

			pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		public string FolderPath => pathBox.Text;


		public bool RemoveTimestamps => editBox.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			var path = pathBox.Text.Trim();
			var exists = path.Length > 0 && System.IO.Directory.Exists(path);

			okButton.Enabled = exists;

			if (exists &&
				backupFolder != null &&
				path.StartsWith(backupFolder, StringComparison.OrdinalIgnoreCase))
			{
				editBox.Checked = false;
				editBox.Enabled = false;
				okButton.Enabled = false;
			}
			else
			{
				editBox.Enabled = true;
				okButton.Enabled = true;
			}
		}


		private void BrowseFolders(object sender, EventArgs e)
		{
			try
			{
				string path = pathBox.Text;

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using var dialog = new FolderBrowserDialog()
					{
						Description = Resx.OpenFolderDialog_Text,
						SelectedPath = path
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						path = dialog.SelectedPath;
					}
				})
				{
					Name = $"{nameof(ExportDialog)}Thread"
				};

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				thread.Join();

				pathBox.Text = path;

			}
			catch (Exception exc)
			{
				logger.WriteLine("error running FolderBrowserDialog", exc);
			}
		}
	}
}
