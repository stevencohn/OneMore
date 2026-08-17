//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using Resx = Properties.Resources;


	internal partial class RemoveDuplicatesDialog : UI.MoreForm
	{
		public enum DepthKind
		{
			Simple,
			Basic,
			Deep
		}


		public RemoveDuplicatesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.RemoveDuplicatesDialog_Text;

				Localize(new string[]
				{
					"depthBox",
					"simpleRadio",
					"basicRadio",
					"deepRadio",
					"includeTitlesBox",
					"fuzzyBox",
					"scopeGroupBox=word_Scope",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			tooltip.SetToolTip(simpleRadio, Resx.RemoveDuplicatesDialog_simpleRadioTip);
			tooltip.SetToolTip(basicRadio, Resx.RemoveDuplicatesDialog_basicRadioTip);
			tooltip.SetToolTip(deepRadio, Resx.RemoveDuplicatesDialog_deepRadioTip);

			deepRadio.CheckedChanged += (s, e) =>
			{
				fuzzyBox.Enabled = !deepRadio.Checked;
				if (deepRadio.Checked)
				{
					fuzzyBox.Checked = false;
				}
			};
		}


		public DepthKind Depth =>
			basicRadio.Checked
				? DepthKind.Basic
				: (simpleRadio.Checked ? DepthKind.Simple : DepthKind.Deep);


		public bool IncludeTitles => includeTitlesBox.Checked;


		public bool DetectSimilar => fuzzyBox.Checked;


		public SelectorScope Scope => scopeSelector.Scope;


		public IEnumerable<string> SelectedNotebooks => scopeSelector.SelectedNotebooks;
	}
}
