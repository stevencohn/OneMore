//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtagContextControl : MoreUserControl
	{
		private int radius = 5;


		public HashtagContextControl()
		{
			InitializeComponent();
			Radius = 5;
		}


		public HashtagContextControl(HashtagContext item)
			: this()
		{
			var hintColor = manager.GetColor("HintText");
			var grayColor = manager.GetColor("GrayText");
			var hoverColor = manager.GetColor("HoverColor");

			PageID = item.PageID;

			checkbox.Enabled = item.Available;

			pageLink.Text = $"{item.HierarchyPath}/{item.PageTitle}";
			var oid = string.IsNullOrWhiteSpace(item.TitleID) ? string.Empty : item.TitleID;
			pageLink.Links.Add(0, pageLink.Text.Length, (item.PageID, oid));
			pageLink.HoverColor = hoverColor;
			tooltip.SetToolTip(pageLink, Resx.HashtagContext_jumpTip);
			pageLink.Enabled = item.Available;

			// LastModified...

			dateLabel.Text = DateTime
				.Parse(item.LastModified, CultureInfo.InvariantCulture)
				.ToShortFriendlyString();

			dateLabel.Left = Width - dateLabel.Width - 8;


			// snippets...

			var height = snippetsPanel.Height;

			foreach (var snippet in item.Snippets)
			{
				var fore = snippet.DirectHit ? hintColor : grayColor;

				var link = new MoreLinkLabel
				{
					Text = snippet.Snippet,
					ActiveLinkColor = grayColor,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Enabled = item.Available,
					ForeColor = fore,
					HoverColor = hoverColor,
					LinkColor = fore,
					Location = new Point(30, 40),
					Margin = new Padding(20, 6, 10, 6),
					Size = new Size(530, 20),
					TabStop = true,
					VisitedLinkColor = grayColor,
					StrictColors = true
				};

				link.LinkClicked += NavigateTo;
				link.Links.Add(0, link.Text.Length, (item.PageID, snippet.ObjectID));

				var date = DateTime
					.Parse(snippet.LastModified, CultureInfo.InvariantCulture)
					.ToShortFriendlyString();

				tooltip.SetToolTip(link, string.Format(Resx.HashtagContext_jumpParaTip, date));

				snippetsPanel.Controls.Add(link);
			}

			if (snippetsPanel.Height > height)
			{
				Height += snippetsPanel.Height - height;
			}

			Tag = item;
		}


		public event EventHandler Checked;


		public bool IsChecked
		{
			get => checkbox.Checked;
			set
			{
				checkbox.Checked = value;
				Checked?.Invoke(this, new EventArgs());
			}
		}


		public string PageID { get; private set; }


		[DefaultValue(5)]
		public int Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				RecreateRegion();
			}
		}


		private void RecreateRegion()
		{
			Region = Region.FromHrgn(Native.CreateRoundRectRgn(
				ClientRectangle.Left, ClientRectangle.Top,
				ClientRectangle.Right, ClientRectangle.Bottom,
				radius, radius));

			Invalidate();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			RecreateRegion();
		}


		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			IsChecked = !IsChecked;
		}


		private void DoCheckChanged(object sender, EventArgs e)
		{
			IsChecked = checkbox.Checked;
		}


		private async void NavigateTo(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData == null)
			{
				Logger.Current.WriteLine("linkData is empty");
				return;
			}

			try
			{
				var (pageID, objectID) = ((string pageID, string objectID))e.Link.LinkData;
				var success = await new OneNote().NavigateTo(pageID, objectID);
				if (!success)
				{
					MoreMessageBox.ShowError(this, Resx.HashtagDialog_badLink);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					"linkData is bad, possible broken reference to page object", exc);
			}
		}
	}
}
