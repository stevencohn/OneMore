//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ImportEvernoteDialog : UI.MoreForm
	{
		private readonly bool initialized;


		public ImportEvernoteDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ImportEvernoteDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"fileLabel",
					"includeSubfoldersCheckBox",
					"abortCheckBox",
					"errorLabel=phrase_PathNotFound",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			var settings = new SettingsProvider().GetCollection("ImportEvernote");
			if (settings is not null)
			{
				var path = settings["path"];
				pathBox.Text = string.IsNullOrWhiteSpace(path)
					? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
					: path;

				includeSubfoldersCheckBox.Checked = settings.Get("includeSubfolders", false);
				abortCheckBox.Checked = settings.Get("abortOnEncrypted", false);
			}

			initialized = true;
		}


		/// <summary>
		/// One or more pipe ('|') delimited entries, each an explicit file path, a
		/// wildcard pattern, or a folder path.
		/// </summary>
		public string FilePath => pathBox.Text;

		public bool IncludeSubfolders => includeSubfoldersCheckBox.Checked;

		public bool AbortOnEncrypted => abortCheckBox.Checked;


		private void ChangePath(object sender, EventArgs e)
		{
			if (!initialized)
			{
				return;
			}

			var text = pathBox.Text.Trim();
			if (text.Length == 0)
			{
				errorLabel.Visible = false;
				okButton.Enabled = false;
				return;
			}

			try
			{
				var entries = text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				var ok = entries.Length > 0 && entries.All(entry => IsValidEntry(entry.Trim()));

				errorLabel.Visible = !ok;
				okButton.Enabled = ok;
			}
			catch
			{
				errorLabel.Visible = true;
				okButton.Enabled = false;
			}
		}


		private static bool IsValidEntry(string entry)
		{
			if (entry.Length == 0)
			{
				return false;
			}

			if (Directory.Exists(entry))
			{
				return true;
			}

			return PathHelper.HasWildFileName(entry)
				? Directory.GetFiles(Path.GetDirectoryName(entry), Path.GetFileName(entry)).Length > 0
				: File.Exists(entry);
		}


		private async void BrowseFile(object sender, EventArgs e)
		{
			try
			{
				// OpenFileDialog must run in an STA thread
				var paths = await SingleThreaded.Invoke(() =>
				{
					using var dialog = new OpenFileDialog
					{
						AddExtension = true,
						CheckFileExists = true,
						DefaultExt = ".enex",
						Filter = Resx.ImportEvernoteDialog_OpenFileFilter,
						InitialDirectory = GetInitialDirectory(),
						Multiselect = true,
						Title = Resx.ImportEvernoteDialog_OpenFileTitle
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						return dialog.FileNames;
					}

					return null;
				});

				if (paths is { Length: > 0 })
				{
					AppendPath(string.Join("|", paths));
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running OpenFileDialog", exc);
			}
		}


		private void BrowseFolder(object sender, EventArgs e)
		{
			try
			{
				string picked = null;
				var initial = GetInitialDirectory();

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using var dialog = new FolderBrowserDialog
					{
						Description = Resx.ImportEvernoteDialog_OpenFolderTitle,
						SelectedPath = initial
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						picked = dialog.SelectedPath;
					}
				})
				{
					Name = $"{nameof(ImportEvernoteDialog)}Thread"
				};

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				thread.Join();

				if (picked != null)
				{
					AppendPath(picked);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running FolderBrowserDialog", exc);
			}
		}


		private void AppendPath(string newEntry)
		{
			pathBox.Text = string.IsNullOrEmpty(pathBox.Text)
				? newEntry
				: $"{pathBox.Text}|{newEntry}";
		}


		private string GetInitialDirectory()
		{
			var first = pathBox.Text
				.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
				.FirstOrDefault()?.Trim();

			if (!string.IsNullOrEmpty(first))
			{
				if (Directory.Exists(first))
				{
					return first;
				}

				var dir = Path.GetDirectoryName(first);
				if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
				{
					return dir;
				}
			}

			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				var settings = new SettingsProvider();
				var collection = settings.GetCollection("ImportEvernote");

				var first = pathBox.Text
					.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
					.FirstOrDefault()?.Trim();

				if (!string.IsNullOrEmpty(first))
				{
					var dir = Directory.Exists(first) ? first : Path.GetDirectoryName(first);
					if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
					{
						collection.Add("path", dir);
					}
				}

				collection.Add("includeSubfolders", includeSubfoldersCheckBox.Checked);
				collection.Add("abortOnEncrypted", abortCheckBox.Checked);
				settings.SetCollection(collection);
				settings.Save();
			}

			base.OnClosing(e);
		}
	}
}
