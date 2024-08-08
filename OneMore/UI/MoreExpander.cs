//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class MoreExpander : UserControl
	{
		private readonly Image image;
		private readonly Image grayed;
		private string title;
		private bool expanded;
		private bool expandedIcon;


		public MoreExpander()
		{
			InitializeComponent();

			header.Image = new Bitmap(header.Width, header.Height);
			image = (Bitmap)Resx.ExpandArrow.Clone();

			grayed = new ImageEditor { Style = ImageEditor.Stylization.GrayScale }
				.Apply(image);

			expanded = expandedIcon = false;
		}



		[Description("Control to expand and collapse")]
		public Control ContentControl { get; set; }


		[Description("Gets or sets whether the expander is expanded")]
		public bool Expanded
		{
			get => expanded;
			set
			{
				expanded = value;
				header.Invalidate();
			}
		}


		[Description("Max expanded height of content control")]
		public int MaxContentHeight { get; set; }


		[Description("Title line")]
		public string Title
		{
			get => title;
			set
			{
				title = value;
				header.Invalidate();
			}
		}


		private void Toggle(object sender, System.EventArgs e)
		{
			Expanded = !Expanded;

			if (expanded)
			{
				ParentForm.Height += MaxContentHeight;
				ContentControl.Height = MaxContentHeight;
			}
			else
			{
				ContentControl.Height = 0;
				ParentForm.Height -= MaxContentHeight;
			}
		}


		private void Repaint(object sender, PaintEventArgs e)
		{
			if (Expanded != expandedIcon)
			{
				image.RotateFlip(RotateFlipType.RotateNoneFlipY);
				grayed.RotateFlip(RotateFlipType.RotateNoneFlipY);
				expandedIcon = Expanded;
			}

			var g = e.Graphics;
			g.Clear(BackColor);
			g.DrawImage(Enabled ? image : grayed, 5, 5, 30, 30);

			using var font = new Font(Font, FontStyle.Bold);
			var size = g.MeasureString(Title, font);

			var fore = Enabled ? ForeColor : Color.Gray;

			g.DrawString(Title, font, new SolidBrush(fore),
				45, (header.Height / 2) - size.Height + 5);

			var y = header.Height / 2 + 10;
			using var pen = new Pen(Color.DarkGray, 2);
			g.DrawLine(pen, 45, y, header.Width, y);
		}
	}
}
