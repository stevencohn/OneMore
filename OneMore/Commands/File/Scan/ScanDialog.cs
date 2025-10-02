//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing.Printing;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using WIA;


	internal partial class ScanDialog : MoreForm
	{
		#region Private classes
		private sealed class Scanner
		{
			public string DeviceID { get; private set; }
			public string Name { get; private set; }
			public ScanCapabilities Capabilities { get; set; }
			public Scanner(DeviceInfo info)
			{
				DeviceID = info.DeviceID;
				Name = info.Properties.Get<string>("Name");

			}
			public override string ToString() => Name;
		}

		private sealed class ScanPaperSize
		{
			public string Name { get; private set; }
			public int Height { get; private set; }
			public int Width { get; private set; }
			public ScanPaperSize(PaperSize size)
			{
				var win = (size.Width / 100.0).ToString("0.0");
				var hin = (size.Height / 100.0).ToString("0.0");
				var wmm = (int)Math.Round(size.Width * 0.254);
				var hmm = (int)Math.Round(size.Height * 0.254);
				var dims = $"{win} x {hin} in ({wmm} x {hmm} mm)";
				Name = size.Kind == PaperKind.Custom
					? $"{size.PaperName} {dims} Custom"
					: $"{size.PaperName} {dims}";

				Height = size.Height;
				Width = size.Width;
			}
			public override string ToString()
			{
				return Name;
			}
		}
		#endregion Private classes

		private readonly List<Scanner> scanners;
		private Timer timer;
		private bool cancelled;


		public ScanDialog()
		{
			InitializeComponent();
		}


		public ScanDialog(IEnumerable<DeviceInfo> devices)
			: this()
		{
			scanners = new List<Scanner>();
			foreach (var device in devices)
			{
				scanners.Add(new Scanner(device));
			}
			scannerBox.DataSource = scanners;

			var sizes = new List<ScanPaperSize>();
			var printDoc = new PrintDocument();
			foreach (PaperSize size in printDoc.PrinterSettings.PaperSizes)
			{
				sizes.Add(new ScanPaperSize(size));
			}
			sizeBox.DataSource = sizes;

			sourceBox.SelectedIndex = 0;
			sizeBox.SelectedIndex = 0;
			colorBox.SelectedIndex = 0;
			//resolutionBox.SelectedIndex = 0;
		}


		public string ImageData { get; private set; }


		public int ImageHeight { get; private set; }


		public int ImageWidth { get; private set; }


		private void ChangeScanner(object sender, EventArgs e)
		{
			var scanner = scannerBox.SelectedItem as Scanner;
			if (scanner.Capabilities is null)
			{
				SetState(false);
				using var manager = new ScannerManager(scanner.DeviceID);
				scanner.Capabilities = manager.GetCapabilities();
				SetState(true);
			}

			modelLabel.Text = scanner.Capabilities.Model;
		}


		private void SetState(bool enabled)
		{
			scannerBox.Enabled = enabled;
			profileBox.Enabled = enabled;
			sourceBox.Enabled = enabled;
			sizeBox.Enabled = enabled;
			colorBox.Enabled = enabled;
			resolutionBox.Enabled = enabled;
			brightnessBox.Enabled = enabled;
			brightnessSlider.Enabled = enabled;
			contrastBox.Enabled = enabled;
			contrastSlider.Enabled = enabled;
			okButton.Enabled = enabled;
		}


		private void ChangedSlider(object sender, System.EventArgs e)
		{
			if (sender == brightnessSlider)
			{
				brightnessBox.Value = brightnessSlider.Value;
			}
			else
			{
				contrastBox.Value = contrastSlider.Value;
			}
		}


		private void ChangedUpDown(object sender, System.EventArgs e)
		{
			if (sender == brightnessBox)
			{
				brightnessSlider.Value = (int)brightnessBox.Value;
			}
			else
			{
				contrastSlider.Value = (int)contrastBox.Value;
			}
		}


		private void BeginScanning(object sender, EventArgs e)
		{
			okButton.Visible = false;
			progressBar.Visible = true;
			progressBar.Value = 0;
			cancelled = false;

			timer = new Timer { Interval = 1000 };
			timer.Tick += MoveSlider;
			timer.Start();

			SetState(false);

			Task.Run(() =>
			{
				try
				{
					using var manager = new ScannerManager(((Scanner)scannerBox.SelectedItem).DeviceID);
					ImageData = manager.Scan();
					ImageHeight = manager.ImageHeight;
					ImageWidth = manager.ImageWidth;

					Invoke((MethodInvoker)(() =>
					{
						timer.Stop();
						timer.Dispose();

						if (cancelled)
						{
							progressBar.Visible = false;
							okButton.Visible = true;
							SetState(true);
							cancelled = false;
							return;
						}

						DialogResult = DialogResult.OK;
						Close();
					}));
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);

					timer.Stop();
					timer.Dispose();
					progressBar.Visible = false;
					okButton.Visible = true;
					SetState(true);
				}
			});
		}

		private void MoveSlider(object sender, EventArgs args)
		{
			if (progressBar.Value < progressBar.Maximum)
			{
				progressBar.Increment(1);
			}
		}


		private void Cancel(object sender, EventArgs e)
		{
			if (timer is null)
			{
				DialogResult = DialogResult.Cancel;
				Close();
				return;
			}

			cancelled = true;
			timer.Stop();
			timer.Dispose();
			progressBar.Visible = false;
			okButton.Visible = true;
			SetState(true);
		}


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
		}
	}
}
