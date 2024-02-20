//************************************************************************************************
// Copyright © 2023 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class QuickNotesSheet : SheetBase
	{
		private const string RightArrow = "\u2192";

		private bool eventing;
		private string notebookID;
		private string sectionID;


		public QuickNotesSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(QuickNotesSheet);
			Title = Resx.QuickNotesSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"notebookGroup=word_Notebook",
					"notebookButton",
					"notebookLink",
					"groupingLabel",
					"groupingBox",
					"sectionGroup=word_Section",
					"sectionButton",
					"sectionLink",
					"optionsGroup=word_Options",
					"titleBox",
					"stampBox"
				});
			}

			var settings = provider.GetCollection(Name);
			var organization = settings.Get("organization", "notebook");
			if (organization == "notebook")
			{
				notebookButton.Checked = true;
				sectionButton.Checked = false;
			}
			else
			{
				sectionButton.Checked = true;
				notebookButton.Checked = false;
			}

			notebookID = settings.Get<string>("notebookID");

			if (!string.IsNullOrWhiteSpace(notebookID))
			{
				Task.Run(async () => { await SetLinkName(notebookLink, notebookID); }).Wait();
			}

			groupingBox.SelectedIndex = settings.Get("grouping", 0);

			sectionID = settings.Get<string>("sectionID");

			if (!string.IsNullOrWhiteSpace(sectionID))
			{
				Task.Run(async () => { await SetLinkName(sectionLink, sectionID); }).Wait();
			}

			titleBox.Checked = settings.Get("titled", false);
			stampBox.Checked = settings.Get("stamped", false);

			eventing = true;
			ChangeOrgOption(notebookButton, null);
		}


		private async Task SetLinkName(LinkLabel link, string nodeID)
		{
			await using var one = new OneNote();

			try
			{
				if (link == notebookLink)
				{
					var node = one.GetHierarchyNode(nodeID);
					if (node != null)
					{
						link.Text = node.Name;
					}
				}
				else
				{
					var info = await one.GetSectionInfo(nodeID);
					if (info != null)
					{
						link.Text = info.Path.Substring(1).Replace("/", $" {RightArrow} ");
					}
				}
			}
			catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
			{
				logger.WriteLine("could not find expected quicknotes target");
			}
		}


		private void ChangeOrgOption(object sender, System.EventArgs e)
		{
			if (eventing)
			{
				eventing = false;
				if (sender == notebookButton && notebookButton.Checked)
				{
					notebookLink.Enabled = true;
					groupingBox.Enabled = true;
					sectionButton.Checked = false;
					sectionLink.Enabled = false;
				}
				else if (sectionButton.Checked)
				{
					sectionLink.Enabled = true;
					notebookButton.Checked = false;
					notebookLink.Enabled = false;
					groupingBox.Enabled = false;
				}
				eventing = true;
			}
		}


		private async void SelectNotebook(object sender, System.EventArgs e)
		{
			await SingleThreaded.Invoke(async () =>
			{
				await using var one = new OneNote();
				one.SelectLocation(
					Resx.QuickNotesSheet_SelectNotebookTitle,
					Resx.QuickNotesSheet_SelectNotebookIntro,
					OneNote.Scope.Notebooks,
					async (sourceID) =>
					{
						if (!string.IsNullOrEmpty(sourceID))
						{
							notebookID = sourceID;
							await SetLinkName(notebookLink, notebookID);
							await Task.Yield();
						}
					});
			});
		}


		private async void SelectSection(object sender, System.EventArgs e)
		{
			await SingleThreaded.Invoke(async () =>
			{
				await using var one = new OneNote();
				one.SelectLocation(
					Resx.QuickNotesSheet_SelectSectionTitle,
					Resx.QuickNotesSheet_SelectSectionIntro,
					OneNote.Scope.Sections,
					async (sourceID) =>
					{
						if (!string.IsNullOrEmpty(sourceID))
						{
							sectionID = sourceID;
							await SetLinkName(sectionLink, sectionID);
							await Task.Yield();
						}
					});
			});
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			if (notebookButton.Checked && !string.IsNullOrEmpty(notebookID))
			{
				settings.Add("organization", "notebook");
				settings.Add("notebookID", notebookID);
				settings.Add("grouping", groupingBox.SelectedIndex);
				settings.Remove("sectionID");
			}
			else if (!string.IsNullOrEmpty(sectionID))
			{
				settings.Add("organization", "section");
				settings.Add("sectionID", sectionID);
				settings.Remove("notebookID");
				settings.Remove("grouping");
			}

			if (titleBox.Checked)
				settings.Add("titled", "True");
			else
				settings.Remove("titled");

			if (stampBox.Checked)
				settings.Add("stamped", "True");
			else
				settings.Remove("stamped");

			provider.SetCollection(settings);

			// restart not required
			return false;
		}
	}
}
