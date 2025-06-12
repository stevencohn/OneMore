//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	internal sealed class Variable
	{
		public string Name { get; private set; }

		public double Value { get; private set; }


		public Variable(string name, double value)
		{
			Name = name;
			Value = value;
		}
	}
}
