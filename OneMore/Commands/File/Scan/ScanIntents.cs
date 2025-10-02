//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;

	internal static class ScanHandling
	{
		public static int Feeder => 0x01;
		public static int Flatbed => 0x02;
	}


	internal static class ScanIntents
	{
		public static int Color => 0x01;

		public static int Grayscale => 0x02;

		public static int BlackAndWhite => 0x04;
	}


	internal class ScanCapabilities
	{
		public string Model { get; set; }
		public bool HasFeeder { get; set; }
		public IEnumerable<int> FlatbedResoltuions { get; set; }
		public IEnumerable<int> FeederResoltuions { get; set; }
		public IEnumerable<int> Intents { get; set; }
	}
}
