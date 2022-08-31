//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using Win = System.Windows;


	internal partial class TimerWindow : LocalizableForm
	{

		private readonly float scalingX;
		private readonly float scalingY;
		private int maxLeft;
		private int maxTop;


		public TimerWindow()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.TimerWindow_Text;

				Localize(new string[]
				{
					"copyButton",
					"resetButton",
					"closeButton"
				});
			}

			// create mask with rounded corners to overlay form
			Region = Region.FromHrgn(Native.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
			TopMost = true;
			TopLevel = true;

			(scalingX, scalingY) = UIHelper.GetScalingFactors();
			toolstrip.ImageScalingSize = new Size((int)(16 * scalingX), (int)(16 * scalingY));
		}


		public int Seconds { get; private set; }


		private void TimerWindow_Load(object sender, EventArgs e)
		{
			// deal with primary/secondary displays in either duplicate or extended mode...
			Rectangle area;
			using (var one = new OneNote())
			{
				//for (int i = 0; i < Screen.AllScreens.Length; i++)
				//{
				//	var s = Screen.AllScreens[i];
				//	logger.WriteLine($"Screen[{i}] ({s.DeviceName}), primary={s.Primary}, size={s.Bounds}");
				//}

				var screen = Screen.FromHandle(one.WindowHandle);
				//logger.WriteLine($"using screen ({screen.DeviceName}), primary={screen.Primary}, size={screen.Bounds}");
				Location = screen.WorkingArea.Location;
				area = screen.WorkingArea;
			}

			// must add to area.X here to handle extended mode in which the coord of the secondary
			// display is an extension of the first, so X would be greater than zero
			Left = (int)(area.X + (area.Width - Width - (10 * scalingX)));
			Top = (int)((SystemInformation.CaptionHeight + 5) * scalingY);

			maxLeft = Left;
			maxTop = area.Height - Height - 50;

			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				Left = (int)(10 * scalingX);
			}

			Seconds = 0;
			timer.Enabled = true;
			timer.Start();
		}


		private void MoveWindow(object sender, EventArgs e)
		{
			if (maxLeft > 0)
			{
				if (Left < 10) Left = 10;
				if (Left > maxLeft) Left = maxLeft;
				if (Top < 10) Top = 10;
				if (Top > maxTop) Top = maxTop;
			}
		}


		private void Tick(object sender, EventArgs e)
		{
			var span = TimeSpan.FromSeconds(++Seconds);
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
			var stamp = TimeSpan.FromSeconds(++Seconds).ToString("c");

			SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.SetText(stamp, Win.TextDataFormat.Text);
			});

		}


		private void RestartTimer(object sender, EventArgs e)
		{
			// start at -1 so the user will see Time Zero
			Seconds = -1;
		}


		private void CloseWindow(object sender, EventArgs e)
		{
			timer.Stop();
			timer.Enabled = false;
			timer.Dispose();
			Close();
		}
	}
}
