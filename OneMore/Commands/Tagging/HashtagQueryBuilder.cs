//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1144 // Unused private types or members should be removed

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq.Dynamic.Core;
	using System.Text;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Hacky solution to convert user entered query string to proper SQL WHERE clause
	/// </summary>
	internal class HashtagQueryBuilder
	{
		private sealed class HashtagCriteria
		{
			public string Tags { get; set; }
		}

		public string BuildFormattedWhereClause(string query)
		{
			var sql = ExplodeQuery(query);

			var config = new ParsingConfig
			{
				IsCaseSensitive = false,
				RenameParameterExpression = true
			};

			var lambda = DynamicExpressionParser
				.ParseLambda<HashtagCriteria, bool>(config, true, sql);

			var code = lambda.ToString();

			// 5 is length of "g => "
			var where = code.Substring(5)
				.Replace("==", "LIKE")
				.Replace("\"", "'")
				.Replace("AndAlso", "AND")
				.Replace("OrElse", "OR");

			where = Regex.Replace(where, @"'([^']+)'", (m) =>
			{
				return FormatCriteria(m.Value);
			});

			where = where.Replace("tags LIKE", "g.tags LIKE");
			return $"WHERE {where}";
		}


		private string ExplodeQuery(string query)
		{
			var builder = new StringBuilder("g => ");

			var parts = Tokenize(query);
			for (int i = 0; i < parts.Count; i++)
			{
				var part = parts[i].ToUpper();

				if (part == "AND" || part == "OR" || part == "(" || part == ")")
				{
					builder.Append(part);
					builder.Append(" ");
				}
				else
				{
					builder.Append(@$"tags=""{parts[i]}""");
					if (i < parts.Count - 1)
					{
						builder.Append(" ");
						var next = parts[i + 1].ToUpper();
						if (next != "AND" && next != "OR" && next != ")")
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

			if (criteria[0] != '#' && criteria[0] != '%')
			{
				criteria = $"%{criteria}";
			}

			// allow "abc." to be interpreted as "%abc" but "abc" will be "%abc%"
			if (!terminated && !criteria.EndsWith("%"))
			{
				criteria = $"{criteria}%";
			}

			return $"'{criteria.Replace('*', '%')}'";
		}
	}
}
