//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RibbonBarSheet : SheetBase
	{
		public RibbonBarSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

            Name = "RibbonBarSheet";
            Title = Resx.RibbonBarSheet_Title;

            if (NeedsLocalizing())
            {
                Localize(new string[]
                {
                    "introBox",
                    "editGroup",
                    "editIconBox",
                    "formulaGroup",
                    "formulaIconBox"
                });
            }

            var settings = provider.GetCollection(Name);
            editRibbonBox.Checked = settings.Get<bool>("editCommands");
            editIconBox.Checked = settings.Get<bool>("editIconsOnly");
            formulaRibbonBox.Checked = settings.Get<bool>("formulaCommands");
            formulaIconBox.Checked = settings.Get<bool>("formulaIconsOnly");
        }


        public override void CollectSettings()
        {
            var settings = provider.GetCollection(Name);
            settings.Add("editCommands", editRibbonBox.Checked);
            settings.Add("editIconsOnly", editIconBox.Checked);
            settings.Add("formulaCommands", formulaRibbonBox.Checked);
            settings.Add("formulaIconsOnly", formulaIconBox.Checked);

            provider.SetCollection(settings);
        }


		private void CheckedChanged(object sender, System.EventArgs e)
		{
            if (sender == editRibbonBox)
			{
                editIconBox.Enabled = editRibbonBox.Checked;
			}
            else
			{
                formulaIconBox.Enabled = formulaRibbonBox.Checked;
			}
		}


		private void ClickPicture(object sender, System.EventArgs e)
		{
            if (sender == editPicture)
			{
                editRibbonBox.Checked = !editRibbonBox.Checked;
			}
            else
			{
                formulaRibbonBox.Checked = !formulaRibbonBox.Checked;
            }
		}
	}
}
