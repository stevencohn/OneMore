//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ScheduleScanDialog : MoreForm
	{
		#region Private classes
		private sealed class NotebookList : MoreFlowLayoutPanel
		{
			public NotebookList()
			{
				FlowDirection = FlowDirection.LeftToRight;
				AutoScroll = true;
				WrapContents = true;
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
					Width = Width - SystemInformation.VerticalScrollBarWidth
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


		private readonly NotebookList bookList;
		private List<string> knownNotebookIDs;


		/// <summary>
		/// Constructor for visual designer; use the other constructors!
		/// </summary>
		public ScheduleScanDialog()
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
		}


		public ScheduleScanDialog(string[] notebookIDs)
			: this()
		{
			dateTimePicker.Value = DateTime.Today.AddDays(1);
			knownNotebookIDs = notebookIDs.ToList();

			if (notebookIDs.Any())
			{
				bookList = new NotebookList
				{
					Dock = DockStyle.Fill
				};

				bookList.SelectionsChanged += DoSelectionsChanged;

				booksPanel.Controls.Add(bookList);

				AlignHyperlinks();
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


		public ScheduleScanDialog(string[] notebookIDs, DateTime time)
			: this(notebookIDs)
		{
			dateTimePicker.Value = time;
		}


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


		public string[] PreferredNotebooks { get; set; }


		protected override async void OnLoad(EventArgs e)
		{
			if (notebooksPanel.Visible)
			{
				if (HashtagProvider.DatabaseExists())
				{
					using var provider = new HashtagProvider();
					knownNotebookIDs = provider.ReadNotebookIDs();
				}
				else
				{
					knownNotebookIDs = new List<string>();
				}

				await using var one = new OneNote();
				var books = await one.GetNotebooks();
				var ns = one.GetNamespace(books);

				foreach (var book in books.Elements(ns + "Notebook"))
				{
					var id = book.Attribute("ID").Value;

					var isChecked =
						(PreferredNotebooks is not null && PreferredNotebooks.Contains(id)) ||
						!knownNotebookIDs.Contains(id);

					bookList.AddNotebook(
						book.Attribute("name").Value, id, isChecked);
				}

				if (!okButton.Enabled)
				{
					cancelButton.Focus();
				}
			}

			base.OnLoad(e);
		}


		public DateTime StartTime => nowRadio.Checked
			? DateTime.Now
			// DateTimePicker returns dates with Unspecified Kind, so force Local
			: DateTime.SpecifyKind(dateTimePicker.Value, DateTimeKind.Local);


		public string[] GetSelectedNotebooks()
		{
			return bookList?.Controls.Cast<MoreCheckBox>()
				.Where(c => c.Checked)
				.Select(c => c.Tag.ToString())
				.ToArray()
				?? new string[0];
		}


		public void SetIntroText(string text)
		{
			introBox.Text = text;
		}


		private void DoSelectionsChanged(object sender, EventArgs e)
		{
			okButton.Enabled = bookList.Controls.Cast<MoreCheckBox>().Any(b => b.Checked);

			if (okButton.Enabled)
			{
				okButton.Focus();
			}
			else
			{
				cancelButton.Focus();
			}
		}


		private void ResetSelections(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in bookList.Controls)
			{
				box.Checked = !knownNotebookIDs.Contains((string)box.Tag);
			}
		}


		private void SelectAllNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in bookList.Controls)
			{
				box.Checked = true;
			}
		}


		private void SelectNoneNotebooks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (MoreCheckBox box in bookList.Controls)
			{
				box.Checked = false;
			}
		}
	}
}
