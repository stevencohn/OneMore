//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts for the text or expression to select on the current page.
	/// </summary>
	internal partial class SearchAndSelectDialog : UI.MoreForm
	{
		public SearchAndSelectDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchAndSelectDialog_Text;
				whatLabel.Text = Resx.SearchAndReplaceDialog_whatLabel_Text;
				matchBox.Text = Resx.SearchAndReplaceDialog_matchBox_Text;
				regBox.Text = Resx.SearchAndReplaceDialog_regBox_Text;
				okButton.Text = Resx.word_OK;
				cancelButton.Text = Resx.word_Cancel;
			}

			statusLabel.Text = string.Empty;
		}


		/// <summary>
		/// Gets or sets the text or regular expression to find.
		/// </summary>
		public string WhatText
		{
			get => whatBox.Text;
			set => whatBox.Text = value;
		}


		/// <summary>
		/// Gets or sets a value indicating whether the search is case sensitive.
		/// </summary>
		public bool MatchCase
		{
			get => matchBox.Checked;
			set => matchBox.Checked = value;
		}


		/// <summary>
		/// Gets or sets a value indicating whether the text is a regular expression.
		/// </summary>
		public bool UseRegex
		{
			get => regBox.Checked;
			set => regBox.Checked = value;
		}


		private void FocusOnWhat(object sender, EventArgs e)
		{
			whatBox.Focus();
		}


		// Enables OK only for a non-empty pattern and, for regex, one that compiles
		private void CheckPattern(object sender, EventArgs e)
		{
			statusLabel.Text = string.Empty;

			if (whatBox.Text.Length == 0)
			{
				okButton.Enabled = false;
				return;
			}

			if (regBox.Checked)
			{
				try
				{
					_ = new Regex(whatBox.Text);
				}
				catch (ArgumentException exc)
				{
					statusLabel.Text = exc.Message;
					okButton.Enabled = false;
					return;
				}
			}

			okButton.Enabled = true;
		}
	}
}
