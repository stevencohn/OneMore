//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Ribbon;
	using System;
	using Resx = Properties.Resources;


	internal partial class RibbonBarSheet : SheetBase
	{
		private bool grouped;

		public RibbonBarSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "RibbonBarSheet";
			Title = Resx.RibbonBarSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"layoutLabel",
					"layoutBox",
					"positionIntroLabel",
					"positionGroup",
					"positionLabel",
					"positionBox",
					"introBox",
					"quickGroup",
					"hashtagsRibbonBox",
					"hashtagsIconBox",
					"editRibbonBox",
					"editIconBox",
					"formulaRibbonBox",
					"formulaIconBox"
				});
			}

			var settings = provider.GetCollection(Name);

			grouped = settings.Get("layout", "group") == "group";
			layoutBox.SelectedIndex = grouped ? 0 : 1;

			var position = settings.Get("position", positionBox.Items.Count - 1);
			if (layoutBox.SelectedIndex == 1)
				position -= (int)RibbonPosiition.TabHome;

			positionBox.SelectedIndex = Math.Min(position, positionBox.Items.Count - 1);

			hashtagsRibbonBox.Checked = settings.Get<bool>("hashtagCommands");
			hashtagsIconBox.Checked = hashtagsRibbonBox.Checked && settings.Get<bool>("hashtagIconsOnly");

			editRibbonBox.Checked = settings.Get<bool>("editCommands");
			editIconBox.Checked = editRibbonBox.Checked && settings.Get<bool>("editIconsOnly");

			formulaRibbonBox.Checked = settings.Get<bool>("formulaCommands");
			formulaIconBox.Checked = formulaRibbonBox.Checked && settings.Get<bool>("formulaIconsOnly");
		}


		private void ChangeSelectedLayout(object sender, System.EventArgs e)
		{
			var text = layoutBox.SelectedIndex == 0
				? Resx.RibbonBarSheet_positionBox_Text
				: Resx.RibbonBarSheet_positionTabBox_Text;

			positionBox.Items.Clear();
			positionBox.Items.AddRange(
				text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

			positionBox.SelectedIndex = layoutBox.SelectedIndex == 0
				? positionBox.Items.Count - 1
				: positionBox.Items.Count - 2;

			grouped = layoutBox.SelectedIndex == 0;
			hashtagsPicture.Enabled = grouped;
			hashtagsRibbonBox.Enabled = grouped;
			hashtagsIconBox.Enabled = grouped && hashtagsRibbonBox.Checked;
			editPicture.Enabled = grouped;
			editRibbonBox.Enabled = grouped;
			editIconBox.Enabled = grouped && editRibbonBox.Checked;
			formulaPicture.Enabled = grouped;
			formulaRibbonBox.Enabled = grouped;
			formulaIconBox.Enabled = grouped && formulaRibbonBox.Checked;
		}


		private void CheckedChanged(object sender, System.EventArgs e)
		{
			if (sender == hashtagsRibbonBox)
			{
				hashtagsIconBox.Enabled = grouped && hashtagsRibbonBox.Checked;
			}
			else if (sender == editRibbonBox)
			{
				editIconBox.Enabled = grouped && editRibbonBox.Checked;
			}
			else
			{
				formulaIconBox.Enabled = grouped && formulaRibbonBox.Checked;
			}
		}


		private void ClickPicture(object sender, System.EventArgs e)
		{
			if (sender == hashtagsPicture)
			{
				hashtagsRibbonBox.Checked = !hashtagsRibbonBox.Checked;
			}
			else if (sender == editPicture)
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
			if (!hashtagsRibbonBox.Checked &&
				!editRibbonBox.Checked &&
				!formulaRibbonBox.Checked &&
				positionBox.SelectedIndex == positionBox.Items.Count)
			{
				provider.RemoveCollection(Name);
				return true;
			}

			var settings = provider.GetCollection(Name);
			var updated = false;

			var layout = layoutBox.SelectedIndex == 0 ? "group" : "tab";
			if (settings.Add("layout", layout)) updated = true;

			var position = layout == "group"
				? positionBox.SelectedIndex
				: positionBox.SelectedIndex + (int)RibbonPosiition.TabHome;

			if (settings.Add("position", position)) updated = true;

			// NOTE that the indexes MUST match RibbonGroups enum or it will break user's
			// established settings so may need migration if changed...

			if (layout == "group")
			{
				// only update these if layout is 'group'; otherwise leave them as-is...

				if (settings.Add("hashtagCommands", hashtagsRibbonBox.Checked)) updated = true;
				if (settings.Add("hashtagIconsOnly", hashtagsIconBox.Checked)) updated = true;
				if (settings.Add("editCommands", editRibbonBox.Checked)) updated = true;
				if (settings.Add("editIconsOnly", editIconBox.Checked)) updated = true;
				if (settings.Add("formulaCommands", formulaRibbonBox.Checked)) updated = true;
				if (settings.Add("formulaIconsOnly", formulaIconBox.Checked)) updated = true;
			}

			if (updated)
			{
				provider.SetCollection(settings);
			}

			return updated;
		}
	}
}
