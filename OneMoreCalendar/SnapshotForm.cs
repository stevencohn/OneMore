//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window showing a snapshot of a page
	/// </summary>
	internal partial class SnapshotForm : RoundedForm
	{

		/// <summary>
		/// Consumers should call SnapshotForm(string)
		/// </summary>
		public SnapshotForm()
			: base()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Initialize the form
		/// </summary>
		/// <param name="path">Path of the .emf file to display</param>
		public SnapshotForm(CalendarPage page, string path)
			: this()
		{
			Path = path;
			pathLabel.Text = page.Path;
		}


		public string Path { get; private set; }



		/// <summary>
		/// Preload the years to display
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			// call RoundForm.base to draw background
			base.OnLoad(e);

			if (!DesignMode && File.Exists(Path))
			{
				using (var source = new Metafile(Path))
				{
					var target = new Bitmap(pictureBox.Width, pictureBox.Height);
					using (var g = Graphics.FromImage(target))
					{
						// resize the image 150%
						g.DrawImage(source,
							new Rectangle(0, 0, (int)(target.Width * 1.5), (int)(target.Height * 1.5)),
							new Rectangle(0, 0, source.Width, source.Height),
							GraphicsUnit.Pixel
							);

						//var path = System.IO.Path.Combine(
						//	System.IO.Path.GetTempPath(),
						//	System.IO.Path.GetRandomFileName() + ".png");

						//target.Save(path, ImageFormat.Png);
						//pictureBox.ImageLocation = path;

						pictureBox.Image = target;
					}
				}
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
