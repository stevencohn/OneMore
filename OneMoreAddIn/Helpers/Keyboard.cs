//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;


	public static class Keyboard
	{
		#region Interop

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct INPUT
		{
			public uint Type;
			public MOUSEKEYBDHARDWAREINPUT Data;
		}

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct MOUSEKEYBDHARDWAREINPUT
		{
			[FieldOffset(0)]
			public HARDWAREINPUT Hardware;
			[FieldOffset(0)]
			public KEYBDINPUT Keyboard;
			[FieldOffset(0)]
			public MOUSEINPUT Mouse;
		}

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct HARDWAREINPUT
		{
			public uint Msg;
			public ushort ParamL;
			public ushort ParamH;
		}

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct KEYBDINPUT
		{
			public ushort Vk;
			public ushort Scan;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		/// <summary>
		/// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct MOUSEINPUT
		{
			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		public enum KeyCode : ushort
		{
			// media

			MEDIA_NEXT_TRACK = 0xb0,
			MEDIA_PLAY_PAUSE = 0xb3,
			MEDIA_PREV_TRACK = 0xb1,
			MEDIA_STOP = 0xb2,

			// math

			ADD = 0x6b,
			MULTIPLY = 0x6a,
			DIVIDE = 0x6f,
			SUBTRACT = 0x6d,

			// Browser
			BROWSER_BACK = 0xa6,
			BROWSER_FAVORITES = 0xab,
			BROWSER_FORWARD = 0xa7,
			BROWSER_HOME = 0xac,
			BROWSER_REFRESH = 0xa8,
			BROWSER_SEARCH = 170,
			BROWSER_STOP = 0xa9,

			// Numpad numbers

			NUMPAD0 = 0x60,
			NUMPAD1 = 0x61,
			NUMPAD2 = 0x62,
			NUMPAD3 = 0x63,
			NUMPAD4 = 100,
			NUMPAD5 = 0x65,
			NUMPAD6 = 0x66,
			NUMPAD7 = 0x67,
			NUMPAD8 = 0x68,
			NUMPAD9 = 0x69,

			// region Fkeys

			F1 = 0x70,
			F10 = 0x79,
			F11 = 0x7a,
			F12 = 0x7b,
			F13 = 0x7c,
			F14 = 0x7d,
			F15 = 0x7e,
			F16 = 0x7f,
			F17 = 0x80,
			F18 = 0x81,
			F19 = 130,
			F2 = 0x71,
			F20 = 0x83,
			F21 = 0x84,
			F22 = 0x85,
			F23 = 0x86,
			F24 = 0x87,
			F3 = 0x72,
			F4 = 0x73,
			F5 = 0x74,
			F6 = 0x75,
			F7 = 0x76,
			F8 = 0x77,
			F9 = 120,

			// Other

			OEM_1 = 0xba,
			OEM_102 = 0xe2,
			OEM_2 = 0xbf,
			OEM_3 = 0xc0,
			OEM_4 = 0xdb,
			OEM_5 = 220,
			OEM_6 = 0xdd,
			OEM_7 = 0xde,
			OEM_8 = 0xdf,
			OEM_CLEAR = 0xfe,
			OEM_COMMA = 0xbc,
			OEM_MINUS = 0xbd,
			OEM_PERIOD = 190,
			OEM_PLUS = 0xbb,

			// KEYS

			KEY_0 = 0x30,
			KEY_1 = 0x31,
			KEY_2 = 50,
			KEY_3 = 0x33,
			KEY_4 = 0x34,
			KEY_5 = 0x35,
			KEY_6 = 0x36,
			KEY_7 = 0x37,
			KEY_8 = 0x38,
			KEY_9 = 0x39,
			KEY_A = 0x41,
			KEY_B = 0x42,
			KEY_C = 0x43,
			KEY_D = 0x44,
			KEY_E = 0x45,
			KEY_F = 70,
			KEY_G = 0x47,
			KEY_H = 0x48,
			KEY_I = 0x49,
			KEY_J = 0x4a,
			KEY_K = 0x4b,
			KEY_L = 0x4c,
			KEY_M = 0x4d,
			KEY_N = 0x4e,
			KEY_O = 0x4f,
			KEY_P = 80,
			KEY_Q = 0x51,
			KEY_R = 0x52,
			KEY_S = 0x53,
			KEY_T = 0x54,
			KEY_U = 0x55,
			KEY_V = 0x56,
			KEY_W = 0x57,
			KEY_X = 0x58,
			KEY_Y = 0x59,
			KEY_Z = 90,

			// volume

			VOLUME_DOWN = 0xae,
			VOLUME_MUTE = 0xad,
			VOLUME_UP = 0xaf,

			// TheRest

			SNAPSHOT = 0x2c,
			RightClick = 0x5d, // right of space between Alt and left-arrow
			BACKSPACE = 8,
			CANCEL = 3,
			CAPS_LOCK = 20,
			CONTROL = 0x11,
			ALT = 18,
			DECIMAL = 110,
			DELETE = 0x2e,
			DOWN = 40,
			END = 0x23,
			ESC = 0x1b,
			HOME = 0x24,
			INSERT = 0x2d,
			LAUNCH_APP1 = 0xb6,
			LAUNCH_APP2 = 0xb7,
			LAUNCH_MAIL = 180,
			LAUNCH_MEDIA_SELECT = 0xb5,
			LCONTROL = 0xa2,
			LEFT = 0x25,
			LSHIFT = 160,
			LWIN = 0x5b,
			PAGEDOWN = 0x22,
			NUMLOCK = 0x90,
			PAGE_UP = 0x21,
			RCONTROL = 0xa3,
			ENTER = 13,
			RIGHT = 0x27,
			RSHIFT = 0xa1,
			RWIN = 0x5c,
			SHIFT = 0x10,
			SPACE_BAR = 0x20,
			TAB = 9,
			UP = 0x26
		}
		#endregion Interop


		public static void Press(KeyCode[] keys)
		{
			var inputs = new INPUT[keys.Length * 2];

			for (int a = 0; a < keys.Length; ++a)
			{
				for (int b = 0; b < 2; ++b)
				{
					inputs[(b == 0) ? a : inputs.Length - 1 - a].Type = 1;
					inputs[(b == 0) ? a : inputs.Length - 1 - a].Data.Keyboard = new KEYBDINPUT()
					{
						Vk = (ushort)keys[a],
						Scan = 0,
						Flags = Convert.ToUInt32((b == 0) ? 0 : 2),
						Time = 0,
						ExtraInfo = IntPtr.Zero,
					};
				}
			}

			if (SendInput(Convert.ToUInt32(inputs.Length), inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
			{
				throw new Exception();
			}
		}
	}
}
