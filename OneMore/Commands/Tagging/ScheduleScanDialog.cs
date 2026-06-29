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
		private readonly MoreCheckList booksList;
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
				booksList = new MoreCheckList
				{
					Dock = DockStyle.Fill,
					HeaderStyle = ColumnHeaderStyle.None,
					MultiSelect = false,
					SelectedBackColorKey = "LinkHighlight",
					SelectedForeColorKey = "ControlText"
				};

				booksList.Columns.Add(string.Empty);
				booksList.SetColumnProportions(1f);
				booksList.CheckChanged += DoSelectionsChanged;
				booksPanel.Controls.Add(booksList);
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

			if (booksList is not null)
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

				booksList.BeginUpdate();
				foreach (var book in books.Elements(ns + "Notebook"))
				{
					var id = book.Attribute("ID").Value;

					var unscanned = scannedIDs is null || !scannedIDs.Contains(id);

					var isChecked =
						(preferredIDs is not null && preferredIDs.Contains(id)) ||
						unscanned;

					var name = book.Attribute("name").Value;
					if (unscanned)
					{
						name = $"{name} ({Resx.ScheduleScanDialog_neverScanned})";
					}

					booksList.Items.Add(new ListViewItem(name) { Tag = id, Checked = isChecked });

					if (isChecked)
					{
						anyChecked = true;
					}
				}
				booksList.EndUpdate();

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
			return booksList is null
				? new string[0]
				: booksList.Items.Cast<ListViewItem>()
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


		private void DoSelectionsChanged(object sender, MoreCheckList.CheckChangedEventArgs e)
		{
			okButton.Enabled = booksList.Items.Cast<ListViewItem>().Any(b => b.Checked);
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
			foreach (ListViewItem item in booksList.Items)
			{
				item.Checked = !scannedIDs.Contains((string)item.Tag);
			}
			booksList.Invalidate();
			okButton.Enabled = booksList.Items.Cast<ListViewItem>().Any(b => b.Checked);
			FocusButtons();
		}


		private void SelectAllNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in booksList.Items)
			{
				item.Checked = true;
			}
			booksList.Invalidate();
			okButton.Enabled = booksList.Items.Count > 0;
			FocusButtons();
		}


		private void SelectNoneNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in booksList.Items)
			{
				item.Checked = false;
			}
			booksList.Invalidate();
			okButton.Enabled = false;
			FocusButtons();
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
