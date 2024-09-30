//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig.Helpers;
	using Markdig.Parsers;
	using Markdig.Syntax;


	/// <summary>
	/// From https://github.com/Temetra/MarkdigExtensions/tree/main under MIT license
	/// </summary>
	internal class WikilinkParser : InlineParser
	{
		public WikilinkParser()
		{
			OpeningCharacters = new[] { '[', ']', '!' };
		}

		public override bool Match(InlineProcessor processor, ref StringSlice slice)
		{
			// Check opening wikilink character combination
			var result = CheckOpening(processor, ref slice);
			if (result != null) return result.Value;

			// Check closing wikilink character combination
			result = CheckClosing(processor, ref slice);
			if (result != null) return result.Value;

			return false;
		}


		private bool? CheckOpening(InlineProcessor processor, ref StringSlice slice)
		{
			bool isImage = false;
			_ = processor.GetSourcePosition(slice.Start, out int line, out int column);

			if (slice.CurrentChar == '!')
			{
				if (slice.NextChar() != '[')
				{
					return false;
				}

				isImage = true;
			}

			if (slice.CurrentChar == '[')
			{
				if (slice.NextChar() != '[')
				{
					return false;
				}

				// Create wikilink inline
				processor.Inline = new WikilinkInline()
				{
					IsImage = isImage,
					Span = new SourceSpan(slice.Start - 1 - (isImage ? 1 : 0), slice.Start),
					Line = line,
					Column = column
				};

				// Continue
				slice.SkipChar();
				return true;
			}

			return null;
		}

		public static bool? CheckClosing(InlineProcessor processor, ref StringSlice slice)
		{
			if (slice.CurrentChar == ']')
			{
				if (slice.NextChar() != ']')
				{
					return false;
				}

				var openParent = processor.Inline!.FirstParentOfType<WikilinkInline>();

				// Set end of wikilink span
				if (openParent is not null)
				{
					// Set end of wikilink
					openParent.Span.End = slice.Start;
					openParent.IsClosed = true;

					// Get link and label spans
					var offset = 2 + (openParent.IsImage ? 1 : 0);
					var idx = slice.Text.IndexOf('|', openParent.Span.Start, openParent.Span.Length);

					if (idx == -1)
					{
						// No pipe
						openParent.Link = new StringSlice(slice.Text, openParent.Span.Start + offset, openParent.Span.End - 2);
						openParent.Label = openParent.Link;
					}
					else
					{
						// Yes pipe
						openParent.Link = new StringSlice(slice.Text, openParent.Span.Start + offset, idx - 1);
						openParent.Label = new StringSlice(slice.Text, idx + 1, openParent.Span.End - 2);
					}

					// Remove children from wikilink
					while (openParent.FirstChild != null)
						openParent.FirstChild?.Remove();

					// Set inline
					processor.Inline = openParent;
				}

				// Complete process
				slice.SkipChar();
				return true;
			}

			return null;
		}
	}
}
