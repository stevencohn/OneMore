//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Win32;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.IO.Pipes;
	using System.Text;
	using System.Windows.Forms;
	using Win = System.Windows;


	internal partial class TimerWindow : LocalizableForm
	{

		private const string KeyPath = @"River.OneMoreAddIn\CLSID";

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
			TopMost = true;
			TopLevel = true;

			(scalingX, scalingY) = UIHelper.GetScalingFactors();
			toolstrip.ImageScalingSize = new Size((int)(16 * scalingX), (int)(16 * scalingY));
		}


		private void TimerWindow_Load(object sender, EventArgs e)
		{
			var area = Screen.FromControl(this).WorkingArea;

			Left = (int)(area.Width - Width - (20 * scalingX));
			Top = (int)((SystemInformation.CaptionHeight + 5) * scalingY);

			maxLeft = Left;
			maxTop = area.Height - Height - 50;

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
			timer.Stop();
			timer.Enabled = false;
			timer.Dispose();
			Close();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private void SendCommand(string arg)
		{
			var msg = $"onemore://InsertTimerCommand/{seconds}";

			try
			{
				string pipeName = null;
				var key = Registry.ClassesRoot.OpenSubKey(KeyPath, false);
				if (key != null)
				{
					// get default value string
					pipeName = (string)key.GetValue(string.Empty);
				}

				if (string.IsNullOrEmpty(pipeName))
				{
					// problem!
					return;
				}

				using (var client = new NamedPipeClientStream(".",
					pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
				{
					// being an event-driven program to bounce messages back to OneMore
					// set timeout because we shouldn't need to wait for server ever!
					client.Connect(500);

					var buffer = Encoding.UTF8.GetBytes(msg);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error writing '{msg}'");
				logger.WriteLine(exc);
			}
		}
	}
}
