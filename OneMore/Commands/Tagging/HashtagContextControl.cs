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
			PageID = item.PageID;

			pageLink.Text = $"{item.HierarchyPath}/{item.PageTitle}";
			var oid = string.IsNullOrWhiteSpace(item.TitleID) ? string.Empty : item.TitleID;
			pageLink.Links.Add(0, pageLink.Text.Length, (item.PageID, oid));
			tooltip.SetToolTip(pageLink, "Jump to this page");

			// scanTime...

			dateLabel.Text = DateTime
				.Parse(item.ScanTime, CultureInfo.InvariantCulture)
				.ToShortFriendlyString();

			dateLabel.Left = Width - dateLabel.Width - 8;


			// snippets...

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
					Margin = new Padding(20, 6, 10, 6),
					Size = new Size(530, 20),
					TabStop = true,
					VisitedLinkColor = SystemColors.GrayText
				};

				link.LinkClicked += NavigateTo;
				link.Links.Add(0, link.Text.Length, (item.PageID, snippet.ObjectID));

				var date = DateTime
					.Parse(snippet.ScanTime, CultureInfo.InvariantCulture)
					.ToShortFriendlyString();

				tooltip.SetToolTip(link, $"Jump to this paragraph; last updated {date}");

				snippetsPanel.Controls.Add(link);
			}

			if (snippetsPanel.Height > height)
			{
				Height += snippetsPanel.Height - height;
			}
		}


		public bool Checked
		{
			get => checkbox.Checked;
			set => checkbox.Checked = value;
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
			Checked = !Checked;
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
