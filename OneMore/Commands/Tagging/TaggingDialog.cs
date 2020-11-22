//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class TaggingDialog : LocalizableForm
	{

		#region TagsTExtBox
		/// <summary>
		/// The default TextBox.KeyPressed/Up/Down handlers don't really prevent the accept
		/// button from being bubbled up to the dialog so this class provides an alternate method
		/// of intercepting the Enter key with its own event handler.
		/// </summary>
		private class TagsTextBox : TextBox
		{
			private const int WM_KEYDOWN = 256;

			public event EventHandler TagsChanged;

			protected override bool ProcessCmdKey(ref Message m, Keys k)
			{
				if (m.Msg == WM_KEYDOWN && k == Keys.Enter)
				{
					if (TagsChanged != null)
						TagsChanged(this, new EventArgs());

					// stop further interpretation
					return true;
				}
				// else default handlers...
				return base.ProcessCmdKey(ref m, k);
			}
		}
		#endregion TagsTExtBox


		public TaggingDialog()
		{
			InitializeComponent();
			tagBox.TagsChanged += AcceptInput;
			FetchRecentTags();
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 5));
			UIHelper.SetForegroundWindow(this);
		}

		public List<string> Tags
		{
			get
			{
				return tagsFlow.Controls.OfType<TagLabel>().ToList().ConvertAll(t => t.Label);
			}

			set
			{
				tagsFlow.Controls.AddRange(value.ConvertAll(t => new TagLabel(t)).ToArray());
			}
		}


		private void FetchRecentTags()
		{
			using (var one = new OneNote())
			{
				var root = one.SearchMeta(string.Empty, Page.TaggingMetaName);
				var ns = root.GetNamespaceOfPrefix("one");
				var pages = root.Descendants(ns + "Page")
					.OrderByDescending(e => e.Attribute("lastModifiedTime").Value);

				var tags = new Dictionary<string, string>();

				var count = 0;
				foreach (var page in pages)
				{
					var meta = page.Element(ns + "Meta");
					if (meta != null)
					{
						var parts = meta.Attribute("content").Value
							.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (var part in parts)
						{
							var p = part.Trim();
							var key = p.ToLower();
							if (!tags.ContainsKey(key))
							{
								tags.Add(key, p);

								count++;
								if (count > 19) break;
							}
						}
					}

					if (count > 19) break;
				}

				if (tags.Count > 0)
				{
					var sorted = tags.OrderBy(k => k.Key.StartsWith("#") ? k.Key.Substring(1) : k.Key);

					foreach (var s in sorted)
					{
						recentFlow.Controls.Add(MakeLabel(s.Value));
					}
				}
			}
		}

		private void FetchPageWords()
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				//
			}
		}


		private Label MakeLabel(string text)
		{
			var label = new Label
			{
				Margin = new Padding(4),
				Text = text,
				BackColor = SystemColors.ControlLight,
				Cursor = Cursors.Hand
			};

			label.Click += (sender, e) =>
			{
				AddTag(((Label)sender).Text);
			};

			label.MouseEnter += (sender, e) =>
			{
				((Label)sender).BackColor = Color.FromKnownColor(KnownColor.LightSkyBlue);
			};

			label.MouseLeave += (sender, e) =>
			{
				((Label)sender).BackColor = SystemColors.ControlLight;
			};

			return label;
		}


		private void AcceptInput(object sender, EventArgs e)
		{
			var text = tagBox.Text.Trim();
			if (text.Length > 0)
			{
				var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					AddTag(part);
				}

				tagBox.Text = string.Empty;
			}
		}

		private void AddTag(string text)
		{
			if (!tagsFlow.Controls.ContainsKey(text))
			{
				var tag = new TagLabel(text)
				{
					Name = text
				};

				tag.Deleting += DeleteTag;
				tagsFlow.Controls.Add(tag);
			}
		}

		private void DeleteTag(object sender, EventArgs e)
		{
			var tag = sender as TagLabel;
			tagsFlow.Controls.Remove(tag);

		}
	}
}
