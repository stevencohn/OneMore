//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportDialog : UI.LocalizableForm
	{
		public enum Formats
		{
			Word,
			PowerPoint,
			Xml,
			OneNote,
			Markdown
		}

		public ImportDialog()
		{
			InitializeComponent();

			wordGroup.Visible = false;
			powerGroup.Visible = false;

			browseButton.Top = pathBox.Top;
			browseButton.Height = pathBox.Height;

			if (NeedsLocalizing())
			{
				Text = Resx.ImportDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"fileLabel",
					"wordGroup",
					"wordAppendButton",
					"wordCreateButton",
					"powerGroup",
					"powerAppendButton",
					"powerCreateButton",
					"powerSectionButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		public string FilePath => pathBox.Text;

		public Formats Format { get; private set; }

		public bool AppendToPage
		{
			get
			{
				if (Format == Formats.Xml) return false;
				return Format == Formats.Word ? wordAppendButton.Checked : powerAppendButton.Checked;
			}
		}

		public bool CreateSection => powerSectionButton.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			try
			{
				var ext = Path.GetExtension(pathBox.Text);

				switch (ext)
				{
					case ".docx":
						wordGroup.Visible = true;
						powerGroup.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Word;
						break;

					case ".md":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Markdown;
						break;

					case ".pptx":
						wordGroup.Visible = false;
						powerGroup.Visible = true;
						okButton.Enabled = true;
						Format = Formats.PowerPoint;
						break;

					case ".xml":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Xml;
						break;

					case ".one":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						okButton.Enabled = true;
						Format = Formats.OneNote;
						break;

					default:
						wordGroup.Visible = powerGroup.Visible = false;
						okButton.Enabled = false;
						break;
				}
			}
			catch
			{
				okButton.Enabled = false;
			}
		}


		private async void BrowseFile(object sender, EventArgs e)
		{
			try
			{
				// OpenFileDialog must run in an STA thread
				var path = await SingleThreaded.Invoke(() =>
				{
					using (var dialog = new OpenFileDialog()
					{
						AddExtension = true,
						CheckFileExists = true,
						DefaultExt = ".docx",
						Filter = Resx.ImportDialog_OpenFileFilter,
						InitialDirectory = pathBox.Text,
						Multiselect = false,
						Title = Resx.ImportDialog_OpenFileTitle
					})
					{
						// cannot use owner parameter here or it will hang! cross-threading
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							return dialog.FileName;
						}
					}

					return null;
				});

				if (path != null)
				{
					pathBox.Text = path;
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error running OpenFileDialog", exc);
			}
		}
	}
}
