//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Base class for all parameter constraints. A constraint is applied by the CLI host
	/// after type-converting raw user input; it validates the converted value and returns a
	/// human-readable error message when the value is unacceptable.
	/// </summary>
	public abstract class CliConstraint
	{
		/// <summary>
		/// Validates a candidate value.
		/// </summary>
		/// <param name="value">The converted value to validate.</param>
		/// <param name="errorMessage">
		/// When the method returns <c>false</c>, contains a human-readable explanation
		/// of why the value is invalid. <c>null</c> on success.
		/// </param>
		/// <returns><c>true</c> if the value satisfies the constraint; otherwise <c>false</c>.</returns>
		public abstract bool Validate(object value, out string errorMessage);
	}

	/// <summary>
	/// Constrains a numeric parameter to an inclusive [<see cref="Minimum"/>, <see cref="Maximum"/>]
	/// range. Works with any type implementing <see cref="IComparable"/> (int, double, DateTime, …).
	/// </summary>
	public sealed class RangeConstraint : CliConstraint
	{
		/// <summary>Gets the inclusive lower bound.</summary>
		public IComparable Minimum { get; }

		/// <summary>Gets the inclusive upper bound.</summary>
		public IComparable Maximum { get; }

		/// <summary>
		/// Initializes a new <see cref="RangeConstraint"/> with the specified bounds.
		/// </summary>
		public RangeConstraint(IComparable minimum, IComparable maximum)
		{
			Minimum = minimum ?? throw new ArgumentNullException(nameof(minimum));
			Maximum = maximum ?? throw new ArgumentNullException(nameof(maximum));
		}

		/// <inheritdoc/>
		public override bool Validate(object value, out string errorMessage)
		{
			if (value is IComparable c
				&& c.CompareTo(Minimum) >= 0
				&& c.CompareTo(Maximum) <= 0)
			{
				errorMessage = null;
				return true;
			}

			errorMessage = $"Value must be between {Minimum} and {Maximum}.";
			return false;
		}
	}

	/// <summary>
	/// Constrains a parameter to a fixed set of allowed string values.
	/// The CLI host displays the list as a numbered menu so the user can pick by number
	/// or by typing the value directly.
	/// </summary>
	public sealed class EnumConstraint : CliConstraint
	{
		/// <summary>Gets the ordered list of allowed values.</summary>
		public IReadOnlyList<string> Values { get; }

		/// <summary>
		/// Gets a value indicating whether comparisons against <see cref="Values"/>
		/// are case-sensitive. Defaults to <c>false</c> (case-insensitive).
		/// </summary>
		public bool CaseSensitive { get; }

		/// <summary>
		/// Initializes a new <see cref="EnumConstraint"/> with the given allowed values.
		/// </summary>
		public EnumConstraint(IEnumerable<string> values, bool caseSensitive = false)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			Values = new List<string>(values).AsReadOnly();
			CaseSensitive = caseSensitive;
		}

		/// <inheritdoc/>
		public override bool Validate(object value, out string errorMessage)
		{
			var s = value?.ToString();
			var comparison = CaseSensitive
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			foreach (var allowed in Values)
			{
				if (string.Equals(s, allowed, comparison))
				{
					errorMessage = null;
					return true;
				}
			}

			errorMessage = $"Value must be one of: {string.Join(", ", Values)}.";
			return false;
		}
	}

	/// <summary>
	/// Constrains a string parameter to values that match a regular expression.
	/// <see cref="Hint"/> is shown to the user as a human-readable format example
	/// (e.g. <c>"YYYY-MM-DD"</c>) instead of exposing the raw regex pattern.
	/// </summary>
	public sealed class FormatConstraint : CliConstraint
	{
		/// <summary>Gets the regular expression pattern used for validation.</summary>
		public string Pattern { get; }

		/// <summary>
		/// Gets an optional human-readable format hint shown to the user
		/// (e.g. <c>"YYYY-MM-DD"</c>, <c>"{section-id}"</c>).
		/// When <c>null</c>, a generic "does not match required pattern" message is used.
		/// </summary>
		public string Hint { get; }

		private readonly Regex regex;

		/// <summary>
		/// Initializes a new <see cref="FormatConstraint"/> with a compiled regex and
		/// an optional display hint.
		/// </summary>
		public FormatConstraint(string pattern, string hint = null)
		{
			Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
			Hint = hint;
			regex = new Regex(pattern, RegexOptions.Compiled);
		}

		/// <inheritdoc/>
		public override bool Validate(object value, out string errorMessage)
		{
			var s = value?.ToString() ?? string.Empty;

			if (regex.IsMatch(s))
			{
				errorMessage = null;
				return true;
			}

			errorMessage = Hint != null
				? $"Value must match format: {Hint}."
				: "Value does not match the required pattern.";
			return false;
		}
	}
}
