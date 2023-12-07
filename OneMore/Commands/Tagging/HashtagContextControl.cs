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
			pageLink.Links.Add(0, pageLink.Text.Length, tag.PageURL);
			tooltip.SetToolTip(pageLink, "Jump to this page");

			contextLink.Text = tag.Context;
			contextLink.Links.Clear();
			contextLink.Links.Add(0, contextLink.Text.Length, tag.ObjectURL);
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
	}
}
