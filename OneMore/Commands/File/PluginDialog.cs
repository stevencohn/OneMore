//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
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
					"cmdLabel",
					"argsLabel",
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
			this.plugin = new Plugin
			{
				Version = plugin.Version,
				Path = plugin.Path,
				Name = plugin.Name,
				OriginalName = plugin.OriginalName,
				Command = plugin.Command,
				Arguments = plugin.Arguments,
				CreateNewPage = plugin.CreateNewPage,
				PageName = plugin.PageName,
				AsChildPage = plugin.AsChildPage,
			};

			single = true;

			Text = Resx.PluginDialog_editText;

			saveButton.Location = okButton.Location;
			saveButton.DialogResult = DialogResult.OK;
			AcceptButton = saveButton;

			// disable so it's no longer a tab-stop
			okButton.Enabled = false;
			okButton.Visible = false;
		}


		public Plugin Plugin => new Plugin
		{
			Name = nameBox.Text,
			OriginalName = nameBox.Text,
			Command = cmdBox.Text,
			Arguments = argsBox.Text,
			CreateNewPage = createRadio.Checked,
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

				if (plugin.CreateNewPage)
					createRadio.Checked = true;
				else
					updateRadio.Checked = true;

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
					plugin.CreateNewPage = createRadio.Checked;
					plugin.AsChildPage = childBox.Checked = settings.Get<bool>("child");

					var keys = settings.Keys.ToList();

					var update = !keys.Contains("update") || settings.Get<bool>("update");
					if (update)
						createRadio.Checked = true;
					else
						updateRadio.Checked = true;

					pageNameBox.Enabled = createRadio.Checked;
					childBox.Enabled = createRadio.Checked;
				}
			}

			pluginsBox.SelectedIndex = 0;
			nameBox.Text = plugin.Name;
			nameBox.Focus();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private void ViewPredefined(object sender, EventArgs e)
		{
			plugin = pluginsBox.SelectedItem as Plugin;

			nameBox.Text = plugin.Name;
			cmdBox.Text = plugin.Command;
			argsBox.Text = plugin.Arguments;

			if (plugin.CreateNewPage)
				createRadio.Checked = true;
			else
				updateRadio.Checked = true;

			pageNameBox.Text = plugin.PageName;
			childBox.Checked = plugin.AsChildPage;

			var read = pluginsBox.SelectedIndex > 0;
			nameBox.ReadOnly = read;
			cmdBox.ReadOnly = read;
			argsBox.ReadOnly = read;
			createRadio.Enabled = !read;
			updateRadio.Enabled = !read;
			pageNameBox.ReadOnly = read;
			childBox.Enabled = !read;
			saveButton.Enabled = !read;
		}


		private void ChangeText(object sender, EventArgs e)
		{
			var valid = true;

			if (sender == nameBox)
			{
				var name = nameBox.Text.Trim();
				valid = ValidateName(name);

				if (valid)
				{
					plugin.Name = nameBox.Text;

					if (pluginsBox.Enabled)
					{
						((BindingList<Plugin>)pluginsBox.DataSource)
							.ResetItem(pluginsBox.SelectedIndex);
					}
				}
			}
			else if (sender == cmdBox)
				plugin.Command = cmdBox.Text.Trim();
			else if (sender == argsBox)
				plugin.Arguments = argsBox.Text.Trim();
			else if (sender == pageNameBox)
				plugin.PageName = pageNameBox.Text.Trim();

			saveButton.Enabled =
				valid &&
				!string.IsNullOrWhiteSpace(plugin.Name) &&
				!string.IsNullOrWhiteSpace(plugin.Command);

			okButton.Enabled = valid &&
				saveButton.Enabled &&
				(
					updateRadio.Checked || 
					(createRadio.Checked && !string.IsNullOrWhiteSpace(plugin.PageName))
				);
		}


		private char[] invalidChars;
		private bool ValidateName(string name)
		{
			if (pluginsBox.SelectedIndex > 0)
			{
				return true;
			}

			if (name.Length == 0)
			{
				return false;
			}

			name = name.ToLower();

			if (invalidChars == null)
			{
				invalidChars = Path.GetInvalidFileNameChars();
			}

			if (name.IndexOfAny(invalidChars) >= 0)
			{
				errorBox.Visible = true;
				return false;
			}

			if (plugin.Name == plugin.OriginalName)
			{
				return true;
			}

			if (predefinedNames == null)
			{
				// would be null in edit-mode
				predefinedNames = new PluginsProvider().GetNames()
					.Except(new List<string> { plugin.OriginalName })
					.ToArray();
			}

			if (predefinedNames.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase)))
			{
				errorBox.Visible = true;
				return false;
			}

			errorBox.Visible = false;
			return true;
		}


		private void updateRadio_CheckedChanged(object sender, EventArgs e)
		{
			pageNameBox.Enabled = !updateRadio.Checked;
			childBox.Enabled = !updateRadio.Checked;

			plugin.CreateNewPage = createRadio.Checked;
		}


		private void BrowsePath(object sender, EventArgs e)
		{
			// both cmdBox and argsBox use this handler
			var box = sender as TextBox;

			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "All files (*.*)|*.*";
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Title = Resx.Plugin_Title;
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
				dialog.InitialDirectory = GetValidPath(box.Text);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					box.Text = dialog.FileName;
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

		private void ChangeAsChild(object sender, EventArgs e)
		{
			plugin.AsChildPage = childBox.Checked;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private async void SavePlugin(object sender, EventArgs e)
		{
			try
			{
				var provider = new PluginsProvider();

				if (plugin.Name != plugin.OriginalName)
				{
					await provider.Rename(plugin, plugin.Name);
				}
				else
				{
					await provider.Save(plugin, plugin.Name);
				}


				if (!single)
				{
					UIHelper.ShowMessage($"{plugin.Name} has been saved");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error saving plugin", exc);
				UIHelper.ShowMessage("Plugin could not be saved; see log for details"); // translate
			}
		}


		private void OK(object sender, EventArgs e)
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
