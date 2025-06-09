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

		public OpenFolderDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.OpenFolderDialog_Text;

				Localize(new string[]
				{
					"folderLabel=word_Folder",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		public string FolderPath => pathBox.Text;


		private void ChangePath(object sender, EventArgs e)
		{
			okButton.Enabled =
				pathBox.Text.Trim().Length > 0 &&
				System.IO.Directory.Exists(pathBox.Text);
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
						Description = "Folder to open as a new notebook:",
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
