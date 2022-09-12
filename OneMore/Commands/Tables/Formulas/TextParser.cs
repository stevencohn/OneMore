//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************
/*
 * Based on the C# Expression Evaluator by Jonathan Wood, 2010
 *
 * This source code and all associated files and resources are copyrighted by
 * the author(s). This source code and all associated files and resources may
 * be used as long as they are used according to the terms and conditions set
 * forth in The Code Project Open License (CPOL), which may be viewed at
 * http://www.blackbeltcoder.com/Legal/Licenses/CPOL.
 * Copyright (c) 2010 Jonathan Wood
 */

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;


	internal class TextParser
	{
		public const char NullChar = (char)0;

		public TextParser()
		{
			Reset(null);
		}

		public TextParser(string text)
		{
			Reset(text);
		}


		public string Text { get; private set; }

		public int Position { get; private set; }

		public int Remaining { get { return Text.Length - Position; } }



		/// <summary>
		/// Resets the current position to the start of the current document
		/// </summary>
		public void Reset()
		{
			Position = 0;
		}

		/// <summary>
		/// Sets the current document and resets the current position to the start of it
		/// </summary>
		/// <param name="html"></param>
		public void Reset(string text)
		{
			Text = text ?? string.Empty;
			Position = 0;
		}

		/// <summary>
		/// Indicates if the current position is at the end of the current document
		/// </summary>
		public bool EndOfText
		{
			get { return (Position >= Text.Length); }
		}

		/// <summary>
		/// Returns the character at the current position, or a null character if we're
		/// at the end of the document
		/// </summary>
		/// <returns>The character at the current position</returns>
		public char Peek()
		{
			return Peek(0);
		}

		/// <summary>
		/// Returns the character at the specified number of characters beyond the current
		/// position, or a null character if the specified position is at the end of the
		/// document
		/// </summary>
		/// <param name="ahead">The number of characters beyond the current position</param>
		/// <returns>The character at the specified position</returns>
		public char Peek(int ahead)
		{
			int pos = (Position + ahead);
			if (pos < Text.Length)
				return Text[pos];
			return NullChar;
		}


		/// <summary>
		/// Returns the character at the specified index within the text string.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public char PeekAt(int index)
		{
			if (index >= 0 && index < Text.Length)
				return Text[index];
			return NullChar;
		}

		/// <summary>
		/// Extracts a substring from the specified position to the end of the text
		/// </summary>
		/// <param name="start"></param>
		/// <returns></returns>
		public string Extract(int start)
		{
			return Extract(start, Text.Length);
		}

		/// <summary>
		/// Extracts a substring from the specified range of the current text
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public string Extract(int start, int end)
		{
			return Text.Substring(start, end - start);
		}

		/// <summary>
		/// Moves the current position ahead one character
		/// </summary>
		public void MoveAhead()
		{
			MoveAhead(1);
		}

		/// <summary>
		/// Moves the current position ahead the specified number of characters
		/// </summary>
		/// <param name="ahead">The number of characters to move ahead</param>
		public void MoveAhead(int ahead)
		{
			Position = Math.Min(Position + ahead, Text.Length);
		}

		/// <summary>
		/// Moves to the next occurrence of the specified string
		/// </summary>
		/// <param name="s">String to find</param>
		/// <param name="ignoreCase">Indicates if case-insensitive comparisons are used</param>
		public void MoveTo(string s, bool ignoreCase = false)
		{
			Position = Text.IndexOf(s, Position, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
			if (Position < 0)
				Position = Text.Length;
		}

		/// <summary>
		/// Moves to the next occurrence of the specified character
		/// </summary>
		/// <param name="c">Character to find</param>
		public void MoveTo(char c)
		{
			Position = Text.IndexOf(c, Position);
			if (Position < 0)
				Position = Text.Length;
		}

		/// <summary>
		/// Moves to the next occurrence of any one of the specified
		/// characters
		/// </summary>
		/// <param name="chars">Array of characters to find</param>
		public void MoveTo(char[] chars)
		{
			Position = Text.IndexOfAny(chars, Position);
			if (Position < 0)
				Position = Text.Length;
		}

		/// <summary>
		/// Moves to the next occurrence of any character that is not one
		/// of the specified characters
		/// </summary>
		/// <param name="chars">Array of characters to move past</param>
		public void MovePast(char[] chars)
		{
			while (IsInArray(Peek(), chars))
				MoveAhead();
		}

		/// <summary>
		/// Determines if the specified character exists in the specified
		/// character array.
		/// </summary>
		/// <param name="c">Character to find</param>
		/// <param name="chars">Character array to search</param>
		/// <returns></returns>
		private static bool IsInArray(char c, char[] chars)
		{
			foreach (char ch in chars)
			{
				if (c == ch)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Moves the current position to the first character that is part of a newline
		/// </summary>
		public void MoveToEndOfLine()
		{
			char c = Peek();
			while (c != '\r' && c != '\n' && !EndOfText)
			{
				MoveAhead();
				c = Peek();
			}
		}

		/// <summary>
		/// Moves the current position to the next character that is not whitespace
		/// </summary>
		public void MovePastWhitespace()
		{
			while (char.IsWhiteSpace(Peek()))
				MoveAhead();
		}
	}
}
