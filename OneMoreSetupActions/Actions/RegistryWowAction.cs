//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;


	/// <summary>
	/// Clones the OneMore CLSID branch to support both 32bit and 64bit installs of OneNote
	/// </summary>
	internal class RegistryWowAction : CustomAction
	{

		public RegistryWowAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		/// <summary>
		/// Note this is invoked as its own CustomAction, not as part of Program
		/// </summary>
		/// <returns></returns>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine($"RegistryWowAction.Install --- x64:{Environment.Is64BitProcess}");

			if (CloningRequired())
			{
				// delete subtrees to start from scratch
				// then create new subtrees

				if (UnregisterWow() == SUCCESS &&
					RegisterWow() == SUCCESS)
				{
					return SUCCESS;
				}
			}

			return SUCCESS;
		}


		/*
		 * Initial attempt was to use RegistryKey.OpenBase specifying the RegistryHive and
		 * RegistryView.Registry32. This required configuring the project as AnyCPU and
		 * disabling the Prefer 32-bit option. However, this did not work consistently as
		 * it should. After trial and error, realized that only the ClassesRoot\CLSID\{guid}
		 * key needed to be cloned to WOW6432Node\CLSID and that could be done directly.
		 */
		private int RegisterWow()
		{
			logger.WriteLine("cloning CLSID");
			using (var source = Registry.ClassesRoot.OpenSubKey($@"CLSID\{RegistryHelper.OneNoteID}"))
			{
				if (source != null)
				{
					using (var target = Registry.ClassesRoot.OpenSubKey(@"WOW6432Node\CLSID", true))
					{
						logger.WriteLine($"copying from {source.Name} to {target.Name}");
						source.CopyTo(target);
					}
				}
			}

			return SUCCESS;
		}


		//========================================================================================

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine($"RegistryWowAction.Uninstall --- x64:{Environment.Is64BitProcess}");

			if (CloningRequired())
			{
				return UnregisterWow();
			}

			return SUCCESS;
		}


		private int UnregisterWow()
		{
			logger.WriteLine("deleting CLSID clone");
			using (var key = Registry.ClassesRoot.OpenSubKey(@"WOW6432Node\CLSID", true))
			{
				if (key != null)
				{
					key.DeleteSubKeyTree(RegistryHelper.OneNoteID, false);
					key.DeleteSubKey(RegistryHelper.OneNoteID, false);
				}
				else
				{
					logger.WriteLine("CLSID clone note found");
				}
			}

			return SUCCESS;
		}



		// Determines if 32-bit OneNote is installed.
		// When this is true, the guid will exist under ClassesRoot\CLSID however the path
		// will be blank and instead the the path is under ClassesRoot\Wow6432Node\CLSID\{guid}
		private bool CloningRequired()
		{
			string clsid = null;
			using (var key = Registry.ClassesRoot.OpenSubKey(@"\OneNote.Application\CLSID"))
			{
				if (key != null)
				{
					clsid = (string)key.GetValue(string.Empty); // default value
				}
			}

			string path = null;
			if (!string.IsNullOrEmpty(clsid))
			{
				using (var key = Registry.ClassesRoot
					.OpenSubKey($@"CLSID\{clsid}\Localserver32"))
				{
					if (key != null)
					{
						path = (string)key.GetValue(string.Empty); // default value
					}
				}

				if (path == null)
				{
					using (var key = Registry.ClassesRoot
						.OpenSubKey($@"WOW6432Node\CLSID\{clsid}\Localserver32"))
					{
						if (key != null)
						{
							path = (string)key.GetValue(string.Empty); // default value
						}
					}
				}
			}

			if (path == null)
			{
				logger.WriteLine("OneNote application path not found; continuing optimistically");
				return true;
			}
			else if (path.Contains(@"\Program Files (x86)\"))
			{
				logger.WriteLine("detected 32-bit install");
				return true;
			}

			logger.WriteLine("detected 64-bit install");
			return false;
		}
	}
}
