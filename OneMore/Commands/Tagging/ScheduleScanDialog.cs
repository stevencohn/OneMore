//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Presents to the user options to schedule a scan of selected notebooks.
	/// </summary>
	internal partial class ScheduleScanDialog : MoreForm
	{
		#region Private classes
		private sealed class NotebooksPanel : MoreFlowLayoutPanel
		{
			public NotebooksPanel()
			{
				FlowDirection = FlowDirection.TopDown;
				AutoScroll = true;
				WrapContents = false;
			}


			public event EventHandler SelectionsChanged;

			public void AddNotebook(string name, string notebookID, bool isChecked)
			{
				var box = new MoreCheckBox
				{
					Checked = isChecked,
					Padding = new Padding(4, 2, 10, 2),
					Tag = notebookID,
					Text = name,
					Width = Width - SystemInformation.VerticalScrollBarWidth * 2
				};

				box.CheckedChanged += DoChangeSelections;

				Controls.Add(box);
			}

			private void DoChangeSelections(object sender, EventArgs e)
			{
				SelectionsChanged?.Invoke(sender, e);
			}
		}
		#endregion Private classes


		private readonly NotebooksPanel booksFlow;
		private List<string> scannedIDs;
		private List<string> preferredIDs;


		/// <summary>
		/// Create a new schedule, starting midnight tonight, optionally allowing the user
		/// to select notebooks to scan.
		/// </summary>
		/// <param name="showNotebooks">
		/// True to show the notebooks selection panel; false to hide it.
		/// </param>
		public ScheduleScanDialog(bool showNotebooks)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ScheduleScanDialog_Title;

				Localize(new string[]
				{
					"introBox",
					"laterRadio",
					"hintLabel",
					"scheduleLabel=word_Schedule",
					"nowRadio",
					"warningLabel",
					"notebooksLabel=word_Notebooks",
					"selectAllLink=phrase_SelectAll",
					"selectNoneLink=phrase_SelectNone",
					"resetLink=word_Reset",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			dateTimePicker.Value = DateTime.Today.AddDays(1);

			if (showNotebooks)
			{
				booksFlow = new NotebooksPanel
				{
					Dock = DockStyle.Fill
				};

				booksFlow.SelectionsChanged += DoSelectionsChanged;
				booksPanel.Controls.Add(booksFlow);
				okButton.Enabled = false;
			}
			else
			{
				var height = Height - notebooksPanel.Height;
				MinimumSize = new System.Drawing.Size(MinimumSize.Width, height);
				MaximumSize = MinimumSize;
				Height = height;
				notebooksPanel.Visible = false;
			}
		}


		/// <summary>
		/// Use this constructor when modifying an existing schedule with a preset start time.
		/// </summary>
		/// <param name="showNotebooks">
		/// True to show the notebooks selection panel; false to hide it.
		/// </param>
		/// <param name="time">
		/// The start time of the existing schedule to be modified.
		/// </param>
		public ScheduleScanDialog(bool showNotebooks, DateTime time)
			: this(showNotebooks)
		{
			dateTimePicker.Value = time;
		}


		public DateTime StartTime => nowRadio.Checked
			? DateTime.Now
			// DateTimePicker returns dates with Unspecified Kind, so force Local
			: DateTime.SpecifyKind(dateTimePicker.Value, DateTimeKind.Local);


		#region Load Helpers
		private void AlignHyperlinks()
		{
			var resetSize = TextRenderer.MeasureText(resetLink.Text, resetLink.Font);
			resetLink.Left = notebooksPanel.Width - notebooksPanel.Padding.Right - resetSize.Width;

			var sep2Size = TextRenderer.MeasureText(sep2.Text, sep2.Font);
			sep2.Left = resetLink.Left - resetLink.Margin.Left - sep2Size.Width - sep2.Margin.Right;

			var noneSize = TextRenderer.MeasureText(selectNoneLink.Text, selectNoneLink.Font);
			selectNoneLink.Left = sep2.Left - sep2.Margin.Left - noneSize.Width - selectNoneLink.Margin.Right;

			var sep1Size = TextRenderer.MeasureText(sep1.Text, sep1.Font);
			sep1.Left = selectNoneLink.Left - selectNoneLink.Margin.Left - sep1Size.Width - sep1.Margin.Right;

			var allSize = TextRenderer.MeasureText(selectAllLink.Text, selectAllLink.Font);
			selectAllLink.Left = sep1.Left - sep1.Margin.Left - allSize.Width - selectAllLink.Margin.Right;
		}

		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AlignHyperlinks();

			if (booksFlow is not null)
			{
				if (HashtagProvider.CatalogExists())
				{
					using var provider = new HashtagProvider();
					scannedIDs = provider.ReadTaggedNotebookIDs();
				}

				await using var one = new OneNote();
				var books = await one.GetNotebooks();
				var ns = one.GetNamespace(books);
				var anyChecked = false;

				foreach (var book in books.Elements(ns + "Notebook"))
				{
					var id = book.Attribute("ID").Value;

					var isChecked =
						(preferredIDs is not null && preferredIDs.Contains(id)) ||
						scannedIDs is null || !scannedIDs.Contains(id);

					booksFlow.AddNotebook(
						book.Attribute("name").Value, id, isChecked);

					if (isChecked)
					{
						anyChecked = true;
					}
				}

				okButton.Enabled = anyChecked;
			}

			FocusButtons();
		}
		#endregion Load Helpers


		/// <summary>
		/// Gets the list of selected notebook IDs
		/// </summary>
		/// <returns>An array of notebook IDs</returns>
		public string[] GetSelectedNotebooks()
		{
			return booksFlow is null
				? new string[0]
				: booksFlow.Controls.Cast<MoreCheckBox>()
					.Where(c => c.Checked).Select(c => c.Tag.ToString()).ToArray();
		}


		/// <summary>
		/// Override the default intro text with customized text.
		/// </summary>
		/// <param name="text"></param>
		public void SetIntroText(string text)
		{
			introBox.Text = text;
		}


		/// <summary>
		/// Sets the list of preferred (selected) notebook IDs. These are chosen by the user
		/// as targets for a full [re]scan. They can be either known or new notebooks.
		/// </summary>
		/// <param name="preferredIDs"></param>
		public void SetPreferredIDs(string[] preferredIDs)
		{
			this.preferredIDs = preferredIDs.ToList();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Handlers...

		private void DoCheckedChanged(object sender, EventArgs e)
		{
			dateTimePicker.Enabled = laterRadio.Checked;
			FocusButtons();
		}


		private void DoSelectionsChanged(object sender, EventArgs e)
		{
			okButton.Enabled = booksFlow.Controls.Cast<MoreCheckBox>().Any(b => b.Checked);
			FocusButtons();
		}


		private void FocusButtons()
		{
			if (okButton.Enabled)
			{
				AcceptButton = okButton;
				okButton.Focus();
			}
			else
			{
				AcceptButton = cancelButton;
				cancelButton.Focus();
			}
		}


		private void ResetSelections(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in booksFlow.Controls)
			{
				box.Checked = !scannedIDs.Contains((string)box.Tag);
			}
		}


		private void SelectAllNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in booksFlow.Controls)
			{
				box.Checked = true;
			}
		}


		private void SelectNoneNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in booksFlow.Controls)
			{
				box.Checked = false;
			}
		}


		private void CheckOptionsOnFormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.Cancel || !nowRadio.Checked)
			{
				return;
			}

			var result = MoreMessageBox.Show(this,
				Resx.ScheduleScanDialog_nowWarning,
				MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			if (result == DialogResult.Cancel)
			{
				e.Cancel = true;
			}
		}
	}
}
