//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// Represents a reusable scan configuration profile for WIA-based scanning.
	/// </summary>
	public class ScanProfile
	{
		/// <summary>
		/// Display name of the profile (e.g., "Photo 3x5", "Text Letter").
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Horizontal and vertical resolution in DPI.
		/// </summary>
		public int Dpi { get; set; } = 300;

		/// <summary>
		/// Physical width of scan area in inches.
		/// </summary>
		public double WidthInInches { get; set; } = 8.5;

		/// <summary>
		/// Physical height of scan area in inches.
		/// </summary>
		public double HeightInInches { get; set; } = 11.0;

		/// <summary>
		/// Brightness adjustment (-100 to +100).
		/// </summary>
		public int Brightness { get; set; } = 0;

		/// <summary>
		/// Contrast adjustment (-100 to +100).
		/// </summary>
		public int Contrast { get; set; } = 0;

		/// <summary>
		/// Scan intent: Color, Grayscale, Black and White.
		/// </summary>
		public int Intent { get; set; } = ScanIntents.Color;

		/// <summary>
		/// True to use feeder, false for flatbed.
		/// </summary>
		public bool UseFeeder { get; set; } = false;

		/// <summary>
		/// Output format ID (e.g., FormatID.wiaFormatJPEG).
		/// </summary>
		public string FormatId { get; set; } = ScanFormatID.wiaFormatJPEG;

		/// <summary>
		/// Optional output path for saving the scanned image.
		/// </summary>
		public string OutputPath { get; set; }

		/// <summary>
		/// Optional notes or description for audit or UI display.
		/// </summary>
		public string Notes { get; set; }
	}
}
