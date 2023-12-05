//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;

	internal partial class HashtagDialog : LocalizableForm
	{
		private readonly MoreAutoCompleteList palette;
		private IEnumerable<string> names;
		private IEnumerable<string> recentNames;


		public HashtagDialog()
		{
			InitializeComponent();

			palette = new MoreAutoCompleteList
			{
				AllowEscapeToHide = true
			};

			palette.SetAutoCompleteList(tagBox, palette);

			if (NeedsLocalizing())
			{
				// ...
			}
		}


		public void PopulateTags(IEnumerable<string> names, IEnumerable<string> recentNames)
		{
			palette.LoadCommands(names.ToArray(), recentNames.ToArray());
			this.names = names;
			this.recentNames = recentNames;

			//clearLink.Enabled = recentNames?.Length > 0;
		}


		private void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}


		private void DoPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Escape && !palette.IsPopupVisible)
			{
				Close();
			}
		}
	}
}
