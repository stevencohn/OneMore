//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Threading;
	using System.Windows.Forms;


	/// <summary>
	/// A lightweight, themed progress dialog used to report incremental progress on a
	/// long-running operation, with an optional cancel button.
	/// </summary>
	internal partial class ProgressDialog : ThemedForm
	{
		private const float DesignDpi = 144f;

		private readonly CancellationTokenSource source;
		private Font ambientFont;


		/// <summary>
		/// Initializes a new progress dialog with a message area, progress bar, and cancel button
		/// </summary>
		public ProgressDialog()
		{
			InitializeComponent();

			Translator.Localize(this, new[] { "this", "cancelButton" });

			source = new CancellationTokenSource();
		}


		/// <summary>
		/// Gets the cancellation token that is signaled when the user clicks Cancel
		/// </summary>
		public CancellationToken Token => source.Token;


		protected override void OnLoad(EventArgs e)
		{
			// the ambient font comes from the system, which isn't rescaled per monitor with
			// autoscaling off; pin it to the point size the layout was designed with
			ambientFont = new Font("Segoe UI", 9F);
			Font = ambientFont;

			// the designer layout was authored at 150% (144 DPI) with autoscaling off; scale it
			// to the actual DPI before centering so the final size is used
			this.ScaleLayout(DesignDpi);

			base.OnLoad(e);

			// StartPosition is Manual; CenterParent is unreliable for a non-modally
			// shown (Show, not ShowDialog) form, so center over the owner explicitly
			if (Owner is not null)
			{
				Location = new Point(
					Owner.Location.X + ((Owner.Width - Width) / 2),
					Owner.Location.Y + ((Owner.Height - Height) / 2));
			}
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			ambientFont?.Dispose();
			ambientFont = null;
		}


		/// <summary>
		/// Sets the maximum increment value and resets the current value to zero
		/// </summary>
		/// <param name="value">The number of steps expected</param>
		public void SetMaximum(int value)
		{
			progressBar.Maximum = value;
			progressBar.Value = 0;
		}


		/// <summary>
		/// Sets the message displayed above the progress bar
		/// </summary>
		/// <param name="value">The message to display</param>
		public void SetMessage(string value)
		{
			messageLabel.Text = value;
			messageLabel.Refresh();
		}


		/// <summary>
		/// Increments the progress bar by one step
		/// </summary>
		public void Increment()
		{
			if (progressBar.Value < progressBar.Maximum)
			{
				progressBar.Value++;
			}
		}


		private void Cancel(object sender, EventArgs e)
		{
			source.Cancel();
			Close();
		}
	}
}
