//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Linq;


	#region LinqPad
	/*
	var basicColors = new List<Color>
	{
		Color.FromArgb(255, 255, 128, 128),
		Color.FromArgb(255, 255, 255, 128),
		Color.FromArgb(255, 128, 255, 128),
		Color.FromArgb(255, 0, 255, 128),
		Color.FromArgb(255, 128, 255, 255),
		Color.FromArgb(255, 0, 128, 255),
		Color.FromArgb(255, 255, 128, 192),
		Color.FromArgb(255, 255, 128, 255),
		Color.FromArgb(255, 255, 0, 0),
		Color.FromArgb(255, 255, 255, 0),
		Color.FromArgb(255, 128, 255, 0),
		Color.FromArgb(255, 0, 255, 64),
		Color.FromArgb(255, 0, 255, 255),
		Color.FromArgb(255, 0, 128, 192),
		Color.FromArgb(255, 128, 128, 192),
		Color.FromArgb(255, 255, 0, 255),
		Color.FromArgb(255, 128, 64, 64),
		Color.FromArgb(255, 255, 128, 64),
		Color.FromArgb(255, 0, 255, 0),
		Color.FromArgb(255, 0, 128, 128),
		Color.FromArgb(255, 0, 64, 128),
		Color.FromArgb(255, 128, 128, 255),
		Color.FromArgb(255, 128, 0, 64),
		Color.FromArgb(255, 255, 0, 128),
		Color.FromArgb(255, 128, 0, 0),
		Color.FromArgb(255, 255, 128, 0),
		Color.FromArgb(255, 0, 128, 0),
		Color.FromArgb(255, 0, 128, 64),
		Color.FromArgb(255, 0, 0, 255),
		Color.FromArgb(255, 0, 0, 160),
		Color.FromArgb(255, 128, 0, 128),
		Color.FromArgb(255, 128, 0, 255),
		Color.FromArgb(255, 64, 0, 0),
		Color.FromArgb(255, 128, 64, 0),
		Color.FromArgb(255, 0, 64, 0),
		Color.FromArgb(255, 0, 64, 64),
		Color.FromArgb(255, 0, 0, 128),
		Color.FromArgb(255, 0, 0, 64),
		Color.FromArgb(255, 64, 0, 64),
		Color.FromArgb(255, 64, 0, 128),
		Color.FromArgb(255, 0, 0, 0),
		Color.FromArgb(255, 128, 128, 0),
		Color.FromArgb(255, 128, 128, 64),
		Color.FromArgb(255, 128, 128, 128),
		Color.FromArgb(255, 64, 128, 128),
		Color.FromArgb(255, 192, 192, 192),
		Color.FromArgb(255, 64, 0, 64),
		Color.FromArgb(255, 255, 255, 255)
	};
			
	foreach (var color in basicColors)
	{
		("0x"+ color.ToArgb().ToString("X6").Substring(2) + ",").Dump();
	}
	*/
	#endregion

	internal static class BasicColors
	{
		private static readonly int[] colors =
		{
			0xFF8080,
			0xFFFF80,
			0x80FF80,
			0x00FF80,
			0x80FFFF,
			0x0080FF,
			0xFF80C0,
			0xFF80FF,
			0xFF0000,
			0xFFFF00,
			0x80FF00,
			0x00FF40,
			0x00FFFF,
			0x0080C0,
			0x8080C0,
			0xFF00FF,
			0x804040,
			0xFF8040,
			0x00FF00,
			0x008080,
			0x004080,
			0x8080FF,
			0x800040,
			0xFF0080,
			0x800000,
			0xFF8000,
			0x008000,
			0x008040,
			0x0000FF,
			0x0000A0,
			0x800080,
			0x8000FF,
			0x400000,
			0x804000,
			0x004000,
			0x004040,
			0x000080,
			0x000040,
			0x400040,
			0x400080,
			0x000000,
			0x808000,
			0x808040,
			0x808080,
			0x408080,
			0xC0C0C0,
			0x400040,
			0xFFFFFF
		};


		public static Color BlackSmoke => Color.FromArgb(64, 64, 64);


		public static bool IsKnown (int candidate)
		{
			return colors.Contains(candidate);
		}
	}
}
