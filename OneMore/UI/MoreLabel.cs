//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Windows.Forms;


	internal class MoreLabel : Label, ILoadControl
	{
		private const int PixelsPerStep = 4;
		private const int FinePixelsPerStep = 16;
		private const int DragThreshold = 3;

		private bool dragging;
		private bool scrubbing;
		private int startX;
		private decimal startValue;


		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }


		/// <summary>
		/// Gets or sets a NumericUpDown or TrackBar whose value is adjusted by clicking this
		/// label and dragging left or right. Shift is fine-grained, Ctrl is coarse.
		/// </summary>
		[DefaultValue(null)]
		public Control Scrubber { get; set; }


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;

			BackColor = Parent != null
				? Parent.BackColor
				: manager.GetColor("Control", ThemedBack);

			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");
		}


		private bool CanScrub =>
			Scrubber is NumericUpDown or TrackBar && Scrubber.Enabled && Enabled;


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Cursor = CanScrub ? Cursors.SizeWE : Cursors.Default;
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left && CanScrub)
			{
				dragging = true;
				scrubbing = false;
				startX = e.X;
				startValue = Scrubber is NumericUpDown nud
					? nud.Value
					: ((TrackBar)Scrubber).Value;
			}
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!dragging)
			{
				return;
			}

			var delta = e.X - startX;
			if (!scrubbing)
			{
				if (Math.Abs(delta) < DragThreshold)
				{
					return;
				}

				scrubbing = true;
				Capture = true;
			}

			var shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
			var ctrl = (ModifierKeys & Keys.Control) == Keys.Control;

			var steps = delta / (decimal)(shift ? FinePixelsPerStep : PixelsPerStep);
			if (ctrl)
			{
				steps *= 10;
			}

			if (Scrubber is NumericUpDown box)
			{
				var value = startValue + (Math.Truncate(steps) * box.Increment);
				value = Math.Max(box.Minimum, Math.Min(box.Maximum, value));
				if (value != box.Value)
				{
					box.Value = value;
				}
			}
			else if (Scrubber is TrackBar bar)
			{
				var value = (int)(startValue + (Math.Truncate(steps) * Math.Max(1, bar.SmallChange)));
				value = Math.Max(bar.Minimum, Math.Min(bar.Maximum, value));
				if (value != bar.Value)
				{
					bar.Value = value;
				}
			}
		}


		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			dragging = false;
			scrubbing = false;
			Capture = false;
		}
	}
}
