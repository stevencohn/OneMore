//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal partial class HashtagContextControl : UserControl
	{

		public HashtagContextControl()
		{
			InitializeComponent();
		}


		public HashtagContextControl(Hashtag tag)
			: this()
		{
			pageLink.Text = $"{tag.HierarchyPath}/{tag.PageTitle}";
			pageLink.Links.Clear();
			pageLink.Links.Add(0, pageLink.Text.Length, tag.PageURL);

			contextLink.Text = tag.Context;
			contextLink.Links.Clear();
			contextLink.Links.Add(0, contextLink.Text.Length, tag.ObjectURL);
		}


		private int radius = 3;
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

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
			int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);


		private void RecreateRegion()
		{
			var bounds = ClientRectangle;
			Region = Region.FromHrgn(CreateRoundRectRgn(bounds.Left, bounds.Top,
				bounds.Right, bounds.Bottom, Radius, radius));

			Invalidate();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			RecreateRegion();
		}
	}
}
