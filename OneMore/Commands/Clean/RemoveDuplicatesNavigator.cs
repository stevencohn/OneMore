//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Drawing;
	using System.Linq;
	using System.Net.PeerToPeer.Collaboration;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;



	internal partial class RemoveDuplicatesNavigator : UI.LocalizableForm
	{
		public RemoveDuplicatesNavigator()
		{
			InitializeComponent();

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
			foreach (var node in hashes)
			{
				var item = new ListViewItem
				{
					Text = node.Title,
					Tag = node
				};

				view.Items.Add(item);

				foreach (var sibling in node.Siblings)
				{
					var sibitem = new ListViewItem
					{
						IndentCount = 2,
						Text = sibling.Title,
						Tag = sibling
					};

					sibitem.SubItems.Add(new ListViewItem.ListViewSubItem
					{
						Text = sibling.TextHash == node.TextHash ? "=" : "Different"
					});

					sibitem.SubItems.Add(new ListViewItem.ListViewSubItem
					{
						Text = sibling.XmlHash == node.XmlHash ? "=" : "Different"
					});

					sibitem.SubItems.Add(new ListViewItem.ListViewSubItem
					{
						Text = sibling.Distance.ToString()
					});

					view.Items.Add(sibitem);
				}
			}
		}
	}
}
