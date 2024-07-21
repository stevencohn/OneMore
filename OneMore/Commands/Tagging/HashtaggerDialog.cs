//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtaggerDialog : MoreForm
	{
		private const string SettingsKey = "Hashtagger";

		private readonly Page page;
		private readonly Color tagBack;
		private bool recentLoaded;
		private bool commonLoaded;


		public HashtaggerDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.HashtaggerDialog_Text;

				Localize(new string[]
				{
					"tagsLabel",
					"bankBox",
					"clearLink=word_Clear",
					"recentGroup",
					"commonGroup",
					"cloudGroup",
					"showRecentMenuItem=HashtaggerDialog_hideRecentMenuItem",
					"showCommonMenuItem=HashtaggerDialog_hideCommonMenuItem",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			tagBack = ThemeManager.Instance.GetColor("Control");
		}


		public HashtaggerDialog(Page page)
			: this()
		{
			this.page = page;
		}


		/// <summary>
		/// Gets a value indicating that tags should be added to the page tag bank.
		/// Default is to add at the insertion point.
		/// </summary>
		public bool AddToBank => bankBox.Checked;



		/// <summary>
		/// Gets the string containing the selected tags
		/// </summary>
		public string Tags => tagsBox.Text;


		private void LoadTagsOnLoad(object sender, EventArgs e)
		{
			var provider = new HashtagProvider();
			var settings = new SettingsProvider().GetCollection(SettingsKey);

			if (settings.Get("showRecent", true))
			{
				LoadRecent(provider);
			}
			else
			{
				recentGroup.Visible = false;
			}

			if (settings.Get("showCommon", true))
			{
				LoadCommon();
			}
			else
			{
				commonGroup.Visible = false;
			}

			var tags = provider.ReadTagNames();
			if (tags.Any())
			{
				foreach (var word in tags.Select(w => new PageReader.CountedWord(w, 0)))
				{
					cloudFlow.Controls.Add(MakeLabel(word));
				}
			}


			ResizeFlows(sender, e);
		}


		private void LoadRecent(HashtagProvider provider)
		{
			var latest = provider.ReadLatestTagNames();
			if (latest.Any())
			{
				foreach (var word in latest.Select(w => new PageReader.CountedWord(w, 0)))
				{
					recentFlow.Controls.Add(MakeLabel(word));
				}
			}

			recentLoaded = true;
		}


		private void LoadCommon()
		{
			var reader = new PageReader(page);
			var words = reader.ReadCommonWords();

			if (words.Any())
			{
				// keep order of most recent first
				foreach (var word in words)
				{
					commonFlow.Controls.Add(MakeLabel(word));
				}
			}

			commonLoaded = true;
		}


		private Label MakeLabel(PageReader.CountedWord word)
		{
			var manager = ThemeManager.Instance;

			var label = new MoreLabel
			{
				Name = word.Word.Replace("#", ""),
				Tag = word,
				AutoSize = true,
				BorderStyle = BorderStyle.FixedSingle,
				Cursor = Cursors.Hand,
				Margin = new Padding(4),
				Padding = new Padding(4, 5, 4, 5),
				Text = word.Count == 0 ? word.Word : $"{word.Word} ({word.Count})",
				BackColor = tagBack,
				ForeColor = manager.GetColor("ControlText"),
				ThemedBack = "Control",
				ThemedFore = "ControlText"
			};

			label.Click += (sender, e) =>
			{
				if (sender is Label label &&
					label.Tag is PageReader.CountedWord word)
				{
					EditTags(word.Word);
				}
			};

			label.MouseEnter += (sender, e) =>
			{
				((Label)sender).BackColor = ThemeManager.Instance.GetColor("ButtonHighlight");
			};

			label.MouseLeave += (sender, e) =>
			{
				((Label)sender).BackColor = tagBack;
			};

			return label;
		}


		private void ResizeFlows(object sender, EventArgs e)
		{
			var width = mainFlow.Width -
				mainFlow.Padding.Left - mainFlow.Padding.Right -
				(SystemInformation.VerticalScrollBarWidth * 2);

			recentGroup.Width = width;
			var size = new Size(width, CalculateFlowHeight(recentFlow));
			recentGroup.Size = size;
			recentGroup.MaximumSize = size;
			recentGroup.MinimumSize = size;

			commonGroup.Width = width;
			size = new Size(width, CalculateFlowHeight(commonFlow));
			commonGroup.Size = size;
			commonGroup.MaximumSize = size;
			commonGroup.MinimumSize = size;

			cloudGroup.Width = width;
			size = new Size(width, CalculateFlowHeight(cloudFlow));
			cloudGroup.Size = size;
			cloudGroup.MaximumSize = size;
			cloudGroup.MinimumSize = size;

			Invalidate();
		}


		private int CalculateFlowHeight(FlowLayoutPanel layout)
		{
			var width = 0;
			var height = 80;
			foreach (var control in layout.Controls)
			{
				var tag = (Control)control;
				var buffer = tag.Margin.Left + tag.Margin.Right + layout.Padding.Left + layout.Padding.Right;

				width += tag.Width + buffer;
				if (width > layout.Width)
				{
					height += tag.Height + tag.Margin.Top + tag.Margin.Bottom;
					width = 0;
				}
			}

			return height;
		}


		private void ShowMenu(object sender, EventArgs e)
		{
			contextMenu.Show(menuButton, new Point(
				-(contextMenu.Width - menuButton.Width),
				menuButton.Height));
		}


		private void PrepareContextMenu(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (recentGroup.Visible)
			{
				showRecentMenuItem.Image = Resx.e_CheckMark;
				showRecentMenuItem.Text = Resx.HashtaggerDialog_hideRecentMenuItem;
			}
			else
			{
				showRecentMenuItem.Image = null;
				showRecentMenuItem.Text = Resx.HashtaggerDialog_showRecentMenuItem;
			}

			if (commonGroup.Visible)
			{
				showCommonMenuItem.Image = Resx.e_CheckMark;
				showCommonMenuItem.Text = Resx.HashtaggerDialog_hideCommonMenuItem;
			}
			else
			{
				showCommonMenuItem.Image = null;
				showCommonMenuItem.Text = Resx.HashtaggerDialog_showCommonMenuItem;
			}
		}


		private void ShowHideGroup(object sender, EventArgs e)
		{
			if (sender == showRecentMenuItem)
			{
				recentGroup.Visible = !recentGroup.Visible;
				if (recentGroup.Visible && !recentLoaded)
				{
					LoadRecent(new HashtagProvider());
					ResizeFlows(sender, e);
				}
			}
			else
			{
				commonGroup.Visible = !commonGroup.Visible;
				if (commonGroup.Visible && !commonLoaded)
				{
					LoadCommon();
					ResizeFlows(sender, e);
				}
			}
		}


		private void EditTags(string text)
		{
			// add # to common word
			if (text[0] != '#')
			{
				text = $"#{text}";
			}

			if (!tagsBox.Text.Contains(text))
			{
				tagsBox.Text = $"{tagsBox.Text} {text}";
			}
			else
			{
				// matches of "<space>tag" or beginning of line; it also looks for a following
				// space or end of line but leaves that so we don't end up squishing two tags
				// together; this is why we replace with only $2 - to keep the following space.
				tagsBox.Text = Regex.Replace(tagsBox.Text, @$"(^|\s){text}($|\s)", "$2").Trim();
			}

			okButton.Enabled = !string.IsNullOrWhiteSpace(tagsBox.Text);
		}

		private void ClearTags(object sender, LinkLabelLinkClickedEventArgs e)
		{
			tagsBox.Text = string.Empty;
			okButton.Enabled = false;
		}


		private void SaveSettings(object sender, FormClosingEventArgs e)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsKey);
			settings.Add("showRecent", recentGroup.Visible);
			settings.Add("showCommon", commonGroup.Visible);

			if (settings.IsModified)
			{
				provider.SetCollection(settings);
				provider.Save();
			}
		}
	}
}
