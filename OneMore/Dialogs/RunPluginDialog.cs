//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Dialogs
{
	using River.OneMoreAddIn.Helpers.Settings;
	using System;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RunPluginDialog : LocalizableForm
	{
		private const int SysMenuId = 1000;


		public RunPluginDialog()
		{
			InitializeComponent();

			browseButton.Top = cmdBox.Top;
			browseButton.Height = cmdBox.Height;

			if (NeedsLocalizing())
			{
				Text = Resx.PluginDialog_Text;

				Localize(new string[]
				{
					"cmdLabel",
					"argsLabel",
					"updateRadio",
					"createRadio",
					"childBox",
					"okButton",
					"cancelButton"
				});
			}

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("runPlugin");
			if (settings != null)
			{
				cmdBox.Text = settings.Get<string>("cmd");
				argsBox.Text = settings.Get<string>("args");
				updateRadio.Checked = !settings.Keys.Contains("update") || settings.Get<bool>("update");
				createRadio.Checked = !updateRadio.Checked;
				nameBox.Enabled = createRadio.Checked;
				childBox.Enabled = createRadio.Checked;
				childBox.Checked = settings.Get<bool>("child");
			}
		}


		public string Command => cmdBox.Text;

		public string Arguments => argsBox.Text;

		public bool UpdatePage => updateRadio.Checked;

		public bool CreateChild => childBox.Checked;

		public string PageName
		{
			get => nameBox.Text;
			set => nameBox.Text = value;
		}


		protected override void WndProc(ref Message m)
		{
			if ((m.Msg == Native.WM_SYSCOMMAND) && (m.WParam.ToInt32() == SysMenuId))
			{
				var provider = new SettingsProvider();
				if (provider.RemoveCollection("runPlugin"))
				{
					provider.Save();
				}

				cmdBox.Text = string.Empty;
				argsBox.Text = string.Empty;
				updateRadio.Checked = true;
				createRadio.Checked = false;
				childBox.Checked = false;

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


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		private void updateRadio_CheckedChanged(object sender, EventArgs e)
		{
			nameBox.Enabled = !updateRadio.Checked;
			childBox.Enabled = !updateRadio.Checked;
		}


		private void nameBox_TextChanged(object sender, EventArgs e)
		{
			okButton.Enabled = updateRadio.Checked ||
				(createRadio.Checked && !string.IsNullOrEmpty(nameBox.Text));
		}


		private void browseButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "All files (*.*)|*.*";
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Title = Resx.Plugin_Title;
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang

				dialog.InitialDirectory = File.Exists(cmdBox.Text)
					? cmdBox.Text
					: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					cmdBox.Text = dialog.FileName;
				}
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var settings = new SettingCollection("runPlugin");
			settings.Add("cmd", cmdBox.Text);
			settings.Add("args", argsBox.Text);
			settings.Add("update", updateRadio.Checked);
			settings.Add("child", childBox.Checked);

			var provider = new SettingsProvider();
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
