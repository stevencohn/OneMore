//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SearchAndReplaceDialog : UI.LocalizableForm
	{
		private XElement whats;


		public SearchAndReplaceDialog ()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchAndReplaceDialog_Text;

				Localize(new string[]
				{
					"whatLabel",
					"withLabel",
					"matchBox",
					"regBox",
					"okButton",
					"cancelButton"
				});
			}

			LoadSettings();
		}


		private void LoadSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchReplace");
			if (settings != null)
			{
				whats = settings.Get<XElement>("whats");
				if (whats != null)
				{
					foreach (var what in whats.Elements())
					{
						whatBox.Items.Add(what.Value);
					}
				}

				var withs = settings.Get<XElement>("withs");
				if (withs != null)
				{
					foreach (var withText in withs.Elements())
					{
						withBox.Items.Add(withText.Value);
					}
				}
			}
		}


		public bool MatchCase => matchBox.Checked;

		public string WithText => withBox.Text;

		public bool UseRegex => regBox.Checked;


		public string WhatText
		{
			get => whatBox.Text;
			set => whatBox.Text = value;
		}


		private void SearchAndReplaceDialog_Shown (object sender, EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
			whatBox.Focus();
		}

		private void WTextChanged (object sender, EventArgs e)
		{
			okButton.Enabled = whatBox.Text.Length > 0;
		}


		private void SelectedWhat(object sender, EventArgs e)
		{
			if (whatBox.SelectedIndex >= 0 && whatBox.SelectedIndex < whats.Elements().Count())
			{
				var what = whats.Elements().ElementAt(whatBox.SelectedIndex);
				var matchCase = what.Attribute("matchCase");
				if (matchCase != null)
				{
					if (bool.TryParse(matchCase.Value, out var check))
					{
						matchBox.Checked = check;
					}
				}

				var useRegex = what.Attribute("useRegex");
				if (useRegex != null)
				{
					if (bool.TryParse(useRegex.Value, out var check))
					{
						regBox.Checked = check;
					}
				}
			}
		}
	}
}


