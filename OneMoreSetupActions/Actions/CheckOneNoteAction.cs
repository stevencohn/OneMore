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
		private string rootPath;


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
				using (var sub = key.OpenSubKey("CLSID", false))
				{
					if (sub == null)
					{
						logger.WriteLine("error finding subkey CLSID");
						return FAILURE;
					}

					onenoteCLSID = (string)sub.GetValue(string.Empty);
					logger.WriteLine($"found CLSID {onenoteCLSID}");
				}

				// onenoteCurVer
				// [HKEY_CLASSES_ROOT\OneNote.Application\CurVer]
				// @="OneNote.Application.15"
				using (var sub = key.OpenSubKey("CurVer", false))
				{
					if (sub == null)
					{
						logger.WriteLine("error finding subkey CurVer");
						return FAILURE;
					}

					onenoteCurVer = (string)sub.GetValue(string.Empty);
					logger.WriteLine($"found CurVer {onenoteCurVer}");
				}
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
			var key = root == "CLSID"
				? Registry.ClassesRoot.OpenSubKey(path, false)
				: Registry.LocalMachine.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return FAILURE;
			}

			logger.WriteLine($"checking {key}");

			try
			{
				// rootPath
				// [HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\LocalServer32]
				// @="C:\\Program Files\\Microsoft Office\\Root\\Office16\\ONENOTE.EXE"
				var p = "LocalServer32";
				using (var sub = key.OpenSubKey(p, false))
				{
					if (sub == null)
					{
						logger.WriteLine($"error finding subkey {p}");
						return FAILURE;
					}

					rootPath = (string)sub.GetValue(string.Empty);
					if (string.IsNullOrWhiteSpace(rootPath) ||
						!File.Exists(rootPath))
					{
						logger.WriteLine($"error confirming LocalServer32 ({rootPath ?? "null"})");
						return FAILURE;
					}

					logger.WriteLine($"verified LocalServer32 {rootPath}");
				}

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\ProgID]
				//@="OneNote.Application.15"
				p = "ProgID";
				using (var sub = key.OpenSubKey(p, false))
				{
					if (sub == null)
					{
						logger.WriteLine($"error finding subkey {p}");
						return FAILURE;
					}

					var progID = (string)sub.GetValue(string.Empty);
					if (string.IsNullOrWhiteSpace(progID) ||
						progID != onenoteCurVer)
					{
						logger.WriteLine($"error confirming ProgID ({progID ?? "null"})");
						return FAILURE;
					}

					logger.WriteLine($"verified ProgID as {progID}");
				}

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\TypeLib]
				//@="{0EA692EE-BB50-4E3C-AEF0-356D91732725}"
				p = "TypeLib";
				using (var sub = key.OpenSubKey(p, false))
				{
					if (sub == null)
					{
						logger.WriteLine($"error finding subkey {p}");
						return FAILURE;
					}

					var typeLib = (string)sub.GetValue(string.Empty);
					if (string.IsNullOrWhiteSpace(typeLib))
					{
						logger.WriteLine($"error confirming TypeLib (null)");
						return FAILURE;
					}

					if (root == "CLSID")
					{
						onenoteTypeLib = typeLib;
						logger.WriteLine($"found TypeLib as {onenoteTypeLib}");
					}
					else if (typeLib != onenoteTypeLib)
					{
						logger.WriteLine($"error confirming TypeLib ({typeLib}) as {onenoteTypeLib}");
						return FAILURE;
					}
					else
					{
						logger.WriteLine($"verified TypeLib as {onenoteTypeLib}");
					}
				}

				//[HKEY_CLASSES_ROOT\CLSID\{DC67E480-C3CB-49F8-8232-60B0C2056C8E}\VersionIndependentProgID]
				//@="OneNote.Application"
				p = "VersionIndependentProgID";
				using (var sub = key.OpenSubKey(p, false))
				{
					if (sub == null)
					{
						logger.WriteLine($"error finding subkey {p}");
						return FAILURE;
					}

					var progID = (string)sub.GetValue(string.Empty);
					if (string.IsNullOrWhiteSpace(progID))
					{
						logger.WriteLine($"error finding VersionIndependentProgID ({progID ?? "null"})");
						return FAILURE;
					}

					var path2 = $"{progID}\\CurVer";
					using (var key2 = Registry.ClassesRoot.OpenSubKey(path2, false))
					{
						if (key2 == null)
						{
							logger.WriteLine($"error finding key {path2}");
							return FAILURE;
						}

						var ver = (string)key2.GetValue(string.Empty);
						if (string.IsNullOrWhiteSpace(ver) ||
							ver != onenoteCurVer)
						{
							logger.WriteLine($"error confirming VersionIndependentProgID ({ver ?? "null"} != {onenoteCurVer})");
							return FAILURE;
						}

						logger.WriteLine($"verified VersionIndependentProgID as {ver}");
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
				using (var sub = key.OpenSubKey(p))
				{
					if (sub == null)
					{
						logger.WriteLine($"error finding subkey {p}");
						return FAILURE;
					}

					var clsid = (string)sub.GetValue(string.Empty);
					if (string.IsNullOrWhiteSpace(rootPath) ||
						!File.Exists(rootPath))
					{
						logger.WriteLine($"error confirming CLSID ({clsid ?? "null"})");
						return FAILURE;
					}

					logger.WriteLine($"verified CLSID {clsid}");
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
							// AssemblyName doesn't return KeyPair so just parse the string
							var matches = Regex.Matches(pia, @"([^,].*), Version=([^,].*), .+?PublicKeyToken=(.*)");
							if (matches.Count != 1 || matches[0].Groups.Count != 4)
							{
								logger.WriteLine($"error parsing PrimaryInteropAssemblyName {pia}");
								return FAILURE;
							}

							var anam = matches[0].Groups[1].Value;
							var aver = matches[0].Groups[2].Value;
							var akey = matches[0].Groups[3].Value;

							var gacPath = Path.Combine(
								Environment.GetEnvironmentVariable("windir"),
								@"assembly\\GAC_MSIL", anam, $"{aver}__{akey}",
								$"{anam}.dll");

							if (!File.Exists(gacPath))
							{
								logger.WriteLine($"error finding GAC assembly {gacPath}");
								return FAILURE;
							}

							logger.WriteLine($"verified {gacPath}");
						}
						else
						{
							//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1]
							//@="Microsoft OneNote 15.0 Object Library"
							var value = (string)sub.GetValue(string.Empty);
							if (Regex.Match(value, @"Microsoft OneNote .+ Object Library").Success)
							{
								//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1\0\Win32]
								//@="C:\\Program Files\\Microsoft Office\\Root\\Office16\\ONENOTE.EXE\\3"
								using (var key32 = sub.OpenSubKey("0\\Win32"))
								{
									if (key32 == null)
									{
										logger.WriteLine($@"warning did not find key HKCR:\\{sub.Name}\0\Win32");
									}
									else
									{
										value = (string)key32.GetValue(string.Empty);
										if (string.IsNullOrWhiteSpace(value) ||
											// strip off trailing "\3"
											!File.Exists(Path.GetDirectoryName(value)))
										{
											logger.WriteLine($@"warning did not find Win32 path {value}");
										}
										else
										{
											logger.WriteLine($"verified Win32 Object Library {value}");
											foundLib = true;
										}
									}
								}

								//[HKEY_CLASSES_ROOT\TypeLib\{0EA692EE-BB50-4E3C-AEF0-356D91732725}\1.1\0\win64]
								//@="C:\\Program Files\\Microsoft Office\\root\\Office16\\ONENOTE.EXE\\3"
								using (var key64 = sub.OpenSubKey("0\\win64"))
								{
									if (key64 == null)
									{
										logger.WriteLine($@"warning did not find key HKCR:\\{sub.Name}\0\win64");
									}
									else
									{
										value = (string)key64.GetValue(string.Empty);
										if (string.IsNullOrWhiteSpace(value) ||
											// strip off trailing "\3"
											!File.Exists(Path.GetDirectoryName(value)))
										{
											logger.WriteLine($@"warning did not find win64 path {value}");
										}
										else
										{
											logger.WriteLine($"verified win64 Object Library {value}");
											foundLib = true;
										}
									}
								}
							}
						}
					}
				}

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
