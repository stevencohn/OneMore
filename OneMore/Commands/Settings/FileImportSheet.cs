//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class FileImportSheet : SheetBase
	{
		private const decimal DefaultWidth = 600M;
		private const string RightArrow = "→";

		private string sectionID;


		public FileImportSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(FileImportSheet);
			Title = Resx.FileImportSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"ppGroup",
					"widthLabel",
					"quickGroup",
					"quickIntroLabel",
					"folderLabel=word_Folder",
					"sectionLabel=word_Section",
					"sectionLink"
				});
			}

			var settings = provider.GetCollection(Name);

			widthBox.Value = settings.Get("width", DefaultWidth);

			folderBox.Text = settings.Get<string>("quickImportFolder");

			sectionID = settings.Get<string>("quickImportSectionID");
			if (!string.IsNullOrWhiteSpace(sectionID))
			{
				Task.Run(async () => { await SetLinkName(sectionID); }).Wait();
			}
		}


		private void BrowseFolder(object sender, EventArgs e)
		{
			try
			{
				var path = folderBox.Text;

				// FolderBrowserDialog must run in an STA thread
				var thread = new Thread(() =>
				{
					using var dialog = new FolderBrowserDialog()
					{
						Description = Resx.FileImportSheet_SelectFolderText,
						SelectedPath = path
					};

					// cannot use owner parameter here or it will hang! cross-threading
					if (dialog.ShowDialog(/* leave empty */) == DialogResult.OK)
					{
						path = dialog.SelectedPath;
					}
				})
				{
					Name = $"{nameof(FileImportSheet)}Thread"
				};

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				thread.Join();

				folderBox.Text = path;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running FolderBrowserDialog", exc);
			}
		}


		private async void SelectSection(object sender, EventArgs e)
		{
			await SingleThreaded.Invoke(async () =>
			{
				await using var one = new OneNote();
				one.SelectLocation(
					Resx.FileImportSheet_SelectSectionTitle,
					Resx.FileImportSheet_SelectSectionIntro,
					OneNote.Scope.Sections,
					async (sourceID) =>
					{
						if (!string.IsNullOrEmpty(sourceID))
						{
							sectionID = sourceID;
							await SetLinkName(sectionID);
							await Task.Yield();
						}
					});
			});
		}


		private async Task SetLinkName(string nodeID)
		{
			await using var one = new OneNote();

			try
			{
				var info = await one.GetSectionInfo(nodeID);
				if (info != null)
				{
					sectionLink.Text = info.Path.Substring(1).Replace("/", $" {RightArrow} ");
				}
			}
			catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
			{
				logger.WriteLine("could not find expected quick-import target section");
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("width", widthBox.Value.ToString());

			settings.Add("quickImportFolder", folderBox.Text.Trim());

			if (!string.IsNullOrEmpty(sectionID))
			{
				settings.Add("quickImportSectionID", sectionID);
			}
			else
			{
				settings.Remove("quickImportSectionID");
			}

			provider.SetCollection(settings);

			// restart not required
			return false;
		}
	}
}
