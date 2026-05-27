//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// An ordered, fluent builder of <see cref="CliParameter"/> descriptors.
	/// <para>
	/// Returned by <see cref="ICliCommand.DefineParameters"/>; iterated by the CLI host to
	/// prompt the user for values in the order they were added. Convenience <c>Add*</c>
	/// methods cover the most common parameter types and constraints. For anything not
	/// covered, construct a <see cref="CliParameter"/> directly and call <see cref="Add"/>.
	/// </para>
	/// </summary>
	/// <example>
	/// <code>
	/// public CliParameterDefinition DefineParameters() => new CliParameterDefinition()
	///     .AddString ("outputPath",  "Destination folder",           required: true)
	///     .AddEnum   ("format",      "Output format",
	///                  new[] { "HTML", "PDF", "Word" },              defaultValue: "HTML")
	///     .AddInteger("maxDepth",    "Maximum nesting depth to export",
	///                  minimum: 1, maximum: 10,                      required: false, defaultValue: 5)
	///     .AddBoolean("attachments", "Include embedded attachments",  required: false, defaultValue: true);
	/// </code>
	/// </example>
	public sealed class CliParameterDefinition : IEnumerable<CliParameter>
	{
		private readonly List<CliParameter> items = new List<CliParameter>();

		// low-level entry point - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Appends a pre-constructed <see cref="CliParameter"/> to the definition.
		/// All fluent <c>Add*</c> helpers delegate here.
		/// </summary>
		public CliParameterDefinition Add(CliParameter parameter)
		{
			if (parameter == null) throw new ArgumentNullException(nameof(parameter));
			items.Add(parameter);
			return this;
		}

		// fluent convenience methods  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Adds an unconstrained <see cref="string"/> parameter.
		/// </summary>
		public CliParameterDefinition AddString(
			string name,
			string description,
			bool required = true,
			string defaultValue = null)
			=> Add(new CliParameter(name, description, typeof(string), required, defaultValue));

		/// <summary>
		/// Adds an <see cref="int"/> parameter constrained to
		/// [<paramref name="minimum"/>, <paramref name="maximum"/>].
		/// </summary>
		public CliParameterDefinition AddInteger(
			string name,
			string description,
			int minimum,
			int maximum,
			bool required = true,
			int? defaultValue = null)
			=> Add(new CliParameter(
				name, description, typeof(int), required, defaultValue,
				new RangeConstraint(minimum, maximum)));

		/// <summary>
		/// Adds a <see cref="double"/> parameter constrained to
		/// [<paramref name="minimum"/>, <paramref name="maximum"/>].
		/// </summary>
		public CliParameterDefinition AddDouble(
			string name,
			string description,
			double minimum,
			double maximum,
			bool required = true,
			double? defaultValue = null)
			=> Add(new CliParameter(
				name, description, typeof(double), required, defaultValue,
				new RangeConstraint(minimum, maximum)));

		/// <summary>
		/// Adds a <see cref="bool"/> parameter.
		/// The CLI host should accept <c>true</c>/<c>false</c>, <c>yes</c>/<c>no</c>,
		/// or <c>1</c>/<c>0</c> as input.
		/// </summary>
		public CliParameterDefinition AddBoolean(
			string name,
			string description,
			bool required = true,
			bool defaultValue = false)
			=> Add(new CliParameter(name, description, typeof(bool), required, defaultValue));

		/// <summary>
		/// Adds a <see cref="string"/> parameter restricted to one of
		/// <paramref name="allowedValues"/>.
		/// The CLI host displays the allowed values as a numbered list.
		/// </summary>
		public CliParameterDefinition AddEnum(
			string name,
			string description,
			string[] allowedValues,
			bool required = true,
			string defaultValue = null,
			bool caseSensitive = false)
			=> Add(new CliParameter(
				name, description, typeof(string), required, defaultValue,
				new EnumConstraint(allowedValues, caseSensitive)));

		/// <summary>
		/// Adds a <see cref="string"/> parameter whose value must match
		/// <paramref name="regexPattern"/>.
		/// <paramref name="formatHint"/> (e.g. <c>"YYYY-MM-DD"</c>) is shown to the user
		/// instead of the raw pattern.
		/// </summary>
		public CliParameterDefinition AddFormatted(
			string name,
			string description,
			string regexPattern,
			string formatHint,
			bool required = true,
			string defaultValue = null)
			=> Add(new CliParameter(
				name, description, typeof(string), required, defaultValue,
				new FormatConstraint(regexPattern, formatHint)));

		/// <summary>
		/// Adds a <see cref="DateTime"/> parameter validated against the
		/// ISO-8601 date format <c>YYYY-MM-DD</c>.
		/// </summary>
		public CliParameterDefinition AddDateTime(
			string name,
			string description,
			bool required = true,
			DateTime? defaultValue = null)
			=> Add(new CliParameter(
				name, description, typeof(DateTime), required, defaultValue,
				new FormatConstraint(@"^\d{4}-\d{2}-\d{2}$", "YYYY-MM-DD")));

		// IEnumerable<CliParameter> - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <inheritdoc/>
		public IEnumerator<CliParameter> GetEnumerator() => items.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>Gets the number of parameters in this definition.</summary>
		public int Count => items.Count;
	}
}
