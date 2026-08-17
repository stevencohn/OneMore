//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemoveDuplicatesNavigator : UI.MoreForm
	{
		private readonly OneNote one;
		private readonly ToolTip tooltip;

		public RemoveDuplicatesNavigator()
			: base()
		{
			InitializeComponent();

			tooltip = new ToolTip();

			view.Columns.Add(new MoreColumnHeader(Resx.word_Page, 450) { AutoSizeItems = true });
			view.Columns.Add(new MoreColumnHeader(Resx.word_Text, 150));
			view.Columns.Add(new MoreColumnHeader(Resx.word_XML, 150));
			view.Columns.Add(new MoreColumnHeader(Resx.word_Distance, 150));
			view.Columns.Add(new MoreColumnHeader(Resx.word_Delete, 100));

			if (NeedsLocalizing())
			{
				Text = Resx.RemoveDuplicatesDialog_Text;

				Localize(new string[]
				{
					"cancelButton=word_Cancel"
				});
			}
		}


		public RemoveDuplicatesNavigator(List<RemoveDuplicatesCommand.HashNode> hashes)
			: this()
		{
			view.BeginUpdate();
			foreach (var node in hashes)
			{
				var group = view.Groups.Cast<ListViewGroup>()
					.FirstOrDefault(g => g.Name == node.GroupID);

				if (group == null)
				{
					var isEmptyGroup = node.PageID == null;
					var hasSimilar = node.Siblings.Any(
						s => s.MatchKind == RemoveDuplicatesCommand.MatchKind.Similar);

					var header = isEmptyGroup
						? node.Title
						: String.Format(hasSimilar
							? Resx.RemoveDuplicatesNavigator_pagesSimilarTo
							: Resx.RemoveDuplicatesNavigator_duplicatesOf,
							node.Title);

					group = new ListViewGroup(node.GroupID, header);
					view.Groups.Add(group);
				}

				// PageID will be null for the Empty Pages node
				if (node.PageID != null)
				{
					var item = view.AddHostedItem(MakeLinkLabel(node));

					item.Tag = node;
					item.Group = group;

					item.AddHostedSubItem(String.Empty);
					item.AddHostedSubItem(String.Empty);
					item.AddHostedSubItem(MakeKeepNewestButton(node));
					item.AddHostedSubItem(MakeButton(node));
				}

				MoreHostedListViewSubItem subitem;

				foreach (var sibling in node.Siblings)
				{
					var sibitem = view.AddHostedItem(MakeLinkLabel(sibling));
					sibitem.Tag = sibling;
					sibitem.Group = group;

					if (sibling.TextHash == string.Empty)
					{
						subitem = sibitem.AddHostedSubItem("-");
					}
					else if (sibling.MatchKind == RemoveDuplicatesCommand.MatchKind.Similar)
					{
						subitem = sibitem.AddHostedSubItem(
							sibling.Similarity is double similarity ? $"{similarity:P0}" : "-");
					}
					else
					{
						subitem = sibitem.AddHostedSubItem(
							MakePictureBox(sibling.TextHash == node.TextHash));
					}
					subitem.Alignment = ContentAlignment.MiddleCenter;

					if (sibling.XmlHash is null)
					{
						subitem = sibitem.AddHostedSubItem("-");
					}
					else
					{
						subitem = sibitem.AddHostedSubItem(string.Empty,
							MakePictureBox(sibling.XmlHash == node.XmlHash));
					}
					subitem.Alignment = ContentAlignment.MiddleCenter;

					sibitem.AddHostedSubItem(sibling.Distance?.ToString() ?? "-");

					var button = MakeButton(node);
					if (node.PageID == null)
					{
						sibitem.ForeColor = manager.GetColor("GrayText");
						tooltip.SetToolTip(button, Resx.RemoveDuplicatesNavigator_emptyPageTip);
					}
					sibitem.AddHostedSubItem(button);
				}
			}

			view.Items[0].Selected = true;
			view.EndUpdate();

			one = new OneNote();
		}


		protected override void OnLoad(EventArgs e)
		{
			// set view colors *before* base.OnLoad: MoreForm.OnLoad walks hosted controls
			// (e.g. MoreLinkLabel) and self-themes them from Parent.BackColor, so view's
			// colors must already be final by the time that walk runs
			view.BackColor = manager.GetColor("ListView");
			view.ForeColor = manager.GetColor("WindowText");
			view.HeaderBackColor = manager.GetColor("Control");
			view.HeaderForeColor = manager.GetColor("ControlText");

			// a subtle tint between the list background and the full accent Highlight color:
			// distinct enough to read as "selected" without the full-saturation Highlight,
			// paired with normal text color rather than the system's HighlightText (which can
			// go white-on-near-white against a soft tint)
			view.HighlightBackground = Blend(
				manager.GetColor("ListView"), manager.GetColor("Highlight"), 0.35f);
			view.HighlightForeground = manager.GetColor("WindowText");

			BackColor = manager.GetColor("Control");
			ForeColor = manager.GetColor("ControlText");

			base.OnLoad(e);
		}


		private static Color Blend(Color from, Color to, float amount)
		{
			return Color.FromArgb(
				(int)(from.R + ((to.R - from.R) * amount)),
				(int)(from.G + ((to.G - from.G) * amount)),
				(int)(from.B + ((to.B - from.B) * amount)));
		}


		private MoreLinkLabel MakeLinkLabel(RemoveDuplicatesCommand.HashNode node)
		{
			var label = new MoreLinkLabel
			{
				Text = node.Title
			};

			label.LinkClicked += NavigateToPage;
			label.Click += NavigateToPage;

			return label;
		}


		private void NavigateToPage(object sender, EventArgs e)
		{
			if (((Control)sender).Tag is ListViewItem host)
			{
				view.SelectIf(host);
				if (host.Tag is RemoveDuplicatesCommand.HashNode node)
				{
					if (node.PageID != null && !node.PageID.Equals(one.CurrentPageId))
					{
						Task.Run(async () => { await one.NavigateTo(node.PageID); });
					}
				}
			}
		}


		private Button MakeButton(RemoveDuplicatesCommand.HashNode node)
		{
			Image image = Resx.m_Delete;
			if (manager.DarkMode)
			{
				using var original = image;
				image = new ImageEditor { Style = ImageEditor.Stylization.Invert }.Apply(original);
			}

			var button = new Button
			{
				Image = image,
				Padding = new Padding(0),
				Margin = new Padding(0),
				FlatStyle = FlatStyle.Flat,
				Width = 40,
				Height = 24,
				BackColor = manager.GetColor("ButtonFace")
			};

			button.FlatAppearance.BorderColor = manager.GetColor("ButtonBorder");

			button.MouseClick += DeletePages;

			return button;
		}


		private void DeletePages(object sender, EventArgs e)
		{
			if (((Control)sender).Tag is ListViewItem host)
			{
				view.SelectIf(host);
			}

			DeleteSelected();
		}


		private Button MakeKeepNewestButton(RemoveDuplicatesCommand.HashNode node)
		{
			var button = new Button
			{
				Text = Resx.RemoveDuplicatesNavigator_keepNewest,
				TextAlign = ContentAlignment.MiddleCenter,
				Padding = new Padding(0),
				Margin = new Padding(0),
				FlatStyle = FlatStyle.Flat,
				Width = 140,
				Height = 26,
				BackColor = manager.GetColor("ButtonFace"),
				ForeColor = manager.GetColor("ControlText")
			};

			button.FlatAppearance.BorderColor = manager.GetColor("ButtonBorder");

			button.Click += KeepNewestOnly;

			return button;
		}


		private void KeepNewestOnly(object sender, EventArgs e)
		{
			if (((Control)sender).Tag is not ListViewItem host)
			{
				return;
			}

			var group = host.Group;

			var members = view.Items.Cast<ListViewItem>()
				.Where(i => i.Group == group && i.Tag is RemoveDuplicatesCommand.HashNode)
				.ToList();

			if (members.Count < 2)
			{
				return;
			}

			var newest = members.OrderByDescending(
				i => ((RemoveDuplicatesCommand.HashNode)i.Tag).LastModified).First();

			view.SelectedItems.Clear();

			foreach (var item in members.Where(i => i != newest))
			{
				item.Selected = true;
			}

			if (view.SelectedItems.Count > 0)
			{
				DeleteSelected();
			}
		}


		private void DeleteSelected()
		{
			var msg = view.SelectedItems.Count == 1
				? Resx.RemoveDuplicatesNavigator_confirm1
				: String.Format(Resx.RemoveDuplicatesNavigator_confirmAll, view.SelectedItems.Count);

			var result = MoreMessageBox.Show(Owner, msg, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}

			while (view.SelectedItems.Count > 0)
			{
				var item = view.SelectedItems[0];
				if (item.Tag is RemoveDuplicatesCommand.HashNode node)
				{
					logger.WriteLine($"deleting page '{node.Title}'; moved to recyclebin");
					one.DeleteHierarchy(node.PageID);
					view.Items.Remove(item);
				}
			}
		}


		private PictureBox MakePictureBox(bool same)
		{
			var box = new PictureBox
			{
				Image = same ? Resx.Equal : Resx.NotEqual,
				BackColor = Color.Transparent,
				Height = 22,
				Width = 22
			};

			box.Click += new EventHandler((s, e) =>
			{
				if (((Control)s).Tag is ListViewItem host)
				{
					view.SelectIf(host);
				}
			});

			return box;
		}


		private void CloseDialog(object sender, EventArgs e)
		{
			Close();
		}
	}
}
