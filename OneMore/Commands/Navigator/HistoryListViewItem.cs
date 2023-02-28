//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal class HistoryListViewItem : UserControl, IChameleon
	{
		private readonly MoreLinkLabel link;


		public HistoryListViewItem(string ico, string text, string url)
		{
			var picture = new PictureBox
			{
				Image = new Bitmap(24, 24),
				Dock = DockStyle.Left,
				Width = 34
			};

			using var g = Graphics.FromImage(picture.Image);
			g.Clear(SystemColors.Window);
			g.DrawImage(Image.FromFile(@"C:\Github\OneMore\OneMore\Properties\Images\Logo.png"), 0, 0, 24, 24);

			link = new MoreLinkLabel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
				Text = text,
				Tag = url,
				Font = new Font("Segoe UI", 8, FontStyle.Regular),
				Padding = new Padding(0),
				Margin = new Padding(4, 0, 0, 0)
			};

			link.LinkClicked += new LinkLabelLinkClickedEventHandler((s, e) =>
			{
				if (s is MoreLinkLabel label)
				{
					MessageBox.Show($"click [{label.Text}] @ {label.Tag}");
				}
			});

			BackColor = Color.Transparent;
			Width = 100;
			Height = 24;
			Margin = new Padding(0, 2, 0, 2);

			BackColorChanged += new EventHandler((s, e) =>
			{
				link.BackColor = ((Control)s).BackColor;
			});

			Controls.Add(picture);
			Controls.Add(link);
		}


		public void ApplyBackground(Color color)
		{
			BackColor = color;
			link.BackColor = color;
		}


		public void ResetBackground()
		{
			BackColor = Color.Transparent;
			link.BackColor = Color.Transparent;
		}
	}
}
