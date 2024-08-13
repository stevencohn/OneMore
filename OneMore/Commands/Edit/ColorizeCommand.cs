//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Colorizer;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ColorizeCommand : Command
	{
		private const string DepthAttributeName = "omDepth";

		private Page page;
		private XNamespace ns;
		private readonly bool fontOverride;


		public ColorizeCommand()
		{
		}


		public ColorizeCommand(Page page, bool fontOverride)
		{
			this.page = page;
			ns = page.Namespace;
			this.fontOverride = fontOverride;
		}


		/// <summary>
		/// Execute the colorizer from the command factory
		/// DO NOT call this from other location; use the method below instead
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out page, out ns);

			AddDepth(page.Root);

			var runs = page.Root.Descendants(ns + "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

			if (!runs.Any())
			{
				return;
			}

			var updated = Colorize(args[0] as string, runs);

			if (updated)
			{
				RemoveDepth(page.Root);
				await one.Update(page);
			}
		}


		/// <summary>
		/// Colorize the given runs using the specified language
		/// </summary>
		/// <param name="key"></param>
		/// <param name="runs"></param>
		/// <returns></returns>
		public bool Colorize(string key, IEnumerable<XElement> runs)
		{
			var pageColor = page.GetPageColor(out var automatic, out var black);

			var dark =
				(black || pageColor.GetBrightness() < 0.5) &&
				!(black && pageColor.GetBrightness() >= 0.5);

			var theme = dark ? "dark" : "light";
			//logger.WriteLine($"theme: {theme} (color:{pageColor} automatic:{automatic} black:{black})");

			var colorizer = new Colorizer(
				key,
				theme,
				automatic); // || (black && theme == "dark"));

			if (fontOverride)
			{
				colorizer.EnableSecondaryFont();
			}

			var updated = false;

			foreach (var run in runs.ToList())
			{
				run.SetAttributeValue("lang", "yo");
				run.Parent.Attributes("spaceAfter").Remove();
				run.Parent.Attributes("spaceBefore").Remove();
				var runoffs = new List<XElement>();

				var cdata = run.GetCData();

				if (cdata.Value.Contains("<br>"))
				{
					// special handling to expand soft line breaks (Shift + Enter) into
					// hard breaks, splitting the line into multiple ines.
					// presume that br is always followed by newline...

					// need to move children to last runoff to maintain positioning
					var children = run.Parent.Elements(ns + "OEChildren").ToList();
					if (children.Any())
					{
						children.ForEach(c => c.Remove());
					}

					var text = cdata.GetWrapper().Value;
					text = Regex.Replace(text, @"\r\n", "\n");

					var lines = text.Split(new string[] { "\n" }, StringSplitOptions.None);

					// update current cdata with first line
					cdata.Value = colorizer.ColorizeOne(lines[0]);

					// collect subsequent lines from soft-breaks
					for (int i = 1; i < lines.Length; i++)
					{
						var colorized = colorizer.ColorizeOne(lines[i]);

						runoffs.Add(new XElement(ns + "OE",
							run.Parent.Attributes(),
							new XElement(ns + "T", new XCData(colorized)
							)));
					}

					run.Parent.AddAfterSelf(runoffs);

					// restore children, appending to last runoff
					if (children.Any())
					{
						runoffs[runoffs.Count - 1].Add(children);
					}

					updated = true;
				}
				else
				{
					cdata.Value = colorizer.ColorizeOne(cdata.GetWrapper().Value);
					updated = true;
				}


				if (run.GetAttributeValue(DepthAttributeName, out int depth, 1) && depth == 0)
				{
					var text = run.Value.PlainText();
					if (text.Length > 0)
					{
						if (text[0] == '+')
						{
							colorizer.ColorizeDiffs(run.Parent, true);
							runoffs.ForEach(r => colorizer.ColorizeDiffs(r, true));
						}
						else if (text[0] == '-')
						{
							colorizer.ColorizeDiffs(run.Parent, false);
							runoffs.ForEach(r => colorizer.ColorizeDiffs(r, false));
						}
					}
				}
			}

			return updated;
		}


		private void AddDepth(XElement root)
		{
			void AddDepth(XElement parent, int depth)
			{
				parent.Elements(ns + "OE").ForEach(e =>
				{
					e.Elements(ns + "T").ForEach(t =>
					{
						var a = t.Attribute(DepthAttributeName);
						if (a == null)
						{
							t.Add(new XAttribute(DepthAttributeName, depth.ToString()));
						}
					});

					e.Elements(ns + "OEChildren").ForEach(e => AddDepth(e, depth + 1));
				});
			}

			root.Elements(ns + "Outline")
				.Elements(ns + "OEChildren")
				.ForEach(e => AddDepth(e, 0));
		}


		private void RemoveDepth(XElement root)
		{
			root.Descendants(ns + "T")
				.Attributes(DepthAttributeName)
				.Remove();
		}
	}
}
