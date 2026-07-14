//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// Draws a row of connected circular step badges indicating progress through a
	/// linear wizard, e.g. "Folders > Categories > Contacts". The current step is
	/// highlighted, steps before it are shown as completed with a checkmark, and steps
	/// after it are shown as upcoming.
	/// </summary>
	internal sealed class MoreStepIndicator : Control
	{
		private const int BadgeSize = 22;
		private const int TopMargin = 4;

		private string[] labels = Array.Empty<string>();
		private int currentStep = 1;


		public MoreStepIndicator()
		{
			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

			Height = 46;
		}


		/// <summary>
		/// Gets or sets the label shown under each step badge, in order. Setting this
		/// also determines the total number of steps.
		/// </summary>
		public string[] Labels
		{
			get => labels;
			set { labels = value ?? Array.Empty<string>(); Invalidate(); }
		}


		/// <summary>
		/// Gets or sets the current 1-based step. Steps before this are drawn as
		/// completed (checkmark); the current step is highlighted; steps after it are
		/// drawn as upcoming.
		/// </summary>
		public int CurrentStep
		{
			get => currentStep;
			set { currentStep = value; Invalidate(); }
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(BackColor);

			var count = labels.Length;
			if (count == 0 || Width <= 0)
			{
				return;
			}

			var manager = ThemeManager.Instance;
			var connectorColor = manager.GetColor("GrayText");
			var currentColor = manager.GetColor("Highlight");
			var completeColor = manager.GetColor("SuccessFill");
			var upcomingColor = manager.GetColor("ControlDark");
			var currentLabelColor = manager.GetColor("ControlText");
			var otherLabelColor = manager.GetColor("GrayText");

			var segment = Width / count;

			using var badgeFont = new Font(Font, FontStyle.Bold);
			using var whiteBrush = new SolidBrush(Color.White);

			for (var i = 0; i < count; i++)
			{
				var centerX = (segment * i) + (segment / 2);
				var badgeRect = new Rectangle(centerX - (BadgeSize / 2), TopMargin, BadgeSize, BadgeSize);

				if (i < count - 1)
				{
					var nextCenterX = (segment * (i + 1)) + (segment / 2);
					using var connectorPen = new Pen(connectorColor);
					g.DrawLine(connectorPen,
						badgeRect.Right, TopMargin + (BadgeSize / 2),
						nextCenterX - (BadgeSize / 2), TopMargin + (BadgeSize / 2));
				}

				var step = i + 1;
				var fill = step < currentStep
					? completeColor
					: step == currentStep ? currentColor : upcomingColor;

				using (var fillBrush = new SolidBrush(fill))
				{
					g.FillEllipse(fillBrush, badgeRect);
				}

				var glyph = step < currentStep ? "✓" : step.ToString();
				var glyphSize = g.MeasureString(glyph, badgeFont);
				g.DrawString(glyph, badgeFont, whiteBrush,
					badgeRect.X + ((badgeRect.Width - glyphSize.Width) / 2),
					badgeRect.Y + ((badgeRect.Height - glyphSize.Height) / 2));

				var label = labels[i];
				var labelFont = step == currentStep ? badgeFont : Font;
				var labelColor = step == currentStep ? currentLabelColor : otherLabelColor;

				var labelSize = g.MeasureString(label, labelFont);
				using var labelBrush = new SolidBrush(labelColor);
				g.DrawString(label, labelFont, labelBrush,
					centerX - (labelSize.Width / 2), TopMargin + BadgeSize + 4);
			}
		}
	}
}
