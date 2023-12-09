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


		public enum Commands
		{
			Index,
			Copy,
			Move
		}


		public HashtagDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				// ...
			}

			palette = new MoreAutoCompleteList
			{
				HideListOnLostFocus = true,
				RecentKicker = "recent tags",
				OtherKicker = "all tags"
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


		public Commands Command { get; private set; }


		public IEnumerable<string> SelectedPages
		{
			get
			{
				for (var i = 0; i < contextPanel.Controls.Count; i++)
				{
					if (contextPanel.Controls[i] is HashtagContextControl item)
					{
						if (item.Checked)
						{
							yield return item.PageID;
						}
					}
				}
			}
		}


		private void PopulateTags(object sender, EventArgs e)
		{
			var provider = new HashtagProvider();

			var names = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadTagNames(notebookID: notebookID),
				2 => provider.ReadTagNames(sectionID: sectionID),
				_ => provider.ReadTagNames(),
			};

			var recent = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadLatestTagNames(notebookID: notebookID),
				2 => provider.ReadLatestTagNames(sectionID: sectionID),
				_ => provider.ReadLatestTagNames(),
			};

			logger.Verbose($"discovered {names.Count()} tags, {recent.Count()} mru");

			palette.LoadCommands(names.ToArray(), recent.ToArray());
		}


		private void DoPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Escape && !palette.IsPopupVisible)
			{
				Close();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				SearchTags(sender, e);
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

			logger.Verbose($"found {tags.Count} tags");

			if (tags.Any())
			{
				var items = CollateTags(tags);
				tags.Clear();

				var controls = new HashtagContextControl[items.Count];
				var width = contextPanel.ClientSize.Width -
					(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2 - 20;

				for (var i = 0; i < items.Count; i++)
				{
					controls[i] = new HashtagContextControl(items[i])
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


		private HashtagContexts CollateTags(Hashtags tags)
		{
			// transform Hashtags collection to HashtagContexts collection...

			var items = new HashtagContexts();

			// tags should be sorted by p.path, p.name so collate based on that assumption
			HashtagContext context = null;
			foreach (var tag in tags)
			{
				if (context == null || context.MoreID != tag.MoreID)
				{
					context = new HashtagContext(tag);
					items.Add(context);
				}
				else
				{
					// de-dupe the paragraphs; if there are multiple tags in one paragraph
					if (!context.Snippets.Exists(s => s.ObjectID == tag.ObjectID))
					{
						context.Snippets.Add(new HashtagSnippet(
							tag.ObjectID, tag.Snippet, tag.ScanTime));
					}
				}
			}

			return items;
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


		private void ToggleAllChecks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var ticked = sender == checkAllLink;
			for (var i = 0; i < contextPanel.Controls.Count; i++)
			{
				if (contextPanel.Controls[i] is HashtagContextControl item)
				{
					item.Checked = ticked;
				}
			}
		}


		private void DoSomething(object sender, EventArgs e)
		{
			if (sender == indexButton)
			{
				Command = Commands.Index;
			}
			else if (sender == copyButton)
			{
				Command = Commands.Copy;
			}
			else
			{
				Command = Commands.Move;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private void DoCancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
