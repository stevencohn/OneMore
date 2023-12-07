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
				logger.StartClock();

				var controls = new HashtagContextControl[tags.Count];
				var width = contextPanel.ClientSize.Width -
					(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2 - 20;

				for (var i = 0; i < tags.Count; i++)
				{
					controls[i] = new HashtagContextControl(tags[i])
					{
						Width = width
					};
				}

				logger.WriteTime($"expanded {tags.Count} hyperlinks", true);

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.AddRange(controls);
				contextPanel.ResumeLayout();

				logger.WriteTime("SearchTags done");
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			var width = contextPanel.ClientSize.Width -
				(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2;

			for (var i = 0; i < contextPanel.Controls.Count; i++)
			{
				contextPanel.Controls[i].Width = width;
			}
		}
	}
}
