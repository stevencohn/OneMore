//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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


		private async void ImportWebDialog_Load(object sender, EventArgs e)
		{
			var text = await new ClipboardProvider().GetText();
			if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
			{
				addressBox.Text = text;
			}
		}


		private void addressBox_TextChanged(object sender, EventArgs e)
		{
			okButton.Enabled = Uri.IsWellFormedUriString(addressBox.Text.Trim(), UriKind.Absolute);
		}
	}
}
