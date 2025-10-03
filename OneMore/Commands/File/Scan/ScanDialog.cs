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
			public bool IsCustom { get; private set; }
			public PaperKind Kind { get; private set; }
			public ScanPaperSize(PaperSize size)
			{
				var win = (size.Width / 100.0).ToString("0.0");
				var hin = (size.Height / 100.0).ToString("0.0");
				var wmm = (int)Math.Round(size.Width * 0.254);
				var hmm = (int)Math.Round(size.Height * 0.254);
				var dims = $"{win} x {hin} in ({wmm} x {hmm} mm)";
				Kind = size.Kind;
				Name = IsCustom
					? $"{size.PaperName} {dims} Custom"
					: $"{size.PaperName} {dims}";

				Height = size.Height;
				Width = size.Width;
				IsCustom = size.Kind == PaperKind.Custom;
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
		private List<ScanPaperSize> paperSizes;
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
			// do this first, before ChangeScanner is called
			DiscoverPaperSizes();

			profileBox.DataSource = ScanProfile.MakeDefaultProfiles();
			profileBox.SelectedIndex = 0;
			profileBox.SelectedIndexChanged += ChangedProfile;

			// discover scanners
			scanners = new List<Scanner>();
			foreach (var device in devices)
			{
				scanners.Add(new Scanner(device));
			}
			scannerBox.DataSource = scanners;
		}


		public string ImageData { get; private set; }


		public int ImageHeight { get; private set; }


		public int ImageWidth { get; private set; }


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChangedProfile(null, e);
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// ProcessCmdKey intercepts Key messages before they reach individual controls
			if (keyData == Keys.Escape && okButton.Visible)
			{
				DialogResult = DialogResult.Cancel;
				Close();
				return true; // handled
			}

			if (keyData == Keys.Enter && okButton.Visible)
			{
				BeginScanning(null, EventArgs.Empty);
				return true; // handled
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}



		private void DiscoverPaperSizes()
		{
			var sizes = new List<ScanPaperSize>();
			var printDoc = new PrintDocument();
			foreach (PaperSize size in printDoc.PrinterSettings.PaperSizes)
			{
				sizes.Add(new ScanPaperSize(size));
			}

			paperSizes = new List<ScanPaperSize>(sizes.Count);

			Move(PaperKind.Letter);
			Move(PaperKind.Legal);
			Move(PaperKind.A4);
			paperSizes.AddRange(sizes.Where(s => !s.IsCustom).OrderBy(s => s.Name));
			paperSizes.AddRange(sizes.Where(s => s.IsCustom).OrderBy(s => s.Name));

			void Move(PaperKind kind)
			{
				if (sizes.FirstOrDefault(s => s.Kind == kind) is ScanPaperSize item)
				{
					sizes.Remove(item);
					paperSizes.Add(item);
				}
			}
		}


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


		private void ChangedProfile(object sender, EventArgs e)
		{
			var profile = (ScanProfile)profileBox.SelectedItem;

			var mask = profile.UseFeeder ? ScanHandling.Feeder : ScanHandling.Flatbed;
			var index = sourceBox.Items.Cast<ScanSource>().ToList().FindIndex(s => s.Bitmask == mask);
			sourceBox.SelectedIndex = index >= 0 ? index : 0;

			colorBox.SelectedIndex = profile.Intent switch
			{
				ScanIntents.Color => 0,
				ScanIntents.Grayscale => 1,
				ScanIntents.BlackAndWhite => 2,
				_ => 0
			};

			var dpi = $"{profile.Dpi} DPI";
			index = resolutionBox.Items.Cast<string>().ToList().FindIndex(r => r == dpi);
			resolutionBox.SelectedIndex = index >= 0 ? index : 0;

			brightnessSlider.Value = profile.Brightness;
			contrastSlider.Value = profile.Contrast;
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

			// SelectedIndex will be set when ChangeProfile is called
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

					var dpi = int.Parse(resolutionBox.SelectedItem.ToString().Split(' ')[0]);
					var props = new ScannerManager.ScanProperties
					{
						UseFeeder = ((ScanSource)sourceBox.SelectedItem).Bitmask == ScanHandling.Feeder,
						ColorIntent = 1 << colorBox.SelectedIndex,
						Brightness = (int)brightnessBox.Value * 10,
						Contrast = (int)contrastBox.Value * 10,
						HorizontalResolution = dpi,
						VerticalResolution = dpi
					};

					ImageData = manager.Scan(props);
					ImageHeight = manager.ImageHeight;
					ImageWidth = manager.ImageWidth;

					if (!cancelled)
					{
						Invoke((MethodInvoker)(() =>
						{
							DialogResult = DialogResult.OK;
							Close();
						}));
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);

					progressBar.Visible = false;
					okButton.Visible = true;
					SetState(true);
				}
				finally
				{
					timer.Stop();
					timer.Dispose();
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
			cancelled = true;

			if (timer is not null)
			{
				timer.Stop();
				timer.Dispose();
			}

			DialogResult = DialogResult.Cancel;
			Close();
		}


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
		}
	}
}
