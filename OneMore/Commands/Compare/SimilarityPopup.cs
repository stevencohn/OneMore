//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A small, tooltip-like popup showing the Compare Hierarchy command's page-content
	/// similarity score and rubric breakdown (see SimilarityEngine), positioned near the
	/// invoking "Compare contents..." button. Dismissed by Esc or by clicking anywhere
	/// outside it, matching a tooltip/flyout rather than a normal dialog.
	/// </summary>
	internal partial class SimilarityPopup : MoreForm
	{
		private const int ContentWidth = 320;
		private const int BarHeight = 5;

		private readonly Color accentColor;
		private readonly Color trackColor;
		private readonly Color borderColor;


		/// <param name="leftName">The left (source) page's name</param>
		/// <param name="rightName">
		/// The right (target) page's name - may differ from <paramref name="leftName"/> when
		/// comparing an ad-hoc, unrelated pair rather than a matched row
		/// </param>
		/// <param name="result">The computed similarity result to display</param>
		public SimilarityPopup(string leftName, string rightName, SimilarityResult result)
		{
			InitializeComponent();

			var manager = ThemeManager.Instance;
			accentColor = manager.GetColor("HotTrack");
			trackColor = manager.GetColor("ControlLight");
			borderColor = manager.GetColor("ButtonBorder");

			// "Window" (the main dialog's own background) is too close to this borderless
			// popup's own border/track colors to read as a distinct surface floating over
			// the dialog; "Control" is the same alternate background NavigatorWindow already
			// uses for the same reason - a floating window that needs to look visually
			// separate from the plain white/dark Window background behind it
			BackColor = manager.GetColor("Control");

			// the matched-row case (today's only case until ad-hoc pairing) has the same
			// name on both sides, so keep that simple single-name look; only an ad-hoc
			// pair of genuinely different pages needs the "A ↔ B" form
			var headerText = leftName == rightName
				? leftName
				: string.Format(Resx.SimilarityPopup_pairFormat, leftName, rightName);

			Text = headerText;

			BuildContent(headerText, result);
		}


		private void BuildContent(string headerText, SimilarityResult result)
		{
			var root = new TableLayoutPanel
			{
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 1,
				Padding = new Padding(14, 12, 14, 12)
			};
			root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ContentWidth));

			AddRow(root, BuildHeader(headerText));
			AddRow(root, BuildOverall(result.Overall));

			foreach (var rubric in result.Rubrics)
			{
				AddRow(root, BuildRubricRow(rubric));
			}

			// drawn on root itself, last - after its own background fill but before any of its
			// children paint, none of which reach root's own edge thanks to its 14/12px Padding
			// - rather than via the form's own OnPaint: root's ambient-inherited BackColor
			// (unset, so it resolves to the form's own BackColor) spans the form's entire
			// ClientSize and paints over the form afterward, which is why a form-level border
			// here rendered invisible - root itself painting its own edge last does not
			root.Paint += (s, e) =>
			{
				using var pen = new Pen(borderColor);
				e.Graphics.DrawRectangle(pen, 0, 0, root.Width - 1, root.Height - 1);
			};

			Controls.Add(root);
		}


		private static void AddRow(TableLayoutPanel root, Control control)
		{
			root.RowCount++;
			root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			root.Controls.Add(control, 0, root.RowCount - 1);
		}


		private Control BuildHeader(string headerText)
		{
			var header = new TableLayoutPanel
			{
				AutoSize = true,
				ColumnCount = 2,
				Margin = new Padding(0, 0, 0, 10)
			};
			header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

			var nameLabel = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Text = headerText,
				MaximumSize = new Size(ContentWidth - 40, 0)
			};

			var closeLabel = new MoreLabel
			{
				AutoSize = true,
				Text = "✕",
				Cursor = Cursors.Hand,
				ThemedFore = "GrayText",
				AccessibleName = Resx.SimilarityPopup_closeAccessibleName,
				Margin = new Padding(8, 0, 0, 0)
			};
			closeLabel.Click += (s, e) => Close();

			header.Controls.Add(nameLabel, 0, 0);
			header.Controls.Add(closeLabel, 1, 0);

			return header;
		}


		private Control BuildOverall(double overall)
		{
			return new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font.FontFamily, 19f, FontStyle.Bold),
				ThemedFore = "HotTrack",
				Text = string.Format(Resx.SimilarityPopup_overallFormat, FormatPercent(overall)),
				Margin = new Padding(0, 0, 0, 14)
			};
		}


		private Control BuildRubricRow(RubricScore rubric)
		{
			var innerWidth = ContentWidth - 28;

			var panel = new TableLayoutPanel
			{
				AutoSize = true,
				ColumnCount = 1,
				RowCount = 3,
				Margin = new Padding(0, 0, 0, 10)
			};
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, innerWidth));
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			var labelRow = new TableLayoutPanel
			{
				AutoSize = true,
				ColumnCount = 2
			};
			labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

			var nameLabel = new MoreLabel
			{
				AutoSize = true,
				Text = string.Format(Resx.SimilarityPopup_weightFormat,
					rubric.Name, FormatPercent(rubric.Weight))
			};

			var scoreLabel = new MoreLabel
			{
				AutoSize = true,
				Font = new Font("Consolas", Font.SizeInPoints),
				Text = FormatPercent(rubric.Score)
			};

			labelRow.Controls.Add(nameLabel, 0, 0);
			labelRow.Controls.Add(scoreLabel, 1, 0);

			var bar = new Panel
			{
				Size = new Size(innerWidth, BarHeight),
				Margin = new Padding(0, 4, 0, 4)
			};
			bar.Paint += (s, e) =>
			{
				using var trackBrush = new SolidBrush(trackColor);
				e.Graphics.FillRectangle(trackBrush, 0, 0, bar.Width, bar.Height);

				var clamped = Math.Max(0.0, Math.Min(1.0, rubric.Score));
				var fillWidth = (int)Math.Round(bar.Width * clamped);
				if (fillWidth > 0)
				{
					using var fillBrush = new SolidBrush(accentColor);
					e.Graphics.FillRectangle(fillBrush, 0, 0, fillWidth, bar.Height);
				}
			};

			var reasoningLabel = new MoreLabel
			{
				AutoSize = true,
				ThemedFore = "GrayText",
				MaximumSize = new Size(innerWidth, 0),
				Text = rubric.Reasoning
			};

			panel.Controls.Add(labelRow, 0, 0);
			panel.Controls.Add(bar, 0, 1);
			panel.Controls.Add(reasoningLabel, 0, 2);

			return panel;
		}


		private static string FormatPercent(double value)
		{
			var clamped = Math.Max(0.0, Math.Min(1.0, value));
			return ((int)Math.Round(clamped * 100)).ToString(CultureInfo.InvariantCulture) + "%";
		}


		// click-away dismissal: this popup has no owner window to key off of, so losing
		// activation to anything else - including the CompareDialog it was invoked from -
		// is the signal to close, matching a tooltip/flyout rather than a normal dialog
		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			if (!IsDisposed)
			{
				Close();
			}
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
