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


	internal partial class RemoveDuplicatesNavigator : UI.LocalizableForm
	{
		private OneNote one;

		public RemoveDuplicatesNavigator()
			: base()
		{
			InitializeComponent();

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
					group = new ListViewGroup(node.GroupID, node.PageID == null 
						? node.Title 
						: String.Format(Resx.RemoveDuplicatesNavigator_pagesSimilarTo, node.Title)
						);

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
					item.AddHostedSubItem(String.Empty);
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
					else
					{
						subitem = sibitem.AddHostedSubItem(
							MakePictureBox(sibling.TextHash == node.TextHash));
					}
					subitem.Alignment = ContentAlignment.MiddleCenter;

					if (sibling.XmlHash == string.Empty)
					{
						subitem = sibitem.AddHostedSubItem("-");
					}
					else
					{
						subitem = sibitem.AddHostedSubItem(string.Empty,
							MakePictureBox(sibling.XmlHash == node.XmlHash));
					}
					subitem.Alignment = ContentAlignment.MiddleCenter;

					sibitem.AddHostedSubItem(sibling.Distance.ToString());
					sibitem.AddHostedSubItem(MakeButton(node));
				}
			}

			view.Items[0].Selected = true;
			view.EndUpdate();

			one = new OneNote();
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
			var button = new Button
			{
				Image = Resx.Delete,
				Padding = new Padding(0),
				Margin = new Padding(0),
				FlatStyle = FlatStyle.Flat,
				Width = 40,
				Height = 24
			};

			button.MouseClick += DeletePages;

			return button;
		}


		private void DeletePages(object sender, EventArgs e)
		{
			if (((Control)sender).Tag is ListViewItem host)
			{
				view.SelectIf(host);
			}

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
