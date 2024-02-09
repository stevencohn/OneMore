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


	internal partial class HashtagErrorControl : MoreUserControl
	{
		private int radius = 5;

		public HashtagErrorControl()
		{
			InitializeComponent();
			Radius = radius;
		}


		public HashtagErrorControl(string message, string notes)
			: this()
		{
			messageBox.Text = message;

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
