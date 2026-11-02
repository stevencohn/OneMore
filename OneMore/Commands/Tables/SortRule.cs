//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	internal enum SortDataType
	{
		Auto,
		Text,
		DateTime,
		Number
	}


	/// <summary>
	/// Describes one level of a multi-column table sort.
	/// </summary>
	internal class SortRule
	{
		public int ColumnIndex { get; set; }
		public bool Descending { get; set; }
		public SortDataType DataType { get; set; }


		/// <summary>
		/// Serializes rules to a compact string of comma-separated tokens, each
		/// {column index}{+|-}{t|d|n}, for example "2-n,0+t".
		/// </summary>
		public static string Serialize(IEnumerable<SortRule> rules)
		{
			return string.Join(",", rules.Select(r =>
				$"{r.ColumnIndex}{(r.Descending ? '-' : '+')}{GetTypeCode(r.DataType)}"));
		}


		/// <summary>
		/// Parses the output of Serialize, skipping malformed tokens, columns outside the
		/// table, and repeated columns.
		/// </summary>
		public static List<SortRule> Parse(string content, int columnCount)
		{
			var rules = new List<SortRule>();
			if (string.IsNullOrWhiteSpace(content))
			{
				return rules;
			}

			var used = new HashSet<int>();

			foreach (var token in content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var t = token.Trim();
				if (t.Length < 3)
				{
					continue;
				}

				var type = t[t.Length - 1] switch
				{
					't' => SortDataType.Text,
					'd' => SortDataType.DateTime,
					'n' => SortDataType.Number,
					_ => SortDataType.Auto
				};

				var dir = t[t.Length - 2];
				if (type == SortDataType.Auto || (dir != '+' && dir != '-'))
				{
					continue;
				}

				if (!int.TryParse(t.Substring(0, t.Length - 2), out var column) ||
					column < 0 || column >= columnCount || !used.Add(column))
				{
					continue;
				}

				rules.Add(new SortRule
				{
					ColumnIndex = column,
					Descending = dir == '-',
					DataType = type
				});
			}

			return rules;
		}


		private static char GetTypeCode(SortDataType type)
		{
			switch (type)
			{
				case SortDataType.DateTime: return 'd';
				case SortDataType.Number: return 'n';
				default: return 't';
			}
		}
	}
}
