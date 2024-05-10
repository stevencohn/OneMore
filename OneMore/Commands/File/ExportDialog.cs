//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ExportDialog : UI.MoreForm
	{
		private readonly bool wordInstalled;


		public ExportDialog(int pageCount)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ExportDialog_Text;

				Localize(new string[]
				{
					"folderLabel=word_Folder",
					"formatLabel=word_Format",
					"formatBox",
					"underBox",
					"attachmentsBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			wordInstalled = Office.IsInstalled("Word");

			groupBox.Text = pageCount == 1
				? Resx.ExportDialog_groupBox_OneText
				: string.Format(Resx.ExportDialog_groupBox_Text, pageCount);

			pathBox.Text = LoadDefaultPath();
			formatBox.SelectedIndex = 0;
		}


		public bool Embedded => embeddedBox.Checked;


		public string FolderPath => pathBox.Text;


		public bool WithAttachments => attachmentsBox.Enabled && attachmentsBox.Checked;


		public bool UseUnderscores => underBox.Checked;


		public OneNote.ExportFormat Format
		{
			get
			{
				return formatBox.SelectedIndex switch
				{
					0 => OneNote.ExportFormat.HTML,
					1 => OneNote.ExportFormat.PDF,
					2 => OneNote.ExportFormat.Word,
					3 => OneNote.ExportFormat.XML,
					4 => OneNote.ExportFormat.Markdown,
					_ => OneNote.ExportFormat.OneNote,
				};
			}
		}


		private string LoadDefaultPath()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("Export");
			if (settings != null)
			{
				var path = settings.Get<string>("path");
				if (!string.IsNullOrEmpty(path))
				{
					return path;
				}
			}

			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		private void ChangePath(object sender, EventArgs e)
		{
			okButton.Enabled = pathBox.Text.Trim().Length > 0;
		}


		private void ChangeFormat(object sender, EventArgs e)
		{
			okButton.Enabled =
				formatBox.SelectedIndex != 2 || wordInstalled;

			attachmentsBox.Enabled =
				formatBox.SelectedIndex == 0 ||     // HTML
				formatBox.SelectedIndex == 2 ||     // Word
				formatBox.SelectedIndex == 3 ||     // XML
				formatBox.SelectedIndex == 4;       // Markdown

			embeddedBox.Enabled =
				formatBox.SelectedIndex == 2 &&     // Word
				attachmentsBox.Checked;
		}

		private void ChangeIncludeAttachments(object sender, EventArgs e)
		{
			embeddedBox.Enabled =
				formatBox.SelectedIndex == 2 &&     // Word
				attachmentsBox.Checked;
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
						Description = "Export pages to this folder:",
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
