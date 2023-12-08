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
		private readonly string notebookID;
		private readonly string sectionID;


		public HashtagDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				// ...
			}

			palette = new MoreAutoCompleteList
			{
				HideListOnLostFocus = true
			};

			palette.SetAutoCompleteList(tagBox, palette);
			scopeBox.SelectedIndex = 0;
		}


		public HashtagDialog(string notebookID, string sectionID)
			: this()
		{
			this.notebookID = notebookID;
			this.sectionID = sectionID;
		}


		private void PopulateTags(object sender, EventArgs e)
		{
			var provider = new HashtagProvider();

			var names = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadTagNames(sectionID: sectionID),
				2 => provider.ReadTagNames(notebookID: notebookID),
				_ => provider.ReadTagNames(),
			};

			var recent = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadLatestTagNames(sectionID: sectionID),
				2 => provider.ReadLatestTagNames(notebookID: notebookID),
				_ => provider.ReadLatestTagNames(),
			};

			logger.WriteLine($"discovered {names.Count()} tags, {recent.Count()} mru");

			palette.LoadCommands(names.ToArray(), recent.ToArray());
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

			// allow "abc." to be interpreted as "%abc" but "abc" will be "%abc%"
			if (name.EndsWith("."))
			{
				name = name.Substring(0, name.Length - 1);
			}
			else if (!name.EndsWith("%"))
			{
				name = $"{name}%";
			}

			name = name.Replace('*', '%');

			var provider = new HashtagProvider();

			var tags = scopeBox.SelectedIndex switch
			{
				1 => provider.SearchTags(name, notebookID: notebookID),
				2 => provider.SearchTags(name, sectionID: sectionID),
				_ => provider.SearchTags(name)
			};

			if (tags.Any())
			{
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

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.AddRange(controls);
				contextPanel.ResumeLayout();
			}
			else
			{
				contextPanel.Controls.Clear();
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
