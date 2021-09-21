//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Win = System.Windows;


	internal partial class TimerWindow : LocalizableForm
	{

		private readonly float scalingX;
		private readonly float scalingY;
		private int maxLeft;
		private int maxTop;
		private int seconds;


		public TimerWindow()
		{
			InitializeComponent();

			// create mask with rounded corners to overlay form
			Region = Region.FromHrgn(Native.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

			(scalingX, scalingY) = UIHelper.GetScalingFactors();

			toolstrip.ImageScalingSize = new Size((int)(16 * scalingX), (int)(16 * scalingY));
		}


		private void TimerWindow_Load(object sender, EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
			TopMost = true;
			TopLevel = true;

			Top = (int)((SystemInformation.CaptionHeight + 5) * scalingY);
			Left = (int)(Screen.FromControl(this).WorkingArea.Width - Width - (20 * scalingX));

			maxTop = Screen.FromControl(this).WorkingArea.Height - Height - 50;
			maxLeft = Left;

			seconds = 0;
			timer.Enabled = true;
			timer.Start();
		}


		private void MoveWindow(object sender, EventArgs e)
		{
			if (maxLeft > 0)
			{
				if (Left < 1) Left = 1;
				if (Left > maxLeft) Left = maxLeft;
				if (Top < 1) Top = 1;
				if (Top > maxTop) Top = maxTop;
			}
		}


		private void Tick(object sender, EventArgs e)
		{
			var span = TimeSpan.FromSeconds(++seconds);
			timeLabel.Text = span.ToString("c");
			timeLabel.Update();
			TopMost = true;
		}


		private void StartWindowDrag(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Native.ReleaseCapture();
				Native.SendMessage(Handle, Native.WM_NCLBUTTONDOWN, Native.HT_CAPTION, 0);
			}
		}


		private void CopyTime(object sender, EventArgs e)
		{
			var stamp = TimeSpan.FromSeconds(++seconds).ToString("c");

			SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.SetText(stamp, Win.TextDataFormat.Text);
			});

		}


		private void RestartTimer(object sender, EventArgs e)
		{
			seconds = 0;
		}


		private void CloseWindow(object sender, EventArgs e)
		{
			Close();
		}
	}
}
