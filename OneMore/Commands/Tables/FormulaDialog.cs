//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class FormulaDialog : UI.MoreForm
	{
		private readonly Calculator calculator;
		private readonly Table table;

		// one list of entries and one list view per reference tab
		private readonly List<ReferenceEntry>[] entries = new List<ReferenceEntry>[4];
		private readonly ReferenceListBox[] lists = new ReferenceListBox[4];

		private IReadOnlyList<string> targetNames = new[] { "A1" };
		private TableSelectionRange targetRange = TableSelectionRange.Single;
		private List<Variable> customVariables = new();
		private PictureBox boltBox;


		public FormulaDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.FormulaDialog_Text;

				Localize(new string[]
				{
					"formulaLabel",
					"formatLabel=word_Format",
					"decLabel",
					"tagBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel",
					"searchLabel=FormulaDialog_search",
					"hintLabel=FormulaDialog_hint",
					"insertButton=FormulaDialog_insert"
				});

				formatBox.Items.Clear();
				formatBox.Items.AddRange(Resx.FormulaDialog_formatBox_Items.Split('\n'));
			}

			formatBox.SelectedIndex = 0;

			calculator = new Calculator();
			calculator.GetCellValue += GetCellValue;

			BuildTabs();

			// refresh the preview when format or decimal places change
			formatBox.SelectedIndexChanged += ChangedFormula;
			decimalBox.ValueChanged += ChangedFormula;
			tagBox.CheckedChanged += ChangedFormula;

			countifPanel.BackClicked += (s, e) => ShowCountif(false);
			countifPanel.Inserted += (s, expression) =>
			{
				formulaBox.InsertAtCursor(expression);
				ShowCountif(false);
			};
		}


		public FormulaDialog(Table table)
			: this()
		{
			this.table = table;

			calculator.SetVariable("tablecols", table.ColumnCount);
			calculator.SetVariable("tablerows", table.RowCount);

			var cell = table.GetSelectedCells(out _).First();
			calculator.SetVariable("col", cell.ColNum);
			calculator.SetVariable("row", cell.RowNum);

			customVariables = LoadVariables(calculator);
		}


		public int DecimalPlaces
		{
			get { return (int)decimalBox.Value; }
			set { decimalBox.Value = value; }
		}


		public FormulaFormat Format
		{
			get { return (FormulaFormat)formatBox.SelectedIndex; }
			set { formatBox.SelectedIndex = (int)value; }
		}


		public string Formula
		{
			get => formulaBox.Text;
			set => formulaBox.Text = value;
		}


		public bool Tagged
		{
			get => tagBox.Checked;
			set => tagBox.Checked = value;
		}


		/// <summary>
		/// Sets the names of the selected cells, in order, and how they are arranged. Drives the
		/// header chip, the result caption and the per-cell formula preview.
		/// </summary>
		public void SetTargetCells(IReadOnlyList<string> names, TableSelectionRange range)
		{
			targetNames = names;
			targetRange = range;

			var multi = names.Count > 1;
			var first = names[0];

			targetLabel.Text = multi
				? string.Format(Resx.FormulaDialog_target_many, first, names[names.Count - 1], names.Count)
				: string.Format(Resx.FormulaDialog_target_one, first);

			resultCaption.Text = multi
				? string.Format(Resx.FormulaDialog_resultInMany, first, names.Count - 1)
				: string.Format(Resx.FormulaDialog_resultIn, first);

			tagHint.Text = multi
				? string.Format(Resx.FormulaDialog_tagHintMany, names.Count)
				: Resx.FormulaDialog_tagHint;

			okButton.Text = multi
				? string.Format(Resx.FormulaDialog_okMany, names.Count)
				: Resx.word_OK;

			if (multi)
			{
				formulaLabel.Text = string.Format(Resx.FormulaDialog_formulaFirst, first);
				cellsLabel.Text = Resx.FormulaDialog_cellsLabel;
				cellsLabel.Visible = true;
				cellsBox.Visible = true;
			}
			else
			{
				// give the space of the per-cell list to the result card
				resultPanel.Height += formatLabel.Top - cellsLabel.Top;
			}

			ChangedFormula(this, EventArgs.Empty);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AlignRightMargin();
			PopulateReference();
			tagHint.Text = string.IsNullOrEmpty(tagHint.Text) ? Resx.FormulaDialog_tagHint : tagHint.Text;
			countifPanel.SetRange("A1:cell(0,-1)");
			ChangedFormula(this, EventArgs.Empty);
			formulaBox.Focus();
		}


		/// <summary>
		/// Makes the right margin match the left margin, measured in actual pixels, so the
		/// reference panel never touches the edge of the dialog at any DPI.
		/// </summary>
		private void AlignRightMargin()
		{
			var right = ClientSize.Width - targetLabel.Left;

			foreach (Control control in new Control[]
				{ searchBox, tabs, countifPanel })
			{
				control.Width = right - control.Left;
			}

			var gap = cancelButton.Left - okButton.Right;
			insertButton.Left = right - insertButton.Width;
			cancelButton.Left = right - cancelButton.Width;
			okButton.Left = cancelButton.Left - gap - okButton.Width;
		}


		private static List<Variable> LoadVariables(Calculator calculator)
		{
			using var provider = new VariableProvider();
			var variables = provider.ReadVariables();
			foreach (var variable in variables)
			{
				calculator.SetVariable(variable.Name, variable.Value);
			}

			return variables;
		}


		#region Reference panel
		private void BuildTabs()
		{
			var names = Resx.FormulaDialog_tabs.Split('\n');
			tabs.TabPages.Clear();

			for (var i = 0; i < names.Length && i < lists.Length; i++)
			{
				var list = new ReferenceListBox { Dock = DockStyle.Fill };

				list.SelectedIndexChanged += (s, e) => ShowDetail();
				list.DoubleClick += InsertSelected;

				var page = new TabPage(names[i].Trim()) { Name = $"tab{i}" };
				page.Controls.Add(list);
				tabs.TabPages.Add(page);
				lists[i] = list;
			}
		}


		private void PopulateReference()
		{
			entries[0] = FormulaReference.Functions();
			entries[1] = FormulaReference.Operators();
			entries[2] = FormulaReference.Variables(customVariables);
			entries[3] = FormulaReference.Cells(targetNames[0]);

			for (var i = 0; i < lists.Length; i++)
			{
				FillList(i, searchBox.Text);
			}

			ShowDetail();
		}


		private void FillList(int index, string filter)
		{
			lists[index].SetEntries(entries[index].Where(e => e.Matches(filter)));
		}


		private ReferenceEntry SelectedEntry()
		{
			return lists[tabs.SelectedIndex].SelectedEntry;
		}


		private void ShowDetail()
		{
			var entry = SelectedEntry();
			insertButton.Enabled = entry is not null;
			insertButton.Text = entry is { Drill: true }
				? Resx.FormulaDialog_openBuilder
				: Resx.FormulaDialog_insert;
		}


		private void ChangedSearch(object sender, EventArgs e)
		{
			if (entries[0] is null)
			{
				return;
			}

			for (var i = 0; i < lists.Length; i++)
			{
				FillList(i, searchBox.Text.Trim());
			}

			ShowDetail();
		}


		private void ChangedTab(object sender, EventArgs e)
		{
			ShowDetail();
		}


		private void InsertSelected(object sender, EventArgs e)
		{
			var entry = SelectedEntry();
			if (entry is null)
			{
				return;
			}

			if (entry.Drill)
			{
				ShowCountif(true);
				return;
			}

			formulaBox.InsertAtCursor(entry.Insert);
		}


		private void ShowCountif(bool show)
		{
			countifPanel.Visible = show;
			tabs.Visible = !show;
			searchBox.Visible = !show;
			searchLabel.Visible = !show;
			insertButton.Visible = !show;
		}
		#endregion Reference panel


		private void ChangedFormula(object sender, EventArgs e)
		{
			var formula = formulaBox.Text.Trim();

			formatHint.Text = Format == FormulaFormat.Time ? Resx.FormulaDialog_timeHint : string.Empty;
			decimalBox.Enabled = Format != FormulaFormat.Time;

			if (formula.Length > 0)
			{
				try
				{
					var result = calculator.Compute(formula);
					var text = FormatResult(result);

					formulaBox.Invalid = false;
					resultValue.ForeColor = manager.GetColor("ControlText");
					resultValue.Text = text;
					ShowBolt(tagBox.Checked);
					resultDetail.Text = $"{formatBox.Text} · {DecimalPlaces} {decLabel.Text.TrimEnd(':')}";

					okButton.Enabled = true;
				}
				catch (Exception exc)
				{
					formulaBox.Invalid = true;
					resultValue.ForeColor = manager.GetColor("ErrorText");
					resultValue.Text = exc.Message;
					ShowBolt(false);
					resultDetail.Text = Resx.FormulaDialog_status_Invalid;

					okButton.Enabled = false;
				}
			}
			else
			{
				formulaBox.Invalid = false;
				resultValue.ForeColor = manager.GetColor("GrayText");
				resultValue.Text = Resx.FormulaDialog_emptyResult;
				ShowBolt(false);
				resultDetail.Text = string.Empty;
				okButton.Enabled = false;
			}

			UpdateCellList(formula);
		}


		/// <summary>
		/// Shows a yellow lightning bolt, as OneNote draws the tag on the page, before the
		/// result value; the value is nudged right by padding so the bolt has room.
		/// </summary>
		private void ShowBolt(bool show)
		{
			if (boltBox is null)
			{
				boltBox = new PictureBox
				{
					BackColor = System.Drawing.Color.Transparent,
					SizeMode = PictureBoxSizeMode.Zoom,
					Image = CreateBoltImage(),
					Visible = false
				};

				resultPanel.Controls.Add(boltBox);
				boltBox.BringToFront();
			}

			var size = resultValue.Font.Height;
			boltBox.Size = new System.Drawing.Size(size, size);
			boltBox.Location = new System.Drawing.Point(resultValue.Left, resultValue.Top + 2);
			boltBox.Visible = show;

			resultValue.Padding = new Padding(show ? size + (size / 4) : 0, 0, 0, 0);
		}


		private static System.Drawing.Bitmap CreateBoltImage()
		{
			var bitmap = new System.Drawing.Bitmap(64, 64);
			using var g = System.Drawing.Graphics.FromImage(bitmap);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			var points = new System.Drawing.PointF[]
			{
				new(38, 2), new(10, 36), new(28, 36), new(22, 62), new(54, 24), new(34, 24), new(46, 2)
			};

			using var fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 204, 0));
			using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(184, 134, 0), 2F);
			g.FillPolygon(fill, points);
			g.DrawPolygon(pen, points);
			return bitmap;
		}


		private void UpdateCellList(string formula)
		{
			if (!cellsBox.Visible)
			{
				return;
			}

			var builder = new StringBuilder();
			for (var i = 0; i < targetNames.Count; i++)
			{
				var shifted = FormulaShifter.Shift(formula, i, targetRange);
				builder.AppendLine($"{targetNames[i],-5}{shifted}");
			}

			cellsBox.Text = builder.ToString();
		}


		private string FormatResult(double result)
		{
			var dplaces = DecimalPlaces;

			switch (Format)
			{
				case FormulaFormat.Currency:
					return result.ToString($"C{dplaces}", AddIn.Locale);

				case FormulaFormat.Number:
					return result.ToString($"N{dplaces}", AddIn.Locale);

				case FormulaFormat.Percentage:
					return (result / 100).ToString($"P{dplaces}", AddIn.Locale);

				case FormulaFormat.Time:
					return TimeSpan.FromMilliseconds(result).ToString();

				default:
					return $"{result}";
			}
		}


		private void GetCellValue(object sender, GetCellValueEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell is null)
			{
				e.Value = string.Empty;
				return;
			}

			var text = cell.GetTextLines()
				.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim()
				?? string.Empty;

			if (Processor.TryParseNumber(text, out var dvalue))
			{
				e.Value = dvalue.ToString(CultureInfo.InvariantCulture);
			}
			else if (TimeSpan.TryParse(text, AddIn.Locale, out var tvalue))
			{
				e.Value = tvalue.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				e.Value = text;
			}

			logger.Verbose($"FormulaDialog.GetCellValue({e.Name}) = [{e.Value}]");
		}
	}
}
