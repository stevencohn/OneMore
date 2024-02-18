#pragma warning disable S101 // Types should be named in PascalCase
#pragma warning disable S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
#pragma warning disable S1854 // Unused assignments should be removed

namespace OneMoreService
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Runtime.InteropServices;


	internal static class ServiceNative
	{
		public struct ParentProcessUtilities
		{
			internal IntPtr Reserved1;
			internal IntPtr PebBaseAddress;
			internal IntPtr Reserved2_0;
			internal IntPtr Reserved2_1;
			internal IntPtr UniqueProcessId;
			internal IntPtr InheritedFromUniqueProcessId;

			[DllImport("ntdll.dll")]
			private static extern int NtQueryInformationProcess(
				IntPtr processHandle,
				int processInformationClass,
				ref ParentProcessUtilities processInformation,
				int processInformationLength,
				out int returnLength);

			public static Process GetParentProcess()
			{
				return GetParentProcess(Process.GetCurrentProcess().Handle);
			}

			public static Process GetParentProcess(int id)
			{
				Process process = Process.GetProcessById(id);
				return GetParentProcess(process.Handle);
			}

			public static Process GetParentProcess(IntPtr handle)
			{
				ParentProcessUtilities pbi = new ParentProcessUtilities();
				int returnLength;
				int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
				if (status != 0)
					throw new Win32Exception(status);

				try
				{
					return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
				}
				catch (ArgumentException)
				{
					// Parent process not found
					return null;
				}
			}
		}


		public const int NotAllAssigned = 1300; // ERROR_NOT_ALL_ASSIGNED 

		// Constants that are going to be used during our procedure.
		public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
		public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
		public const uint STANDARD_RIGHTS_READ = 0x00020000;
		public const uint TOKEN_ASSIGN_PRIMARY = 0x00000001;
		public const uint TOKEN_DUPLICATE = 0x00000002;
		public const uint TOKEN_IMPERSONATE = 0x00000004;
		public const uint TOKEN_QUERY = 0x00000008;
		public const uint TOKEN_QUERY_SOURCE = 0x00000010;
		public const uint TOKEN_ADJUST_PRIVILEGES = 0x00000020;
		public const uint TOKEN_ADJUST_GROUPS = 0x00000040;
		public const uint TOKEN_ADJUST_DEFAULT = 0x00000080;
		public const uint TOKEN_ADJUST_SESSIONID = 0x00000100;
		public const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;

		public const uint TOKEN_ALL_ACCESS =
			STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE |
			TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS |
			TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;

		public const uint MAXIMUM_ALLOWED = 0x02000000;

		public const uint DETACHED_PROCESS = 0x00000008;
		public const uint CREATE_NEW_CONSOLE = 0x00000010;


		public enum SECURITY_IMPERSONATION_LEVEL
		{
			SecurityAnonymous,
			SecurityIdentification,
			SecurityImpersonation,
			SecurityDelegation
		}

		public enum TOKEN_TYPE
		{
			TokenPrimary = 1,
			TokenImpersonation
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct LocallyUniqueIdentifier // LUID
		{
			internal int LowPart;
			internal uint HighPart;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct TOKEN_PRIVILEGES
		{
			internal int PrivilegeCount;
			internal LocallyUniqueIdentifier Luid;
			internal uint Attributes;
		}

		// This also works with CharSet.Ansi as long as the calling function uses the same character set.
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct STARTUPINFO
		{
			public int cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public int dwX;
			public int dwY;
			public int dwXSize;
			public int dwYSize;
			public int dwXCountChars;
			public int dwYCountChars;
			public int dwFillAttribute;
			public int dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}

		public enum LogonFlags
		{
			WithProfile = 1,
			NetCredentialsOnly
		}

		public enum CreationFlags
		{
			DefaultErrorMode = 0x04000000,
			DetachedProcess = 0x00000008,
			NewConsole = 0x00000010,
			NewProcessGroup = 0x00000200,
			SeparateWOWVDM = 0x00000800,
			Suspended = 0x00000004,
			UnicodeEnvironment = 0x00000400,
			ExtendedStartupInfoPresent = 0x00080000
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ProfileInfo
		{
			/// Specifies the size of the structure, in bytes.
			public int dwSize;

			/// This member can be one of the following flags: PI_NOUI or PI_APPLYPOLICY
			public int dwFlags;

			/// Pointer to the name of the user. 
			/// This member is used as the base name of the directory in which to store a new profile.
			public string lpUserName;

			/// Pointer to the roaming user profile path. 
			/// If the user does not have a roaming profile, this member can be NULL.
			public string lpProfilePath;

			/// Pointer to the default user profile path. This member can be NULL. 
			public string lpDefaultPath;

			/// Pointer to the name of the validating domain controller, in NetBIOS format. 
			/// If this member is NULL, the Windows NT 4.0-style policy will not be applied. 
			public string lpServerName;

			/// Pointer to the path of the Windows NT 4.0-style policy file. This member can be NULL. 
			public string lpPolicyPath;

			/// Handle to the HKEY_CURRENT_USER registry key. 
			public IntPtr hProfile;
		}


		public enum SecurityEntity
		{
			// enum names equate exactly to their relevant privilege names, making it easy
			// to ToString each enum to lookup the LUID of the related privilege

			SeAssignPrimaryTokenPrivilege,      // SE_ASSIGNPRIMARYTOKEN_NAME
			SeAuditPrivilege,                   // SE_AUDIT_NAME
			SeBackupPrivilege,                  // SE_BACKUP_NAME
			SeChangeNotifyPrivilege,            // SE_CHANGE_NOTIFY_NAME
			SeCreateGlobalPrivilege,            // SE_CREATE_GLOBAL_NAME
			SeCreatePagefilePrivilege,          // SE_CREATE_PAGEFILE_NAME
			SeCreatePermanentPrivilege,         // SE_CREATE_PERMANENT_NAME
			SeCreateSymbolicLinkPrivilege,      // SE_CREATE_SYMBOLIC_LINK_NAME
			SeCreateTokenPrivilege,             // SE_CREATE_TOKEN_NAME
			SeDebugPrivilege,                   // SE_DEBUG_NAME
			SeEnableDelegationPrivilege,        // SE_ENABLE_DELEGATION_NAME
			SeImpersonatePrivilege,             // SE_IMPERSONATE_NAME
			SeIncreaseBasePriorityPrivilege,    // SE_INC_BASE_PRIORITY_NAME
			SeIncreaseQuotaPrivilege,           // SE_INCREASE_QUOTA_NAME
			SeIncreaseWorkingSetPrivilege,      // SE_INC_WORKING_SET_NAME
			SeLoadDriverPrivilege,              // SE_LOAD_DRIVER_NAME
			SeLockMemoryPrivilege,              // SE_LOCK_MEMORY_NAME
			SeMachineAccountPrivilege,          // SE_MACHINE_ACCOUNT_NAME
			SeManageVolumePrivilege,            // SE_MANAGE_VOLUME_NAME
			SeProfileSingleProcessPrivilege,    // SE_PROF_SINGLE_PROCESS_NAME
			SeRelabelPrivilege,                 // SE_RELABEL_NAME
			SeRemoteShutdownPrivilege,          // SE_REMOTE_SHUTDOWN_NAME
			SeRestorePrivilege,                 // SE_RESTORE_NAME
			SeSecurityPrivilege,                // SE_SECURITY_NAME
			SeShutdownPrivilege,                // SE_SHUTDOWN_NAME
			SeSyncAgentPrivilege,               // SE_SYNC_AGENT_NAME
			SeSystemEnvironmentPrivilege,       // SE_SYSTEM_ENVIRONMENT_NAME
			SeSystemProfilePrivilege,           // SE_SYSTEM_PROFILE_NAME
			SeSystemtimePrivilege,              // SE_SYSTEMTIME_NAME
			SeTakeOwnershipPrivilege,           // SE_TAKE_OWNERSHIP_NAME
			SeTcbPrivilege,                     // SE_TCB_NAME
			SeTimeZonePrivilege,                // SE_TIME_ZONE_NAME
			SeTrustedCredManAccessPrivilege,    // SE_TRUSTED_CREDMAN_ACCESS_NAME
			SeUndockPrivilege                   // SE_UNDOCK_NAME
		}

		#region DllImports
		[DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool CreateProcessWithTokenW(IntPtr hToken, LogonFlags dwLogonFlags, string lpApplicationName, string lpCommandLine,
			CreationFlags dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool CreateProcessAsUserW(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
			bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll")]
		public static extern uint WTSGetActiveConsoleSessionId();

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LocallyUniqueIdentifier lpLuid);


		[DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool LoadUserProfile(IntPtr hToken, ref ProfileInfo lpProfileInfo);


		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AdjustTokenPrivileges(IntPtr tokenhandle,
								 [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
								 [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES newstate,
								 uint bufferlength, IntPtr previousState, IntPtr returnlength);
		// OpenProcessToken
		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool OpenThreadToken(IntPtr ThreadHandle, uint DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);


		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);


		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public extern static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes,
			SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool SetThreadToken(IntPtr pHandle, IntPtr hToken);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentThread();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern Boolean CloseHandle(IntPtr hObject);

		[DllImport("userenv.dll", SetLastError = true)]
		public static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

		#endregion DllImports


		public static bool RequestPrivilege(SecurityEntity entity)
		{
			var name = entity.ToString();

			try
			{
				var luid = new LocallyUniqueIdentifier();
				if (!LookupPrivilegeValue(null, name, ref luid))
				{
					throw new InvalidOperationException(
						$"LookupPrivilegeValue failed, SecurityEntity:{name}",
						new Win32Exception());
				}

				var handle = IntPtr.Zero;

				try
				{
					var process = GetCurrentProcess();
					if (!OpenProcessToken(process,
						TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
						out handle))
					{
						throw new InvalidOperationException(
							$"OpenProcessToken failed, CurrentProcess:{process.ToInt32()}",
							new Win32Exception());
					}

					var privileges = new TOKEN_PRIVILEGES
					{
						PrivilegeCount = 1,
						Attributes = SE_PRIVILEGE_ENABLED,
						Luid = luid
					};

					if (!AdjustTokenPrivileges(
						handle, false, ref privileges, 1024, IntPtr.Zero, IntPtr.Zero))
					{
						throw new InvalidOperationException(
							"AdjustTokenPrivileges failed", new Win32Exception());
					}

					var lastError = Marshal.GetLastWin32Error();
					if (lastError == NotAllAssigned)
					{
						// user does not have requested privileges
						return false;
					}
					else if (lastError != 0)
					{
						throw new InvalidOperationException(
							$"AdjustTokenPrivileges failed (last error:{lastError}",
							new Win32Exception());
					}

					return true;

				}
				finally
				{
					if (handle != IntPtr.Zero)
					{
						CloseHandle(handle);
					}
				}
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					$"GrandPrivilege failed. SecurityEntity: {name}", e);
			}
		}
	}
}