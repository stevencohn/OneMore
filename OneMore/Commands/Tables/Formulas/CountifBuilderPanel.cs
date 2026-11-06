//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Builds a countif(range, criteria) call from a range, match type, operator and value.
	/// Fields are laid out from the actual font height so spacing holds at any DPI.
	/// </summary>
	internal class CountifBuilderPanel : MoreUserControl
	{
		private readonly MoreLinkLabel backLink;
		private readonly MoreLabel title;
		private readonly MoreLabel help;
		private readonly MoreLabel opsHelp;
		private readonly MoreLabel rangeCaption;
		private readonly MoreTextBox rangeBox;
		private readonly MoreLabel typeCaption;
		private readonly MoreComboBox typeBox;
		private readonly MoreLabel opCaption;
		private readonly MoreComboBox opBox;
		private readonly MoreLabel valueCaption;
		private readonly MoreTextBox valueBox;
		private readonly MoreLabel previewLabel;
		private readonly MoreButton insertButton;


		public CountifBuilderPanel()
		{
			SuspendLayout();

			backLink = new MoreLinkLabel { AutoSize = true, Text = "← Functions" };
			backLink.Click += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);

			// authored (unscaled) size; the control's OnLoad scales explicit fonts once
			title = new MoreLabel
			{
				AutoSize = true,
				Text = "countif(range, criteria)",
				Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold)
			};

			help = new MoreLabel
			{
				AutoSize = false,
				Text = Properties.Resources.FormulaDialog_countifHelp,
				ThemedFore = "GrayText"
			};

			opsHelp = new MoreLabel
			{
				AutoSize = false,
				Text = Properties.Resources.FormulaDialog_countifOperators,
				ThemedFore = "GrayText"
			};

			rangeCaption = Caption("1 · Range");
			rangeBox = new MoreTextBox();

			typeCaption = Caption("2 · Match type");
			typeBox = NewCombo("Number", "Text", "True/False tag");

			opCaption = Caption("3 · Operator");
			opBox = NewCombo("Equals", "Less than", "Greater than", "Not equal");

			valueCaption = Caption("Value");
			valueBox = new MoreTextBox();

			// the sample formula is drawn as a card: bold monospaced text inside a dashed border;
			// authored (unscaled) font, scaled once by this control's OnLoad
			previewLabel = new MoreLabel
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = new Font("Consolas", 9F, FontStyle.Bold)
			};

			previewLabel.Paint += PaintPreviewBorder;

			insertButton = new MoreButton { Text = "Insert into formula" };
			insertButton.Click += (s, e) => Inserted?.Invoke(this, Expression);

			void Changed(object s, EventArgs e)
			{
				previewLabel.Text = Expression;
			}

			rangeBox.TextChanged += Changed;
			valueBox.TextChanged += Changed;
			typeBox.SelectedIndexChanged += Changed;
			opBox.SelectedIndexChanged += Changed;

			Controls.AddRange(new Control[]
			{
				backLink, title, help,
				rangeCaption, rangeBox,
				typeCaption, typeBox,
				opCaption, opBox,
				valueCaption, valueBox,
				opsHelp, previewLabel, insertButton
			});

			ResumeLayout(false);
			Changed(null, EventArgs.Empty);
			LayoutFields();
		}


		public event EventHandler BackClicked;

		public event EventHandler<string> Inserted;


		/// <summary>
		/// Sets whether the builder is editing a countif already in the formula, in which
		/// case the button reads "Update formula" instead of "Insert into formula".
		/// </summary>
		public bool Updating
		{
			set => insertButton.Text = value ? "Update formula" : "Insert into formula";
		}


		/// <summary>
		/// Gets the countif expression as currently built.
		/// </summary>
		public string Expression
		{
			get
			{
				var range = string.IsNullOrWhiteSpace(rangeBox.Text) ? "range" : rangeBox.Text.Trim();
				var value = valueBox.Text.Trim();

				var op = opBox.SelectedIndex switch
				{
					1 => "< ",
					2 => "> ",
					3 => "! ",
					_ => string.Empty
				};

				return $"countif({range}, {op}{value})";
			}
		}


		/// <summary>
		/// Sets the default range, for example A1:cell(0,-1), when first shown.
		/// </summary>
		public void SetRange(string range)
		{
			if (string.IsNullOrWhiteSpace(rangeBox.Text))
			{
				rangeBox.Text = range;
			}
		}


		/// <summary>
		/// Finds the countif call in the given formula that contains the caret, or the only
		/// countif call if the caret is not within one, and seeds the builder fields from its
		/// parameters.
		/// </summary>
		/// <param name="formula">The formula text</param>
		/// <param name="caret">The caret position within the formula</param>
		/// <param name="start">The start index of the call within the formula</param>
		/// <param name="length">The length of the call, including parentheses</param>
		/// <returns>True if a countif call was found and the fields were seeded</returns>
		public bool Seed(string formula, int caret, out int start, out int length)
		{
			start = 0;
			length = 0;

			if (string.IsNullOrEmpty(formula))
			{
				return false;
			}

			const string Name = "countif(";
			var found = 0;
			var index = 0;
			var bestStart = -1;
			var bestEnd = -1;
			var onlyStart = -1;
			var onlyEnd = -1;

			while ((index = formula.IndexOf(Name, index, StringComparison.OrdinalIgnoreCase)) >= 0)
			{
				var end = FindClose(formula, index + Name.Length - 1);
				found++;
				onlyStart = index;
				onlyEnd = end;

				if (caret >= index && caret <= end + 1 && bestStart < 0)
				{
					bestStart = index;
					bestEnd = end;
				}

				index += Name.Length;
			}

			if (bestStart < 0)
			{
				if (found != 1)
				{
					return false;
				}

				bestStart = onlyStart;
				bestEnd = onlyEnd;
			}

			var open = bestStart + Name.Length - 1;
			var innerEnd = formula[bestEnd] == ')' ? bestEnd : bestEnd + 1;
			var inner = formula.Substring(open + 1, innerEnd - open - 1);
			var comma = FindTopLevelComma(inner);

			var range = (comma < 0 ? inner : inner.Substring(0, comma)).Trim();
			var criteria = comma < 0 ? string.Empty : inner.Substring(comma + 1).Trim();

			var opIndex = 0;
			if (criteria.Length > 0)
			{
				var c = criteria[0];
				if (c == '<' || c == '>' || c == '!')
				{
					opIndex = c == '<' ? 1 : c == '>' ? 2 : 3;
					criteria = criteria.Substring(1).Trim();
				}
			}

			rangeBox.Text = range;
			opBox.SelectedIndex = opIndex;
			valueBox.Text = criteria;

			typeBox.SelectedIndex = double.TryParse(criteria, out _) ? 0 : 1;

			start = bestStart;
			length = bestEnd - bestStart + 1;
			return true;
		}


		// returns the index of the matching close paren, or the last index if unbalanced
		private static int FindClose(string text, int open)
		{
			var depth = 0;
			for (var i = open; i < text.Length; i++)
			{
				if (text[i] == '(')
				{
					depth++;
				}
				else if (text[i] == ')')
				{
					depth--;
					if (depth == 0)
					{
						return i;
					}
				}
			}

			return text.Length - 1;
		}


		// returns the index of the first comma not nested within parentheses, or -1
		private static int FindTopLevelComma(string text)
		{
			var depth = 0;
			for (var i = 0; i < text.Length; i++)
			{
				switch (text[i])
				{
					case '(':
						depth++;
						break;
					case ')':
						depth--;
						break;
					case ',' when depth == 0:
						return i;
				}
			}

			return -1;
		}


		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			LayoutFields();
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// explicit fonts were just scaled, so the measurements must be redone
			LayoutFields();
		}


		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			LayoutFields();
		}


		// stacks the fields top to bottom with generous gaps, all measured from the font height
		private void LayoutFields()
		{
			if (backLink is null || insertButton is null)
			{
				return;
			}

			var unit = Font.Height;
			var margin = unit / 2;
			var group = (int)(unit * 1.25);  // space between field groups
			var inner = unit / 3;            // space between a caption and its field
			var width = Math.Max(1, ClientSize.Width - (margin * 2));
			var y = margin;

			backLink.Location = new Point(margin, y);
			y += backLink.Height + group;

			title.Location = new Point(margin, y);
			y += title.Height + inner;

			// room for up to three wrapped lines of help text
			help.SetBounds(margin, y, width, unit * 3);
			y += help.Height + (group / 2);

			y = Place(rangeCaption, rangeBox, margin, y, width, inner, group);
			y = Place(typeCaption, typeBox, margin, y, width, inner, group);

			// operator and value share a row
			var gap = unit;
			var half = (width - gap) / 2;
			opCaption.Location = new Point(margin, y);
			valueCaption.Location = new Point(margin + half + gap, y);
			y += opCaption.Height + inner;

			opBox.SetBounds(margin, y, half, opBox.Height);
			valueBox.SetBounds(margin + half + gap, y, half, valueBox.Height);
			y += Math.Max(opBox.Height, valueBox.Height) + (group / 2);

			// operator legend, up to three wrapped lines
			opsHelp.SetBounds(margin, y, width, (int)(unit * 3.6));
			y += opsHelp.Height + inner;

			previewLabel.Padding = new Padding(unit / 2, 0, unit / 2, 0);
			previewLabel.SetBounds(margin, y, width, (int)(unit * 3));
			y += previewLabel.Height + group;

			insertButton.SetBounds(margin, y, Math.Min(width, unit * 14), (int)(unit * 2.4));
		}


		private void PaintPreviewBorder(object sender, PaintEventArgs e)
		{
			using var pen = new Pen(manager.GetColor("ActiveBorder"))
			{
				DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
			};

			e.Graphics.DrawRectangle(pen, 0, 0, previewLabel.Width - 1, previewLabel.Height - 1);
		}


		private static int Place(
			Label caption, Control field, int margin, int y, int width, int inner, int group)
		{
			caption.Location = new Point(margin, y);
			y += caption.Height + inner;

			field.SetBounds(margin, y, width, field.Height);
			return y + field.Height + group;
		}


		private static MoreLabel Caption(string text)
		{
			return new MoreLabel { AutoSize = true, Text = text };
		}


		private static MoreComboBox NewCombo(params string[] items)
		{
			var box = new MoreComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
			box.Items.AddRange(items);
			box.SelectedIndex = 0;
			return box;
		}
	}
}
