//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal enum EmbedFormat { Formatted, PlainText }
	internal enum EmbedStyle { Normal, Italic, Gray, Quote, Citation }


	internal partial class EmbedDialog : MoreForm
	{
		public EmbedDialog(string sourceName, string targetName)
		{
			InitializeComponent();

			sourceNameLabel.Text = sourceName;
			targetNameLabel.Text = targetName;

			SetNote(null, null);

			if (NeedsLocalizing())
			{
				Text = Resx.EmbedDialog_Title;

				Localize(new string[]
				{
					"sourceLabel=word_Source",
					"targetLabel=word_Target",
					"beginTagLabel",
					"endTagLabel",
					"formatLabel=word_Format",
					"formattedRadio",
					"plaintextRadio",
					"styleLabel",
					"styleBox",
					"indentCheck",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool Indent => indentCheck.Checked;


		public string BeginTag => beginTagBox.Text.Trim();


		public string EndTag => endTagBox.Text.Trim();


		public EmbedFormat Format =>
			formattedRadio.Checked ? EmbedFormat.Formatted : EmbedFormat.PlainText;


		public EmbedStyle Style => (EmbedStyle)styleBox.SelectedIndex;


		private void ToggleStyle(object sender, EventArgs e)
		{
			stylePanel.Visible = plaintextRadio.Checked;
		}

		private void SetNote(object sender, EventArgs e)
		{
			var beginTag = beginTagBox.Text.Trim();
			var endTag = endTagBox.Text.Trim();

			if (beginTag.Length == 0 && endTag.Length == 0)
			{
				noteLabel.Text = Resx.EmbedDialog_noteLabel_FullPage;
			}
			else if (beginTag.Length > 0 && endTag.Length > 0)
			{
				noteLabel.Text = string.Format(Resx.EmbedDialog_noteLabel_Between, beginTag, endTag);
			}
			else if (beginTag.Length > 0)
			{
				noteLabel.Text = string.Format(Resx.EmbedDialog_noteLabel_After, beginTag);
			}
			else
			{
				noteLabel.Text = string.Format(Resx.EmbedDialog_noteLabel_Before, endTag);
			}
		}
	}
}
