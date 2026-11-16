//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class SearchErrorControl : MoreUserControl
	{
		private int radius = 5;

		public SearchErrorControl()
		{
			InitializeComponent();
			Radius = radius;
		}


		public SearchErrorControl(string message, string notes)
			: this(message, notes, false)
		{
		}


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		/// <param name="message">The message, e.g. "No results found"</param>
		/// <param name="notes">Optional details displayed below the message</param>
		/// <param name="experimental">
		/// True to tag the message with an Experimental pill, i.e. "[pill] No results found"
		/// </param>
		public SearchErrorControl(string message, string notes, bool experimental)
			: this()
		{
			messageBox.Text = message;

			if (experimental)
			{
				experimentalPill.Text = Properties.Resources.phrase_Experimental;
				experimentalPill.Visible = true;

				// center the pill on the first line of the message and shift the message after it
				var shift = experimentalPill.Width + 6;
				experimentalPill.Top = messageBox.Top
					+ Math.Max(0, (messageBox.Font.Height - experimentalPill.Height) / 2);
				messageBox.Left += shift;
				messageBox.Width -= shift;
			}

			if (notes == null)
			{
				notesBox.Visible = false;
				Height -= notesBox.Height;
			}
			else
			{
				notesBox.Text = notes;
			}

			LoadControls(Controls);
		}


		[DefaultValue(5)]
		public int Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				RecreateRegion();
			}
		}


		private void LoadControls(Control.ControlCollection controls)
		{
			foreach (Control child in controls)
			{
				if (child is ILoadControl loader)
				{
					loader.OnLoad();
				}

				if (child.Controls.Count > 0)
				{
					LoadControls(child.Controls);
				}
			}
		}


		private void RecreateRegion()
		{
			Region = Region.FromHrgn(Native.CreateRoundRectRgn(
				ClientRectangle.Left, ClientRectangle.Top,
				ClientRectangle.Right, ClientRectangle.Bottom,
				radius, radius));

			Invalidate();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			RecreateRegion();
		}
	}
}
