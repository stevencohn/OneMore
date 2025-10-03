//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using Resx = Properties.Resources;


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
		public int WidthInInches { get; set; } = 8500;

		/// <summary>
		/// Physical height of scan area in inches.
		/// </summary>
		public int HeightInInches { get; set; } = 11000;

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


		public override string ToString() => Name;


		public static List<ScanProfile> MakeDefaultProfiles()
		{
			return new List<ScanProfile>
			{
				new()
				{
					Name = Resx.word_Document,
					Dpi = 300,
					Brightness = 0,
					Contrast = 0,
					Intent = ScanIntents.Grayscale,
					UseFeeder = false,
					FormatId = ScanFormatID.wiaFormatTIFF
				},

				new()
				{
					Name = Resx.word_Photo,
					Dpi = 600,
					Brightness = 10,
					Contrast = 0,
					Intent = ScanIntents.Color,
					UseFeeder = false,
					FormatId = ScanFormatID.wiaFormatJPEG
				}
			};
		}
	}
}
