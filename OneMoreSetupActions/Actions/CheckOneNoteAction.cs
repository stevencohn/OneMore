//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Text.RegularExpressions;


	/// <summary>
	/// verifies the OneNote configuration to derisk OneMore not activating.
	/// </summary>
	internal class CheckOneNoteAction : CustomAction
	{
		private string onenoteCLSID;
		private string onenoteCurVer;
		private string onenoteTypeLib;


		public CheckOneNoteAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("CheckOneNoteAction.Install ---");

			if (VerifyOneNoteApplication() == SUCCESS &&
				VerifyRootClass("CLSID") == SUCCESS &&
				VerifyRootClass(@"WOW6432Node\CLSID") == SUCCESS &&
				VerifyRootClass(@"Software\Classes\CLSID") == SUCCESS &&
				VerifyCurVer() == SUCCESS &&
				VerifyTypeLib() == SUCCESS)
			{
				return SUCCESS;
			}

			logger.Indented = false;
			return FAILURE;
		}


		private int VerifyOneNoteApplication()
		{
			logger.WriteLine(nameof(VerifyOneNoteApplication) + "()");
			logger.Indented = true;

			// [HKEY_CLASSES_ROOT\OneNote.Application]
			// @= "Application Class"
			var path = "OneNote.Application";
			var key = Registry.ClassesRoot.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return FAILURE;
			}

			logger.WriteLine($"checking {key}");

			try
			{
				// onenoteCLSID
				// [HKEY_CLASSES_ROOT\OneNote.Application\CLSID]
				// @="{DC67E480-C3CB-49F8-8232-60B0C2056C8E}"
				onenoteCLSID = key.GetSubKeyPropertyValue("CLSID", string.Empty);
				if (onenoteCLSID == null)
				{
					return FAILURE;
				}

				logger.WriteLine($"found CLSID {onenoteCLSID}");

				// onenoteCurVer
				// [HKEY_CLASSES_ROOT\OneNote.Application\CurVer]
				// @="OneNote.Application.15"
				onenoteCurVer = key.GetSubKeyPropertyValue("CurVer", string.Empty);
				if (onenoteCurVer == null)
				{
					return FAILURE;
				}

				logger.WriteLine($"found CurVer {onenoteCurVer}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading OneNote.Application info");
				logger.WriteLine(exc);
				return FAILURE;
			}
			finally
			{
				key.Dispose();
			}

			logger.WriteLine("OK");
			logger.Indented = false;
			return SUCCESS;
		}


		private int VerifyRootClass(string root)
		{
			logger.WriteLine(nameof(VerifyRootClass) + $"({root})");
			logger.Indented = true;

			// [HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}]
			// @="Application2 Class"
			var path = $"{root}\\{onenoteCLSID}";
			var key = root.StartsWith("Software")
				? Registry.LocalMachine.OpenSubKey(path, false)
				: Registry.ClassesRoot.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return FAILURE;
			}

			logger.WriteLine($"checking {key}");

			try
			{
				// [HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\InprocServer32]
				// Assembly="Microsoft.Office.Interop.OneNote, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71E9BCE111E9429C"
				var p = "InprocServer32";
				var aname = key.GetSubKeyPropertyValue(p, "Assembly");
				if (aname == null)
				{
					return FAILURE;
				}

				if (VerifyGacAssembly(aname) != SUCCESS)
				{
					logger.WriteLine($"error confirming {p} '{aname}'");
					return FAILURE;
				}

				logger.WriteLine($"verified {p} {aname}");

				// remaining properties are WARNINGs/Informational...

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\ProgID]
				//@="OneNote.Application.15"
				p = "ProgID";
				var progID = key.GetSubKeyPropertyValue(p, string.Empty, false);
				if (progID == null)
				{
					logger.WriteLine($"warning finding subkey {key.Name}\\{p}");
				}
				else if (progID != onenoteCurVer)
				{
					logger.WriteLine($"warning confirming ProgID '{progID}' as {onenoteCurVer}");
				}
				else
				{
					logger.WriteLine($"verified ProgID as {progID}");
				}

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\TypeLib]
				//@="{0EA692EE-BB50-4E3C-AEF0-356D91732725}"
				p = "TypeLib";
				var typelib = key.GetSubKeyPropertyValue(p, string.Empty, false);
				if (progID == null)
				{
					logger.WriteLine($"warning finding subkey {key.Name}\\{p}");
				}
				else if (!root.StartsWith("Software"))
				{
					onenoteTypeLib = typelib;
					logger.WriteLine($"found TypeLib as {onenoteTypeLib}");
				}
				else if (typelib != onenoteTypeLib)
				{
					logger.WriteLine($"warning confirming TypeLib '{typelib}' as {onenoteTypeLib}");
				}
				else
				{
					logger.WriteLine($"verified TypeLib as {onenoteTypeLib}");
				}

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\VersionIndependentProgID]
				//@="OneNote.Application"
				p = "VersionIndependentProgID";
				var vip = key.GetSubKeyPropertyValue(p, string.Empty, false);
				if (string.IsNullOrWhiteSpace(vip))
				{
					logger.WriteLine($"warning finding subkey {key.Name}\\{p}");
				}
				else
				{
					var path2 = $"{vip}\\CurVer";
					using (var key2 = Registry.ClassesRoot.OpenSubKey(path2, false))
					{
						if (key2 == null)
						{
							logger.WriteLine($"warning finding key {path2}");
						}
						else
						{
							var ver = (string)key2.GetValue(string.Empty);
							if (string.IsNullOrWhiteSpace(ver) ||
								ver != onenoteCurVer)
							{
								logger.WriteLine($"warning confirming VersionIndependentProgID ({ver ?? "null"} != {onenoteCurVer})");
							}
							else
							{
								logger.WriteLine($"verified VersionIndependentProgID as {ver}");
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading CLSID info");
				logger.WriteLine(exc);
				return FAILURE;
			}
			finally
			{
				key.Dispose();
			}

			logger.WriteLine("OK");
			logger.Indented = false;
			return SUCCESS;
		}


		private int VerifyGacAssembly(string fullName)
		{
			// AssemblyName class doesn't expose token so just parse the string
			var matches = Regex.Matches(fullName, @"([^,].*), Version=([^,].*), .+?PublicKeyToken=(.*)");
			if (matches.Count != 1 || !matches[0].Success)
			{
				logger.WriteLine($"error parsing GAC assembly name {fullName}");
				return FAILURE;
			}

			var name = matches[0].Groups[1].Value;
			var version = matches[0].Groups[2].Value;
			var token = matches[0].Groups[3].Value;

			var path = Path.Combine(
				Environment.GetEnvironmentVariable("windir"),
				@"assembly\\GAC_MSIL", name, $"{version}__{token}",
				$"{name}.dll");

			if (!File.Exists(path))
			{
				return FAILURE;
			}

			return SUCCESS;
		}


		private int VerifyCurVer()
		{
			logger.WriteLine(nameof(VerifyCurVer) + "()");
			logger.Indented = true;

			//[HKEY_CLASSES_ROOT\OneNote.Application.15]
			//@= "Application2 Class"
			var path = onenoteCurVer;
			var key = Registry.ClassesRoot.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return FAILURE;
			}

			logger.WriteLine($"checking {key}");

			try
			{
				//[HKEY_CLASSES_ROOT\OneNote.Application.15\CLSID]
				//@= "{DC67E480-C3CB-49F8-8232-60B0C2056C8E}"
				var p = "CLSID";
				var clsid = key.GetSubKeyPropertyValue(p, string.Empty);
				if (clsid == null)
				{
					return FAILURE;
				}

				if (clsid != onenoteCLSID)
				{
					logger.WriteLine($"error confirming CLSID '{clsid}' as {onenoteCLSID}");
					return FAILURE;
				}

				logger.WriteLine($"verified CLSID {clsid}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading CurVer info");
				logger.WriteLine(exc);
				return FAILURE;
			}
			finally
			{
				key.Dispose();
			}

			logger.WriteLine("OK");
			logger.Indented = false;
			return SUCCESS;
		}


		private int VerifyTypeLib()
		{
			logger.WriteLine(nameof(VerifyTypeLib) + "()");
			logger.Indented = true;

			//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}]
			var path = $"TypeLib\\{onenoteTypeLib}";
			var key = Registry.ClassesRoot.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return FAILURE;
			}

			logger.WriteLine($"checking {key}");

			try
			{
				string p;
				var foundLib = false;

				// 1.0, 1.1, ...
				foreach (var subname in key.GetSubKeyNames())
				{
					using (var sub = key.OpenSubKey(subname, false))
					{
						//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.0]
						//"PrimaryInteropAssemblyName"="Microsoft.Office.Interop.OneNote, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71E9BCE111E9429C"
						// =>  C:\Windows\assembly\GAC_MSIL\Microsoft.Office.Interop.OneNote\15.0.0.0__71e9bce111e9429c
						var pia = (string)sub.GetValue("PrimaryInteropAssemblyName");
						if (!string.IsNullOrWhiteSpace(pia))
						{
							var status = VerifyGacAssembly(pia);
							if (status == FAILURE)
							{
								logger.WriteLine($"error finding GAC assembly {pia}");
							}
							else
							{
								logger.WriteLine($"verified {pia}");
							}
						}
						else
						{
							//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1]
							//@="Microsoft OneNote 15.0 Object Library"
							var value = (string)sub.GetValue(string.Empty);
							if (Regex.Match(value, @"Microsoft OneNote .+ (?:Object|Type) Library").Success)
							{
								//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1\0\Win32]
								//@="C:\\Program Files\\Microsoft Office\\Root\\Office16\\ONENOTE.EXE\\3"
								p = "0\\Win32";
								var win32 = sub.GetSubKeyPropertyValue(p, string.Empty, false);
								if (win32 == null)
								{
									logger.WriteLine($@"warning did not find key HKCR:\\{sub.Name}\{p}");
								}
								// strip off trailing "\3"
								else if (!File.Exists(Path.GetDirectoryName(win32)))
								{
									logger.WriteLine($@"warning did not find Win32 path {win32}");
								}
								else
								{
									logger.WriteLine($"verified Win32 Object Library {win32}");
									foundLib = true;
								}

								//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1\0\win64]
								//@="C:\\Program Files\\Microsoft Office\\root\\Office16\\ONENOTE.EXE\\3"
								p = "0\\Win32";
								var win64 = sub.GetSubKeyPropertyValue(p, string.Empty, false);
								if (win64 == null)
								{
									logger.WriteLine($@"warning did not find key HKCR:\\{sub.Name}\{p}");
								}
								// strip off trailing "\3"
								else if (!File.Exists(Path.GetDirectoryName(win64)))
								{
									logger.WriteLine($@"warning did not find win64 path {win64}");
								}
								else
								{
									logger.WriteLine($"verified win64 Object Library {win64}");
									foundLib = true;
								}
							}
						}
					}
				}

				// if neither Win32 nor win64 subkeys were found
				if (!foundLib)
				{
					logger.WriteLine($"error finding object library info under {path}");
					return FAILURE;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading CurVer info");
				logger.WriteLine(exc);
				return FAILURE;
			}
			finally
			{
				key.Dispose();
			}

			logger.WriteLine("OK");
			logger.Indented = false;
			return SUCCESS;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
