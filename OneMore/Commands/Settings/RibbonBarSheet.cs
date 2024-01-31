//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


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
                    "positionIntroLabel",
                    "positionGroup",
                    "positionLabel",
                    "positionBox",
                    "introBox",
                    "quickGroup",
                    "editRibbonBox",
                    "editIconBox",
                    "formulaRibbonBox",
                    "formulaIconBox"
                });
            }
			
            var settings = provider.GetCollection(Name);
			positionBox.SelectedIndex = settings.Get("position", positionBox.Items.Count - 1);
			editRibbonBox.Checked = settings.Get<bool>("editCommands");
            editIconBox.Checked = settings.Get<bool>("editIconsOnly");
            formulaRibbonBox.Checked = settings.Get<bool>("formulaCommands");
            formulaIconBox.Checked = settings.Get<bool>("formulaIconsOnly");
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


        public override bool CollectSettings()
        {
            if (!editRibbonBox.Checked && !formulaRibbonBox.Checked && 
                positionBox.SelectedIndex == positionBox.Items.Count)
			{
                provider.RemoveCollection(Name);
                return true;
			}

            var settings = provider.GetCollection(Name);
            var updated = false;

            // NOTE that the indexes MUST match RibbonGroups enum or it will break user's
            // established settings so may need migration if changed...

			if (settings.Add("position", positionBox.SelectedIndex)) updated = true;

			if (settings.Add("editCommands", editRibbonBox.Checked)) updated = true;
			if (settings.Add("editIconsOnly", editIconBox.Checked)) updated = true;
            if (settings.Add("formulaCommands", formulaRibbonBox.Checked)) updated = true;
            if (settings.Add("formulaIconsOnly", formulaIconBox.Checked)) updated = true;

            if (updated)
            {
                provider.SetCollection(settings);
            }

            return updated;
        }
    }
}
