//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using Newtonsoft.Json;


	internal class HighlightColorSet
	{
		public List<string> Colors { get; set; }
		public List<string> Extended { get; set; }
	}


	/// <summary>
	/// A named scheme with colors for light pages and, as the colors OneNote should render,
	/// for dark pages. Basic colors rotate first; extended colors follow when enabled.
	/// </summary>
	internal class HighlightColorScheme : HighlightColorSet
	{
		public HighlightColorSet Dark { get; set; }


		public HighlightColorSet GetSet(bool dark)
		{
			return dark && Dark is not null ? Dark : this;
		}
	}


	internal class HighlightColorSchemes
	{
		// Light colors are stored as is. Dark colors are the colors we want rendered on a dark
		// page; callers store ColorPalette.ToLight(color) so OneNote's inversion yields them.
		// Extended colors fill hue gaps (and neutral gray) so all ten stay distinguishable.
		private const string ColorSchemesJson =
			@"{
  ""Bright"": {
    ""colors"": [ ""#FFFF00"", ""#00FF00"", ""#00FFFF"", ""#FF00FF"", ""#6060FF"" ],
    ""extended"": [ ""#FF0000"", ""#00AAFF"", ""#FF8000"", ""#FF0080"", ""#C0C0C0"" ],
    ""dark"": {
      ""colors"": [ ""#7B7B00"", ""#008B00"", ""#008585"", ""#D200D2"", ""#6161FF"" ],
      ""extended"": [ ""#EF0000"", ""#007DBC"", ""#BA5E00"", ""#E80074"", ""#777777"" ]
    }
  },
  ""Faded"": {
    ""colors"": [ ""#FFFF99"", ""#CCFFCC"", ""#CCFFFF"", ""#FF99CC"", ""#99CCFF"" ],
    ""extended"": [ ""#FFCC99"", ""#CC99FF"", ""#FF9999"", ""#C0C0C0"", ""#E0B8E0"" ],
    ""dark"": {
      ""colors"": [ ""#5C5C14"", ""#166816"", ""#156363"", ""#A52364"", ""#205B97"" ],
      ""extended"": [ ""#804E1B"", ""#7A2BC9"", ""#AB2424"", ""#595959"", ""#992099"" ]
    }
  },
  ""Deep"": {
    ""colors"": [ ""#FFC000"", ""#92D050"", ""#33CCCC"", ""#CC99FF"", ""#00B0F0"" ],
    ""extended"": [ ""#FF5050"", ""#4F6BED"", ""#8C8C8C"", ""#E84CA8"", ""#A0522D"" ],
    ""dark"": {
      ""colors"": [ ""#84670F"", ""#44770D"", ""#0D7777"", ""#9037E9"", ""#117396"" ],
      ""extended"": [ ""#D31717"", ""#3D5CE9"", ""#6A6A6A"", ""#C91680"", ""#B54814"" ]
    }
  }
}";


		private readonly Dictionary<string, HighlightColorScheme> colorSchemes;


		public HighlightColorSchemes()
		{

			colorSchemes = JsonConvert
				.DeserializeObject<Dictionary<string, HighlightColorScheme>>(ColorSchemesJson);
		}


		public Dictionary<string, HighlightColorScheme> Schemes => colorSchemes;


		public HighlightColorScheme GetScheme(string name)
		{
			if (colorSchemes.TryGetValue(name, out var scheme))
			{
				return scheme;
			}

			return null;
		}


		public string GetColor(string name, int index, bool dark = false)
		{
			var scheme = GetScheme(name)?.GetSet(dark);
			if (scheme is not null)
			{
				if (index >= 0 && index < scheme.Colors.Count)
				{
					return scheme.Colors[index];
				}
				else if (index >= 0 && index < scheme.Colors.Count + scheme.Extended.Count)
				{
					return scheme.Extended[index - scheme.Colors.Count];
				}
			}

			return null;
		}
	}
}
