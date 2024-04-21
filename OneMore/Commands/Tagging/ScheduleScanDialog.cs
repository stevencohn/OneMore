﻿//************************************************************************************************
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

				Controls.Add(box);
			}
		}
		#endregion Private classes


		private readonly NotebookList bookList;


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
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			dateTimePicker.Value = DateTime.Today.AddDays(1);

			bookList = new NotebookList
			{
				Dock = DockStyle.Fill
			};

			booksPanel.Controls.Add(bookList);

		}


		public ScheduleScanDialog(DateTime time)
			: this()
		{
			dateTimePicker.Value = time;
		}


		public string[] PreferredNotebooks { get; set; }


		protected override async void OnLoad(EventArgs e)
		{
			List<string> knownIDs;
			if (HashtagProvider.DatabaseExists())
			{
				using var provider = new HashtagProvider();
				knownIDs = provider.ReadNotebookIDs();
			}
			else
			{
				knownIDs = new List<string>();
			}

			await using var one = new OneNote();
			var books = await one.GetNotebooks();
			var ns = one.GetNamespace(books);

			foreach (var book in books.Elements(ns + "Notebook"))
			{
				var id = book.Attribute("ID").Value;

				var isChecked = 
					(PreferredNotebooks is not null && PreferredNotebooks.Contains(id)) ||
					!knownIDs.Contains(id);

				bookList.AddNotebook(
					book.Attribute("name").Value, id, isChecked);
			}

			base.OnLoad(e);
		}


		public DateTime StartTime => nowRadio.Checked
			? DateTime.Now
			// DateTimePicker returns dates with Unspecified Kind, so force Local
			: DateTime.SpecifyKind(dateTimePicker.Value, DateTimeKind.Local);


		public string[] GetSelectedNotebooks()
		{
			return bookList.Controls.Cast<MoreCheckBox>()
				.Where(c => c.Checked)
				.Select(c => c.Tag.ToString())
				.ToArray();
		}


		public void SetIntroText(string text)
		{
			introBox.Text = text;
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
