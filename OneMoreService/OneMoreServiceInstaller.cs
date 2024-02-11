//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreService
{
	using System.ComponentModel;
	using System.Configuration.Install;
	using System.ServiceProcess;


	[RunInstaller(true)]
	public class OneMoreServiceInstaller : Installer
	{

		/// <summary>
		/// Called by the .NET framework when the service is installed
		/// </summary>
		public OneMoreServiceInstaller()
		{
			// instantiate installers for process and services
			var procInstaller = new ServiceProcessInstaller();
			var hostInstaller = new ServiceInstaller();

			procInstaller.Account = ServiceAccount.LocalSystem;
			hostInstaller.StartType = ServiceStartMode.Automatic;

			// serviceName must equal those on ServiceBase derived classes!
			hostInstaller.ServiceName = Properties.Resources.ServiceName;
			hostInstaller.Description = Properties.Resources.ServiceDescription;
			hostInstaller.DisplayName = Properties.Resources.ServiceDisplayName;

			// add installers to collection; the order is not important
			Installers.Add(hostInstaller);
			Installers.Add(procInstaller);
		}
	}
}
