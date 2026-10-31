//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
			ClientSize = new Size(443, 330);
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

			var okButton = AddButton("okButton", Resx.word_OK, DialogResult.OK, 184, 6);
			var cancelButton = AddButton("cancelButton", Resx.word_Cancel, DialogResult.Cancel, 310, 7);

			Controls.Add(introLabel);
			AcceptButton = okButton;
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
				Location = new Point(left, 270),
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
