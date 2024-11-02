//************************************************************************************************
// Copyright © 2018 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class SearchAndReplaceDialog : UI.MoreForm
	{
		private XElement whats;


		public SearchAndReplaceDialog()
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
					"scopeLabel=word_Scope",
					"scopeBox",
					"rawBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			whatStatusLabel.Text = string.Empty;
			withStatusLabel.Text = string.Empty;

			scopeBox.SelectedIndex = 0;

			LoadSettings();
		}


		private void LoadSettings()
		{
			var provider = new SettingsProvider();

			var experimental = provider.GetCollection("GeneralSheet").Get<bool>("experimental");
			if (!experimental)
			{
				rawBox.Visible = false;
				Height -= rawBox.Height;
			}

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
					foreach (var withText in withs.Elements().Select(e => e.Value))
					{
						var text = withText;
						if (text.StartsWith("&lt;") && text.EndsWith("&gt;"))
						{
							text = HttpUtility.HtmlDecode(text);
						}

						withBox.Items.Add(text);
					}
				}
			}
		}


		public bool MatchCase => matchBox.Checked;

		public XElement RawXml { get; private set; }

		public OneNote.Scope Scope { get; private set; } = OneNote.Scope.Self;

		public string WithText => withBox.Text;

		public bool UseRegex => regBox.Checked;


		public string WhatText
		{
			get => whatBox.Text;
			set => whatBox.Text = value;
		}


		private void FocusOnWhat(object sender, EventArgs e)
		{
			whatBox.Focus();
		}


		private void ToggleRegex(object sender, EventArgs e)
		{
			CheckPattern(sender, e);
		}


		private void ToggleRawXml(object sender, EventArgs e)
		{
			CheckXmlFormat(sender, e);
		}


		private void CheckPattern(object sender, EventArgs e)
		{
			var text = whatBox.Text.Trim();
			if (text.Length == 0)
			{
				whatStatusLabel.Text = string.Empty;
				withStatusLabel.Text = string.Empty;
			}

			if (regBox.Checked)
			{
				try
				{
					var regex = new Regex(text);

					var count = regex.GetGroupNumbers().Length - 1;
					if (count > 0)
					{
						var range = count == 1
							? "$1"
							: $"$1 - ${count}";

						whatStatusLabel.Text = string.Empty;

						withStatusLabel.Text =
							string.Format(Resx.SearchAndReplaceDialog_substitutions, range);
					}
					else
					{
						whatStatusLabel.Text = string.Empty;
						withStatusLabel.Text = string.Empty;
					}

					okButton.Enabled = whatBox.Text.Length > 0;
				}
				catch (Exception exc)
				{
					logger.WriteLine(text, exc);
					whatStatusLabel.Text = exc.Message;
					withStatusLabel.Text = string.Empty;
					okButton.Enabled = false;
				}
			}
			else
			{
				whatStatusLabel.Text = string.Empty;
				withStatusLabel.Text = string.Empty;
				okButton.Enabled = whatBox.Text.Length > 0;
			}
		}


		private void CheckXmlFormat(object sender, EventArgs e)
		{
			if (rawBox.Checked)
			{
				var xml = withBox.Text.Trim();
				try
				{
					RawXml = XElement.Parse(xml);
				}
				catch
				{
					withStatusLabel.Text = "XML format is not correct";
					return;
				}
			}
			else
			{
				RawXml = null;
			}

			// ensure whatStatus is restored correctly
			CheckPattern(sender, e);
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

		private void ChangeScope(object sender, EventArgs e)
		{
			Scope = scopeBox.SelectedIndex switch
			{
				1 => OneNote.Scope.Pages,
				2 => OneNote.Scope.Sections,
				3 => OneNote.Scope.Notebooks,
				_ => OneNote.Scope.Self
			};
		}
	}
}


