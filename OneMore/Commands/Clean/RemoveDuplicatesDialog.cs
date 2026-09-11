//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class RemoveDuplicatesDialog : UI.MoreForm
	{
		// two collapsed groups rather than one checkbox per rubric: each group's Rubric value
		// ORs together the three SimilarityEngine rubrics it stands for, and its Text is a
		// resx-supplied "Title\nSubtitle" pair (MoreCheckBox natively lays out an embedded
		// newline as two lines - see CreateMetricBox for why Size must be set before Text).
		// Presentation defaults unchecked since its Media rubric is what drives the more
		// expensive OneNote.PageDetail.BinaryData page fetch
		private static readonly (Rubric Rubric, string Text, bool DefaultChecked, string Tip)[]
			MetricDefinitions =
			{
				(Rubric.TfIdf | Rubric.Lexical | Rubric.Structural,
					Resx.RemoveDuplicatesDialog_contentSimilarityBox, true,
					Resx.RemoveDuplicatesDialog_contentSimilarityTip),

				(Rubric.Stylistic | Rubric.Entity | Rubric.Media,
					Resx.RemoveDuplicatesDialog_presentationSimilarityBox, false,
					Resx.RemoveDuplicatesDialog_presentationSimilarityTip)
			};

		// row geometry within metricsBox - all three checkboxes (the two metric groups plus
		// "Show only exact duplicates") share this same left-justified column and spacing
		// scheme, computed together here so the group box's own Size (see .Designer.cs) stays
		// in sync with however many rows/gaps actually get laid out
		private const int MetricRowX = 20;
		private const int MetricRowWidth = 527;
		private const int MetricBoxHeight = 36;   // a two-line "combined metric" checkbox
		private const int MetricFirstRowY = 36;
		private const int MetricRowGap = 14;
		private const int ExactOnlyRowGap = 18;    // a bit more air before the mode toggle
		private const int ExactOnlyBoxHeight = 25;

		private readonly List<(Rubric Rubric, MoreCheckBox Box)> metrics = new();
		private MoreCheckBox exactOnlyBox;


		public RemoveDuplicatesDialog()
		{
			InitializeComponent();

			var y = MetricFirstRowY;
			for (var i = 0; i < MetricDefinitions.Length; i++)
			{
				var definition = MetricDefinitions[i];
				var box = CreateMetricBox(definition, y);
				metricsBox.Controls.Add(box);
				metrics.Add((definition.Rubric, box));
				y += MetricBoxHeight + MetricRowGap;
			}

			y += ExactOnlyRowGap - MetricRowGap;
			exactOnlyBox = CreateExactOnlyBox(y);
			metricsBox.Controls.Add(exactOnlyBox);

			if (NeedsLocalizing())
			{
				Text = Resx.RemoveDuplicatesDialog_Text;

				Localize(new string[]
				{
					"metricsBox",
					"scopeGroupBox=word_Scope",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		private MoreCheckBox CreateMetricBox(
			(Rubric Rubric, string Text, bool DefaultChecked, string Tip) definition, int y)
		{
			var box = new MoreCheckBox
			{
				AutoSize = false,
				Checked = definition.DefaultChecked,
				Cursor = Cursors.Hand,
				Location = new Point(MetricRowX, y),
				// set before Text: MoreCheckBox.OnTextChanged only auto-grows when the
				// control's current bounds are smaller than what it measures for the text,
				// and its own auto-height calc only accounts for a single line, which would
				// clip the second line of a two-line label if it ran before this size existed
				Size = new Size(MetricRowWidth, MetricBoxHeight),
				StylizeImage = false,
				Text = definition.Text,
				UseVisualStyleBackColor = true
			};

			tooltip.SetToolTip(box, definition.Tip);
			return box;
		}


		private MoreCheckBox CreateExactOnlyBox(int y)
		{
			return new MoreCheckBox
			{
				Cursor = Cursors.Hand,
				Location = new Point(MetricRowX, y),
				Size = new Size(MetricRowWidth, ExactOnlyBoxHeight),
				StylizeImage = false,
				Text = Resx.RemoveDuplicatesDialog_exactOnlyBox_Text,
				UseVisualStyleBackColor = true
			};
		}


		/// <summary>
		/// The similarity rubrics the user checked in the metrics group, combined into a
		/// single flags value for SimilarityOptions/RemoveDuplicatesCommand.
		/// </summary>
		public Rubric EnabledRubrics
		{
			get
			{
				var enabled = Rubric.None;
				foreach (var (rubric, box) in metrics)
				{
					if (box.Checked)
					{
						enabled |= rubric;
					}
				}

				return enabled;
			}
		}


		/// <summary>
		/// True unless the user checked "Show only exact duplicates" - the near-duplicate
		/// scoring pass runs by default; checking that box narrows results down to hash-proven
		/// 100% matches only.
		/// </summary>
		public bool DetectSimilar => !exactOnlyBox.Checked;


		public SelectorScope Scope => scopeSelector.Scope;


		public IEnumerable<string> SelectedNotebooks => scopeSelector.SelectedNotebooks;
	}
}
