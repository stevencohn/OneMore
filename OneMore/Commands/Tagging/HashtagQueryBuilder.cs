//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1144 // Unused private types or members should be removed

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Hacky solution to convert simple user entered query string to proper SQL WHERE clause.
	/// This lets the user enter something like "a b" and converts it to
	/// WHERE tag LIKE '%a%' AND tag LIKE '%b%'
	/// or for case-sensitive queries
	/// The AND operator is implicit; user must explicitly specify OR operator.
	/// Parenthesis are allowed.
	/// </summary>
	/// <remarks>
	/// Type any part of one or more hashtags. Wildcards are implied unless a tag is ended
	/// with a period. Parenthesis and logical operators are allowed.
	/// </remarks>
	internal class HashtagQueryBuilder
	{
		private readonly string fieldRef;
		private readonly bool caseSensitive;


		public HashtagQueryBuilder(string fieldRef, bool caseSensitive)
		{
			this.fieldRef = fieldRef;
			this.caseSensitive = caseSensitive;
		}


		public string BuildFormattedWhereClause(string query, out string parsed)
		{
			parsed = ExplodeQuery(query);

			parsed = Regex.Replace(parsed, @"'([^']+)'", (m) =>
			{
				return FormatCriteria(m.Value);
			});

			return $"WHERE {parsed}";
		}


		private string ExplodeQuery(string query)
		{
			var builder = new StringBuilder();

			var parts = Tokenize(query);
			for (int i = 0; i < parts.Count; i++)
			{
				var part = parts[i].Trim().ToUpper();

				if (part == "AND" || part == "&&")
				{
					builder.Append("AND ");
				}
				else if (part == "OR" || part == "||")
				{
					builder.Append("OR ");
				}
				else if (part == "(" || part == ")")
				{
					builder.Append($"{part} ");
				}
				else
				{
					var op = caseSensitive ? "GLOB" : "LIKE";
					builder.Append(@$"{fieldRef} {op} '{parts[i]}'");
					if (i < parts.Count - 1)
					{
						builder.Append(" ");
						var next = parts[i + 1].Trim().ToUpper();
						if (next != "AND" && next != "&&" && next != "OR" && next != "||" && next != ")")
						{
							builder.Append("AND ");
						}
					}
				}
			}
			return builder.ToString();
		}


		private List<string> Tokenize(string query)
		{
			const char Space = ' ';
			var tokens = new List<string>();
			var token = string.Empty;
			char prev = Space;
			for (int i = 0; i < query.Length; i++)
			{
				var c = query[i];

				// to avoid SQL Injection, strip out semicolons and single quotes; should have
				// only double-quotes at this point and will convert those to single quotes later
				if (c == ';' || c == '\'')
				{
					continue;
				}

				if (!(c == Space && prev == Space))
				{
					if (c == Space && token.Length > 0)
					{
						tokens.Add(token);
						token = string.Empty;
					}
					else if (c == '(' || c == ')')
					{
						if (token.Length > 0)
						{
							tokens.Add(token);
							token = string.Empty;
						}
						tokens.Add(c.ToString());
					}
					else
					{
						token = $"{token}{c}";
					}

					prev = c;
				}
			}

			if (token.Length > 0)
			{
				tokens.Add(token);
			}

			return tokens;
		}


		private string FormatCriteria(string criteria)
		{
			criteria = criteria.Trim().Trim('\'');
			if (criteria.Length == 0)
			{
				return criteria;
			}

			var terminated = criteria.EndsWith(".");

			// strip out non-tag characters
			criteria = Regex.Replace(criteria, @"[^\w\d\-_#]", string.Empty);
			var wildcard = caseSensitive ? "*" : "%";

			if (criteria.Length == 0)
			{
				criteria = wildcard;
			}
			else if (criteria[0] != wildcard[0])
			{
				criteria = $"{wildcard}{criteria}";
			}

			// allow "abc." to be interpreted as "%abc" but "abc" will be "%abc%"
			if (!terminated && !criteria.EndsWith(wildcard))
			{
				criteria = $"{criteria}{wildcard}";
			}

			var joker = wildcard[0] == '*' ? '%' : '*';
			return $"'{criteria.Replace(joker, wildcard[0])}'";
		}


		/// <summary>
		/// Extract the wildcard tags from a parsed string into a regex expression
		/// that can be used to highlight context lines.
		/// </summary>
		/// <param name="parsed"></param>
		/// <returns></returns>
		public Regex GetMatchingPattern(string parsed)
		{
			var pattern = string.Empty;
			var matches = Regex.Matches(parsed, @"'([^']+)'");
			foreach (Match match in matches)
			{
				var wildcard = caseSensitive ? "*" : "%";
				var value = match.Groups[1].Value.Replace(wildcard, string.Empty);

				// ignore "%" wildcard, from user input "*"
				if (value.Length > 0)
				{
					if (pattern.Length > 0)
					{
						pattern = $"{pattern}|";
					}

					if (value[0] != '#')
					{
						// make sure we're looking for a hashtag and not some random text
						value = $"#[\\w\\d\\-_]*{value}";
					}

					pattern = $"{pattern}{value}";
				}
			}

			return new Regex(pattern, RegexOptions.IgnoreCase);
		}
	}
}
