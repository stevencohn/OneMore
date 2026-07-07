//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using Newtonsoft.Json;


	internal class HighlightColorScheme
	{
		public List<string> Colors { get; set; }
		public List<string> Extended { get; set; }
	}


	internal class HighlightColorSchemes
	{
		private const string ColorSchemesJson =
			@"{
  ""Bright"": {
    ""colors"": [ ""#FFFF00"", ""#00FF00"", ""#00FFFF"", ""#FF00FF"", ""#0000FF"" ],
    ""extended"": [ ""#FF0000"", ""#000080"", ""#32CD32"", ""#00CED1"", ""#9400D3"" ]
  },
  ""Faded"": {
    ""colors"": [ ""#FFFF99"", ""#CCFFCC"", ""#CCFFFF"", ""#FF99CC"", ""#99CCFF"" ],
    ""extended"": [ ""#B8CCE4"", ""#E5B9B7"", ""#D7E3BC"", ""#CCC1D9"", ""#FBD5B5"" ]
  },
  ""Deep"": {
    ""colors"": [ ""#FFC000"", ""#92D050"", ""#33CCCC"", ""#CC99FF"", ""#00B0F0"" ],
    ""extended"": [ ""#407FFF"", ""#AA6CEC"", ""#FF6D6D"", ""#71A43E"", ""#FF8444"" ]
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


		public string GetColor(string name, int index)
		{
			var scheme = GetScheme(name);
			if (scheme is not null)
			{
				if (index >= 0 && index < scheme.Colors.Count)
				{
					return scheme.Colors[index];
				}
				else if (index < scheme.Colors.Count + scheme.Extended.Count)
				{
					return scheme.Extended[index - scheme.Colors.Count];
				}
			}

			return null;
		}
	}
}