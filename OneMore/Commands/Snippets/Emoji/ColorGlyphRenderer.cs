//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	using Vortice.Direct2D1;
	using Vortice.Direct3D11;
	using Vortice.DirectWrite;
	using Vortice.DXGI;
	using Vortice.Mathematics;
	using D3D11 = Vortice.Direct3D11.D3D11;
	using FeatureLevel = Vortice.Direct3D.FeatureLevel;


	/// <summary>
	/// Renders Segoe UI Emoji glyphs into GDI+ bitmaps using Direct2D/DirectWrite, since
	/// GDI+ itself cannot render the color layers of a color font. This lets the Emoji
	/// picker show any Unicode glyph in color without a pre-rendered PNG per character.
	/// </summary>
	/// <remarks>
	/// UI-thread only; the underlying D3D11/Direct2D resources are not thread-safe. Held
	/// for the lifetime of the process; never explicitly disposed, same as other long-lived
	/// native resources in this add-in (e.g. the OneNote Application COM object).
	/// </remarks>

	internal sealed class ColorGlyphRenderer : IDisposable
	{
		private static ColorGlyphRenderer instance;

		private readonly ID3D11Device device;
		private readonly ID2D1DeviceContext context;
		private readonly IDWriteFactory dwriteFactory;
		private ID3D11Texture2D renderTexture;
		private ID3D11Texture2D stagingTexture;
		private ID2D1Bitmap1 targetBitmap;
		private int targetSize;


		private ColorGlyphRenderer()
		{
			D3D11.D3D11CreateDevice(
				null, Vortice.Direct3D.DriverType.Warp, DeviceCreationFlags.BgraSupport,
				new[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 },
				out device).CheckError();

			using var dxgiDevice = device.QueryInterface<IDXGIDevice>();
			using var d2dDevice = D2D1.D2D1CreateDevice(dxgiDevice, null);
			context = d2dDevice.CreateDeviceContext(DeviceContextOptions.None);

			dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>(
				Vortice.DirectWrite.FactoryType.Shared);
		}


		/// <summary>
		/// Gets the singleton instance, created on first use
		/// </summary>

		public static ColorGlyphRenderer Instance => instance ??= new ColorGlyphRenderer();


		/// <summary>
		/// Renders the given glyph into a new square bitmap, filling glyphs that have no
		/// native color layer with the given fallback color.
		/// </summary>
		/// <param name="glyph">The Unicode glyph or glyph sequence to render</param>
		/// <param name="sizePx">The width and height in pixels of the resulting bitmap</param>
		/// <param name="background">The color to pre-fill the bitmap with</param>
		/// <param name="fallbackColor">The color to use for glyphs with no color layer</param>
		/// <returns>A new Bitmap owned by the caller</returns>

		public Bitmap RenderGlyph(string glyph, int sizePx, 
			System.Drawing.Color background, System.Drawing.Color fallbackColor)
		{
			EnsureRenderTarget(sizePx);

			using var textFormat = dwriteFactory.CreateTextFormat(
				"Segoe UI Emoji", null,
				FontWeight.Normal, Vortice.DirectWrite.FontStyle.Normal, FontStretch.Normal,
				sizePx * 0.7f, "en-us");

			textFormat.TextAlignment = TextAlignment.Center;
			textFormat.ParagraphAlignment = ParagraphAlignment.Center;

			using var textLayout = dwriteFactory.CreateTextLayout(glyph, textFormat, sizePx, sizePx);
			using var brush = context.CreateSolidColorBrush(ToColor4(fallbackColor));

			context.BeginDraw();
			context.Clear(ToColor4(background));

			context.DrawTextLayout(
				new PointF(0, 0), textLayout, brush, DrawTextOptions.EnableColorFont);

			context.EndDraw();

			device.ImmediateContext.CopyResource(stagingTexture, renderTexture);

			var mapped = device.ImmediateContext.Map(
				stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

			try
			{
				return CopyToBitmap(mapped, sizePx);
			}
			finally
			{
				device.ImmediateContext.Unmap(stagingTexture, 0);
			}
		}


		private static IReadOnlyList<int> supportedCodepoints;


		/// <summary>
		/// Gets every codepoint in the given (inclusive) range for which the installed
		/// "Segoe UI Emoji" font actually defines a glyph, ordinally ordered. Computed
		/// once per process and cached, by asking the font for its exact supported
		/// Unicode ranges directly (one cmap read) rather than probing every candidate
		/// codepoint individually.
		/// </summary>
		/// <param name="first">The first Unicode codepoint in the range, inclusive</param>
		/// <param name="last">The last Unicode codepoint in the range, inclusive</param>
		/// <returns>An ordinally ordered list of supported codepoints</returns>

		public IReadOnlyList<int> GetSupportedCodepoints(int first, int last)
		{
			return supportedCodepoints ??= ScanSupportedCodepoints(first, last);
		}


		private List<int> ScanSupportedCodepoints(int first, int last)
		{
			var codepoints = new List<int>();

			using var collection = dwriteFactory.GetSystemFontCollection(false);
			if (!collection.FindFamilyName("Segoe UI Emoji", out var familyIndex))
			{
				return codepoints;
			}

			using var family = collection.GetFontFamily(familyIndex);
			using var font = family.GetFirstMatchingFont(
				FontWeight.Normal, FontStretch.Normal, Vortice.DirectWrite.FontStyle.Normal);

			using var fontFace = font.CreateFontFace();
			using var fontFace1 = fontFace.QueryInterface<IDWriteFontFace1>();

			// generous fixed size: color emoji fonts have at most a few hundred disjoint
			// cmap ranges, nowhere near this buffer's capacity
			var ranges = new UnicodeRange[8192];
			fontFace1.GetUnicodeRanges(ranges.Length, ranges, out var actualCount);

			for (var i = 0; i < actualCount; i++)
			{
				var rangeFirst = Math.Max(ranges[i].First, first);
				var rangeLast = Math.Min(ranges[i].Last, last);

				for (var codepoint = rangeFirst; codepoint <= rangeLast; codepoint++)
				{
					codepoints.Add(codepoint);
				}
			}

			return codepoints;
		}


		// (Re)creates the size-dependent render target/textures only when the requested
		// icon size changes; in practice EmojiDialog always requests the same fixed size.
		private void EnsureRenderTarget(int sizePx)
		{
			if (targetSize == sizePx)
			{
				return;
			}

			targetBitmap?.Dispose();
			renderTexture?.Dispose();
			stagingTexture?.Dispose();

			var description = new Texture2DDescription(
				Format.B8G8R8A8_UNorm, sizePx, sizePx, 1, 1,
				BindFlags.RenderTarget, ResourceUsage.Default, CpuAccessFlags.None,
				1, 0, ResourceOptionFlags.None);

			renderTexture = device.CreateTexture2D(description, null);

			description.BindFlags = BindFlags.None;
			description.Usage = ResourceUsage.Staging;
			description.CpuAccessFlags = CpuAccessFlags.Read;
			stagingTexture = device.CreateTexture2D(description, null);

			using var surface = renderTexture.QueryInterface<IDXGISurface>();
			targetBitmap = context.CreateBitmapFromDxgiSurface(surface, null);
			context.Target = targetBitmap;

			targetSize = sizePx;
		}


		private static Bitmap CopyToBitmap(MappedSubresource mapped, int sizePx)
		{
			var bitmap = new Bitmap(sizePx, sizePx, PixelFormat.Format32bppArgb);
			var data = bitmap.LockBits(
				new Rectangle(0, 0, sizePx, sizePx),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			try
			{
				var row = new byte[sizePx * 4];
				for (var y = 0; y < sizePx; y++)
				{
					Marshal.Copy(IntPtr.Add(mapped.DataPointer, y * mapped.RowPitch), row, 0, row.Length);
					Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}

			return bitmap;
		}


		private static Color4 ToColor4(System.Drawing.Color color)
		{
			return new Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}


		public void Dispose()
		{
			targetBitmap?.Dispose();
			renderTexture?.Dispose();
			stagingTexture?.Dispose();
			dwriteFactory?.Dispose();
			context?.Dispose();
			device?.Dispose();
		}
	}
}
