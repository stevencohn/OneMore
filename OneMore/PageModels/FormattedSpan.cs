//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	/// <summary>
	/// A logical text span with a fully-resolved style (cascade applied, fragments reassembled).
	/// Produced by OENode.FormattedSpans; read-only view of the merged character formatting.
	/// </summary>
	internal sealed class FormattedSpan
	{
		public FormattedSpan(string text, StyleString style)
		{
			Text = text;
			Style = style;
		}


		/// <summary>Plain text content of this span.</summary>
		public string Text { get; }


		/// <summary>
		/// Merged style: QuickStyleDef → OE style → T style cascade already applied.
		/// </summary>
		public StyleString Style { get; }
	}
}
