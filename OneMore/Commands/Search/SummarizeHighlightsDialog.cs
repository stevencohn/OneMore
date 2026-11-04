//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts for the scope of the Summarize Highlights report.
	/// </summary>
	internal class SummarizeHighlightsDialog : UI.MoreForm
	{
		private readonly UI.MoreRadioButton pageRadio;
		private readonly UI.MoreRadioButton sectionRadio;
		private readonly UI.MoreRadioButton groupRadio;
		private readonly UI.MoreRadioButton notebookRadio;
		private readonly UI.MoreRadioButton notebooksRadio;
		private readonly UI.MoreComboBox dateSelector;
		private readonly UI.MoreDateTimePicker datePicker;
		private readonly UI.MorePictureBox colorBox;
		private readonly UI.MoreButton clearButton;
		private Color highlightColor = Color.Empty;

		/// <param name="allowPage">
		/// False to disable the page choice, such as when the current page is a report
		/// </param>
		/// <param name="allowGroup">
		/// False to disable the section group choice, such as when the current section is
		/// not within a section group
		/// </param>
		public SummarizeHighlightsDialog(bool allowPage = true, bool allowGroup = true)
		{
			SuspendLayout();

			AutoScaleDimensions = new SizeF(9F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.ControlLight;
			ForeColor = SystemColors.ControlText;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(443, 454);
			Padding = new Padding(20, 20, 10, 10);
			StartPosition = FormStartPosition.Manual;
			Text = Resx.SummarizeHighlightsDialog_Text;

			var introLabel = new UI.MoreLabel
			{
				AutoSize = true,
				Location = new Point(23, 20),
				Name = "introLabel",
				Text = Resx.SummarizeHighlightsDialog_Intro
			};

			pageRadio = AddRadio("pageRadio", Resx.SummarizeHighlightsDialog_Page, 56, 1);
			sectionRadio = AddRadio("sectionRadio", Resx.phrase_TheCurrentSection, 90, 2);
			groupRadio = AddRadio("groupRadio", Resx.SummarizeHighlightsDialog_Group, 124, 3);
			notebookRadio = AddRadio(
				"notebookRadio", Resx.phrase_AllSectionInTheCurrentNotebook, 158, 4);
			notebooksRadio = AddRadio("notebooksRadio", Resx.phrase_AllNotebooks, 192, 5);
			pageRadio.Enabled = allowPage;
			groupRadio.Enabled = allowGroup;
			sectionRadio.Checked = true;

			var filterLabel = new UI.MoreLabel
			{
				AutoSize = true,
				Location = new Point(23, 244),
				Name = "filterLabel",
				Text = Resx.SummarizeHighlightsDialog_FilterBy
			};

			dateSelector = new UI.MoreComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				FormattingEnabled = true,
				Location = new Point(23, 272),
				Name = "dateSelector",
				Size = new Size(190, 28),
				TabIndex = 6
			};

			dateSelector.Items.AddRange(new object[]
			{
				Resx.SummarizeHighlightsDialog_AllDates,
				Resx.SummarizeHighlightsDialog_ModifiedAfter,
				Resx.SummarizeHighlightsDialog_ModifiedBefore
			});

			dateSelector.SelectedIndex = 0;
			dateSelector.SelectedIndexChanged += ChangeDateSelector;

			datePicker = new UI.MoreDateTimePicker
			{
				Enabled = false,
				Format = DateTimePickerFormat.Short,
				Location = new Point(228, 272),
				MaxDate = DateTime.Today.AddDays(1),
				MinDate = new DateTime(1990, 1, 1),
				Name = "datePicker",
				Size = new Size(190, 26),
				TabIndex = 7,
				Value = DateTime.Today
			};

			var colorLabel = new UI.MoreLabel
			{
				AutoSize = true,
				Location = new Point(23, 324),
				Name = "colorLabel",
				Text = Resx.SummarizeHighlightsDialog_Color
			};

			colorBox = new UI.MorePictureBox
			{
				BorderStyle = BorderStyle.FixedSingle,
				Location = new Point(228, 320),
				Name = "colorBox",
				Size = new Size(160, 28),
				TabIndex = 8,
				TabStop = false
			};

			colorBox.Click += ChooseColor;
			colorBox.Paint += PaintColorBox;

			clearButton = new UI.MoreButton
			{
				BackColor = Color.FromArgb(231, 231, 231),
				ForeColor = Color.Black,
				Location = new Point(394, 320),
				Name = "clearButton",
				ShowBorder = true,
				Size = new Size(28, 28),
				TabIndex = 9,
				Text = "×",
				UseVisualStyleBackColor = true,
				Enabled = false
			};

			clearButton.Click += ClearColor;

			var okButton = AddButton("okButton", Resx.word_OK, DialogResult.OK, 184, 10);
			var cancelButton = AddButton("cancelButton", Resx.word_Cancel, DialogResult.Cancel, 310, 11);

			Controls.Add(introLabel);
			Controls.Add(filterLabel);
			Controls.Add(dateSelector);
			Controls.Add(datePicker);
			Controls.Add(colorLabel);
			Controls.Add(colorBox);
			Controls.Add(clearButton);
			AcceptButton = okButton;
			ActiveControl = okButton;
			CancelButton = cancelButton;

			ResumeLayout(false);
			PerformLayout();
		}


		public SummarizeHighlightsCommand.HighlightScope Scope
		{
			get
			{
				if (pageRadio.Checked) return SummarizeHighlightsCommand.HighlightScope.Page;
				if (groupRadio.Checked) return SummarizeHighlightsCommand.HighlightScope.SectionGroup;
				if (notebookRadio.Checked) return SummarizeHighlightsCommand.HighlightScope.Notebook;
				if (notebooksRadio.Checked) return SummarizeHighlightsCommand.HighlightScope.Notebooks;
				return SummarizeHighlightsCommand.HighlightScope.Section;
			}
		}


		public SummarizeHighlightsCommand.DateFilter Filter
		{
			get
			{
				return dateSelector.SelectedIndex switch
				{
					1 => SummarizeHighlightsCommand.DateFilter.After,
					2 => SummarizeHighlightsCommand.DateFilter.Before,
					_ => SummarizeHighlightsCommand.DateFilter.None
				};
			}
		}


		public DateTime FilterDate => datePicker.Value.Date;


		/// <summary>
		/// Gets the chosen highlight color, or Color.Empty to include all colors.
		/// </summary>
		public Color HighlightColor => highlightColor;


		private void ChangeDateSelector(object sender, EventArgs e)
		{
			datePicker.Enabled = dateSelector.SelectedIndex > 0;
		}


		private void ChooseColor(object sender, EventArgs e)
		{
			var location = colorBox.PointToScreen(new Point(0, colorBox.Height));

			using var picker = new UI.MoreColorPicker(Color.Empty,
				location.X, location.Y, Resx.SummarizeHighlightsDialog_Color)
			{
				Color = highlightColor.IsEmpty ? Color.Yellow : highlightColor,
				AutomaticVisible = false
			};

			if (picker.ShowDialog(this) == DialogResult.OK)
			{
				highlightColor = picker.Color;
				clearButton.Enabled = true;
				colorBox.BackColor = UI.MoreColorPicker.IsDarkPage(Color.Empty)
					? UI.ColorPalette.ToDark(highlightColor)
					: highlightColor;

				colorBox.Refresh();
			}
		}


		private void ClearColor(object sender, EventArgs e)
		{
			highlightColor = Color.Empty;
			clearButton.Enabled = false;
			colorBox.ResetBackColor();
			colorBox.Refresh();
		}


		// with no color chosen, the box reads "All colors"
		private void PaintColorBox(object sender, PaintEventArgs e)
		{
			if (highlightColor.IsEmpty)
			{
				TextRenderer.DrawText(e.Graphics, Resx.SummarizeHighlightsDialog_AllColors,
					colorBox.Font, colorBox.ClientRectangle, ForeColor,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
					TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
			}
		}


		private UI.MoreRadioButton AddRadio(string name, string text, int top, int index)
		{
			// AutoSize so the focus outline hugs the label text instead of the dialog width
			var radio = new UI.MoreRadioButton
			{
				AutoSize = true,
				Cursor = Cursors.Hand,
				Location = new Point(23, top),
				Name = name,
				TabIndex = index,
				TabStop = true,
				Text = text,
				UseVisualStyleBackColor = true
			};

			Controls.Add(radio);
			return radio;
		}


		private UI.MoreButton AddButton(
			string name, string text, DialogResult result, int left, int index)
		{
			var button = new UI.MoreButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				BackColor = Color.FromArgb(231, 231, 231),
				DialogResult = result,
				ForeColor = Color.Black,
				Location = new Point(left, 394),
				Name = name,
				ShowBorder = true,
				Size = new Size(120, 38),
				TabIndex = index,
				Text = text,
				UseVisualStyleBackColor = true
			};

			Controls.Add(button);
			return button;
		}
	}
}
