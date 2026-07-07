//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;


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

			await using var one = new OneNote(out var page, out var ns);
			var updated = false;
			var index = 0;

			var settings = new SettingsProvider().GetCollection(nameof(HighlightsSheet));
			var extended = settings.Get<bool>("extended");

			var schemeName = settings.Get<string>("theme") switch
			{
				"Faded" => "Faded",
				"Deep" => "Deep",
				_ => "Bright"
			};

			var scheme = new HighlightColorSchemes().GetScheme(schemeName);
			var max = extended ? scheme.Colors.Count + scheme.Extended.Count : scheme.Colors.Count;

			var meta = page.GetMetaContent(MetaNames.HighlightIndex);
			if (meta != null)
			{
				if (int.TryParse(meta, out index) && increment > 0)
				{
					index = index < max - 1 ? index + 1 : 0;
				}
			}

			var color = increment < 0
				? StyleBase.Transparent
				: GetColor(scheme, index, extended); //, page.GetPageColor(out _, out _).GetBrightness() < 0.5);

			updated = new PageEditor(page).EditSelected((s) =>
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


		private static string GetColor(HighlightColorScheme scheme, int index, bool extended)
		{
			if (extended && index >= scheme.Colors.Count)
			{
				return scheme.Extended[(index - scheme.Colors.Count) % scheme.Extended.Count];
			}

			// clamp into Colors range even if index is a stale/leftover value from
			// when extended was enabled (e.g. via the "highlight last" repeat command)
			return scheme.Colors[index % scheme.Colors.Count];
		}
	}
}
