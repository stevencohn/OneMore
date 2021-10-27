//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class OutlineDialog : UI.LocalizableForm
	{
		private const int SysMenuId = 1000;


		public OutlineDialog()
		{
			InitializeComponent();

			TagSymbol = 0;

			if (NeedsLocalizing())
			{
				Text = Resx.OutlineDialog_Text;
				tooltip.SetToolTip(numberingBox, Resx.OutlineDialog_numberingBox_Tooltip);
				tooltip.SetToolTip(indentTagBox, Resx.OutlineDialog_indentTagBox_Tooltip);

				Localize(new string[]
				{
					"numberingGroup",
					"numberingBox",
					"alphaRadio",
					"numRadio",
					"cleanBox",
					"indentationsGroup",
					"indentBox",
					"indentTagBox",
					"removeTagsBox",
					"tagLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			RestoreSettings();
		}


		private void RestoreSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("outline");
			if (settings != null)
			{
				numberingBox.Checked = settings.Get<bool>("addNumbering");
				if (numberingBox.Checked)
				{
					alphaRadio.Checked = settings.Get<bool>("alphaNumbering");
					numRadio.Checked = settings.Get<bool>("numericNumbering");
				}

				cleanBox.Checked = settings.Get<bool>("cleanupNumbering");
				indentBox.Checked = settings.Get<bool>("indent");

				indentTagBox.Checked = settings.Get<bool>("indentTagged");
				if (indentTagBox.Checked)
				{
					removeTagsBox.Checked = settings.Get<bool>("removeTags");

					TagSymbol = settings.Get<int>("tagSymbol");
					if (TagSymbol > 0)
					{
						using (var dialog = new UI.TagPickerDialog(0, 0))
						{
							var glyph = dialog.GetGlyph(TagSymbol);
							if (glyph != null)
							{
								tagButton.Text = null;
								tagButton.Image = glyph;
							}
						}
					}
				}
			}
		}


		public bool AlphaNumbering => alphaRadio.Enabled && alphaRadio.Checked;

		public bool NumericNumbering => numRadio.Enabled && numRadio.Checked;

		public bool CleanupNumbering => cleanBox.Checked;

		public bool Indent => indentBox.Checked;

		public bool IndentTagged => indentTagBox.Checked;

		public bool RemoveTags => removeTagsBox.Checked;

		public int TagSymbol { get; private set; }


		protected override void WndProc(ref Message m)
		{
			if ((m.Msg == Native.WM_SYSCOMMAND) && (m.WParam.ToInt32() == SysMenuId))
			{
				var provider = new SettingsProvider();
				if (provider.RemoveCollection("outline"))
				{
					provider.Save();
				}

				numberingBox.Checked = false;
				alphaRadio.Checked = true;
				numRadio.Checked = false;
				cleanBox.Checked = false;
				indentBox.Checked = false;
				indentTagBox.Checked = false;
				removeTagsBox.Checked = false;
				tagButton.Image = null;
				tagButton.Text = "?";

				return;
			}

			base.WndProc(ref m);
		}

		protected override void OnLoad(EventArgs e)
		{
			var hmenu = Native.GetSystemMenu(Handle, false);
			Native.InsertMenu(hmenu, 5, Native.MF_BYPOSITION, SysMenuId, Resx.DialogResetSettings_Text);
			base.OnLoad(e);
		}


		private void numberingBox_CheckedChanged(object sender, EventArgs e)
		{
			alphaRadio.Enabled = numberingBox.Checked;
			numRadio.Enabled = numberingBox.Checked;
			SetOK();
		}


		private void cleanBox_CheckedChanged(object sender, EventArgs e)
		{
			SetOK();
		}


		private void indentBox_CheckedChanged(object sender, EventArgs e)
		{
			SetOK();
		}


		private void indentTagBox_CheckedChanged(object sender, EventArgs e)
		{
			tagButton.Enabled = removeTagsBox.Enabled = indentTagBox.Checked;
			SetOK();
		}


		private void tagButton_Click(object sender, EventArgs e)
		{
			var location = PointToScreen(tagButton.Location);

			using (var dialog = new UI.TagPickerDialog(
				location.X + tagButton.Bounds.Location.X - tagButton.Width,
				location.Y + tagButton.Bounds.Location.Y))
			{
				dialog.Select(TagSymbol);

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					var glyph = dialog.GetGlyph();
					if (glyph != null)
					{
						tagButton.Text = null;
						tagButton.Image = glyph;
					}
					else
					{
						tagButton.Text = "?";
					}

					TagSymbol = dialog.Symbol;
				}
			}
		}


		private void SetOK()
		{
			okButton.Enabled = 
				numberingBox.Checked || cleanBox.Checked || 
				indentBox.Checked || indentTagBox.Checked;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var settings = new SettingsCollection("outline");
			settings.Add("addNumbering", numberingBox.Checked);
			settings.Add("alphaNumbering", AlphaNumbering);
			settings.Add("numericNumbering", NumericNumbering);
			settings.Add("cleanupNumbering", CleanupNumbering);
			settings.Add("indent", Indent);
			settings.Add("indentTagged", IndentTagged);
			settings.Add("removeTags", RemoveTags);
			settings.Add("tagSymbol", TagSymbol);

			var provider = new SettingsProvider();
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
