//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


	internal partial class ConditionalFormatDialog : UI.MoreForm
	{
		public ConditionalFormatDialog()
		{
			InitializeComponent();

			hintLabel.Font = new Font(hintLabel.Font.FontFamily, hintLabel.Font.Size - 1F);

			if (NeedsLocalizing())
			{
				Text = Resx.ConditionalFormatDialog_Text;

				Localize(new string[]
				{
					"patternLabel",
					"builtinRadio",
					"customRadio",
					"styleLabel",
					"hintLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			patternStatusLabel.Text = string.Empty;

			LoadCustomStyles();
		}


		public string Pattern => patternBox.Text.Trim();

		public bool UseBuiltIn => builtinRadio.Checked;

		public StandardStyles SelectedStandardStyle => (StandardStyles)styleBox.SelectedIndex;

		public Style SelectedCustomStyle => styleBox.SelectedItem as Style;


		private void LoadBuiltinStyles()
		{
			styleBox.Items.Clear();

			styleBox.Items.AddRange(Resx.ConditionalFormatDialog_builtinStyles.Split(
				new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));

			if (styleBox.Items.Count > 0)
			{
				styleBox.SelectedIndex = 0;
			}
		}


		private void LoadCustomStyles()
		{
			styleBox.Items.Clear();

			var styles = new ThemeProvider().Theme.GetStyles();
			if (styles.Count > 0)
			{
				styleBox.Items.AddRange(styles.ToArray());
				styleBox.SelectedIndex = 0;
			}
		}


		private void ToggleStyleSource(object sender, EventArgs e)
		{
			if (builtinRadio.Checked)
			{
				LoadBuiltinStyles();
			}
			else
			{
				LoadCustomStyles();
			}

			UpdateOkButtonState();
		}


		private void CheckPattern(object sender, EventArgs e)
		{
			var text = patternBox.Text.Trim();
			if (text.Length == 0)
			{
				patternStatusLabel.Text = string.Empty;
			}
			else
			{
				try
				{
					_ = new Regex(text);
					patternStatusLabel.Text = string.Empty;
				}
				catch (Exception exc)
				{
					patternStatusLabel.Text = exc.Message;
				}
			}

			UpdateOkButtonState();
		}


		private void ChangeStyleSelection(object sender, EventArgs e)
		{
			UpdateOkButtonState();
		}


		private void UpdateOkButtonState()
		{
			var validPattern =
				patternBox.Text.Trim().Length > 0 &&
				patternStatusLabel.Text.Length == 0;

			var hasStyle = styleBox.Items.Count > 0 && styleBox.SelectedIndex >= 0;

			okButton.Enabled = validPattern && hasStyle;
		}
	}
}
