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
		private readonly string clipboardObjectId;
		private readonly string clipboardPreviewText;


		public EmbedDialog(
			string sourcePath, string targetPath, string bookmarkText = null,
			string clipboardSourceId = null, string clipboardSourcePath = null,
			string clipboardObjectId = null, string clipboardPreviewText = null,
			bool isBookmark = true)
		{
			InitializeComponent();

			this.clipboardSourceId = clipboardSourceId;
			this.clipboardSourcePath = clipboardSourcePath;
			this.clipboardObjectId = clipboardObjectId;
			this.clipboardPreviewText = clipboardPreviewText;

			sourceNameLabel.Text = sourcePath;
			targetNameLabel.Text = targetPath;

			if (bookmarkText != null)
			{
				beginTagLabel.Visible = false;
				beginTagBox.Visible = false;
				endTagLabel.Visible = false;
				endTagBox.Visible = false;
				noteLabel.Visible = false;
				SetPreviewLabel(isBookmark);
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


		public string OverrideObjectId { get; private set; }


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
			ApplyOverrideSource(clipboardSourceId, clipboardSourcePath, clipboardObjectId, clipboardPreviewText);
		}


		/// <summary>
		/// Sets the wording of the preview section's caption. A real Bookmark (created via
		/// BookmarkCommand) says "Bookmark"; a paragraph inferred from a clipboard object-id
		/// link says "Paragraph" instead, so the dialog never implies a bookmark exists when
		/// none was ever created.
		/// </summary>
		private void SetPreviewLabel(bool isBookmark)
		{
			bookmarkLabel.Text = isBookmark
				? Resx.EmbedDialog_bookmarkLabel_Text
				: Resx.EmbedDialog_paragraphLabel_Text;
		}


		private void ApplyOverrideSource(
			string sourceId, string sourcePath, string objectId = null, string previewText = null)
		{
			// the picker callback can arrive on the dedicated STA thread spun up by
			// SingleThreaded rather than this dialog's own thread
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => ApplyOverrideSource(sourceId, sourcePath, objectId, previewText)));
				return;
			}

			OverrideSourceId = sourceId;
			OverrideObjectId = objectId;
			sourceNameLabel.Text = sourcePath;

			if (!string.IsNullOrEmpty(objectId))
			{
				// the override source is itself a paragraph link, not a real bookmark;
				// stay in (or switch into) paragraph mode rather than prompting for
				// begin/end tags
				BookmarkCleared = false;

				var text = previewText ?? sourcePath;
				SetPreviewLabel(isBookmark: false);
				bookmarkLabel.Visible = true;
				bookmarkTextLabel.Text = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
				bookmarkTextLabel.Visible = true;
				useClipboardLink.Visible = false;

				beginTagLabel.Visible = false;
				beginTagBox.Visible = false;
				endTagLabel.Visible = false;
				endTagBox.Visible = false;
				noteLabel.Visible = false;
			}
			else if (bookmarkLabel.Visible)
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
