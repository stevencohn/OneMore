﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class PluginDialog : MoreForm
	{
		private string[] predefinedNames;
		private Plugin plugin;
		private bool initializing;
		private readonly bool single = false;


		public PluginDialog()
		{
			initializing = true;

			InitializeComponent();

			// sectionGroup is not visible by default but sits in same location as pageGroup
			sectionGroup.Size = pageGroup.Size;
			sectionGroup.Location = pageGroup.Location;

			// minor position refinements
			browseButton.Top = cmdBox.Top;
			browseButton.Height = cmdBox.Height;

			toolTip.SetToolTip(errorBox, Resx.PluginDialog_badName);
			toolTip.SetToolTip(createRadio, Resx.PluginDialog_nameTip);
			toolTip.SetToolTip(pageNameBox, Resx.PluginDialog_nameTip);

			if (NeedsLocalizing())
			{
				Text = Resx.PluginDialog_Text;

				Localize(new string[]
				{
					"pluginsLabel=word_Plugins",
					"nameLabel=word_Name",
					"cmdLabel=word_Command",
					"argsLabel",
					"userArgsLabel",
					"timeoutLabel",
					"targetLabel=word_Target",
					"targetBox",
					"updateRadio",
					"createRadio",
					"childBox",
					"skipLockRadio",
					"failLockRadio",
					"trialBox",
					"saveButton",
					"okButton", // Run
					"cancelButton=word_Cancel"
				});
			}

			targetBox.SelectedIndex = 0;

			initializing = false;
		}


		public PluginDialog(Plugin plugin)
			: this()
		{
			initializing = true;

			this.plugin = new Plugin
			{
				Version = plugin.Version,
				Path = plugin.Path,
				Name = plugin.Name,
				OriginalName = plugin.OriginalName,
				Command = plugin.Command,
				Arguments = plugin.Arguments,
				UserArguments = plugin.UserArguments,
				Target = plugin.Target,
				CreateNewPage = plugin.CreateNewPage,
				PageName = plugin.PageName,
				AsChildPage = plugin.AsChildPage,
				SkipLocked = plugin.SkipLocked,
				Timeout = plugin.Timeout
			};

			single = true;

			Text = Resx.PluginDialog_editText;

			ViewPlugin(this.plugin);

			saveButton.Location = okButton.Location;
			saveButton.DialogResult = DialogResult.OK;
			AcceptButton = saveButton;

			// disable so it's no longer a tab-stop
			okButton.Enabled = false;
			okButton.Visible = false;

			initializing = false;
		}


		public Plugin Plugin => new()
		{
			Name = nameBox.Text,
			OriginalName = nameBox.Text,
			Command = cmdBox.Text,
			Arguments = argsBox.Text,
			UserArguments = userArgsBox.Text,
			Target = (PluginTarget)targetBox.SelectedIndex,
			CreateNewPage = createRadio.Checked,
			AsChildPage = childBox.Checked,
			PageName = pageNameBox.Text,
			Timeout = (int)timeoutBox.Value,
			SkipLocked = skipLockRadio.Checked,
			// set path for replay functionality
			Path = plugin.Path
		};


		public bool TrialRun => trialBox.Checked;


		public string PageName { set; private get; }


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (single)
			{
				pluginsBox.Enabled = false;
				nameBox.Text = plugin.Name;
				cmdBox.Text = plugin.Command;
				argsBox.Text = plugin.Arguments;
				userArgsBox.Text = plugin.UserArguments;
				targetBox.SelectedIndex = (int)plugin.Target;

				timeoutBox.Value = plugin.Timeout;
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

			pluginsBox.SelectedIndex = 0;
			nameBox.Text = plugin.Name;
			nameBox.Focus();
		}


		private void ChangeTarget(object sender, EventArgs e)
		{
			if (initializing)
			{
				return;
			}

			//if (sender is null)
			//{
			//	targetBox.SelectedIndex = plugin.Target == PluginTarget.Page ? 0 : 1;
			//}

			plugin.Target = (PluginTarget)targetBox.SelectedIndex;

			if (plugin.Target == PluginTarget.Page)
			{
				pageGroup.Visible = true;
				sectionGroup.Visible = false;

				if (plugin.CreateNewPage)
					createRadio.Checked = true;
				else
					updateRadio.Checked = true;

				pageNameBox.Text = plugin.PageName;
				childBox.Checked = plugin.AsChildPage;
				skipLockRadio.Checked = true;
			}
			else
			{
				sectionGroup.Visible = true;
				pageGroup.Visible = false;

				if (plugin.SkipLocked)
					skipLockRadio.Checked = true;
				else
					failLockRadio.Checked = true;

				updateRadio.Checked = true;
				pageNameBox.Text = String.Empty;
				childBox.Checked = false;
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private void ViewPredefined(object sender, EventArgs e)
		{
			plugin = pluginsBox.SelectedItem as Plugin;
			ViewPlugin(plugin);
		}


		private void ViewPlugin(Plugin plugin)
		{
			nameBox.Text = plugin.Name;
			cmdBox.Text = plugin.Command;
			argsBox.Text = plugin.Arguments;
			userArgsBox.Text = plugin.UserArguments;
			timeoutBox.Value = plugin.Timeout;
			targetBox.SelectedIndex = (int)plugin.Target;

			//var read = pluginsBox.SelectedIndex > 0;
			//nameBox.ReadOnly = read;
			//cmdBox.ReadOnly = read;
			//argsBox.ReadOnly = read;
			//timeoutBox.ReadOnly = read;
			//createRadio.Enabled = !read;
			//updateRadio.Enabled = !read;
			//pageNameBox.ReadOnly = read;
			//childBox.Enabled = !read;
			//saveButton.Enabled = !read;
		}


		private void ChangeTimeout(object sender, EventArgs e)
		{
			plugin.Timeout = (int)timeoutBox.Value;
		}


		private void ChangeText(object sender, EventArgs e)
		{
			if (initializing)
			{
				return;
			}

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
			else if (sender == userArgsBox)
				plugin.UserArguments = userArgsBox.Text.Trim();

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

			invalidChars ??= Path.GetInvalidFileNameChars();

			if (name.IndexOfAny(invalidChars) >= 0)
			{
				errorBox.Visible = true;
				return false;
			}

			if (plugin.Name == plugin.OriginalName)
			{
				return true;
			}

			// would be null in edit-mode
			predefinedNames ??= new PluginsProvider().GetNames()
				.Except(new List<string> { plugin.OriginalName })
				.ToArray();

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
			var box = sender == browseButton ? cmdBox : argsBox;

			using var dialog = new OpenFileDialog();
			dialog.Filter = "All files (*.*)|*.*";
			dialog.CheckFileExists = true;
			dialog.Multiselect = false;
			dialog.Title = Resx.Plugin_Title;
			dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
			dialog.InitialDirectory = GetValidPath(box.Text);

			var result = dialog.ShowDialog(/* leave empty */);
			if (result == DialogResult.OK)
			{
				box.Text = dialog.FileName;
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

			if (plugin.Target != PluginTarget.Page)
			{
				plugin.PageName = String.Empty;
				plugin.CreateNewPage = false;
				plugin.AsChildPage = false;
				plugin.SkipLocked = skipLockRadio.Checked;
			}

			try
			{
				var provider = new PluginsProvider();

				if (plugin.Name != plugin.OriginalName &&
					!string.IsNullOrWhiteSpace(plugin.OriginalName))
				{
					await provider.Rename(plugin, plugin.Name);
				}
				else
				{
					await provider.Save(plugin);
				}


				if (!single)
				{
					MoreMessageBox.Show(this, $"{plugin.Name} has been saved");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error saving plugin", exc);

				MoreMessageBox.ShowErrorWithLogLink(
					Owner, "Plugin could not be saved; see log for details"); // translate
			}
		}


		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
