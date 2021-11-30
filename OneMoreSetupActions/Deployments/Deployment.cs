//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{

	internal abstract class Deployment
	{
		protected readonly Logger logger;
		protected readonly Stepper stepper;


		protected Deployment(Logger logger, Stepper stepper)
		{
			this.logger = logger;
			this.stepper = stepper;
		}


		public abstract bool Install();


		public abstract bool Uninstall();
	}
}
