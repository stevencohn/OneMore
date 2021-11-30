//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	internal class Stepper
	{
		private int step;

		public Stepper() { }

		public int Step()
		{
			return step++;
		}
	}
}
