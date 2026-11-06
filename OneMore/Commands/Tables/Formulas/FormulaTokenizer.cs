//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System.Collections.Generic;
	using System.Text.RegularExpressions;


	internal enum FormulaTokenKind
	{
		Plain,
		Function,
		Cell,
		Variable,
		Number,
		Criteria,
		Punctuation
	}


	internal readonly struct FormulaToken
	{
		public FormulaToken(FormulaTokenKind kind, int start, int length)
		{
			Kind = kind;
			Start = start;
			Length = length;
		}

		public FormulaTokenKind Kind { get; }
		public int Start { get; }
		public int Length { get; }
	}


	/// <summary>
	/// Splits a formula into colorable runs. Pure and static so it can be unit tested.
	/// </summary>
	internal static class FormulaTokenizer
	{
		private static readonly Regex TokenRegex = new Regex(
			@"(?<word>[A-Za-z_][A-Za-z_0-9]*)|(?<num>\d+(\.\d+)?)|(?<punct>[()+\-*/^%:,<>!])|(?<ws>\s+)|(?<other>.)",
			RegexOptions.Compiled);

		private static readonly Regex CellRegex = new Regex(
			@"^[A-Za-z]{1,3}\d{1,3}$", RegexOptions.Compiled);


		public static List<FormulaToken> Tokenize(string text)
		{
			var tokens = new List<FormulaToken>();
			if (string.IsNullOrEmpty(text))
			{
				return tokens;
			}

			// criteria is everything after the last top-level comma inside countif(...)
			var criteriaStart = FindCriteriaStart(text);

			foreach (Match match in TokenRegex.Matches(text))
			{
				FormulaTokenKind kind;

				if (criteriaStart >= 0 && match.Index > criteriaStart &&
					!match.Groups["ws"].Success && text[match.Index] != ')')
				{
					kind = FormulaTokenKind.Criteria;
				}
				else if (match.Groups["word"].Success)
				{
					var next = match.Index + match.Length;
					kind = CellRegex.IsMatch(match.Value) ? FormulaTokenKind.Cell
						: next < text.Length && text[next] == '(' ? FormulaTokenKind.Function
						: string.Equals(match.Value, "let", System.StringComparison.OrdinalIgnoreCase)
							? FormulaTokenKind.Plain
							: FormulaTokenKind.Variable;
				}
				else if (match.Groups["num"].Success)
				{
					kind = FormulaTokenKind.Number;
				}
				else if (match.Groups["punct"].Success)
				{
					kind = FormulaTokenKind.Punctuation;
				}
				else
				{
					kind = FormulaTokenKind.Plain;
				}

				tokens.Add(new FormulaToken(kind, match.Index, match.Length));
			}

			return tokens;
		}


		private static int FindCriteriaStart(string text)
		{
			var index = text.IndexOf("countif(", System.StringComparison.OrdinalIgnoreCase);
			if (index < 0)
			{
				return -1;
			}

			var depth = 0;
			var comma = -1;
			for (var i = index + "countif".Length; i < text.Length; i++)
			{
				var c = text[i];
				if (c == '(')
				{
					depth++;
				}
				else if (c == ')')
				{
					depth--;
					if (depth == 0)
					{
						break;
					}
				}
				else if (c == ',' && depth == 1)
				{
					comma = i;
				}
			}

			return comma;
		}
	}
}
