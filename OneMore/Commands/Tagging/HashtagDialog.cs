//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using NStandard;
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
				HideListOnLostFocus = true
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


		private void SearchTags(object sender, EventArgs e)
		{
			var name = tagBox.Text.Trim();
			if (name.IsNullOrEmpty())
			{
				return;
			}

			if (!name.StartsWith("##") && !name.StartsWith("%"))
			{
				name = $"%{name}";
			}

			var provider = new HashtagProvider();
			var tags = provider.SearchTags(name);

			if (tags.Any())
			{
				var one = new OneNote();
				var controls = new HashtagContextControl[tags.Count];

				for (var i=0; i < tags.Count; i++)
				{
					var tag = tags[i];
					tag.PageURL = one.GetHyperlink(tag.PageID, string.Empty);
					tag.ObjectURL= one.GetHyperlink(tag.PageID, tag.ObjectID);

					controls[i] = new HashtagContextControl(tag);
				}

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.AddRange(controls);
				contextPanel.ResumeLayout();
			}
		}
	}
}
