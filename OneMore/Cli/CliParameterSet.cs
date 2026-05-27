//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A named, typed value bag populated by the CLI host and passed as <c>args[0]</c>
	/// to the command's <c>Execute</c> method when <c>runningFromCli</c> is true.
	/// <para>
	/// The host calls <see cref="Set"/> for each parameter after the user has supplied
	/// a valid value. The command implementation reads those values via <see cref="Get{T}"/>
	/// or <see cref="TryGet{T}"/>.
	/// </para>
	/// <para>
	/// Key look-ups are case-insensitive to match the name style used in
	/// <see cref="CliParameter.Name"/> (camelCase) regardless of how the host stores them.
	/// </para>
	/// </summary>
	public sealed class CliParameterSet
	{
		private readonly Dictionary<string, object> values =
			new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		// written by the CLI host - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Stores or overwrites the value for the named parameter.
		/// </summary>
		/// <param name="name">Parameter name (matches <see cref="CliParameter.Name"/>).</param>
		/// <param name="value">The validated, type-converted value to store.</param>
		public void Set(string name, object value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			values[name] = value;
		}

		// read by the command in Execute when runningFromCli is true - - - - - - - - - - - - - - -

		/// <summary>
		/// Returns the value of the named parameter converted to <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Target type; must be compatible with the stored value.</typeparam>
		/// <param name="name">Parameter name (case-insensitive).</param>
		/// <returns>The converted value.</returns>
		/// <exception cref="KeyNotFoundException">
		/// Thrown when no value has been set for <paramref name="name"/>.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// Thrown when the stored value cannot be converted to <typeparamref name="T"/>.
		/// </exception>
		public T Get<T>(string name)
		{
			if (!values.TryGetValue(name, out var raw))
				throw new KeyNotFoundException($"Parameter '{name}' was not provided.");

			return (T)Convert.ChangeType(raw, typeof(T));
		}

		/// <summary>
		/// Attempts to return the value of the named parameter converted to
		/// <typeparamref name="T"/>. Returns <c>false</c> (and sets
		/// <paramref name="value"/> to <c>default</c>) when the parameter is absent.
		/// </summary>
		/// <typeparam name="T">Target type; must be compatible with the stored value.</typeparam>
		/// <param name="name">Parameter name (case-insensitive).</param>
		/// <param name="value">
		/// On success, the converted value; on failure, <c>default(<typeparamref name="T"/>)</c>.
		/// </param>
		/// <returns><c>true</c> if the parameter was found and converted; otherwise <c>false</c>.</returns>
		public bool TryGet<T>(string name, out T value)
		{
			if (values.TryGetValue(name, out var raw))
			{
				value = (T)Convert.ChangeType(raw, typeof(T));
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Returns <c>true</c> if a value has been set for the named parameter.
		/// </summary>
		public bool Contains(string name) => values.ContainsKey(name);


		/// <summary>
		/// Returns the names of all parameters that have been set.
		/// Used by the CLI pipe client to serialise parameters into the request URI.
		/// </summary>
		internal IEnumerable<string> Keys => values.Keys;
	}
}
