//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Converts a simplified SQL-like WHERE clause into a regular expression.
	/// </summary>
	/// <remarks>
	/// Supported syntax includes logical operators AND, OR, NOT, parentheses for grouping,
	/// and wildcard character * to represent any sequence of characters.
	/// </remarks>
	/// <example>
	/// Example clause: (error* OR fail*) AND NOT warning*
	/// Would match any string containing "error" or "fail" (with optional trailing characters),
	/// while excluding any string that contains "warning".
	/// <code>
	/// string clause = "(error* OR fail*) AND NOT warning*";
	/// Regex regex = ClauseRegexBuilder.BuildRegex(clause);
	/// bool isMatch = regex.IsMatch("Critical failure occurred in error handler");
	/// </code>
	/// </example>
	internal class TextMatchBuilder
	{
		#region Supporting types

		private enum TokenType { Term, And, Or, Not, LParen, RParen }

		private sealed class Token
		{
			public TokenType Type { get; set; }
			public string Value { get; set; }
			public Token(TokenType type, string value)
			{
				Type = type;
				Value = value;
			}
		}

		private interface INode { }

		private sealed class TermNode : INode
		{
			public string Value { get; }
			public TermNode(string value) => Value = value;
		}

		private sealed class NotNode : INode
		{
			public INode Child { get; }
			public NotNode(INode child) => Child = child;
		}

		private sealed class AndNode : INode
		{
			public List<INode> Children { get; }
			public AndNode(List<INode> children) => Children = children;
		}

		private sealed class OrNode : INode
		{
			public List<INode> Children { get; }
			public OrNode(List<INode> children) => Children = children;
		}

		#endregion Supporting types


		// the shortest piece of a term the trigram text index can look up
		private const int TextIndexMinimum = 3;

		private readonly bool asRegex;
		private readonly bool caseSensitive;


		public TextMatchBuilder(bool asRegex, bool caseSensitive)
		{
			this.asRegex = asRegex;
			this.caseSensitive = caseSensitive;
		}


		public Regex BuildRegex(string query)
		{
			var options = RegexOptions.Compiled;
			if (!caseSensitive)
			{
				options |= RegexOptions.IgnoreCase;
			}

			if (asRegex)
			{
				try
				{
					return new Regex(query, options);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error parsing user regex [{query}]");
					Logger.Current.WriteLine(exc);
					return null;
				}
			}

			// A paragraph can hold line breaks (a <br /> is followed by a newline in OneNote's
			// text), and a term may be on any line of it. Without Singleline, ".*" stops at the
			// first newline and the terms on later lines are never found.
			options |= RegexOptions.Singleline;

			var tokens = Tokenize(query);
			var ast = Parse(tokens);
			var pattern = $"^{ToRegex(ast)}.*$";
			return new Regex(pattern, options);
		}


		/// <summary>
		/// Builds an FTS5 MATCH expression for the trigram text index that finds every paragraph
		/// the query can match, and possibly others. The caller must still test each paragraph
		/// found with <see cref="BuildRegex"/>, which gives the exact answer; the index only
		/// narrows what has to be tested.
		/// </summary>
		/// <param name="query">The query, as given to <see cref="BuildRegex"/></param>
		/// <returns>The expression, or null if the index cannot narrow the query, such as for a
		/// regular expression, a query of only short terms, or one that is only NOT. Then every
		/// paragraph is a candidate.</returns>
		/// <remarks>
		/// A term is looked up by its literal pieces between wildcards, each as a substring, which
		/// is how the trigram tokenizer sees it and how <see cref="BuildRegex"/> treats a term. A
		/// piece shorter than three characters cannot be looked up and is left to the regex. NOT is
		/// left to the regex as well, because excluding what an inexact lookup found could exclude
		/// paragraphs that do not contain the term.
		/// </remarks>
		public string BuildIndexQuery(string query)
		{
			if (asRegex || string.IsNullOrWhiteSpace(query))
			{
				return null;
			}

			try
			{
				return ToIndexQuery(Parse(Tokenize(query)));
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error translating query [{query}] for the index", exc);
				return null;
			}
		}


		private static string ToIndexQuery(INode node)
		{
			switch (node)
			{
				case TermNode t:
					return TermToIndexQuery(t.Value);

				case AndNode a:
					// a child that cannot narrow is left out; the regex enforces it
					var required = new List<string>();
					foreach (var child in a.Children)
					{
						var part = ToIndexQuery(child);
						if (part is not null)
						{
							required.Add(part);
						}
					}

					return Join(required, " AND ");

				case OrNode o:
					// if any alternative cannot narrow, then any paragraph could be a candidate
					var either = new List<string>();
					foreach (var child in o.Children)
					{
						var part = ToIndexQuery(child);
						if (part is null)
						{
							return null;
						}

						either.Add(part);
					}

					return Join(either, " OR ");

				default:
					return null;
			}

			static string Join(List<string> parts, string op)
			{
				return parts.Count == 0 ? null
					: parts.Count == 1 ? parts[0]
					: "(" + string.Join(op, parts) + ")";
			}
		}


		private static string TermToIndexQuery(string term)
		{
			var pieces = new List<string>();

			foreach (var piece in term.Split('*'))
			{
				if (CountCharacters(piece) >= TextIndexMinimum)
				{
					pieces.Add($"\"{piece.Replace("\"", "\"\"")}\"");
				}
			}

			return pieces.Count == 0 ? null
				: pieces.Count == 1 ? pieces[0]
				: "(" + string.Join(" AND ", pieces) + ")";
		}


		// the trigram tokenizer counts characters, not UTF-16 code units
		private static int CountCharacters(string text)
		{
			var count = 0;
			foreach (var c in text)
			{
				if (!char.IsLowSurrogate(c))
				{
					count++;
				}
			}

			return count;
		}


		private static List<Token> Tokenize(string input)
		{
			var tokens = new List<Token>();
			var pattern = "\"[^\"]+\"|\\(|\\)|\\bAND\\b|\\bOR\\b|\\bNOT\\b|[^()\\s]+";
			var rawTokens = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

			foreach (Match m in rawTokens)
			{
				string val = m.Value;

				TokenType? type = val.ToUpper() switch
				{
					"AND" => TokenType.And,
					"OR" => TokenType.Or,
					"NOT" => TokenType.Not,
					"(" => TokenType.LParen,
					")" => TokenType.RParen,
					_ => null
				};

				if (type is not null)
				{
					tokens.Add(new Token(type.Value, val));
				}
				else
				{
					// strip quotes if present; a quoted phrase becomes one atomic term,
					// an unquoted word is its own atomic term. Bare words are never
					// merged with their neighbors here — juxtaposed terms are combined
					// by the parser as an implicit AND (any order), not as one phrase.
					if (val.Length >= 2 && val.StartsWith("\"") && val.EndsWith("\""))
					{
						val = val.Substring(1, val.Length - 2);
					}

					tokens.Add(new Token(TokenType.Term, val));
				}
			}

			return NormalizeTokens(tokens);
		}


		private static List<Token> NormalizeTokens(List<Token> tokens)
		{
			if (tokens.Count == 0) return tokens;

			// AND or OR at the start has no left operand — demote to raw text
			if (tokens[0].Type == TokenType.And || tokens[0].Type == TokenType.Or)
				tokens[0] = new Token(TokenType.Term, tokens[0].Value);

			// AND, OR, or NOT at the end has no right operand — demote to raw text
			int last = tokens.Count - 1;
			var lastType = tokens[last].Type;
			if (lastType == TokenType.And || lastType == TokenType.Or || lastType == TokenType.Not)
				tokens[last] = new Token(TokenType.Term, tokens[last].Value);

			return tokens;
		}


		private static INode Parse(List<Token> tokens)
		{
			int index = 0;
			return ParseExpression();

			// core recursive decent parser: Walks the token list, builds the abstract syntax
			// tree (AST), and handles nested parentheses, operator precedence, and grouping
			INode ParseExpression()
			{
				var stack = new Stack<INode>();
				while (index < tokens.Count)
				{
					var token = tokens[index];
					switch (token.Type)
					{
						case TokenType.Term:
							stack.Push(new TermNode(token.Value));
							index++;
							break;
						case TokenType.Not:
							index++;
							INode notTarget;
							if (index < tokens.Count && tokens[index].Type == TokenType.LParen)
							{
								// grouped operand — unchanged recursive path, still
								// restricted to a single term by NegateRegex below
								notTarget = ParseExpression();
							}
							else
							{
								// distribute NOT over the run of bare terms that follows:
								// "NOT foo bar" excludes anything containing "foo" or "bar"
								var notTerms = new List<INode>();
								while (index < tokens.Count && tokens[index].Type == TokenType.Term)
								{
									notTerms.Add(new TermNode(tokens[index].Value));
									index++;
								}
								notTarget = notTerms.Count == 1 ? notTerms[0] : new AndNode(notTerms);
							}
							stack.Push(new NotNode(notTarget));
							break;
						case TokenType.LParen:
							index++;
							stack.Push(ParseExpression());
							break;
						case TokenType.RParen:
							index++;
							return stack.Count == 1 ? stack.Pop() : new AndNode(stack.ToList());
						case TokenType.And:
						case TokenType.Or:
							index++;
							var left = stack.Count == 1 ? stack.Pop() : new AndNode(stack.ToList());
							var right = ParseExpression();
							var combined = token.Type == TokenType.And
								? (INode)new AndNode(new List<INode> { left, right })
								: (INode)new OrNode(new List<INode> { left, right });
							return combined;
						default:
							index++;
							break;
					}
				}
				return stack.Count == 1 ? stack.Pop() : new AndNode(stack.ToList());
			}
		}


		private static string ToRegex(INode INode)
		{
			return INode switch
			{
				TermNode t => $@"(?=.*{WildcardToRegex(t.Value)})",
				NotNode n => NegateRegex(n.Child),
				AndNode a => string.Join("", a.Children.ConvertAll(ToRegex)) + ".*",
				OrNode o => "(?:" + string.Join("|", o.Children.ConvertAll(ToRegex)) + ")",
				_ => ""
			};
		}


		private static string WildcardToRegex(string term)
		{
			return Regex.Escape(term).Replace("\\*", ".*");
		}


		private static string NegateRegex(INode node)
		{
			if (node is TermNode t)
			{
				return $@"(?!.*{WildcardToRegex(t.Value)})";
			}

			// "NOT foo bar" — a bare run of terms — excludes anything containing any of them
			if (node is AndNode a && a.Children.TrueForAll(c => c is TermNode))
			{
				return string.Concat(a.Children.ConvertAll(
					c => $@"(?!.*{WildcardToRegex(((TermNode)c).Value)})"));
			}

			throw new InvalidOperationException("NOT only supports single terms or a bare list of terms");
		}
	}
}
