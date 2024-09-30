//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig.Helpers;
	using Markdig.Syntax.Inlines;


	/// <summary>
	/// From https://github.com/Temetra/MarkdigExtensions/tree/main under MIT license
	/// </summary>
	internal class WikilinkInline : ContainerInline
	{
		public bool IsImage { get; set; }

		public StringSlice Link { get; set; }

		public StringSlice Label { get; set; }
	}
}
