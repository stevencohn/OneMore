//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Clean
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemoveDuplicatesDialog : UI.LocalizableForm
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
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public DepthKind Depth =>
			basicRadio.Checked
				? DepthKind.Basic
				: (simpleRadio.Checked ? DepthKind.Simple : DepthKind.Deep);


		public SelectorScope Scope => scopeSelector.Scope;


		public IEnumerable<string> SelectedNotebooks => scopeSelector.SelectedNotebooks;
	}
}
