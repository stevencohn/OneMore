//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class HashtagContextControl : UserControl
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
			pageLink.Text = $"{item.HierarchyPath}/{item.PageTitle}";
			pageLink.Links.Clear();

			var oid = string.IsNullOrWhiteSpace(item.TitleID) ? string.Empty : item.TitleID;
			pageLink.Links.Add(0, pageLink.Text.Length, (item.PageID, oid));
			tooltip.SetToolTip(pageLink, "Jump to this page");

			var height = snippetsPanel.Height;

			foreach (var snippet in item.Snippets)
			{
				var link = new MoreLinkLabel
				{
					Text = snippet.Snippet,
					ActiveLinkColor = SystemColors.GrayText,
					AutoSize = true,
					Cursor = Cursors.Hand,
					ForeColor = SystemColors.GrayText,
					HoverColor = Color.MediumOrchid,
					LinkColor = SystemColors.GrayText,
					Location = new Point(30, 40),
					Margin = new Padding(20, 10, 10, 10),
					Size = new Size(530, 20),
					TabStop = true,
					VisitedLinkColor = SystemColors.GrayText
				};

				link.LinkClicked += NavigateTo;
				link.Links.Add(0, link.Text.Length, (item.PageID, snippet.ObjectID));
				tooltip.SetToolTip(link, "Jump to this paragraph");

				snippetsPanel.Controls.Add(link);
			}

			if (snippetsPanel.Height > height)
			{
				Height += snippetsPanel.Height - height;
			}
		}


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


		private async void NavigateTo(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData == null)
			{
				Logger.Current.WriteLine("linkData is empty");
				return;
			}

			try
			{
				var (PageID, ObjectID) = ((string PageID, string ObjectID))e.Link.LinkData;
				await new OneNote().NavigateTo(PageID, ObjectID);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					"linkData is bad, possible broken reference to page object", exc);
			}
		}
	}
}
