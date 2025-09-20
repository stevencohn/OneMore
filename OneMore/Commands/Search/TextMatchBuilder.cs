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

			var tokens = Tokenize(query);
			var ast = Parse(tokens);
			var pattern = $"^{ToRegex(ast)}.*$";
			return new Regex(pattern, options);
		}


		private static List<Token> Tokenize(string input)
		{
			var tokens = new List<Token>();
			var pattern = "\"[^\"]+\"|\\(|\\)|\\bAND\\b|\\bOR\\b|\\bNOT\\b|[^()\\s]+";
			var rawTokens = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

			List<string> termBuffer = new List<string>();

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

				if (type != null)
				{
					// flush any buffered terms as one phrase
					if (termBuffer.Count > 0)
					{
						tokens.Add(new Token(TokenType.Term, string.Join(" ", termBuffer)));
						termBuffer.Clear();
					}
					tokens.Add(new Token(type.Value, val));
				}
				else
				{
					// strip quotes if present
					if (val.StartsWith("\"") && val.EndsWith("\""))
					{
						val = val.Substring(1, val.Length - 2);
					}

					termBuffer.Add(val);
				}
			}

			// final flush
			if (termBuffer.Count > 0)
				tokens.Add(new Token(TokenType.Term, string.Join(" ", termBuffer)));

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
							var notTarget = ParseExpression();
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
							var left = stack.Pop();
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
				NotNode n => $@"(?!.*{WildcardToRegex(ExtractTerm(n.Child))})",
				AndNode a => string.Join("", a.Children.ConvertAll(ToRegex)) + ".*",
				OrNode o => "(?:" + string.Join("|", o.Children.ConvertAll(ToRegex)) + ")",
				_ => ""
			};
		}


		private static string WildcardToRegex(string term)
		{
			return Regex.Escape(term).Replace("\\*", ".*");
		}


		private static string ExtractTerm(INode node)
		{
			if (node is TermNode t)
			{
				return t.Value;
			}

			throw new InvalidOperationException("NOT only supports single terms");
		}
	}
}
