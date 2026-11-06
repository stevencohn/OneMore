//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

#define xLogging

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Accepts an image, lets the user create a single crop region,
	/// and crops the image to that region
	/// </summary>
	/// <seealso cref="https://www.codeproject.com/articles/27748/marching-ants"/>
	internal partial class CropImageDialog : UI.MoreForm
	{

		#region Supporting classes

		// tracks the current function, state of the cursor
		private enum MoveState
		{
			None,
			Selecting,
			Moving,
			Sizing,
			Panning
		}


		// identifies each of the resize handles around the region
		private enum SizingHandle
		{
			TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left
		}


		// hover boundary around each resize handle (larger than handle glyph itself)
		private sealed class SelectionHandle
		{
			public SizingHandle Position;
			public RectangleF Bounds;
		}

		#endregion Supporting classes


		// visual margin around image, provides room for cursor to select edges
		private const int ImageMargin = 8;

		// diameter of selection handles
		private const int HandleSize = 8;

		// zoom is the display scale, 1.0 being 100% where one image pixel is one screen pixel
		private const double MinZoom = 0.1;
		private const double MaxZoom = 16.0;
		private const double ZoomStep = 10.0; // percent
		private double zoom = 1.0;

		// the image ratio from the last layout, used to rescale the selection when it changes
		private double lastRatio;

		// panning with Space+drag when the zoomed image is larger than the viewport
		private bool hotkeysSuspended;
		private bool spaceDown;
		private Point panPoint;

		// size of the zoomed image plus margins, and the offset of the view within it
		private Size content;
		private Point scroll;
		private bool updatingBars;

		// the selection
		private Point startPoint;
		private Point endPoint;
		private Point movePoint;
		private Rectangle selectionBounds;
		private int antOffset;
		private readonly Image original;
		private readonly Region selectionRegion;
		private readonly GraphicsPath selectionPath;

		// the original image
		private Rectangle imageBounds;
		private SelectionHandle currentHandle;
		private readonly List<SelectionHandle> handles;
		private readonly int brightness;

		// the current cursor function
		private MoveState moveState;


		#region Lifecycle

		/// <summary>
		/// Initialize a new dialog with defaults
		/// </summary>
		public CropImageDialog()
		{
			InitializeComponent();

			selectionRegion = new Region();
			selectionRegion.MakeEmpty();
			selectionPath = new GraphicsPath();
			selectionPath.Reset();

			handles = new List<SelectionHandle>();
			moveState = MoveState.None;

			RememberSize = true;
			pictureBox.MouseWheel += Picture_MouseWheel;
			introPanel.SizeChanged += (s, e) => FitIntroText();

			if (NeedsLocalizing())
			{
				Text = Resx.CropImageDialog_Text;

				Localize(new string[]
				{
					"introText",
					"selectButton=phrase_SelectAll",
					"cropButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		/// <summary>
		/// Initialize a new dialog showing the given image
		/// </summary>
		/// <param name="image">An image to display and crop</param>
		public CropImageDialog(Image image)
			: this()
		{
			Image = original = image;

			UpdateLayout(false);

			brightness = image.GetBrightness();

			sizeStatusLabel.Text = string.Format(
				Resx.CropImageDialog_imageSize, Image.Width, Image.Height);

			pictureBox.Refresh();

#if Logging
			var hasRealDpi = (image.Flags & (int)ImageFlags.HasRealDpi) > 0;
			var hasRealPixelSize = (image.Flags & (int)ImageFlags.HasRealPixelSize) > 0;

			logger.WriteLine(
				$"IMAG hasRealDpi:{hasRealDpi} hasRealPixelSize:{hasRealPixelSize} | " +
				$"hRes:{image.HorizontalResolution} vRes:{image.VerticalResolution} | " +
				$"size:{image.Width}x{image.Height} " +
				$"physical:{image.PhysicalDimension.Width}x{image.PhysicalDimension.Height}\n" +
				$"IMAG bounds:{imageBounds.Width}x{imageBounds.Height}");
#endif
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode && Image is not null)
			{
				// the default size, DPI scaling or centering may have pushed the dialog past the screen
				var area = Screen.FromControl(this).WorkingArea;
				Size = new Size(Math.Min(Width, area.Width), Math.Min(Height, area.Height));
				Location = new Point(
					Math.Max(area.Left, Math.Min(Left, area.Right - Width)),
					Math.Max(area.Top, Math.Min(Top, area.Bottom - Height)));

				// OneMore's global command hotkeys, e.g. Ctrl+D Fill Down, would otherwise
				// intercept keys before this dialog sees them
				HotkeyManager.Suspend();
				hotkeysSuspended = true;
			}
		}


		private void CropImageDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (hotkeysSuspended)
			{
				hotkeysSuspended = false;
				HotkeyManager.Resume();
			}

			if (original != Image)
			{
				original.Dispose();
			}
		}

		#endregion Lifecycle


		/// <summary>
		/// Gets the cropped image, to be used after dialog closes with DialogResult.OK
		/// </summary>
		public Image Image { get; private set; }


		// ---------------------------------------------------------------------------------------
		// Paint

		private void Picture_Hover(object sender, EventArgs e)
		{
			pictureBox.Focus();
		}


		private void MarchingTimer_Tick(object sender, EventArgs e)
		{
			antOffset--;
			antOffset %= 6;
			pictureBox.Refresh();
		}


		// ---------------------------------------------------------------------------------------
		// Zoom and layout

		// image pixels per screen pixel
		private double GetRatio()
		{
			return 1.0 / zoom;
		}


		private void ComputeImageBounds(double ratio)
		{
			imageBounds = new Rectangle(
				ImageMargin, ImageMargin,
				(int)Math.Round(Image.Width / ratio), (int)Math.Round(Image.Height / ratio));
		}


		// wrap the intro text to the dialog width, growing the panel to show all of it
		private void FitIntroText()
		{
			var width = introText.Width;
			if (width <= 0 || string.IsNullOrEmpty(introText.Text))
			{
				return;
			}

			var size = TextRenderer.MeasureText(
				introText.Text, introText.Font,
				new Size(width, int.MaxValue),
				TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

			var panelHeight = Math.Max(66, size.Height + introPanel.Padding.Vertical + 4);
			if (introPanel.Height != panelHeight)
			{
				introPanel.Height = panelHeight;
			}

			var labelHeight = panelHeight - introPanel.Padding.Vertical;
			if (introText.Height != labelHeight)
			{
				introText.Height = labelHeight;
			}
		}


		private void ViewPanel_SizeChanged(object sender, EventArgs e)
		{
			UpdateLayout(true);
			InvalidateButtons();
		}


		// owner-drawn buttons, especially when disabled, can be left with stale pixels after
		// the dialog is resized, so force them to repaint
		private void InvalidateButtons()
		{
			buttonPanel.Invalidate(true);
			selectButton.Invalidate();
			cropButton.Invalidate();
			cancelButton.Invalidate();
		}


		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResizeEnd(e);
			InvalidateButtons();
		}


		/// <summary>
		/// Recomputes the image bounds for the current zoom and viewport, sizing the picture box
		/// so the view panel can scroll when the image is larger than the viewport.
		/// </summary>
		/// <param name="rescale">
		/// True to move/scale the current selection to stay over the same image area
		/// </param>
		private void UpdateLayout(bool rescale)
		{
			if (Image is null)
			{
				return;
			}

			var ratio = GetRatio();

			if (rescale && lastRatio > 0 && Math.Abs(lastRatio - ratio) > 0.00001)
			{
				ComputeImageBounds(ratio);
				RescaleSelection(lastRatio / ratio);
			}

			lastRatio = ratio;
			ComputeImageBounds(ratio);

			content = new Size(
				imageBounds.Width + ImageMargin * 2,
				imageBounds.Height + ImageMargin * 2);

			ConfigureScrollBars();

			zoomLabel.Text = $"{Math.Round(100.0 * zoom)}%";
			pictureBox.Invalidate();
		}


		// The picture box always fills the view and the image is drawn offset by scroll;
		// scroll bars are shown only when the zoomed image is larger than the view.
		private void ConfigureScrollBars()
		{
			var avail = viewPanel.ClientSize;
			var needH = false;
			var needV = false;

			{
				for (var i = 0; i < 2; i++)
				{
					needH = content.Width > avail.Width - (needV ? vBar.Width : 0);
					needV = content.Height > avail.Height - (needH ? hBar.Height : 0);
				}
			}

			updatingBars = true;
			try
			{
				hBar.Visible = needH;
				vBar.Visible = needV;
				viewPanel.PerformLayout();

				var view = pictureBox.ClientSize;
				var x = needH ? Math.Max(0, Math.Min(scroll.X, content.Width - view.Width)) : 0;
				var y = needV ? Math.Max(0, Math.Min(scroll.Y, content.Height - view.Height)) : 0;
				scroll = new Point(x, y);

				if (needH)
				{
					hBar.Minimum = 0;
					hBar.Maximum = Math.Max(0, content.Width - 1);
					hBar.LargeChange = Math.Max(1, view.Width);
					hBar.SmallChange = Math.Max(1, view.Width / 10);
					hBar.Value = Math.Min(x, hBar.Maximum);
				}

				if (needV)
				{
					vBar.Minimum = 0;
					vBar.Maximum = Math.Max(0, content.Height - 1);
					vBar.LargeChange = Math.Max(1, view.Height);
					vBar.SmallChange = Math.Max(1, view.Height / 10);
					vBar.Value = Math.Min(y, vBar.Maximum);
				}
			}
			finally
			{
				updatingBars = false;
			}
		}


		private void SetScroll(int x, int y)
		{
			if (!CanPan())
			{
				return;
			}

			var view = pictureBox.ClientSize;
			scroll = new Point(
				hBar.Visible ? Math.Max(0, Math.Min(x, content.Width - view.Width)) : 0,
				vBar.Visible ? Math.Max(0, Math.Min(y, content.Height - view.Height)) : 0);

			updatingBars = true;
			try
			{
				if (hBar.Visible)
				{
					hBar.Value = Math.Min(scroll.X, hBar.Maximum);
				}

				if (vBar.Visible)
				{
					vBar.Value = Math.Min(scroll.Y, vBar.Maximum);
				}
			}
			finally
			{
				updatingBars = false;
			}

			pictureBox.Invalidate();
		}


		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (!updatingBars)
			{
				scroll = new Point(
					hBar.Visible ? hBar.Value : 0,
					vBar.Visible ? vBar.Value : 0);

				pictureBox.Invalidate();
			}
		}


		// translate a mouse event from picture box coordinates to image content coordinates
		private MouseEventArgs ToContent(MouseEventArgs e)
		{
			return new MouseEventArgs(
				e.Button, e.Clicks, e.X + scroll.X, e.Y + scroll.Y, e.Delta);
		}


		private void View_MouseDown(object sender, MouseEventArgs e)
		{
			Picture_MouseDown(sender, ToContent(e));
		}


		private void View_MouseMove(object sender, MouseEventArgs e)
		{
			Picture_MouseMove(sender, ToContent(e));
		}


		private void View_MouseUp(object sender, MouseEventArgs e)
		{
			Picture_MouseUp(sender, ToContent(e));
		}


		private void RescaleSelection(double factor)
		{
			if (selectionBounds.Width == 0 && selectionBounds.Height == 0)
			{
				return;
			}

			startPoint = ScalePoint(startPoint, factor);
			endPoint = ScalePoint(endPoint, factor);
			movePoint = ScalePoint(movePoint, factor);

			selectionBounds = new Rectangle(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
				);

			SetSelection(selectionBounds);
		}


		private Point ScalePoint(Point point, double factor)
		{
			var x = (int)Math.Round((point.X - ImageMargin) * factor) + ImageMargin;
			var y = (int)Math.Round((point.Y - ImageMargin) * factor) + ImageMargin;

			// rounding may push a point off the edge of the image
			return new Point(
				Math.Max(imageBounds.Left, Math.Min(imageBounds.Right, x)),
				Math.Max(imageBounds.Top, Math.Min(imageBounds.Bottom, y)));
		}


		/// <summary>
		/// Sets the zoom, keeping the image point under the anchor stationary.
		/// </summary>
		/// <param name="value">The new zoom, 1.0 being 100%</param>
		/// <param name="anchor">
		/// A point relative to the view panel, default is the center of the view
		/// </param>
		private void SetZoom(double value, Point? anchor = null)
		{
			value = Math.Max(MinZoom, Math.Min(MaxZoom, value));

			if (Image is null || Math.Abs(value - zoom) < 0.0001)
			{
				return;
			}

			var view = viewPanel.ClientSize;
			var a = anchor ?? new Point(view.Width / 2, view.Height / 2);

			// fraction of the image currently under the anchor
			var fx = (a.X + scroll.X - ImageMargin) / (double)Math.Max(1, imageBounds.Width);
			var fy = (a.Y + scroll.Y - ImageMargin) / (double)Math.Max(1, imageBounds.Height);

			zoom = value;
			UpdateLayout(true);

			SetScroll(
				ImageMargin + (int)Math.Round(fx * imageBounds.Width) - a.X,
				ImageMargin + (int)Math.Round(fy * imageBounds.Height) - a.Y);

			pictureBox.Refresh();
		}


		private void ZoomInButton_Click(object sender, EventArgs e)
		{
			SetZoom(StepZoom(true));
		}


		private void ZoomOutButton_Click(object sender, EventArgs e)
		{
			SetZoom(StepZoom(false));
		}


		// step the displayed zoom percentage up or down by 10, snapping to multiples of 10
		private double StepZoom(bool zoomIn)
		{
			if (Image is null)
			{
				return zoom;
			}

			var percent = zoom * 100.0;
			var target = zoomIn
				? (Math.Floor(percent / ZoomStep + 0.001) + 1) * ZoomStep
				: (Math.Ceiling(percent / ZoomStep - 0.001) - 1) * ZoomStep;

			return Math.Max(ZoomStep, target) / 100.0;
		}


		private void ZoomLabel_Click(object sender, EventArgs e)
		{
			var current = (int)Math.Round(100.0 * zoom);

			// Show() is non-blocking so the menu must outlive this method; dispose it on close
			var menu = new UI.MoreContextMenuStrip();
			menu.Closed += (s, a) => BeginInvoke(new Action(menu.Dispose));
			foreach (var percent in new[] { 100, 125, 150, 200, 400, 800, 1200 })
			{
				var value = percent;
				var item = new ToolStripMenuItem($"{value}%") { Checked = value == current };
				item.Click += (s, a) => SetZoomPercent(value);
				menu.Items.Add(item);
			}

			// open upward from the label since it lives at the bottom of the dialog
			menu.ApplyScale(statusStrip);
			menu.Show(statusStrip, new Point(zoomLabel.Bounds.Left, 0),
				ToolStripDropDownDirection.AboveRight);
		}


		// sets zoom so the displayed percentage equals the given value
		private void SetZoomPercent(int percent)
		{
			SetZoom(percent / 100.0);
		}


		private void Picture_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e is HandledMouseEventArgs handled)
			{
				handled.Handled = true;
			}

			HandleWheel(e.Delta);
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// wheel is routed here when another control, such as a button, has focus
			if (!HandleWheel(e.Delta))
			{
				base.OnMouseWheel(e);
			}
		}


		// Ctrl+wheel zooms at the cursor; wheel scrolls the zoomed image, Shift+wheel horizontally
		private bool HandleWheel(int delta)
		{
			var point = viewPanel.PointToClient(Cursor.Position);
			if (!viewPanel.ClientRectangle.Contains(point))
			{
				return false;
			}

			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				SetZoom(StepZoom(delta > 0), point);
			}
			else if (CanPan())
			{
				var shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

				SetScroll(
					scroll.X - (shift ? delta : 0),
					scroll.Y - (shift ? 0 : delta));
			}

			return true;
		}


		// the zoomed image is larger than the viewport
		private bool CanPan()
		{
			return hBar.Visible || vBar.Visible;
		}


		private bool IsOverView()
		{
			return viewPanel.ClientRectangle.Contains(viewPanel.PointToClient(Cursor.Position));
		}


		private void PictureBox_SizeChanged(object sender, EventArgs e)
		{
			var g = pictureBox.CreateGraphics();
			if (selectionRegion != null && !selectionRegion.IsEmpty(g))
			{
				var bounds = selectionRegion.GetBounds(g);

				if (bounds.Right > imageBounds.Right || bounds.Bottom > imageBounds.Bottom)
				{
					var right = Math.Min(bounds.Right, imageBounds.Right);
					var bottom = Math.Min(bounds.Bottom, imageBounds.Bottom);

					if (right <= bounds.Left || bottom <= bounds.Top)
					{
						selectionBounds = Rectangle.Empty;
						SetSelection(Rectangle.Empty);
					}
					else
					{
						endPoint.X = (int)right;
						endPoint.Y = (int)bottom;

						SetSelection(new Rectangle(
							(int)bounds.X, (int)bounds.Y,
							(int)(right - bounds.X),
							(int)(bottom - bounds.Y)
							));
					}

					SetButtonEnabled();
				}
			}

			pictureBox.Refresh();
		}


		private void Picture_Paint(object sender, PaintEventArgs e)
		{
			handles.Clear();

			// fill picturebox with checkerboard
			using (var brush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.DarkGray, Color.Gray))
			{
				e.Graphics.FillRectangle(brush, 0, 0, pictureBox.Width, pictureBox.Height);
			}

			if (Image == null)
			{
				statusLabel.Text = "No image";
				return;
			}

			// image and selection are in content coordinates; offset by the scroll position
			e.Graphics.TranslateTransform(-scroll.X, -scroll.Y);

			PaintImage(e.Graphics);

			if (selectionRegion.IsEmpty(e.Graphics))
			{
				statusLabel.Text = "No selection";
				return;
			}

			PaintSelection(e.Graphics);
			UpdateStatus();
		}


		private void PaintImage(Graphics g)
		{
			// zoom image into viewable area
			var ratio = GetRatio();
			ComputeImageBounds(ratio);

			// draw outline for images with transparency (png)
			g.DrawRectangle(Pens.Gray, imageBounds);

			if (ratio <= 0.5)
			{
				// magnified, show true pixels to aid precise selection
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.Half;
			}

			// draw image
			g.DrawImage(Image, imageBounds);
		}


		private void PaintSelection(Graphics g)
		{
			// fill inner field of selected region with transparent blue
			using (var fill = new SolidBrush(Color.FromArgb(40, 0, 138, 244)))
			{
				g.FillRegion(fill, selectionRegion);
			}

			// draw marching ants outline
			using var pen = new Pen(Color.White, 1f);
			pen.DashStyle = DashStyle.Dash;
			pen.DashPattern = new float[2] { 3, 3 };
			pen.DashOffset = antOffset;

			// only render the ants for the visible part of the (possibly zoomed) picture box
			var visible = new Rectangle(scroll, pictureBox.ClientSize);

			// set up pen for the ants
			using (var ant = new Bitmap(visible.Width, visible.Height))
			{
				using (var gi = Graphics.FromImage(ant))
				{
					gi.TranslateTransform(-visible.X, -visible.Y);

					// region is magenta but we'll use that as our transparent color
					gi.Clear(Color.Magenta);

					using (var outline = MakeOutlinePath())
					{
						gi.DrawPath(Pens.Black, outline);
						gi.DrawPath(pen, outline);
					}

					gi.FillRegion(Brushes.Magenta, selectionRegion);
				}

				// make center of ant region transparent
				ant.MakeTransparent(Color.Magenta);

				// draw the ants on the image
				g.DrawImageUnscaled(ant, visible.X, visible.Y);
			}

			// draw resize handles
			// path bounds are in content coordinates, unaffected by the graphics transform
			var bounds = selectionPath.GetBounds();

			AddHandle(SizingHandle.TopLeft, bounds.Left, bounds.Top, g);
			AddHandle(SizingHandle.TopRight, bounds.Right, bounds.Top, g);
			AddHandle(SizingHandle.BottomRight, bounds.Right, bounds.Bottom, g);
			AddHandle(SizingHandle.BottomLeft, bounds.Left, bounds.Bottom, g);

			AddHandle(SizingHandle.Top, bounds.Left + ((bounds.Right - bounds.Left) / 2), bounds.Top, g);
			AddHandle(SizingHandle.Right, bounds.Right, bounds.Top + ((bounds.Bottom - bounds.Top) / 2), g);
			AddHandle(SizingHandle.Bottom, bounds.Left + ((bounds.Right - bounds.Left) / 2), bounds.Bottom, g);
			AddHandle(SizingHandle.Left, bounds.Left, bounds.Top + ((bounds.Bottom - bounds.Top) / 2), g);
		}


		private void UpdateStatus()
		{
			var ratio = GetRatio();

			var r = new Rectangle(
				(int)((selectionBounds.X - ImageMargin) * ratio),
				(int)((selectionBounds.Y - ImageMargin) * ratio),
				(int)(selectionBounds.Width * ratio),
				(int)(selectionBounds.Height * ratio)
				);

			statusLabel.Text = string.Format(Resx.CropImageDialog_bounds, r.X, r.Y, r.Width, r.Height);
			statusStrip.Invalidate();
			statusStrip.Refresh();
		}


		private void AddHandle(SizingHandle position, float x, float y, Graphics g)
		{
			var rectangle = new RectangleF(x - (HandleSize / 2), y - (HandleSize / 2), HandleSize, HandleSize);
			var pen = brightness < 50 ? Pens.LightGray : Pens.Black;
			g.DrawArc(pen, rectangle.Left, rectangle.Top, HandleSize, HandleSize, 0, 360);

			// make hover region larger than the handle circle itself so it's easier to hit
			rectangle.Inflate(HandleSize, HandleSize);

			handles.Add(new SelectionHandle
			{
				Position = position,
				Bounds = rectangle
			});
		}


		private void SetSelection(Rectangle rectangle)
		{
			selectionRegion.MakeEmpty();
			selectionPath.Reset();

			if (!rectangle.IsEmpty)
			{
				selectionPath.AddRectangle(rectangle);
				selectionRegion.Union(selectionPath);
			}
		}


		private GraphicsPath MakeOutlinePath()
		{
			var path = new GraphicsPath();
			if (selectionPath.PointCount > 0)
			{
				path.AddPath(selectionPath, false);
				path.Widen(Pens.White);
			}
			return path;
		}


		// ---------------------------------------------------------------------------------------
		// Mouse down

		private void Picture_MouseDown(object sender, MouseEventArgs e)
		{
			if (Image == null)
			{
				return;
			}

			// Space+drag pans a zoomed image larger than the viewport
			if (spaceDown && CanPan())
			{
				panPoint = Cursor.Position;
				moveState = MoveState.Panning;
				pictureBox.Cursor = Cursors.SizeAll;
				return;
			}

			// did we just grab a handle?
			if ((currentHandle = HitHandle(e.Location)) != null)
			{
				switch (currentHandle.Position)
				{
					case SizingHandle.Top:
					case SizingHandle.Left:
					case SizingHandle.TopLeft:
						startPoint.X = selectionBounds.Right;
						startPoint.Y = selectionBounds.Bottom;
						break;

					case SizingHandle.TopRight:
						startPoint.X = selectionBounds.Left;
						startPoint.Y = selectionBounds.Bottom;
						break;

					case SizingHandle.BottomLeft:
						startPoint.X = selectionBounds.Right;
						startPoint.Y = selectionBounds.Top;
						break;

					case SizingHandle.Bottom:
					case SizingHandle.Right:
					case SizingHandle.BottomRight:
						startPoint.X = selectionBounds.Left;
						startPoint.Y = selectionBounds.Top;
						break;
				}

				moveState = MoveState.Sizing;
				return;
			}

			if (selectionBounds.Contains(e.Location))
			{
				movePoint = e.Location;
				moveState = MoveState.Moving;
				return;
			}

			// else starting a new region
			if (imageBounds.Contains(e.Location))
			{
				startPoint.X = e.X;
				startPoint.Y = e.Y;
				moveState = MoveState.Sizing;
				selectionBounds = new Rectangle(e.X, e.Y, 0, 0);
			}
			else
			{
				startPoint.X = startPoint.Y = -1;
				moveState = MoveState.None;
				selectionBounds = Rectangle.Empty;
			}

			SetSelection(Rectangle.Empty);

			if (!marchingTimer.Enabled)
			{
				marchingTimer.Start();
			}

			moveState = MoveState.Selecting;
		}


		private SelectionHandle HitHandle(Point location)
		{
			foreach (var handle in handles)
			{
				if (handle.Bounds.Contains(location))
				{
					return handle;
				}
			}

			return null;
		}


		// ---------------------------------------------------------------------------------------
		// Mouse move

		private void Picture_MouseMove(object sender, MouseEventArgs e)
		{
			if (moveState == MoveState.Panning)
			{
				// use screen coordinates; client coordinates shift as the view scrolls
				var current = Cursor.Position;

				SetScroll(
					scroll.X - (current.X - panPoint.X),
					scroll.Y - (current.Y - panPoint.Y));

				panPoint = current;
			}
			else if (moveState == MoveState.Selecting)
			{
				SelectRegion(e.Location);
			}
			else if (moveState == MoveState.Sizing)
			{
				ResizeRegion(e.Location);
			}
			else if (moveState == MoveState.Moving)
			{
				MoveRegion(e.Location);
			}
			else if (spaceDown && CanPan())
			{
				pictureBox.Cursor = Cursors.SizeAll;
			}
			else
			{
				var handle = HitHandle(e.Location);
				if (handle != null)
				{
					switch (handle.Position)
					{
						case SizingHandle.Left:
						case SizingHandle.Right:
							pictureBox.Cursor = Cursors.SizeWE;
							break;

						case SizingHandle.Top:
						case SizingHandle.Bottom:
							pictureBox.Cursor = Cursors.SizeNS;
							break;

						case SizingHandle.TopLeft:
						case SizingHandle.BottomRight:
							pictureBox.Cursor = Cursors.SizeNWSE;
							break;


						case SizingHandle.TopRight:
						case SizingHandle.BottomLeft:
							pictureBox.Cursor = Cursors.SizeNESW;
							break;
					}
					return;
				}

				if (selectionBounds.Contains(e.Location))
				{
					pictureBox.Cursor = Cursors.SizeAll;
					return;
				}

				pictureBox.Cursor = Cursors.Cross;
			}
		}


		private void SelectRegion(Point location)
		{
			ConstrainLocation(ref location, imageBounds);

			// do we have an in-bounds start point yet?
			if (!imageBounds.Contains(startPoint))
			{
				startPoint.X = location.X;
				startPoint.Y = location.Y;
			}

			// new end point
			endPoint.X = location.X;
			endPoint.Y = location.Y;

			selectionBounds = new Rectangle(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
				);

			SetSelection(selectionBounds);
			pictureBox.Refresh();
		}


		private void ResizeRegion(Point location)
		{
			ConstrainLocation(ref location, imageBounds);

			switch (currentHandle.Position)
			{
				case SizingHandle.Top:
					endPoint.X = selectionBounds.Left;
					endPoint.Y = location.Y;
					break;

				case SizingHandle.Right:
					endPoint.X = location.X;
					endPoint.Y = selectionBounds.Bottom;
					break;

				case SizingHandle.Bottom:
					endPoint.X = selectionBounds.Right;
					endPoint.Y = location.Y;
					break;

				case SizingHandle.Left:
					endPoint.X = location.X;
					endPoint.Y = selectionBounds.Top;
					break;

				case SizingHandle.TopLeft:
				case SizingHandle.TopRight:
				case SizingHandle.BottomLeft:
				case SizingHandle.BottomRight:
					endPoint.X = location.X;
					endPoint.Y = location.Y;
					break;
			}

			selectionBounds = new Rectangle(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
				);

			SetSelection(selectionBounds);
			pictureBox.Refresh();
		}


		private void MoveRegion(Point location)
		{
			var s = new Point(startPoint.X, startPoint.Y);
			s.Offset(location.X - movePoint.X, location.Y - movePoint.Y);

			if (!imageBounds.Contains(s))
			{
				movePoint = location;
				ConstrainLocation(ref movePoint, selectionBounds);
				return;
			}

			var e = new Point(endPoint.X, endPoint.Y);
			e.Offset(location.X - movePoint.X, location.Y - movePoint.Y);

			if (!imageBounds.Contains(e))
			{
				movePoint = location;
				ConstrainLocation(ref movePoint, selectionBounds);
				return;
			}

			startPoint.X = s.X;
			startPoint.Y = s.Y;

			endPoint.X = e.X;
			endPoint.Y = e.Y;

			selectionBounds = new Rectangle(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
				);

			SetSelection(selectionBounds);
			pictureBox.Refresh();

			movePoint = location;
		}


		private void ConstrainLocation(ref Point location, Rectangle bounds)
		{
			if (!imageBounds.Contains(location))
			{
				// force it into bounds
				if (location.X < bounds.Left)
				{
					location.X = bounds.Left;
				}
				else if (location.X > bounds.Right)
				{
					location.X = bounds.Right;
				}

				if (location.Y < bounds.Top)
				{
					location.Y = bounds.Top;
				}
				else if (location.Y > bounds.Bottom)
				{
					location.Y = bounds.Bottom;
				}
			}
		}


		// ---------------------------------------------------------------------------------------
		// Mouse up

		private void Picture_MouseUp(object sender, MouseEventArgs e)
		{
			if (Image == null)
			{
				return;
			}

			if (moveState == MoveState.Selecting)
			{
				if (imageBounds.Contains(e.Location))
				{
					endPoint.X = e.Location.X;
					endPoint.Y = e.Location.Y;
				}

				selectionBounds = new Rectangle(
					Math.Min(startPoint.X, endPoint.X),
					Math.Min(startPoint.Y, endPoint.Y),
					Math.Abs(startPoint.X - endPoint.X),
					Math.Abs(startPoint.Y - endPoint.Y)
					);

				SetSelection(selectionBounds);

				if (selectionBounds.Width == 0 && selectionBounds.Height == 0)
				{
					marchingTimer.Stop();
				}
				else if (!marchingTimer.Enabled)
				{
					marchingTimer.Start();
				}

				pictureBox.Refresh();
				SetButtonEnabled();
			}

			if (moveState == MoveState.Panning)
			{
				pictureBox.Cursor = spaceDown ? Cursors.SizeAll : Cursors.Cross;
			}

			moveState = MoveState.None;
		}


		// ---------------------------------------------------------------------------------------
		// Rotation...

		private void ChangeRotation(object sender, EventArgs e)
		{
			if (sender == rotationBar)
			{
				rotationBox.Value = rotationBar.Value;
			}
			else
			{
				rotationBar.Value = (int)rotationBox.Value;
			}

			Image = Rotate((Bitmap)original, (float)rotationBox.Value);
			UpdateLayout(false);
			pictureBox.Refresh();

			sizeStatusLabel.Text = string.Format(
				Resx.CropImageDialog_imageSize, Image.Width, Image.Height);

			SetButtonEnabled();
		}


		private Bitmap Rotate(Bitmap bitmap, float angle)
		{
			var (width, height) = PredictRotatedSize(bitmap, angle);

			// draw rotated image as a new bitmap
			var rotated = new Bitmap(width, height);
			rotated.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

			using (var g = Graphics.FromImage(rotated))
			{
				// smooth image interpolation
				g.InterpolationMode = InterpolationMode.High;

				// transparent background; could instead use g.Clear(bitmap.GetPixel(0, 0))
				g.Clear(Color.Transparent);

				// rotate image around its center
				using var matrix = new Matrix();
				matrix.RotateAt(angle, new PointF(width / 2f, height / 2f));
				g.Transform = matrix;

				// draw the image centered on the bitmap
				g.DrawImage(bitmap,
					(width - (bitmap.Width)) / 2,
					(height - (bitmap.Height)) / 2);
			}

			return rotated;
		}


		private (int width, int height) PredictRotatedSize(Bitmap bitmap, float angle)
		{
			// return the larger ratio, horizontal or vertical of the image
			var points = new PointF[]
			{
				new(0, 0),
				new(bitmap.Width, 0),
				new(bitmap.Width, bitmap.Height),
				new(0, bitmap.Height),
			};

			using (var matrix = new Matrix())
			{
				// rotate around the origin
				matrix.Rotate(angle);
				matrix.TransformPoints(points);
			}

			// scan for min/max...
			var xmin = points[0].X;
			var xmax = xmin;
			var ymin = points[0].Y;
			var ymax = ymin;
			foreach (var point in points)
			{
				if (xmin > point.X) xmin = point.X;
				if (xmax < point.X) xmax = point.X;
				if (ymin > point.Y) ymin = point.Y;
				if (ymax < point.Y) ymax = point.Y;
			}

			return ((int)Math.Round((xmax - xmin)), (int)Math.Round((ymax - ymin)));
		}


		// ---------------------------------------------------------------------------------------
		// Buttons

		private void SetButtonEnabled()
		{
			cropButton.Enabled =
				(rotationBar.Value > 0 && rotationBar.Value < 360) ||
				(selectionBounds.Width > 0 && selectionBounds.Height > 0);
		}


		private void SelectButton_Click(object sender, EventArgs e)
		{
			Picture_MouseDown(pictureBox,
				new MouseEventArgs(MouseButtons.Left, 1, ImageMargin, ImageMargin, 0));

			var point = new Point(
				ImageMargin + imageBounds.Width,
				ImageMargin + imageBounds.Height
				);

			SelectRegion(point);

			Picture_MouseUp(pictureBox,
				new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
		}


		private void CropButton_Click(object sender, EventArgs e)
		{
			if (selectionBounds.Width == 0 && selectionBounds.Height == 0)
			{
				return;
			}

			// translate absolute selection bounds relative to zoomed image bounds
			var ratio = GetRatio();

			var bounds = new Rectangle(
				(int)Math.Round((selectionBounds.X - ImageMargin) * ratio),
				(int)Math.Round((selectionBounds.Y - ImageMargin) * ratio),
				(int)Math.Round(selectionBounds.Width * ratio),
				(int)Math.Round(selectionBounds.Height * ratio));
#if Logging
			logger.WriteLine(
				$"CROP selectionBounds xy:{selectionBounds.X}x{selectionBounds.Y} " +
				$"siz:{selectionBounds.Width}x{selectionBounds.Height} | " +
				$"bounds xy:{bounds.X}x{bounds.Y} siz:{bounds.Width}x{bounds.Height}");
#endif
			// crop with translated bounds
			var crop = new Bitmap(bounds.Width, bounds.Height);
			crop.SetResolution(Image.HorizontalResolution, Image.VerticalResolution);

			using (var g = Graphics.FromImage(crop))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.DrawImage(Image, 0, 0, bounds, GraphicsUnit.Pixel);

				Image = crop;
			}

			moveState = MoveState.None;

			SetSelection(Rectangle.Empty);
			startPoint.X = startPoint.Y = -1;
			selectionBounds = Rectangle.Empty;
			handles.Clear();
			marchingTimer.Stop();

			UpdateLayout(false);

			pictureBox.Refresh();
		}


		// capture Ctrl+D regardless of which control has focus
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.D))
			{
				Deselect();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		private void Deselect()
		{
			if (selectionBounds.Width == 0 && selectionBounds.Height == 0 &&
				selectionRegion.IsEmpty(pictureBox.CreateGraphics()))
			{
				return;
			}

			moveState = MoveState.None;
			startPoint = endPoint = movePoint = new Point(-1, -1);
			selectionBounds = Rectangle.Empty;
			SetSelection(Rectangle.Empty);
			handles.Clear();
			marchingTimer.Stop();
			SetButtonEnabled();
			pictureBox.Refresh();
		}


		private void CropImageDialog_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && spaceDown)
			{
				spaceDown = false;
				if (moveState != MoveState.Panning)
				{
					pictureBox.Cursor = Cursors.Cross;
				}

				e.Handled = true;
			}
		}


		private void CropImageDialog_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && IsOverView())
			{
				// hold Space to pan; keep it from clicking the focused button
				spaceDown = true;
				if (CanPan() && moveState != MoveState.Panning)
				{
					pictureBox.Cursor = Cursors.SizeAll;
				}

				e.Handled = e.SuppressKeyPress = true;
			}
			else if (e.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
			{
				SetZoom(StepZoom(true));
				e.Handled = true;
			}
			else if (e.Control && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract))
			{
				SetZoom(StepZoom(false));
				e.Handled = true;
			}
			else if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
			{
				SetZoom(1.0);
				e.Handled = true;
			}
			else if (e.Control && e.KeyCode == Keys.A)
			{
				SelectButton_Click(sender, e);
				e.Handled = true;
			}
		}
	}
}
