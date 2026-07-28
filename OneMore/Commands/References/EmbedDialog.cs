//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal enum EmbedFormat { Formatted, PlainText }
	internal enum EmbedStyle { Normal, Italic, Gray, Quote, Citation }


	internal partial class EmbedDialog : MoreForm
	{
		private readonly string clipboardSourceId;
		private readonly string clipboardSourcePath;


		public EmbedDialog(
			string sourcePath, string targetPath, string bookmarkText = null,
			string clipboardSourceId = null, string clipboardSourcePath = null)
		{
			InitializeComponent();

			this.clipboardSourceId = clipboardSourceId;
			this.clipboardSourcePath = clipboardSourcePath;

			sourceNameLabel.Text = sourcePath;
			targetNameLabel.Text = targetPath;

			if (bookmarkText != null)
			{
				beginTagLabel.Visible = false;
				beginTagBox.Visible = false;
				endTagLabel.Visible = false;
				endTagBox.Visible = false;
				noteLabel.Visible = false;
				bookmarkLabel.Visible = true;
				bookmarkTextLabel.Text = bookmarkText.Length > 50
					? bookmarkText.Substring(0, 50) + "..."
					: bookmarkText;
				bookmarkTextLabel.Visible = true;
				useClipboardLink.Visible = clipboardSourceId != null;
			}
			else
			{
				SetNote(null, null);
			}

			if (NeedsLocalizing())
			{
				Text = Resx.EmbedDialog_Title;

				Localize(new string[]
				{
					"sourceLabel=word_Source",
					"targetLabel=word_Target",
					"beginTagLabel",
					"endTagLabel",
					"bookmarkLabel",
					"useClipboardLink",
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


		public bool BookmarkCleared { get; private set; }


		public string OverrideSourceId { get; private set; }


		public string BeginTag => beginTagBox.Text.Trim();


		public string EndTag => endTagBox.Text.Trim();


		public EmbedFormat Format =>
			formattedRadio.Checked ? EmbedFormat.Formatted : EmbedFormat.PlainText;


		public EmbedStyle Style => (EmbedStyle)styleBox.SelectedIndex;


		private void ToggleStyle(object sender, EventArgs e)
		{
			stylePanel.Visible = plaintextRadio.Checked;
		}


		private async void SourceNameLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// The ribbon command thread is MTA; the native QuickFiling dialog (and its
			// async OnDialogClosed callback) requires a genuine STA thread to avoid
			// hanging the COM surrogate, so this must be routed through SingleThreaded,
			// matching QuickNotesSheet.SelectNotebook.
			await SingleThreaded.Invoke(async () =>
			{
				// SelectLocation shows the native QuickFiling dialog and returns immediately;
				// this OneNote instance is only needed to launch it. The callback opens its
				// own fresh instance.
				await using var one = new OneNote();
				one.SelectLocation(
					Resx.EmbedCommand_Select,
					Resx.EmbedCommand_SelectIntro,
					OneNote.Scope.Pages,
					async (sourceId) =>
					{
						if (string.IsNullOrEmpty(sourceId))
						{
							return;
						}

						await using var o = new OneNote();
						var info = await o.GetPageInfo(sourceId);
						var path = EmbedCommand.FormatPath(info?.Path) ?? sourceId;

						ApplyOverrideSource(sourceId, path);
					},
					leaf: true);
			});
		}


		private void UseClipboardLink(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ApplyOverrideSource(clipboardSourceId, clipboardSourcePath);
		}


		private void ApplyOverrideSource(string sourceId, string sourcePath)
		{
			// the picker callback can arrive on the dedicated STA thread spun up by
			// SingleThreaded rather than this dialog's own thread
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => ApplyOverrideSource(sourceId, sourcePath)));
				return;
			}

			OverrideSourceId = sourceId;
			sourceNameLabel.Text = sourcePath;

			if (bookmarkLabel.Visible)
			{
				BookmarkCleared = true;

				bookmarkLabel.Visible = false;
				bookmarkTextLabel.Visible = false;
				useClipboardLink.Visible = false;

				beginTagLabel.Visible = true;
				beginTagBox.Visible = true;
				endTagLabel.Visible = true;
				endTagBox.Visible = true;
				noteLabel.Visible = true;

				SetNote(null, null);
			}
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
