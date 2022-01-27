//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
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
	internal partial class CropImageDialog : UI.LocalizableForm
	{

		#region Supporting classes

		// tracks the current function, state of the cursor
		private enum MoveState
		{
			None,
			Selecting,
			Moving,
			Sizing
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

		// the selection
		private Point startPoint;
		private Point endPoint;
		private Point movePoint;
		private Rectangle selectionBounds;
		private int antOffset;
		private readonly Image original;
		private readonly Region selectionRegion;
		private readonly GraphicsPath selectionPath;
		private readonly MagicScaling scaling;

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

			VerticalOffset = 3;

			selectionRegion = new Region();
			selectionRegion.MakeEmpty();
			selectionPath = new GraphicsPath();
			selectionPath.Reset();

			handles = new List<SelectionHandle>();
			moveState = MoveState.None;

			if (NeedsLocalizing())
			{
				Text = Resx.CropImageDialog_Text;

				Localize(new string[]
				{
					"introText",
					"selectButton",
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

			scaling = new MagicScaling(image.HorizontalResolution, image.VerticalResolution);

			SizeWindow();

			brightness = image.GetBrightness();

			sizeStatusLabel.Text = string.Format(
				Resx.CropImageDialog_imageSize, Image.Width, Image.Height);

			pictureBox.Refresh();

#if Logging
			var hasRealDpi = (image.Flags & (int)ImageFlags.HasRealDpi) > 0;
			var hasRealPixelSize = (image.Flags & (int)ImageFlags.HasRealPixelSize) > 0;

			Logger.Current.WriteLine(
				$"IMAG hasRealDpi:{hasRealDpi} hasRealPixelSize:{hasRealPixelSize} | " +
				$"hRes:{image.HorizontalResolution} vRes:{image.VerticalResolution} | " +
				$"size:{image.Width}x{image.Height} " +
				$"physical:{image.PhysicalDimension.Width}x{image.PhysicalDimension.Height}\n" +
				$"IMAG bounds:{imageBounds.Width}x{imageBounds.Height}");
#endif
		}


		private void SizeWindow()
		{
			// height
			var border =
				SystemInformation.CaptionHeight +               // title bar
				SystemInformation.FrameBorderSize.Height * 2 +  // horizontal borders, top/bottom
				introPanel.Height +                             // intro text panel
				statusStrip.Height +                            // status bar
				buttonPanel.Height;

			var desired = Math.Max(
				MinimumSize.Height,                             // defined min size of dialog
				Image.Height + border + ImageMargin * 2);       // image + borders + margins

			Height = Math.Min(desired, Screen.FromControl(this).WorkingArea.Height);

			// width
			border =
				SystemInformation.FrameBorderSize.Width * 2;    // vertical borders, left/right

			desired = Math.Max(
				MinimumSize.Width,                              // defined min size of dialog
				Image.Width + border + ImageMargin * 2);        // image + borders + margins

			Width = Math.Min(desired, Screen.FromControl(this).WorkingArea.Width);
		}


		private void CropImageDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
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
			var ratio = scaling.GetRatio(Image, pictureBox.Width, pictureBox.Height, ImageMargin);

			imageBounds = new Rectangle(
				ImageMargin, ImageMargin,
				(int)Math.Round(Image.Width / ratio), (int)Math.Round(Image.Height / ratio));

			// draw outline for images with transparency (png)
			g.DrawRectangle(Pens.Gray, imageBounds);
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
			using (var pen = new Pen(Color.White, 1f))
			{
				pen.DashStyle = DashStyle.Dash;
				pen.DashPattern = new float[2] { 3, 3 };
				pen.DashOffset = antOffset;

				// set up pen for the ants
				using (var ant = new Bitmap(pictureBox.Width, pictureBox.Height))
				{
					using (var gi = Graphics.FromImage(ant))
					{
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
					g.DrawImageUnscaled(ant, 0, 0);
				}

				// draw resize handles
				var bounds = selectionRegion.GetBounds(g);

				AddHandle(SizingHandle.TopLeft, bounds.Left, bounds.Top, g);
				AddHandle(SizingHandle.TopRight, bounds.Right, bounds.Top, g);
				AddHandle(SizingHandle.BottomRight, bounds.Right, bounds.Bottom, g);
				AddHandle(SizingHandle.BottomLeft, bounds.Left, bounds.Bottom, g);

				AddHandle(SizingHandle.Top, bounds.Left + ((bounds.Right - bounds.Left) / 2), bounds.Top, g);
				AddHandle(SizingHandle.Right, bounds.Right, bounds.Top + ((bounds.Bottom - bounds.Top) / 2), g);
				AddHandle(SizingHandle.Bottom, bounds.Left + ((bounds.Right - bounds.Left) / 2), bounds.Bottom, g);
				AddHandle(SizingHandle.Left, bounds.Left, bounds.Top + ((bounds.Bottom - bounds.Top) / 2), g);
			}
		}


		private void UpdateStatus()
		{
			var ratio = scaling.GetRatio(Image, pictureBox.Width, pictureBox.Height, ImageMargin);

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
			if (moveState == MoveState.Selecting)
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
				using (var matrix = new Matrix())
				{
					matrix.RotateAt(angle, new PointF(width / 2f, height / 2f));
					g.Transform = matrix;

					// draw the image centered on the bitmap
					g.DrawImage(bitmap,
						(width - (bitmap.Width)) / 2,
						(height - (bitmap.Height)) / 2);
				}
			}

			return rotated;
		}


		private (int width, int height) PredictRotatedSize(Bitmap bitmap, float angle)
		{
			// return the larger ratio, horizontal or vertical of the image
			var points = new PointF[]
			{
				new PointF(0, 0),
				new PointF(bitmap.Width, 0),
				new PointF(bitmap.Width, bitmap.Height),
				new PointF(0, bitmap.Height),
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
			var ratio = scaling.GetRatio(Image, pictureBox.Width, pictureBox.Height, ImageMargin);

			var bounds = new Rectangle(
				(int)Math.Round((selectionBounds.X - ImageMargin) * ratio),
				(int)Math.Round((selectionBounds.Y - ImageMargin) * ratio),
				(int)Math.Round(selectionBounds.Width * ratio),
				(int)Math.Round(selectionBounds.Height * ratio));
#if Logging
			Logger.Current.WriteLine(
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

			pictureBox.Refresh();
		}


		private void CropImageDialog_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.D &&
				selectionBounds.Width > 0 && selectionBounds.Height > 0)
			{
				selectionBounds = Rectangle.Empty;
				SetSelection(Rectangle.Empty);
				SetButtonEnabled();
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
