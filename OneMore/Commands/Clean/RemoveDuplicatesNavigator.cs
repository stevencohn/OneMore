//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RemoveDuplicatesNavigator : UI.LocalizableForm
	{
		public RemoveDuplicatesNavigator()
		{
			InitializeComponent();

			view.Columns.Add(new MoreColumnHeader("Page", 350) { Sortable = false });
			view.Columns.Add(new MoreColumnHeader("Text", 150) { Sortable = false });
			view.Columns.Add(new MoreColumnHeader("Xml", 150) { Sortable = false });
			view.Columns.Add(new MoreColumnHeader("Distance", 150) { Sortable = false });
			view.Columns.Add(new MoreColumnHeader("Tash", 100) { Sortable = false });

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
					group = new ListViewGroup(node.GroupID, node.Title);
					view.Groups.Add(group);
				}

				var item = view.AddHostedItem(MakeLinkLabel(node));
				item.Group = group;

				item.AddHostedSubItem(String.Empty);
				item.AddHostedSubItem(String.Empty);
				item.AddHostedSubItem(String.Empty);
				item.AddHostedSubItem(item.Text, MakeButton(node)); 
				
				
				foreach (var sibling in node.Siblings)
				{
					var sibitem = view.AddHostedItem(MakeLinkLabel(sibling));
					sibitem.Group = group;

					sibitem.AddHostedSubItem(sibling.TextHash == String.Empty
						? "-"
						: (sibling.TextHash == node.TextHash ? "=" : "Different"));

					sibitem.AddHostedSubItem(sibling.XmlHash == String.Empty
						? "-"
						: (sibling.XmlHash == node.XmlHash ? "=" : "Different"));

					sibitem.AddHostedSubItem(sibling.Distance.ToString());
					sibitem.AddHostedSubItem(item.Text, MakeButton(node));
				}
			}

			view.Items[0].Selected = true;
			view.EndUpdate();
		}


		private MoreLinkLabel MakeLinkLabel(RemoveDuplicatesCommand.HashNode node)
		{
			var label = new MoreLinkLabel
			{
				Text = node.Title,
				AutoSize = true
			};

			label.LinkClicked += new LinkLabelLinkClickedEventHandler((s, e) =>
			{
				if (((Control)s).Tag is ListViewItem host)
				{
					view.SelectIf(host);
					var index = view.Items.IndexOf(host);
					MessageBox.Show($"items[{index}]");
				}
			});

			return label;
		}


		private Button MakeButton(RemoveDuplicatesCommand.HashNode node)
		{
			var button = new Button
			{
				Image = Resx.Delete,
				AutoSize = false,
				Padding = new Padding(0),
				Margin = new Padding(0),
				FlatStyle = FlatStyle.Flat,
				Width = 40,
				Height = 24
			};

			button.MouseClick += new MouseEventHandler((s, e) =>
			{
				if (((Control)s).Tag is ListViewItem host)
				{
					view.SelectIf(host);
					var index = view.Items.IndexOf(host);
					MessageBox.Show($"items[{index}]");
				}
			});

			return button;
		}
	}
}
