//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using Win = System.Windows;


	internal enum ImportWebTarget
	{
		Append,
		NewPage,
		ChildPage
	}


	internal partial class ImportWebDialog : UI.LocalizableForm
	{
		public ImportWebDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ImportWebDialog_Text;

				Localize(new string[]
				{
					"addressLabel",
					"newPageButton",
					"newChildButton",
					"appendButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			using (var one = new OneNote(out var page, out _))
			{
				if (page == null)
				{
					newChildButton.Enabled = false;
					appendButton.Enabled = false;
				}
			}
		}


		public string Address => addressBox.Text;


		public bool Experimental => experimentButton.Checked;


		public bool ImportImages => imagesBox.Checked;


		public ImportWebTarget Target
		{
			get
			{
				if (appendButton.Checked) return ImportWebTarget.Append;
				if (newChildButton.Checked) return ImportWebTarget.ChildPage;
				return ImportWebTarget.NewPage;
			}
		}


		private async void ImportWebDialog_Load(object sender, System.EventArgs e)
		{
			var clipboard = await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Text)
					? Win.Clipboard.GetText(Win.TextDataFormat.Text)
					: null;
			});

			if (Uri.IsWellFormedUriString(clipboard, UriKind.Absolute))
			{
				addressBox.Text = clipboard;
			}
		}


		private void addressBox_TextChanged(object sender, EventArgs e)
		{
			okButton.Enabled = Uri.IsWellFormedUriString(addressBox.Text.Trim(), UriKind.Absolute);
		}

		private void SetAvailableOptions(object sender, EventArgs e)
		{
			experimentButton.Enabled = !imagesBox.Checked;
		}
	}
}
