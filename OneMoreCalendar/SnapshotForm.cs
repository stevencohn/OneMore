//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window showing a snapshot of a page
	/// </summary>
	internal partial class SnapshotForm : RoundForm
	{

		private string pathToEmf;


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
		public SnapshotForm(string path)
			: this()
		{
			pathToEmf = path;
		}


		/// <summary>
		/// Preload the years to display
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			// call RoundForm.base to draw background
			base.OnLoad(e);

			if (!DesignMode && File.Exists(pathToEmf))
			{
				var meta = new Metafile(pathToEmf);
				pictureBox.Image = meta;
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
