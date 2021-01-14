//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class PluginDialog : UI.LocalizableForm
	{
		private const int SysMenuId = 1000;

		private string[] predefinedNames;
		private Plugin plugin;
		private bool single = false;


		public PluginDialog()
		{
			InitializeComponent();

			browseButton.Top = cmdBox.Top;
			browseButton.Height = cmdBox.Height;

			if (NeedsLocalizing())
			{
				Text = Resx.PluginDialog_Text;

				Localize(new string[]
				{
					"pluginsLabel",
					"nameLabel",
					"commandLabel",
					"argumentsLabel",
					"updateRadio",
					"createRadio",
					"childBox",
					"saveButton",
					"okButton",
					"cancelButton"
				});
			}
		}


		public PluginDialog(Plugin plugin)
			: this()
		{
			this.plugin = plugin;
			single = true;

			Text = Resx.PluginDialog_editText;

			saveButton.Location = okButton.Location;
			saveButton.DialogResult = DialogResult.OK;
			AcceptButton = saveButton;

			okButton.Visible = false;
		}


		public Plugin Plugin => new Plugin
		{
			Command = cmdBox.Text,
			Arguments = argsBox.Text,
			CreateNewPage = !updateRadio.Checked,
			AsChildPage = childBox.Checked,
			PageName = pageNameBox.Text
		};


		public string PageName { set; private get; }


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


		protected async override void OnLoad(EventArgs e)
		{
			var hmenu = Native.GetSystemMenu(Handle, false);
			Native.InsertMenu(hmenu, 5, Native.MF_BYPOSITION, SysMenuId, Resx.DialogResetSettings_Text);
			base.OnLoad(e);

			if (single)
			{
				pluginsBox.Enabled = false;
				nameBox.Text = plugin.Name;
				cmdBox.Text = plugin.Command;
				argsBox.Text = plugin.Arguments;
				createRadio.Checked = plugin.CreateNewPage;
				pageNameBox.Text = plugin.PageName;
				childBox.Checked = plugin.AsChildPage;
				return;
			}

			var provider = new PluginsProvider();
			var names = provider.GetNames().ToList();
			names.Sort();
			predefinedNames = names.ToArray();

			var binding = new BindingList<Plugin>();
			pluginsBox.DataSource = binding;
			pluginsBox.DisplayMember = "Name";

			plugin = new Plugin
			{
				Name = Resx.PluginDialog_newItem,
				PageName = this.PageName
			};

			binding.Add(plugin);

			foreach (var name in names)
			{
				var p = await provider.LoadByName(name);
				if (p != null)
				{
					binding.Add(p);
				}
			}

			if (names.Count == 0)
			{
				var setProvider = new SettingsProvider();
				var settings = setProvider.GetCollection("runPlugin");
				if (settings != null)
				{
					plugin = new Plugin();
					plugin.Command = cmdBox.Text = settings.Get<string>("cmd");
					plugin.Arguments = argsBox.Text = settings.Get<string>("args");

					var keys = settings.Keys.ToList();
					updateRadio.Checked = !keys.Contains("update") || settings.Get<bool>("update");

					plugin.CreateNewPage = createRadio.Checked = !updateRadio.Checked;
					pageNameBox.Enabled = createRadio.Checked;
					childBox.Enabled = createRadio.Checked;
					plugin.AsChildPage = childBox.Checked = settings.Get<bool>("child");
				}
			}

			pluginsBox.SelectedIndex = 0;
			nameBox.Text = plugin.Name;
			nameBox.Focus();
		}


		private void ViewPredefined(object sender, EventArgs e)
		{
			plugin = pluginsBox.SelectedItem as Plugin;

			nameBox.Text = plugin.Name;
			nameBox.ReadOnly = pluginsBox.SelectedIndex > 0;

			cmdBox.Text = plugin.Command;
			argsBox.Text = plugin.Arguments;
			updateRadio.Checked = !plugin.CreateNewPage;
			createRadio.Checked = plugin.CreateNewPage;
			pageNameBox.Text = plugin.PageName;
			childBox.Checked = plugin.AsChildPage;
		}


		private void ChangeText(object sender, EventArgs e)
		{
			if (sender == nameBox)
				plugin.Name = nameBox.Text.Trim();
			else if (sender == cmdBox)
				plugin.Command = cmdBox.Text.Trim();
			else if (sender == argsBox)
				plugin.Arguments = argsBox.Text.Trim();
			else if (sender == pageNameBox)
				plugin.PageName = pageNameBox.Text.Trim();

			saveButton.Enabled =
				!string.IsNullOrWhiteSpace(plugin.Name) &&
				!string.IsNullOrWhiteSpace(plugin.Command);

			okButton.Enabled = saveButton.Enabled &&
				(
					updateRadio.Checked || 
					(createRadio.Checked && !string.IsNullOrWhiteSpace(plugin.PageName))
				);
		}


		private void updateRadio_CheckedChanged(object sender, EventArgs e)
		{
			pageNameBox.Enabled = !updateRadio.Checked;
			childBox.Enabled = !updateRadio.Checked;

			plugin.CreateNewPage = createRadio.Checked;
			plugin.AsChildPage = childBox.Checked;
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
				dialog.InitialDirectory = GetValidPath(cmdBox.Text);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					cmdBox.Text = dialog.FileName;
				}
			}
		}

		private void BrowseArgumentPath(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "All files (*.*)|*.*";
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Title = Resx.Plugin_Title;
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
				dialog.InitialDirectory = GetValidPath(argsBox.Text);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					argsBox.Text = dialog.FileName;
				}
			}
		}


		private string GetValidPath(string path)
		{
			path = path.Trim();
			if (path == string.Empty)
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			if (File.Exists(path))
			{
				return Path.GetDirectoryName(path);
			}
			else if (Directory.Exists(path))
			{
				return path;
			}

			path = Path.GetDirectoryName(path);
			if (Directory.Exists(path))
			{
				return path;
			}

			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}


		private async void SavePlugin(object sender, EventArgs e)
		{
			string name = null;

			if (single)
			{
				name = plugin.Name;
			}
			else
			{
				using (var dialog = new SavePluginDialog())
				{
					if (pluginsBox.SelectedIndex > 0)
					{
						dialog.PluginName = pluginsBox.SelectedItem.ToString();
					}

					if (dialog.ShowDialog() != DialogResult.OK)
					{
						return;
					}

					name = dialog.PluginName;
				}
			}

			try
			{
				var provider = new PluginsProvider();
				await provider.Save(Plugin, name);

				//if (!editMode)
				{
					UIHelper.ShowMessage($"\"{name}\" plugin is saved"); // translate
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error saving plugin", exc);
				UIHelper.ShowMessage("Plugin could not be saved; see log for details"); // translate
			}
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var settings = new SettingsCollection("runPlugin");
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
