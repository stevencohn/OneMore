//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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


		public HashtagContextControl(Hashtag tag)
			: this()
		{
			pageLink.Text = $"{tag.HierarchyPath}/{tag.PageTitle}";
			pageLink.Links.Clear();

			var oid = string.IsNullOrWhiteSpace(tag.TitleID) ? string.Empty : tag.TitleID;
			pageLink.Links.Add(0, pageLink.Text.Length, (tag.PageID, oid));
			tooltip.SetToolTip(pageLink, "Jump to this page");

			contextLink.Text = tag.Context;
			contextLink.Links.Clear();
			contextLink.Links.Add(0, contextLink.Text.Length, (tag.PageID, tag.ObjectID));
			tooltip.SetToolTip(contextLink, "Jump to this paragraph");
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
