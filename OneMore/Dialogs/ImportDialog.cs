//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportDialog : LocalizableForm
	{

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
					"okButton",
					"cancelButton"
				});
			}

			pathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		public string FilePath => pathBox.Text;

		public bool WordFile { get; private set; }

		public bool AppendToPage => WordFile ? wordAppendButton.Checked : powerAppendButton.Checked;

		public bool CreateSection => powerSectionButton.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			try
			{
				var ext = Path.GetExtension(pathBox.Text);
				if (ext == ".docx")
				{
					wordGroup.Visible = true;
					powerGroup.Visible = false;
					okButton.Enabled = true;
					WordFile = true;
				}
				else if (ext == ".pptx")
				{
					wordGroup.Visible = false;
					powerGroup.Visible = true;
					okButton.Enabled = true;
					WordFile = false;
				}
				else
				{
					wordGroup.Visible = powerGroup.Visible = false;
					okButton.Enabled = false;
				}
			}
			catch
			{
				okButton.Enabled = false;
			}
		}


		private void BrowseFile(object sender, EventArgs e)
		{
			try
			{
				string path = pathBox.Text;

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using (var dialog = new OpenFileDialog()
					{
						AddExtension = true,
						CheckFileExists = true,
						DefaultExt = ".docx",
						Filter = Resx.ImportDialog_OpenFileFilter,
						InitialDirectory = path,
						Multiselect = false,
						Title = Resx.ImportDialog_OpenFileTitle
					})
					{
						// cannot use owner parameter here or it will hang! cross-threading
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							path = dialog.FileName;
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
				Logger.Current.WriteLine("Error running OpenFileDialog", exc);
			}
		}
	}
}
