//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ExportDialog : UI.LocalizableForm
	{
		public ExportDialog(int pageCount)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ExportDialog_Text;

				Localize(new string[]
				{
					"folderLabel",
					"formatLabel",
					"formatBox",
					"underBox",
					"attachmentsBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			groupBox.Text = pageCount == 1
				? Resx.ExportDialog_groupBox_OneText
				: string.Format(Resx.ExportDialog_groupBox_Text, pageCount);

			pathBox.Text = LoadDefaultPath();
			formatBox.SelectedIndex = 0;
		}


		public string FolderPath => pathBox.Text;


		public bool WithAttachments => attachmentsBox.Enabled && attachmentsBox.Checked;


		public bool UseUnderscores => underBox.Checked;


		public OneNote.ExportFormat Format
		{
			get
			{
				switch (formatBox.SelectedIndex)
				{
					case 0: return OneNote.ExportFormat.HTML;
					case 1: return OneNote.ExportFormat.PDF;
					case 2: return OneNote.ExportFormat.Word;
					case 3: return OneNote.ExportFormat.XML;
					case 4: return OneNote.ExportFormat.Markdown;

					default:
						return OneNote.ExportFormat.OneNote;
				}
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
			attachmentsBox.Enabled =
				formatBox.SelectedIndex == 0 ||     // HTML
				formatBox.SelectedIndex == 2 ||     // Word
				formatBox.SelectedIndex == 3 ||     // XML
				formatBox.SelectedIndex == 4;		// Markdown
		}


		private void BrowseFolders(object sender, EventArgs e)
		{
			try
			{
				string path = pathBox.Text;

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using (var dialog = new FolderBrowserDialog()
					{
						Description = "Export pages to this folder:",
						SelectedPath = path
					})
					{
						// cannot use owner parameter here or it will hang! cross-threading
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							path = dialog.SelectedPath;
						}
					}
				});

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				thread.Join();

				pathBox.Text = path;

			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error running FolderBrowserDialog", exc);
			}
		}
	}
}
