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


	/// <summary>
	/// Uses a rotating array of colors to highlight selected text.
	/// </summary>
	internal class HighlightCommand : Command
	{

		public HighlightCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			var updated = false;
			var index = 0;

			var meta = page.GetMetaContent(MetaNames.HighlightIndex);
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
				page.SetMeta(MetaNames.HighlightIndex, index.ToString(CultureInfo.InvariantCulture));
				await one.Update(page);
			}
		}


		private string GetColor(int index, bool dark)
		{
			if (dark)
			{
				return index switch
				{
					1 => "#008000",// Dark Green
					2 => "#00B0F0",// Turquoise
					3 => "#800080",// Dark Purple
					4 => "#0000FF",// Blue
					_ => "#808000",// Dark Yellow
				};
			}

			var theme = new SettingsProvider()
				.GetCollection(nameof(HighlightsSheet))?.Get<string>("theme");

			if (theme == "Faded")
			{
				return index switch
				{
					1 => "#CCFFCC",// Light Green
					2 => "#CCFFFF",// Sky Blue
					3 => "#FF99CC",// Pink
					4 => "#99CCFF",// Light Blue
					_ => "#FFFF99",// Light Yellow
				};
			}

			if (theme == "Deep")
			{
				return index switch
				{
					1 => "#92D050",// Lime
					2 => "#33CCCC",// Teal
					3 => "#CC99FF",// Lavender
					4 => "#00B0F0",// Turquoise
					_ => "#FFC000",// Gold
				};
			}

			// default theme "Normal"
			return index switch
			{
				1 => "#00FF00",// Light Green
				2 => "#00FFFF",// Sky Blue
				3 => "#FF00CC",// Pink
				4 => "#0000FF",// Light Blue
				_ => "#FFFF00",// Light Yellow
			};
		}
	}
}
