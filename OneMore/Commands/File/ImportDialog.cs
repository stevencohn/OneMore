//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ImportDialog : UI.MoreForm
	{
		public enum Formats
		{
			Word,
			PowerPoint,
			Xml,
			OneNote,
			Markdown,
			Pdf
		}


		private readonly bool wordInstalled;
		private readonly bool powerPointInstalled;
		private readonly bool initialized = false;


		public ImportDialog()
		{
			InitializeComponent();

			wordInstalled = Office.IsInstalled("Word");
			powerPointInstalled = Office.IsInstalled("Powerpoint");

			wordGroup.Visible = false;
			powerGroup.Visible = false;
			pdfGroup.Visible = false;
			notInstalledLabel.Visible = false;

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
					"wordAppendButton=phrase_AppendToThisPage",
					"wordCreateButton=phrase_CreateANewPage",
					"powerGroup",
					"powerAppendButton=phrase_AppendToThisPage",
					"powerCreateButton=phrase_CreateANewPage",
					"powerSectionButton",
					"pdfGroup",
					"pdfAppendButton=phrase_AppendToThisPage",
					"pdfCreateButton=phrase_CreateANewPage",
					"notInstalledLabel",
					"errorLabel=phrase_PathNotFound",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}


			var settings = new SettingsProvider();
			var defaultPath = settings.GetCollection("Import")?["path"];
			if (string.IsNullOrWhiteSpace(defaultPath))
			{
				pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			else
			{
				pathBox.Text = defaultPath;
			}

			initialized = true;
		}


		public string FilePath => pathBox.Text;

		public Formats Format { get; private set; }

		public bool AppendToPage
		{
			get
			{
				return Format switch
				{
					Formats.Word => wordAppendButton.Checked,
					Formats.PowerPoint => powerAppendButton.Checked,
					Formats.Pdf => pdfAppendButton.Checked,
					_ => false
				};
			}
		}

		public bool CreateSection => powerSectionButton.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			try
			{
				var wild = PathHelper.HasWildFileName(pathBox.Text);
				var ext = Path.GetExtension(pathBox.Text);

				switch (ext)
				{
					case ".doc":
					case ".docx":
						wordGroup.Visible = wordInstalled;
						powerGroup.Visible = false;
						pdfGroup.Visible = false;
						notInstalledLabel.Visible = !wordInstalled;
						okButton.Enabled = wordInstalled;
						Format = Formats.Word;
						wordAppendButton.Enabled = !wild;
						if (wild)
						{
							wordAppendButton.Checked = false;
							wordCreateButton.Checked = true;
						}
						break;

					case ".md":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						pdfGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = true;
						Format = Formats.Markdown;
						break;

					case ".pdf":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						pdfGroup.Visible = true;
						notInstalledLabel.Visible = false;
						okButton.Enabled = false;
						Format = Formats.Pdf;
						wordAppendButton.Enabled = !wild;
						if (wild)
						{
							pdfAppendButton.Checked = false;
							pdfCreateButton.Checked = true;
						}
						break;

					case ".ppt":
					case ".pptx":
						wordGroup.Visible = false;
						powerGroup.Visible = powerPointInstalled;
						pdfGroup.Visible = false;
						notInstalledLabel.Visible = !powerPointInstalled;
						okButton.Enabled = powerPointInstalled;
						Format = Formats.PowerPoint;
						powerAppendButton.Enabled = !wild;
						if (wild)
						{
							powerAppendButton.Checked = false;
							powerCreateButton.Checked = true;
						}
						break;

					case ".xml":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						pdfGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = !wild;
						Format = Formats.Xml;
						break;

					case ".one":
						wordGroup.Visible = false;
						powerGroup.Visible = false;
						pdfGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = !wild;
						Format = Formats.OneNote;
						break;

					default:
						wordGroup.Visible = powerGroup.Visible = pdfGroup.Visible = false;
						notInstalledLabel.Visible = false;
						okButton.Enabled = false;
						break;
				}

				if (initialized)
				{
					var path = pathBox.Text.Trim();
					if (string.IsNullOrWhiteSpace(path))
					{
						errorLabel.Visible = false;
						okButton.Enabled = false;
					}
					else
					{
						var ok = PathHelper.HasWildFileName(path)
							? Directory.GetFiles(
								Path.GetDirectoryName(path), Path.GetFileName(path)).Length > 0
							: File.Exists(path);

						errorLabel.Visible = !ok;
						okButton.Enabled = ok;
					}
				}
			}
			catch
			{
				okButton.Enabled = false;
				errorLabel.Visible = initialized;
			}
		}


		private async void BrowseFile(object sender, EventArgs e)
		{
			try
			{
				// OpenFileDialog must run in an STA thread
				var path = await SingleThreaded.Invoke(() =>
				{
					using var dialog = new OpenFileDialog
					{
						AddExtension = true,
						CheckFileExists = true,
						DefaultExt = ".docx",
						Filter = Resx.ImportDialog_OpenFileFilter,
						InitialDirectory = pathBox.Text,
						Multiselect = false,
						Title = Resx.ImportDialog_OpenFileTitle
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						return dialog.FileName;
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
				logger.WriteLine("error running OpenFileDialog", exc);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				var path = pathBox.Text;
				if (Path.HasExtension(path))
				{
					// strip off the filename so only the dir is stored
					path = Path.GetDirectoryName(path);
				}

				// save only paths that exist
				if (Directory.Exists(path))
				{
					var settings = new SettingsProvider();
					var collection = settings.GetCollection("Import");
					collection.Add("path", path);
					settings.SetCollection(collection);
					settings.Save();
				}
			}

			base.OnClosing(e);
		}
	}
}
