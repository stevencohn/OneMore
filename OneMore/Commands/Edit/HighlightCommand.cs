//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
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
			var increment = (int)args[0];

			using var one = new OneNote(out var page, out var ns);
			var updated = false;
			var index = 0;

			var meta = page.GetMetaContent(MetaNames.HighlightIndex);
			if (meta != null)
			{
				if (int.TryParse(meta, out index) && increment > 0)
				{
					index = index < 4 ? index + 1 : 0;
				}
			}

			var color = increment < 0
				? StyleBase.Transparent
				: GetColor(index, page.GetPageColor(out _, out _).GetBrightness() < 0.5);

			updated = page.EditSelected((s) =>
			{
				if (s is XText text)
				{
					return new XElement("span", new XAttribute("style", $"background:{color}"), text);
				}

				var span = (XElement)s;
				span.GetAttributeValue("style", out var css, string.Empty);

				if (string.IsNullOrEmpty(css))
				{
					span.SetAttributeValue("style", $"background:{color}");
				}
				else
				{
					// need to parse so we filter out mso-highlight attribute
					// setDefaults to false so it doesn't default font family and size
					var style = new Style(css, setDefaults: false)
					{
						// enable ApplyColors so it emits background attribute
						ApplyColors = true,
						Highlight = color
					};

					span.SetAttributeValue("style", style.ToCss(false));
				}

				return span;
			});

			if (updated)
			{
				if (increment > 0)
				{
					page.SetMeta(MetaNames.HighlightIndex, index.ToString(CultureInfo.InvariantCulture));
				}

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
