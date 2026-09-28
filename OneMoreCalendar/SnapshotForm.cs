//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using River.OneMoreAddIn.Helpers.Extensions;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Xps.Packaging;


	/// <summary>
	/// Present a popup window showing a snapshot of a page
	/// </summary>
	internal partial class SnapshotForm : RoundedForm
	{
		private const float DesignDpi = 144f;

		private readonly Point origin;


		/// <summary>
		/// Consumers should call SnapshotForm(string)
		/// </summary>
		public SnapshotForm()
			: base()
		{
			InitializeComponent();

			StaticColors = true;
		}


		/// <summary>
		/// Initialize the form
		/// </summary>
		/// <param name="page">The page being displayed</param>
		/// <param name="path">Path of the .xps file to display</param>
		/// <param name="origin">
		/// The preferred screen-coordinate top-left location of the popup
		/// </param>
		public SnapshotForm(CalendarPage page, string path, Point origin)
			: this()
		{
			Path = path;
			pathLabel.Text = page.Path;
			this.origin = origin;
		}


		public string Path { get; private set; }



		/// <summary>
		/// Preload the years to display
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			// the designer layout was authored at 150% (144 DPI) with autoscaling off; scale it
			// to the actual DPI before calling base.OnLoad so RoundedForm's rounded region uses
			// the final size
			this.ScaleLayout(DesignDpi);

			// DeviceDpi isn't valid until the handle exists, so the popup can only be kept
			// on screen once its scaled size is known
			Location = Screen.FromPoint(origin).GetBoundedLocation(origin, Size);

			// call RoundForm.base to draw background
			base.OnLoad(e);

			if (!DesignMode && File.Exists(Path))
			{
				using var xpsDoc = new XpsDocument(Path, FileAccess.Read);
				var sequence = xpsDoc.GetFixedDocumentSequence();
				using var page = sequence.DocumentPaginator.GetPage(0);

				// resize the image 150%, matching the picture box's pixel size so any
				// overflow is cropped rather than stretching the whole page into view
				var dpiX = 96.0 * pictureBox.Width * 1.5 / page.Size.Width;
				var dpiY = 96.0 * pictureBox.Height * 1.5 / page.Size.Height;

				var rtb = new RenderTargetBitmap(
					pictureBox.Width, pictureBox.Height, dpiX, dpiY, PixelFormats.Pbgra32);

				rtb.Render(page.Visual);

				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(rtb));

				using var stream = new MemoryStream();
				encoder.Save(stream);

				pictureBox.Image = new Bitmap(Image.FromStream(stream));
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Handlers...

		private void EscapeForm(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				JustLeave(sender, e);
			}
		}


		private void JustLeave(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
