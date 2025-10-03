//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing.Printing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using WIA;
	using Resx = Properties.Resources;


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

		private sealed class ScanSource
		{
			public string Name { get; private set; }
			public int Bitmask { get; private set; }
			public ScanSource(string name, int mask)
			{
				Name = name;
				Bitmask = mask;
			}
			public override string ToString() => Name;
		}
		#endregion Private classes

		private readonly List<Scanner> scanners;
		private readonly List<ScanPaperSize> paperSizes;
		private Timer timer;
		private bool cancelled;


		public ScanDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ScanDialog_Text;

				Localize(new string[]
				{
					"scanLabel",
					"profileLabel",
					"sizeLabel",
					"colorLabel",
					"resolutionLabel",
					"brightnessLabel=word_Brightness",
					"contrastLabel=word_Contrast",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				colorBox.Items.Clear();
				colorBox.Items.AddRange(Resx.ScanDialog_colorBox.Split('\n'));
			}

			okButton.NotifyDefault(true);
		}


		public ScanDialog(IEnumerable<DeviceInfo> devices)
			: this()
		{
			paperSizes = new List<ScanPaperSize>();
			var printDoc = new PrintDocument();
			foreach (PaperSize size in printDoc.PrinterSettings.PaperSizes)
			{
				paperSizes.Add(new ScanPaperSize(size));
			}

			scanners = new List<Scanner>();
			foreach (var device in devices)
			{
				scanners.Add(new Scanner(device));
			}
			scannerBox.DataSource = scanners;

			colorBox.SelectedIndex = 0;
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
			PopulateSource(scanner.Capabilities);
			PopulatePaperSizes(scanner.Capabilities);

			// no need to call PopulateResolutions; triggered by PopulateSource (box.selectedIdx)
		}


		private void PopulateSource(ScanCapabilities caps)
		{
			sourceBox.Items.Clear();
			if (caps.FlatbedResoltuions is not null && caps.FlatbedResoltuions.Any())
			{
				sourceBox.Items.Add(new ScanSource(Resx.word_Flatbed, ScanHandling.Flatbed));
			}

			if (caps.FeederResoltuions is not null && caps.FeederResoltuions.Any())
			{
				sourceBox.Items.Add(new ScanSource(Resx.word_Feeder, ScanHandling.Feeder));
			}

			if (sourceBox.Items.Count > 0)
			{
				sourceBox.SelectedIndex = 0;
			}
		}


		private void PopulatePaperSizes(ScanCapabilities caps)
		{
			// device capabilities are 100th of an inch whereas bed size is 1000th of an inch,
			// so need to divide bed size by 10 to keep them propportional for comparison...

			var sizes = paperSizes
				.Where(s => s.Width <= caps.BedWidth / 10 && s.Height <= caps.BedHeight / 10)
				.ToList();

			if (sizes.Any())
			{
				sizeBox.DataSource = sizes;
				sizeBox.SelectedIndex = 0;
			}
		}


		private void PopulateResolutions(IEnumerable<int> resolutions)
		{
			resolutionBox.Items.Clear();

			foreach (var res in resolutions)
			{
				resolutionBox.Items.Add($"{res} DPI");
			}

			if (resolutionBox.Items.Count > 0)
			{
				resolutionBox.SelectedIndex = 0;
			}
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


		private void ChangeSource(object sender, EventArgs e)
		{
			var scanner = scannerBox.SelectedItem as Scanner;

			var source = (ScanSource)sourceBox.SelectedItem;
			if ((source.Bitmask & ScanHandling.Flatbed) > 0)
			{
				PopulateResolutions(scanner.Capabilities.FlatbedResoltuions);
			}
			else
			{
				PopulateResolutions(scanner.Capabilities.FeederResoltuions);
			}
		}


		private void ChangedSlider(object sender, EventArgs e)
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


		private void ChangedUpDown(object sender, EventArgs e)
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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

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
					var scanner = (Scanner)scannerBox.SelectedItem;
					using var manager = new ScannerManager(scanner.DeviceID);
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
