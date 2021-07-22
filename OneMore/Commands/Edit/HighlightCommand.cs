//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class HighlightCommand : Command
	{

		public HighlightCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var updated = false;
				var index = 0;

				var meta = page.GetMetaContent(Page.HighlightMetaName);
				if (meta != null)
				{
					if (int.TryParse(meta, out index))
					{
						index = index < 4 ? index + 1 : 0;
					}
				}

				var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
				var color = GetColor(index, dark);

				updated = page.EditSelected((s) =>
				{
					if (s is XText text)
					{
						return new XElement("span", new XAttribute("style", $"background:{color}"), text);
					}

					var span = (XElement)s;
					span.GetAttributeValue("style", out var style, string.Empty);

					if (!string.IsNullOrEmpty(style))
						span.SetAttributeValue("style", $"{style};background:{color}");
					else
						span.SetAttributeValue("style", $"background:{color}");

					return span;
				});

				if (updated)
				{
					page.SetMeta(Page.HighlightMetaName, index.ToString(CultureInfo.InvariantCulture));
					await one.Update(page);
				}
			}
		}


		private string GetColor(int index, bool dark)
		{
			if (dark)
			{
				switch (index)
				{
					case 1: return "#008000";   // Dark Green
					case 2: return "#00B0F0";   // Turquoise
					case 3: return "#800080";   // Dark Purple
					case 4: return "#0000FF";   // Blue
					default: return "#808000";  // Dark Yellow
				}
			}

			var theme = new SettingsProvider()
				.GetCollection("HighlightsSheet")?.Get<string>("theme");

			if (theme == "Faded")
			{
				switch (index)
				{
					case 1: return "#CCFFCC";   // Light Green
					case 2: return "#CCFFFF";   // Sky Blue
					case 3: return "#FF99CC";   // Pink
					case 4: return "#99CCFF";   // Light Blue
					default: return "#FFFF99";  // Light Yellow
				}
			}

			if (theme == "Deep")
			{
				switch (index)
				{
					case 1: return "#92D050";   // Lime
					case 2: return "#33CCCC";   // Teal
					case 3: return "#CC99FF";   // Lavender
					case 4: return "#00B0F0";   // Turquoise
					default: return "#FFC000";  // Gold
				}
			}

			// default theme "Normal"
			switch (index)
			{
				case 1: return "#00FF00";   // Light Green
				case 2: return "#00FFFF";   // Sky Blue
				case 3: return "#FF00CC";   // Pink
				case 4: return "#0000FF";   // Light Blue
				default: return "#FFFF00";  // Light Yellow
			}
		}
	}
}
