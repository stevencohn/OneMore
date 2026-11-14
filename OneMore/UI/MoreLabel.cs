//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
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
		private Control scrubber;
		private Form shownForm;
		private bool hasOriginal;
		private decimal originalValue;


		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }


		/// <summary>
		/// Gets or sets a NumericUpDown or TrackBar whose value is adjusted by clicking this
		/// label and dragging left or right. Shift is fine-grained, Ctrl is coarse.
		/// Double-clicking the label restores the value the control had when the form was
		/// first shown.
		/// </summary>
		[DefaultValue(null)]
		public Control Scrubber
		{
			get => scrubber;
			set
			{
				scrubber = value;
				hasOriginal = false;
			}
		}


		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			// dialogs typically assign initial values while loading, so remember the value
			// once the form is shown; otherwise it is captured on first interaction
			if (shownForm is null && Scrubber is not null)
			{
				shownForm = FindForm();
				if (shownForm is not null && !shownForm.Visible)
				{
					shownForm.Shown += CaptureOriginal;
				}
			}
		}


		private void CaptureOriginal(object sender, EventArgs e)
		{
			if (shownForm is not null)
			{
				shownForm.Shown -= CaptureOriginal;
			}

			if (!hasOriginal && Scrubber is not null)
			{
				originalValue = GetScrubValue();
				hasOriginal = true;
			}
		}


		private decimal GetScrubValue()
		{
			return Scrubber is NumericUpDown nud
				? nud.Value
				: ((TrackBar)Scrubber).Value;
		}


		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);

			if (!CanScrub || !hasOriginal)
			{
				return;
			}

			Scrubber.Focus();

			if (Scrubber is NumericUpDown box)
			{
				var value = Math.Max(box.Minimum, Math.Min(box.Maximum, originalValue));
				if (value != box.Value)
				{
					box.Value = value;
				}
			}
			else if (Scrubber is TrackBar bar)
			{
				var value = (int)Math.Max(bar.Minimum, Math.Min(bar.Maximum, originalValue));
				if (value != bar.Value)
				{
					bar.Value = value;
				}
			}
		}


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


		protected override void OnPaint(PaintEventArgs e)
		{
			if (Enabled)
			{
				base.OnPaint(e);
				return;
			}

			// a disabled Label ignores ForeColor and draws its own derived disabled color,
			// which is nearly black in dark mode, so draw the text with the themed GrayText
			// color (the ForeColor set for the disabled state) instead
			var flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
			if (!UseMnemonic)
			{
				flags |= TextFormatFlags.NoPrefix;
			}

			if (RightToLeft == RightToLeft.Yes)
			{
				flags |= TextFormatFlags.RightToLeft;
			}

			flags |= TextAlign switch
			{
				ContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
				ContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
				ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
				ContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
				ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
				ContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,
				ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,
				ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,
				_ => TextFormatFlags.Top | TextFormatFlags.Left
			};

			var bounds = Rectangle.Inflate(ClientRectangle, -Padding.Horizontal / 2, -Padding.Vertical / 2);
			TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ForeColor, flags);
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			// OnLoad picks the color once; keep it in sync when enabled state changes later
			var manager = ThemeManager.Instance;
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
				// move focus to the scrubbed control so it doesn't remain on an unrelated one
				Scrubber.Focus();

				dragging = true;
				scrubbing = false;
				startX = e.X;
				startValue = GetScrubValue();

				// fallback when the form was already shown before this label was linked
				if (!hasOriginal)
				{
					originalValue = startValue;
					hasOriginal = true;
				}
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
