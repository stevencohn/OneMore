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
	using System.Windows.Forms;


	internal partial class RunPluginDialog : LocalizableForm
	{
		public RunPluginDialog()
		{
			InitializeComponent();

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("runPlugin");
			if (settings != null)
			{
				pathBox.Text = settings.Get<string>("path");
			}
		}


		public string PluginPath => pathBox.Text;


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		private void pathBox_TextChanged(object sender, EventArgs e)
		{
			okButton.Enabled = File.Exists(pathBox.Text);
		}


		private void browseButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "All files (*.*)|*.*";
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Title = "Plugin";
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang

				dialog.InitialDirectory = File.Exists(pathBox.Text)
					? pathBox.Text
					: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					pathBox.Text = dialog.FileName;
				}
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var settings = new SettingCollection("runPlugin");
			settings.Add("path", pathBox.Text);

			var provider = new SettingsProvider();
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
