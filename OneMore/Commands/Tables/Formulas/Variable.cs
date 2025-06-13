//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	internal sealed class Variable
	{
		public string Name { get; set; }

		public double Value { get; set; }


		public Variable()
		{
			Name = string.Empty;
			Value = 0.0;
		}


		public Variable(string name, double value)
		{
			Name = name;
			Value = value;
		}
	}
}
