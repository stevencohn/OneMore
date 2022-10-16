//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class MoreBubbleWindow : LocalizableForm
	{
		private const int SolidTime = 500;
		private const int FadeoutTime = 750;
		private float fadeIncrement;
		private int time;


		public MoreBubbleWindow()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"okButton=word_OK"
				});
			}

			// create mask with rounded corners to overlay form
			Region = Region.FromHrgn(Native.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

			// increments to get to 100%
			fadeIncrement = 1 / (FadeoutTime / (float)timer.Interval);
			time = 0;
		}


		public void SetMessage(string text)
		{
			messageBox.Text = text;
		}


		public static DialogResult Show(IWin32Window owner, string text)
		{
			using var box = new MoreBubbleWindow();
			box.SetMessage(text);
			return box.ShowDialog(owner);
		}


		private void PauseTimer(object sender, EventArgs e)
		{
			timer.Stop();
			Opacity = 100;
		}


		private void Tick(object sender, EventArgs e)
		{
			time += timer.Interval;
			if (time < SolidTime)
			{
				return;
			}

			if (Opacity > fadeIncrement)
			{
				Opacity -= fadeIncrement;
				fadeIncrement += 0.001F;
				return;
			}

			CloseWindow(sender, e);
		}


		private void Unclick(object sender, EventArgs e)
		{
			okButton.Focus();
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			timer.Start();
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			timer.Stop();
			Opacity = 100;
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			time = 0;
			timer.Start();
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button == MouseButtons.Left)
			{
				Native.ReleaseCapture();
				Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, Native.HT_CAPTION, 0);
			}
		}


		// unfortunately this doesn't work for ShowDialog; it only applies to Show
		protected override bool ShowWithoutActivation => true;


		private void CloseWindow(object sender, EventArgs e)
		{
			timer.Stop();
			timer.Dispose();
			Close();
		}
	}
}
