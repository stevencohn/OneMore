
namespace River.OneMoreAddIn.Commands.Formula
{
	using System;


	/// <summary>
	/// Manage formula versioning, such as reading, writing, upgrading
	/// </summary>
	internal class Formula
	{

		private const int CurrentVersion = 2;


		/// <summary>
		/// Indicates if this parsed formula is valid
		/// </summary>
		public bool Valid { get; private set; }


		/// <summary>
		/// Gets or sets the formula version, should be 0 through CurrentVersion
		/// </summary>
		public int Version { get; set; }


		/// <summary>
		/// Gets or sets the formula format.
		/// </summary>
		public FormulaFormat Format { get; set; }


		/// <summary>
		/// Gets or sets the decimal places to report.
		/// </summary>
		public int DecimalPlaces { get; set; }


		/// <summary>
		/// Gets or sets the formula expression.
		/// </summary>
		public string Expression { get; set; }


		/// <summary>
		/// Gets the formula range - version 0 only - deprecated
		/// </summary>
		[Obsolete("version 0")]
		public string Range { get; private set; }


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Initialize and new formula with the given properties. Used for formula creation
		/// </summary>
		/// <param name="format"></param>
		/// <param name="dplaces"></param>
		/// <param name="expression"></param>
		public Formula(FormulaFormat format, int dplaces, string expression)
		{
			Version = CurrentVersion;
			Format = format;
			DecimalPlaces = dplaces;
			Expression = expression;
			Valid = true;
		}


		/// <summary>
		/// Initialize a formula by parsing the given meta string. Used for existing formulas.
		/// </summary>
		/// <param name="meta"></param>
		public Formula(string meta)
		{
			Valid = false;

			if (string.IsNullOrEmpty(meta))
			{
				return;
			}

			var parts = meta.Split(';');

			// version is always first part
			if (!int.TryParse(parts[0], out var version))
			{
				return;
			}

			Version = version;

			if (version > CurrentVersion)
			{
				return;
			}

			// version 0;range;function;format
			if (version == 0)
			{
				Range = parts[1];
				Expression = parts[2];
				ParseFormat(parts[3]);
				Valid = true;
				return;
			}


			// version 1;format;expression
			if (version == 1)
			{
				ParseFormat(parts[1]);
				Expression = parts[2];
			}
			// version 2;format;dplaces;expression
			else
			{
				ParseFormat(parts[1]);
				ParseDecimalPlaces(parts[2]);
				Expression = parts[3];
			}

			Valid = !string.IsNullOrEmpty(Expression);
		}


		private void ParseDecimalPlaces(string places)
		{
			if (int.TryParse(places, out var value))
			{
				DecimalPlaces = value;
			}

		}


		private void ParseFormat(string format)
		{
			if (Enum.TryParse<FormulaFormat>(format, true, out var value))
			{
				Format = value;
			}
		}


		/// <summary>
		/// Serializes the formula properties as a string for storing in a meta tag.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{CurrentVersion};{Format};{DecimalPlaces};{Expression}";
		}
	}
}
