//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using Resx = Properties.Resources;


	internal partial class ArrangeContainersDialog : UI.LocalizableForm
	{
		public ArrangeContainersDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ArrangeContainersDialog_Text;

				Localize(new string[]
				{
					"verticalButton",
					"setWidthCheckBox=word_Width",
					"flowButton",
					"columnsLabel",
					"widthLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			var settings = new SettingsProvider().GetCollection("containers");
			setWidthBox.Value = settings.Get("width", 500);
		}


		public bool Vertical => verticalButton.Checked;


		public int Columns => (int)columnsBox.Value;


		public int PageWidth => verticalButton.Checked
			? (setWidthCheckBox.Checked ? (int)setWidthBox.Value : 0)
			: (int)widthBox.Value;


		private void ChangeSelection(object sender, System.EventArgs e)
		{
			setWidthCheckBox.Enabled = setWidthBox.Enabled = verticalButton.Checked;
			columnsBox.Enabled = widthBox.Enabled = flowButton.Checked;
		}

		private void SaveSettingsOnClick(object sender, System.EventArgs e)
		{
			if (verticalButton.Checked && setWidthCheckBox.Checked)
			{
				var provider = new SettingsProvider();
				var settings = provider.GetCollection("containers");
				settings.Add("width", (int)setWidthBox.Value);
				provider.SetCollection(settings);
				provider.Save();
			}
		}
	}
}
