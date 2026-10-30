//************************************************************************************************
// Copyright © 2018 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class SearchAndReplaceDialog : UI.MoreForm
	{
		private readonly bool highlighting;
		private bool canSearch;
		private bool busy;
		private XElement whats;
		private UI.MoreComboBox colorBox;
		private List<(string Display, string Stored)> palette;


		/// <summary>
		/// Initializes a new dialog.
		/// </summary>
		/// <param name="highlighting">
		/// True to replace the "Replace with" field with a highlighter color chooser
		/// </param>
		public SearchAndReplaceDialog(bool highlighting = false)
		{
			InitializeComponent();

			this.highlighting = highlighting;

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
					"findNextButton",
					"replaceButton=word_Replace",
					"replaceAllButton",
					"cancelButton=word_Close"
				});
			}

			whatStatusLabel.Text = string.Empty;
			withStatusLabel.Text = string.Empty;

			scopeBox.SelectedIndex = 0;

			if (highlighting)
			{
				MorphToHighlight();
			}

			LoadSettings();
		}


		// Swap the replacement field for a highlighter chooser
		private void MorphToHighlight()
		{
			Text = Resx.SearchAndHighlightDialog_Text;
			withLabel.Text = Resx.SearchAndHighlightDialog_withLabel;
			replaceButton.Text = Resx.SearchAndHighlightDialog_highlightOneButton;
			replaceAllButton.Text = Resx.SearchAndHighlightDialog_highlightButton;

			withBox.Visible = false;
			withStatusLabel.Visible = false;

			colorBox = new ColorComboBox
			{
				Anchor = withBox.Anchor,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = withBox.Font,
				ItemHeight = withBox.Height - 6,
				Location = withBox.Location,
				Name = "colorBox",
				Size = withBox.Size,
				TabIndex = withBox.TabIndex
			};

			Controls.Add(colorBox);
		}


		/// <summary>
		/// Populates the color chooser. The first choice is always the next color in the
		/// rotating sequence, followed by each color of the configured scheme.
		/// </summary>
		public void SetPalette(List<(string Display, string Stored)> colors)
		{
			palette = colors;

			colorBox.Items.Clear();
			colorBox.Items.Add(Resx.SearchAndHighlightDialog_next);
			foreach (var color in colors)
			{
				colorBox.Items.Add(color.Display);
			}

			colorBox.SelectedIndex = 0;
		}


		// MoreComboBox.OnDrawItem paints the item text over anything a DrawItem handler draws,
		// so the swatches must be drawn by overriding OnDrawItem itself. The first item is the
		// "next in rotation" text; every other item is a hex color drawn as a filled rectangle.
		private sealed class ColorComboBox : UI.MoreComboBox
		{
			protected override void OnDrawItem(DrawItemEventArgs e)
			{
				if (e.Index <= 0 || e.Index >= Items.Count ||
					!ColorMath.TryParseHex((string)Items[e.Index], out var color))
				{
					base.OnDrawItem(e);
					return;
				}

				var manager = ThemeManager.Instance;
				var selected = (e.State & DrawItemState.ComboBoxEdit) == 0 &&
					(e.State & DrawItemState.Selected) != 0;

				var back = selected
					? manager.GetColor("Highlight")
					: manager.GetColor("Window", ThemedBack);

				using (var backBrush = new SolidBrush(back))
				{
					e.Graphics.FillRectangle(backBrush, e.Bounds);
				}

				// a color box twice as wide as it is tall, then 8px, then the #RRGGBB value
				var height = e.Bounds.Height - 4;
				var swatch = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, height * 2, height);

				using (var brush = new SolidBrush(color))
				{
					e.Graphics.FillRectangle(brush, swatch);
				}

				var fore = selected
					? manager.GetColor("HighlightText")
					: manager.GetColor("WindowText", ThemedFore);

				using (var pen = new Pen(fore))
				{
					e.Graphics.DrawRectangle(
						pen, swatch.X, swatch.Y, swatch.Width - 1, swatch.Height - 1);
				}

				var textBounds = new Rectangle(
					swatch.Right + 8, e.Bounds.Y, e.Bounds.Right - swatch.Right - 8, e.Bounds.Height);

				TextRenderer.DrawText(
					e.Graphics, ((string)Items[e.Index]).ToUpperInvariant(), Font, textBounds, fore,
					TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
					TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
			}
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


		/// <summary>
		/// Gets the color chosen when highlighting, or null to use the next color in the
		/// rotating sequence.
		/// </summary>
		public string HighlightColor => HighlightIndex >= 0 ? palette[HighlightIndex].Stored : null;

		/// <summary>
		/// Gets the rotation index of the chosen color, or -1 for the next in sequence.
		/// </summary>
		public int HighlightIndex => colorBox is null ? -1 : colorBox.SelectedIndex - 1;

		public bool MatchCase => matchBox.Checked;

		public OneNote.Scope Scope { get; private set; } = OneNote.Scope.Self;

		/// <summary>Raised when the user asks to select the next match on the page</summary>
		public event EventHandler FindNextClicked;

		/// <summary>Raised when the user asks to replace or highlight the current match</summary>
		public event EventHandler ReplaceClicked;

		/// <summary>Raised when the user asks to replace or highlight every match in scope</summary>
		public event EventHandler ReplaceAllClicked;

		/// <summary>
		/// Disables the action buttons while an action is running.
		/// </summary>
		public void SetBusy(bool busy)
		{
			this.busy = busy;
			UpdateButtonStates();
		}

		/// <summary>
		/// Shows the outcome of the last action.
		/// </summary>
		public void SetStatus(string text)
		{
			statusLabel.Text = text;
		}

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

					canSearch = whatBox.Text.Length > 0;
				}
				catch (Exception exc)
				{
					logger.WriteLine(text, exc);
					whatStatusLabel.Text = exc.Message;
					withStatusLabel.Text = string.Empty;
					canSearch = false;
				}
			}
			else
			{
				whatStatusLabel.Text = string.Empty;
				withStatusLabel.Text = string.Empty;
				canSearch = whatBox.Text.Length > 0;
			}

			UpdateButtonStates();
		}


		// Find Next and Replace step through one page so they only apply to This Page;
		// every scope can replace or highlight all
		private void UpdateButtonStates()
		{
			var single = canSearch && !busy && Scope == OneNote.Scope.Self;

			findNextButton.Enabled = single;
			replaceButton.Enabled = single;
			replaceAllButton.Enabled = canSearch && !busy;

			AcceptButton = single ? findNextButton : replaceAllButton;
		}


		// a modeless form is not closed by a button's DialogResult, so close it explicitly
		private void CloseDialog(object sender, EventArgs e)
		{
			Close();
		}


		private void FindNext(object sender, EventArgs e)
		{
			FindNextClicked?.Invoke(this, e);
		}


		private void ReplaceOne(object sender, EventArgs e)
		{
			ReplaceClicked?.Invoke(this, e);
		}


		private void ReplaceAll(object sender, EventArgs e)
		{
			ReplaceAllClicked?.Invoke(this, e);
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

			UpdateButtonStates();
		}
	}
}


