//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;

	/// <summary>
	/// Describes a single input parameter for a CLI command.
	/// Instances are created by a command's <see cref="ICliCommand.DefineParameters"/>
	/// implementation (typically via the fluent helpers on <see cref="CliParameterDefinition"/>)
	/// and are read-only once constructed.
	/// </summary>
	public sealed class CliParameter
	{
		/// <summary>
		/// Gets the machine-friendly name of this parameter.
		/// Used as the dictionary key in <see cref="CliParameterSet"/> and displayed as
		/// the prompt label by the CLI host. Should be camelCase (e.g. <c>"outputPath"</c>).
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a human-readable description of what the parameter represents.
		/// Displayed alongside the prompt so the user understands what to enter.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Gets the expected .NET runtime type of the value (e.g. <c>typeof(int)</c>,
		/// <c>typeof(string)</c>, <c>typeof(bool)</c>, <c>typeof(DateTime)</c>).
		/// The CLI host uses this to parse and convert the raw string entered by the user.
		/// </summary>
		public Type DataType { get; }

		/// <summary>
		/// Gets a value indicating whether the user must supply a value for this parameter.
		/// When <c>false</c>, the user may press Enter to accept <see cref="DefaultValue"/>.
		/// </summary>
		public bool Required { get; }

		/// <summary>
		/// Gets the value used when the user skips an optional parameter.
		/// Ignored when <see cref="Required"/> is <c>true</c>.
		/// May be <c>null</c> for optional parameters that have no meaningful default.
		/// </summary>
		public object DefaultValue { get; }

		/// <summary>
		/// Gets the optional constraint applied by the CLI host after type conversion.
		/// <c>null</c> means the parameter is unconstrained beyond its <see cref="DataType"/>.
		/// </summary>
		public CliConstraint Constraint { get; }

		/// <summary>
		/// Initializes a new <see cref="CliParameter"/> with the given definition.
		/// </summary>
		/// <param name="name">Machine-friendly parameter name (camelCase).</param>
		/// <param name="description">Human-readable description shown when prompting.</param>
		/// <param name="dataType">Expected .NET type after conversion.</param>
		/// <param name="required">
		/// <c>true</c> if the user must supply a value; <c>false</c> to allow skipping.
		/// </param>
		/// <param name="defaultValue">
		/// Fallback value for optional parameters. Ignored when <paramref name="required"/>
		/// is <c>true</c>.
		/// </param>
		/// <param name="constraint">
		/// Optional validation constraint applied after type conversion.
		/// </param>
		public CliParameter(
			string name,
			string description,
			Type dataType,
			bool required = true,
			object defaultValue = null,
			CliConstraint constraint = null)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
			Required = required;
			DefaultValue = defaultValue;
			Constraint = constraint;
		}

		/// <summary>
		/// Validates a candidate value against this parameter's <see cref="Constraint"/>.
		/// Always returns <c>true</c> (and sets <paramref name="errorMessage"/> to <c>null</c>)
		/// when no constraint is defined.
		/// </summary>
		/// <param name="value">The type-converted value to validate.</param>
		/// <param name="errorMessage">
		/// On failure, a human-readable explanation of the violation; <c>null</c> on success.
		/// </param>
		/// <returns><c>true</c> if valid or unconstrained; otherwise <c>false</c>.</returns>
		public bool TryValidate(object value, out string errorMessage)
		{
			if (Constraint == null)
			{
				errorMessage = null;
				return true;
			}

			return Constraint.Validate(value, out errorMessage);
		}
	}
}
