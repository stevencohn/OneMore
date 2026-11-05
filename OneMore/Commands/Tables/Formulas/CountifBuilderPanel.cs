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
	/// </summary>
	internal class CountifBuilderPanel : MoreUserControl
	{
		private readonly MoreLinkLabel backLink;
		private readonly MoreTextBox rangeBox;
		private readonly MoreComboBox typeBox;
		private readonly MoreComboBox opBox;
		private readonly MoreTextBox valueBox;
		private readonly MoreLabel previewLabel;
		private readonly MoreButton insertButton;


		public CountifBuilderPanel()
		{
			SuspendLayout();

			backLink = new MoreLinkLabel
			{
				AutoSize = true, Location = new Point(8, 8), Text = "← Functions"
			};
			backLink.Click += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);

			var title = new MoreLabel
			{
				AutoSize = true, Location = new Point(8, 40), Text = "countif(range, criteria)"
			};

			rangeBox = new MoreTextBox { Location = new Point(8, 90), Width = 300 };
			typeBox = NewCombo(new Point(8, 150), 300, "Number", "Text", "True/False tag");
			opBox = NewCombo(new Point(8, 210), 140,
				"Equals", "Less than", "Greater than", "Not equal");
			valueBox = new MoreTextBox { Location = new Point(160, 210), Width = 148 };

			previewLabel = new MoreLabel
			{
				AutoSize = false, Location = new Point(8, 260), Size = new Size(300, 28)
			};

			insertButton = new MoreButton
			{
				Location = new Point(8, 296), Size = new Size(200, 36),
				Text = "Insert into formula"
			};
			insertButton.Click += (s, e) => Inserted?.Invoke(this, Expression);

			void Changed(object s, EventArgs e)
			{
				previewLabel.Text = Expression;
			}

			rangeBox.TextChanged += Changed;
			valueBox.TextChanged += Changed;
			typeBox.SelectedIndexChanged += Changed;
			opBox.SelectedIndexChanged += Changed;

			Controls.Add(backLink);
			Controls.Add(title);
			Controls.Add(Caption("1 · Range", 66));
			Controls.Add(rangeBox);
			Controls.Add(Caption("2 · Match type", 126));
			Controls.Add(typeBox);
			Controls.Add(Caption("3 · Operator and value", 186));
			Controls.Add(opBox);
			Controls.Add(valueBox);
			Controls.Add(previewLabel);
			Controls.Add(insertButton);

			ResumeLayout(false);
			PerformLayout();
			Changed(null, EventArgs.Empty);
		}


		public event EventHandler BackClicked;

		public event EventHandler<string> Inserted;


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


		private static MoreLabel Caption(string text, int y)
		{
			return new MoreLabel { AutoSize = true, Location = new Point(8, y), Text = text };
		}


		private static MoreComboBox NewCombo(Point location, int width, params string[] items)
		{
			var box = new MoreComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList, Location = location, Width = width
			};

			box.Items.AddRange(items);
			box.SelectedIndex = 0;
			return box;
		}
	}
}
