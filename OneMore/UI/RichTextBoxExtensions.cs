//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;


	internal static class RichTextBoxExtensions
	{
		public static void AppendFormattedText(
			this RichTextBox box, string text, Color textColour)
		{
			var start = box.TextLength;
			box.AppendText(text);
			box.Select(start, box.TextLength - start);
			box.SelectionColor = textColour;

			//box.SelectionAlignment = alignment;
			//box.SelectionFont = new Font(
			//	 box.SelectionFont.FontFamily,
			//	 box.SelectionFont.Size,
			//	 (isBold ? FontStyle.Bold : FontStyle.Regular));

			box.SelectionLength = 0;
		}
	}
}
