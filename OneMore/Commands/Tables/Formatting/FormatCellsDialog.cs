//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts for the format to apply to table cells and whether to apply it to the selected
	/// cells or to the entire columns containing them, with a preview of the result.
	/// </summary>
	internal partial class FormatCellsDialog : UI.MoreForm
	{
		private const int PreviewCount = 6;

		private static readonly string[] Symbols =
			new[] { "$", "€", "£", "¥", "₹", "₩", "CHF", "kr", "zł" };

		private readonly IReadOnlyList<string> selectedTexts;
		private readonly IReadOnlyList<string> columnTexts;
		private readonly CultureInfo culture = AddIn.Locale;
		private string[] patterns = Array.Empty<string>();
		private bool categoryChosen;
		private bool loading;


		public FormatCellsDialog()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Initializes a new dialog.
		/// </summary>
		/// <param name="selectedTexts">Text of the eligible selected cells, excluding any header</param>
		/// <param name="columnTexts">Text of the eligible cells of the columns, excluding any header</param>
		/// <param name="columnCount">The number of columns containing selected cells</param>
		public FormatCellsDialog(
			IReadOnlyList<string> selectedTexts, IReadOnlyList<string> columnTexts,
			int columnCount)
			: this()
		{
			this.selectedTexts = selectedTexts;
			this.columnTexts = columnTexts;

			if (NeedsLocalizing())
			{
				Text = Resx.FormatCellsDialog_Text;

				Localize(new string[]
				{
					"scopeGroup",
					"categoryLabel",
					"decimalsLabel",
					"separatorBox",
					"negativeLabel",
					"symbolLabel",
					"fractionRadio",
					"percentRadio",
					"patternLabel",
					"previewLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			selectedRadio.Text = string.Format(
				Resx.FormatCellsDialog_SelectedScope, selectedTexts.Count);

			columnsRadio.Text = string.Format(
				Resx.FormatCellsDialog_ColumnsScope, columnCount, columnTexts.Count);

			// texts may be localized and are longer or shorter than the designer's, so size
			// each radio button and check box to its text; this also keeps the focus box
			// tight to the text
			var buttons = new System.Windows.Forms.ButtonBase[]
			{
				selectedRadio, columnsRadio, fractionRadio, percentRadio, separatorBox
			};

			foreach (var button in buttons)
			{
				FitToText(button);
			}

			beforeColumn.Text = Resx.FormatCellsDialog_beforeHeader;
			afterColumn.Text = Resx.FormatCellsDialog_afterHeader;
			previewList.SetColumnProportions(0.5f, 0.5f);

			categoryBox.Items.AddRange(new object[]
			{
				Resx.FormatCellsDialog_General,
				Resx.FormatCellsDialog_Number,
				Resx.FormatCellsDialog_Percent,
				Resx.FormatCellsDialog_Currency,
				Resx.FormatCellsDialog_Date,
				Resx.FormatCellsDialog_Time,
				Resx.FormatCellsDialog_DateTime
			});

			symbolBox.Items.Add(Resx.FormatCellsDialog_DefaultSymbol);
			symbolBox.Items.AddRange(Symbols);

			// every column may need a different format so always start from the defaults
			loading = true;
			try
			{
				selectedRadio.Checked = true;
				separatorBox.Checked = true;
				fractionRadio.Checked = true;
				symbolBox.SelectedIndex = 0;
				AutoDetect();
			}
			finally
			{
				loading = false;
			}

			UpdateState();
		}


		/// <summary>
		/// Gets the format described by the current state of the dialog.
		/// </summary>
		public CellFormat Format => new()
		{
			Category = (CellFormatCategory)categoryBox.SelectedIndex,
			Decimals = (int)decimalsBox.Value,
			UseSeparator = separatorBox.Checked,
			Negative = (NegativeStyle)Math.Max(0, negativeBox.SelectedIndex),
			CurrencySymbol = symbolBox.SelectedIndex > 0
				? Symbols[symbolBox.SelectedIndex - 1]
				: null,
			PercentIsFraction = fractionRadio.Checked,
			Pattern = patternBox.SelectedIndex >= 0 && patternBox.SelectedIndex < patterns.Length
				? patterns[patternBox.SelectedIndex]
				: null,
			Scope = columnsRadio.Checked ? FormatScope.EntireColumns : FormatScope.SelectedCells
		};


		/// <summary>
		/// MoreRadioButton and MoreCheckBox measure their text and grow to fit when the text
		/// changes, but never shrink. Collapse the button and set its text again so that it
		/// is measured afresh.
		/// </summary>
		private static void FitToText(System.Windows.Forms.ButtonBase button)
		{
			var text = button.Text;
			button.Size = new System.Drawing.Size(1, 1);
			button.Text = string.Empty;
			button.Text = text;
		}


		protected override void OnDpiScaleChanged(int oldDpi, int newDpi)
		{
			previewList.SetColumnProportions(0.5f, 0.5f);
		}


		private void ChangeCategory(object sender, EventArgs e)
		{
			if (loading)
			{
				return;
			}

			// the user's choice wins over the detected category from now on, although the
			// detected category remains displayed as a reference
			categoryChosen = true;

			RebuildOptions();
			UpdateState();
		}


		private void ChangeScope(object sender, EventArgs e)
		{
			if (loading)
			{
				return;
			}

			var category = Detect();

			if (categoryChosen)
			{
				ShowDetected(category);
			}
			else
			{
				AutoDetect(category);
			}

			UpdateState();
		}


		private CellFormatCategory Detect()
		{
			var texts = columnsRadio.Checked ? columnTexts : selectedTexts;
			return CellFormatter.DetectCategory(texts, culture);
		}


		/// <summary>
		/// Preselects the category that best describes the cells in the chosen scope.
		/// </summary>
		private void AutoDetect()
		{
			AutoDetect(Detect());
		}


		private void AutoDetect(CellFormatCategory category)
		{
			var wasLoading = loading;
			loading = true;
			try
			{
				categoryBox.SelectedIndex = (int)category;
				RebuildOptions();
			}
			finally
			{
				loading = wasLoading;
			}

			ShowDetected(category);
		}


		private void ShowDetected(CellFormatCategory category)
		{
			detectedLabel.Text = string.Format(
				Resx.FormatCellsDialog_Detected, categoryBox.Items[(int)category]);

			detectedLabel.Visible = true;
		}


		private void ChangeOption(object sender, EventArgs e)
		{
			if (!loading)
			{
				UpdateState();
			}
		}


		/// <summary>
		/// Shows only the options relevant to the chosen category and repopulates the
		/// category-specific lists with examples in the current locale.
		/// </summary>
		private void RebuildOptions()
		{
			var category = (CellFormatCategory)categoryBox.SelectedIndex;

			var isNumber = category == CellFormatCategory.Number;
			var isCurrency = category == CellFormatCategory.Currency;
			var isPercent = category == CellFormatCategory.Percent;
			var isDate = category >= CellFormatCategory.Date;

			decimalsLabel.Visible = decimalsBox.Visible = isNumber || isCurrency || isPercent;
			separatorBox.Visible = isNumber || isCurrency;
			negativeLabel.Visible = negativeBox.Visible = isNumber || isCurrency;
			symbolLabel.Visible = symbolBox.Visible = isCurrency;
			fractionRadio.Visible = percentRadio.Visible = isPercent;
			patternLabel.Visible = patternBox.Visible = isDate;

			if (isNumber || isCurrency)
			{
				// render each style with the formatter so the examples match the results
				var example = new CellFormat { Category = category, Decimals = 0 };
				negativeBox.Items.Clear();

				foreach (var style in new[] { NegativeStyle.Minus, NegativeStyle.Parentheses })
				{
					example.Negative = style;
					negativeBox.Items.Add(CellFormatter.FormatValue(-1234m, example, culture));
				}

				negativeBox.SelectedIndex = 0;
			}

			patterns = CellFormatter.GetPatterns(category);
			patternBox.Items.Clear();
			if (isDate)
			{
				var now = DateTime.Now;
				patternBox.Items.AddRange(
					patterns.Select(p => (object)now.ToString(p, culture)).ToArray());
				patternBox.SelectedIndex = 0;
			}
		}


		private void UpdateState()
		{
			var texts = columnsRadio.Checked ? columnTexts : selectedTexts;
			okButton.Enabled = texts.Count > 0;

			var format = Format;

			previewList.BeginUpdate();
			previewList.Items.Clear();

			foreach (var text in texts.Where(t => !string.IsNullOrWhiteSpace(t)).Take(PreviewCount))
			{
				var item = new System.Windows.Forms.ListViewItem(text);
				item.SubItems.Add(
					CellFormatter.Format(text, format, culture) ?? Resx.FormatCellsDialog_Skipped);

				previewList.Items.Add(item);
			}

			previewList.EndUpdate();
		}
	}
}
