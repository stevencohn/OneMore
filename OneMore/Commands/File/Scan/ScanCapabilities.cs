//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// Provides a collection of constant property names used to identify various device or
	/// document-related attributes.
	/// </summary>
	/// <remarks>
	/// This class contains string constants representing property names commonly used in device
	/// or document handling contexts. These constants can be used as keys or identifiers in
	/// scenarios where such properties need to be referenced.
	/// </remarks>
	internal static class PropertyNames
	{
		// https://learn.microsoft.com/en-us/previous-versions/windows/desktop/wia/-wia-wiaitempropscannerdevice

		public const string BedHeight = "Vertical Bed Size";
		public const string BedWidth = "Horizontal Bed Size";
		public const string CurrentIntent = "Current Intent";
		public const string DocumentHandling = "Document Handling Capabilities";
		public const string HandlingSelect = "Document Handling Select";
		public const string HorizontalResolution = "Horizontal Resolution";
		public const string ModelName = "Model name";
		public const string ModelNumber = "Model number";
		public const string VerticalResolution = "Vertical Resolution";
	}


	/// <summary>
	/// Mapping of WIA.FormatID constants because that class cannot be referenced directly.
	/// </summary>
	internal static class ScanFormatID
	{
		public const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
		public const string wiaFormatPNG = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}";
		public const string wiaFormatGIF = "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}";
		public const string wiaFormatJPEG = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";
		public const string wiaFormatTIFF = "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}";
	}


	/// <summary>
	/// Provides constants representing different scan handling options.
	/// </summary>
	/// <remarks>
	/// These constants can be used to specify the source of a scan, such as a feeder or a flatbed.
	/// </remarks>
	internal static class ScanHandling
	{
		public static int Feeder => 0x01;
		public static int Flatbed => 0x02;
	}


	/// <summary>
	/// Represents predefined scan intents that specify the desired color mode for a scanning
	/// operation.
	/// </summary>
	/// <remarks>
	/// These scan intents are represented as integer constants and can be used to configure 
	/// scanning operations. Each intent corresponds to a specific color mode:
	///  - Color; Scans in full color mode.
	///  - Grayscale: Scans in grayscale mode.
	///  - BlackAndWhite: Scans in black-and-white mode.
	/// </remarks>
	internal static class ScanIntents
	{
		public static int Color => 0x01;

		public static int Grayscale => 0x02;

		public static int BlackAndWhite => 0x04;
	}


	/// <summary>
	/// Represents the scanning capabilities of a specific scanner model, including supported
	/// resolutions, scanning modes, and hardware features.
	/// </summary>
	/// <remarks>
	/// This class provides information about a scanner's capabilities, such as whether it has a
	/// document feeder, the resolutions supported for flatbed and feeder scanning, and the
	/// available scanning intents. It is intended to be used for querying and configuring scanner
	/// settings.
	/// </remarks>
	internal sealed class ScanCapabilities
	{
		public string Model { get; set; }

		public int BedHeight { get; set; }

		public int BedWidth { get; set; }

		public IEnumerable<int> FlatbedResoltuions { get; set; }

		public IEnumerable<int> FeederResoltuions { get; set; }

		public IEnumerable<int> Intents { get; set; }
	}
}
