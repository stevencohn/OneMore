//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Drawing;


	internal sealed class CardModel
	{
		public string Title { get; set; }       // null for page-scope (anonymous) cards
		public string PageId { get; set; }
		public Color SectionColor { get; set; }
		public List<CardHit> Hits { get; } = new List<CardHit>();
		public bool IsChecked { get; set; }
		public bool IsHeader { get; set; }       // true for a non-navigable group header row

		// layout cache — computed by SearchResultsCardView.EnsureLayout
		internal int Y;
		internal int Height;
	}
}
