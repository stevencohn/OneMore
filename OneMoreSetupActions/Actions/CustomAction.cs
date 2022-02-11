//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{

	internal abstract class CustomAction
	{
		// these are CMD exit codes
		public const int SUCCESS = 0;
		public const int FAILURE = 1;

		protected readonly Logger logger;
		protected readonly Stepper stepper;


		protected CustomAction(Logger logger, Stepper stepper)
		{
			this.logger = logger;
			this.stepper = stepper;

			RegistryHelper.SetLogger(logger);
		}


		public abstract int Install();


		public abstract int Uninstall();
	}
}
