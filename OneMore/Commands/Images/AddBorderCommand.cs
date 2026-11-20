//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Adds a colored border and optional drop shadow to each selected image. The image
	/// canvas is enlarged so the original pixels are copied 1:1 and never scaled down.
	/// </summary>
	internal class AddBorderCommand : Command
	{
		// shadow tuning, in on-page pixels; the shadow falls to the right and down
		private const float ShadowOpacity = 0.30f;
		private const int ShadowBlur = 8;
		private const int ShadowDistance = 5;

		// fraction of blur the soft edge may extend past the shadow rectangle before the
		// canvas clips it; 0.72 cuts at ~4/255 alpha, which is imperceptible
		private const float ShadowReach = 0.72f;


		public AddBorderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			var elements = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (!elements.Any())
			{
				ShowInfo(Resx.AddBorderCommand_noImages);
				return;
			}

			using var dialog = new AddBorderDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			var updated = false;
			foreach (var element in elements)
			{
				if (AddBorder(element, ns, dialog.BorderColor, dialog.BorderWidth, dialog.DropShadow))
				{
					updated = true;
				}
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		private bool AddBorder(
			XElement element, XNamespace ns, Color color, float width, bool shadow)
		{
			var data = element.Element(ns + "Data");
			if (data is null || string.IsNullOrEmpty(data.Value))
			{
				return false;
			}

			var oneImage = new OneImage(element);
			using var source = oneImage.ReadImage();

			// displayed size vs pixel size; the border width is specified in displayed
			// pixels so it looks the same regardless of the image's native resolution
			var size = element.Element(ns + "Size");
			var (scaleX, scaleY) = GetScales(size, source.Width, source.Height);

			var borderPixels = width <= 0
				? 0
				: Math.Max(1, (int)Math.Round(width / Math.Min(scaleX, scaleY)));

			var scale = Math.Min(scaleX, scaleY);
			var blur = Math.Max(1, (int)Math.Round(ShadowBlur / scale));
			var distance = Math.Max(1, (int)Math.Round(ShadowDistance / scale));

			using var result = Render(source, color, borderPixels, shadow, blur, distance);

			logger?.WriteLine(
				$"AddBorder pixels={source.Width}x{source.Height} " +
				$"display={size.Attribute("width")?.Value}x{size.Attribute("height")?.Value} " +
				$"scale=({scaleX:0.####},{scaleY:0.####}) width={width:0.##} " +
				$"borderPx={borderPixels} shadow={shadow} blurPx={blur} distancePx={distance} " +
				$"result={result.Width}x{result.Height}");

			oneImage.WriteImage(result);

			size.SetAttributeValue("width", FormattableString.Invariant(
				$"{Math.Round(result.Width * scaleX):0.0}"));

			size.SetAttributeValue("height", FormattableString.Invariant(
				$"{Math.Round(result.Height * scaleY):0.0}"));

			size.SetAttributeValue("isSetByUser", "true");

			// printouts carry XPS attributes that OneNote uses to block edits
			element.Attributes("xpsFileIndex").Remove();
			element.Attributes("originalPageNumber").Remove();
			element.Attributes("isPrintOut").Remove();

			return true;
		}


		/// <summary>
		/// Gets the ratio of displayed size to pixel size for the given one:Size element.
		/// </summary>
		internal static (float X, float Y) GetScales(XElement size, int pixelWidth, int pixelHeight)
		{
			size.GetAttributeValue("width", out float displayWidth, pixelWidth);
			size.GetAttributeValue("height", out float displayHeight, pixelHeight);

			var scaleX = displayWidth > 0 ? displayWidth / pixelWidth : 1f;
			var scaleY = displayHeight > 0 ? displayHeight / pixelHeight : scaleX;
			if (scaleX <= 0) { scaleX = 1f; }
			if (scaleY <= 0) { scaleY = scaleX; }

			return (scaleX, scaleY);
		}


		/// <summary>
		/// Renders the source onto a larger transparent canvas with a border band outside
		/// the original pixels and an optional soft drop shadow beyond that.
		/// </summary>
		internal static Bitmap Render(
			Image source, Color color, int border, bool shadow, int blur, int distance)
		{
			// margins leave room for the visible part of the shadow's soft edge; its faint
			// tail beyond ShadowReach is clipped by the canvas edge
			var reach = (int)Math.Ceiling(ShadowReach * blur);
			var left = border + (shadow ? Math.Max(0, reach - distance) : 0);
			var top = left;
			var right = border + (shadow ? distance + reach : 0);
			var bottom = right;

			var width = source.Width + left + right;
			var height = source.Height + top + bottom;

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			bitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);

			// outer rectangle of the border band
			var outer = new Rectangle(
				left - border, top - border,
				source.Width + (2 * border), source.Height + (2 * border));

			if (shadow)
			{
				WriteShadow(bitmap, outer, blur, distance);
			}

			using var g = Graphics.FromImage(bitmap);

			if (border > 0)
			{
				using (var brush = new SolidBrush(color))
				{
					g.CompositingMode = CompositingMode.SourceOver;
					g.PixelOffsetMode = PixelOffsetMode.Half;
					g.FillRectangle(brush, outer);
				}
			}

			// copy original pixels 1:1, no resampling
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(source,
				new Rectangle(left, top, source.Width, source.Height),
				0, 0, source.Width, source.Height, GraphicsUnit.Pixel);

			return bitmap;
		}


		/// <summary>
		/// Writes a soft black shadow of the outer rectangle, offset right and down by
		/// distance, directly into the bitmap's alpha channel. A rectangle's blur is
		/// separable, so alpha = opacity * fx(x) * fy(y) where each profile is a smoothstep
		/// ramp of total width 2*blur centered on the shadow rectangle's edge.
		/// </summary>
		private static void WriteShadow(Bitmap bitmap, Rectangle outer, int blur, int distance)
		{
			var fx = Profile(bitmap.Width, outer.X + distance, outer.Right + distance, blur);
			var fy = Profile(bitmap.Height, outer.Y + distance, outer.Bottom + distance, blur);

			var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			try
			{
				var row = new byte[bitmap.Width * 4];
				for (var y = 0; y < bitmap.Height; y++)
				{
					// black pixels: only the alpha byte (index 3 of each BGRA) is non-zero
					for (var x = 0; x < bitmap.Width; x++)
					{
						row[(x * 4) + 3] = (byte)Math.Round(ShadowOpacity * 255f * fx[x] * fy[y]);
					}

					Marshal.Copy(row, 0, data.Scan0 + (y * data.Stride), row.Length);
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
		}


		// 1D opacity factor (0..1) per pixel for a span [start, end) softened by blur
		private static float[] Profile(int length, int start, int end, int blur)
		{
			var profile = new float[length];
			var span = 2f * blur;
			for (var i = 0; i < length; i++)
			{
				var p = i + 0.5f;
				var rise = SmoothStep((p - (start - blur)) / span);
				var fall = SmoothStep(((end + blur) - p) / span);
				profile[i] = rise * fall;
			}

			return profile;
		}


		private static float SmoothStep(float t)
		{
			if (t <= 0f) { return 0f; }
			if (t >= 1f) { return 1f; }
			return t * t * (3f - (2f * t));
		}
	}
}
