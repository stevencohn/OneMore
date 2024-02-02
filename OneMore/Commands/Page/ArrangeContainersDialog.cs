//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using Resx = Properties.Resources;


	internal partial class ArrangeContainersDialog : UI.MoreForm
	{
		private const int DefaultWidth = 500;


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
					"indentLabel=word_Indent",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			var settings = new SettingsProvider().GetCollection("containers");

			var width = settings.Get("width", 0);
			if (width > 0)
			{
				setWidthCheckBox.Checked = true;
				setWidthBox.Value = settings.Get("width", width);
			}
			else
			{
				setWidthBox.Value = DefaultWidth;
			}

			indentBox.Value = settings.Get("indent", 0);
		}


		public bool Vertical => verticalButton.Checked;


		public int Columns => (int)columnsBox.Value;


		public int Indent => (int)indentBox.Value;


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
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("containers");

			if (verticalButton.Checked && setWidthCheckBox.Checked)
			{
				settings.Add("width", (int)setWidthBox.Value);
			}

			settings.Add("indent", (int)indentBox.Value);
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
