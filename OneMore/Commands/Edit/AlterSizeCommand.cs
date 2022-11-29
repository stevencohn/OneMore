//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Increases or decreases the font size of all text on the entire page.
	/// </summary>
	internal class AlterSizeCommand : Command
	{
		private const double MinFontSize = 6.0;
		private const double MaxFontSize = 144.0;

		private Page page;
		private int delta;


		public AlterSizeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			delta = (int)args[0]; // +/-1

			using var one = new OneNote(out page, out _);

			if (page == null)
			{
				return;
			}

			var count
				= AlterByName()
				+ AlterElementsByValue()
				+ AlterCDataByValue();

			if (count == 0)
			{
				return;
			}

			await one.Update(page);
		}


		private int AlterByName()
		{
			var count = 0;

			// find all elements that have an attribute named fontSize, e.g. QuickStyleDef or Bullet
			var elements = page.Root.Descendants()
				.Where(p =>
					p.Attribute("name")?.Value != "PageTitle" &&
					p.Attribute("fontSize") != null);

			if (!elements.IsNullOrEmpty())
			{
				foreach (var element in elements)
				{
					if (element != null)
					{
						if (element.Attribute("fontSize") is XAttribute attr)
						{
							if (double.TryParse(attr.Value,
								NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
							{
								size = delta < 0
									? Math.Max(size + delta, MinFontSize)
									: Math.Min(size + delta, MaxFontSize);

								attr.Value = size.ToString("#0") + ".05";
								count++;
							}
						}
					}
				}
			}

			return count;
		}


		// <one:OE alignment="left" spaceBefore="14.0" quickStyleIndex="1"
		//   style="font-family:'Segoe UI';font-size:&#xA;20.0pt;color:#151515">
		private int AlterElementsByValue()
		{
			int count = 0;

			var elements = page.Root.Descendants()
				.Where(p =>
					p.Parent.Name.LocalName != "Title" &&
					p.Attribute("style")?.Value.Contains("font-size:") == true);

			if (!elements.IsNullOrEmpty())
			{
				elements.ForEach(element =>
				{
					if (UpdateSpanStyle(element))
					{
						count++;
					}
				});
			}

			return count;
		}


		private int AlterCDataByValue()
		{
			int count = 0;

			var nodes = page.Root.DescendantNodes().OfType<XCData>()
				.Where(n => n.Value.Contains("font-size:"));

			if (!nodes.IsNullOrEmpty())
			{
				foreach (var cdata in nodes)
				{
					var wrapper = cdata.GetWrapper();

					var spans = wrapper.Elements("span")
						.Where(e => e.Attribute("style")?.Value.Contains("font-size") == true);

					if (!spans.IsNullOrEmpty())
					{
						spans.ForEach(span =>
						{
							if (UpdateSpanStyle(span))
							{
								// unwrap our temporary <cdata>
								cdata.Value = wrapper.GetInnerXml();
								count++;
							}
						});
					}
				}
			}

			return count;
		}


		private bool UpdateSpanStyle(XElement span)
		{
			bool updated = false;

			var attr = span.Attribute("style");
			if (attr != null)
			{
				// remove encoded LF character (&#xA)
				var css = attr.Value.Replace("\n", string.Empty);

				// Cannot use .ToDictionary(p => p[0], p => p[1]); here because there might
				// be duplicate properties, so overwrite duplicates in the dictionary

				var properties = new Dictionary<string, string>();
				var props = css.Split(';');
				foreach (var prop in props)
				{
					var parts = prop.Split(':');
					if (parts.Length > 1)
					{
						if (properties.ContainsKey(parts[0]))
						{
							properties[parts[0]] = parts[1];
						}
						else
						{
							properties.Add(parts[0], parts[1]);
						}
					}
				}

				if (properties.ContainsKey("font-size"))
				{
					var size = ParseFontSize(properties["font-size"]);

					size = delta < 0
						? Math.Max(size + delta, MinFontSize)
						: Math.Min(size + delta, MaxFontSize);

					properties["font-size"] = size.ToString("#0") + ".05pt";

					attr.Value =
						string.Join(";", properties.Select(p => p.Key + ":" + p.Value).ToArray());

					updated = true;
				}
			}

			return updated;
		}


		private static double ParseFontSize(string size)
		{
			var match = Regex.Match(size,
				@"^([0-9]+(?:\" + AddIn.Culture.NumberFormat.NumberDecimalSeparator + "[0-9]+)?)(?:pt){0,1}");

			if (match.Success)
			{
				size = match.Groups[match.Groups.Count - 1].Value;
				if (!string.IsNullOrEmpty(size))
				{
					return double.Parse(size, CultureInfo.InvariantCulture);
				}
			}

			return StyleBase.DefaultFontSize;
		}
	}
}
