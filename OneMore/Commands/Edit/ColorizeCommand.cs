//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
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
	using Resx = Properties.Resources;


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
			using var guard = EnterOnce();
			if (guard is null) { return; }

			try
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
			catch (LanguageException exc)
			{
				// surfaces a malformed/unreadable language definition file (e.g. a user
				// edited Colorizer\Languages\{lang}.json and broke its JSON) so the user
				// gets an actionable message instead of CommandFactory's generic toast
				logger.WriteLine(exc);
				ShowError(string.Format(Resx.ColorizeCommand_LanguageError, exc.Name));
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

			// neighboring runs are colorized together as one batch so constructs that span
			// paragraphs can be recognized; batches never cross cells, outlines or equations
			var batch = new List<RunInfo>();
			var lines = new List<string>();
			XElement batchKey = null;

			foreach (var run in runs.ToList())
			{
				// never touch mathML equations
				if (run.IsMathML())
				{
					updated |= ColorizeBatch(colorizer, batch, lines);
					continue;
				}

				var container = run.Ancestors(ns + "Cell").FirstOrDefault()
					?? run.Ancestors(ns + "Outline").FirstOrDefault();

				if (batch.Count > 0 && container != batchKey)
				{
					updated |= ColorizeBatch(colorizer, batch, lines);
				}

				batchKey = container;

				run.SetAttributeValue("lang", "yo");
				run.Parent.Attributes("spaceAfter").Remove();
				run.Parent.Attributes("spaceBefore").Remove();

				var info = new RunInfo
				{
					Run = run,
					Cdata = run.GetCData(),
					First = lines.Count
				};

				if (info.Cdata.Value.Contains("<br>"))
				{
					// special handling to expand soft line breaks (Shift + Enter) into
					// hard breaks, splitting the line into multiple ines.
					// presume that br is always followed by newline...

					// need to move children to last runoff to maintain positioning
					info.Children = run.Parent.Elements(ns + "OEChildren").ToList();
					info.Children.ForEach(c => c.Remove());

					var text = info.Cdata.GetWrapper().Value;
					text = Regex.Replace(text, @"\r\n", "\n");

					lines.AddRange(text.Split(new string[] { "\n" }, StringSplitOptions.None));
				}
				else
				{
					lines.Add(info.Cdata.GetWrapper().Value);
				}

				info.Count = lines.Count - info.First;
				batch.Add(info);
			}

			updated |= ColorizeBatch(colorizer, batch, lines);

			return updated;
		}


		private sealed class RunInfo
		{
			public XElement Run;
			public XCData Cdata;
			public List<XElement> Children;
			public int First;
			public int Count;
		}


		private bool ColorizeBatch(Colorizer colorizer, List<RunInfo> batch, List<string> lines)
		{
			if (batch.Count == 0)
			{
				return false;
			}

			var colorized = colorizer.ColorizeMany(lines);
			if (colorized.Count != lines.Count)
			{
				// a run contained a raw newline; fall back to colorizing each line alone
				colorized = lines.Select(colorizer.ColorizeOne).ToList();
			}

			foreach (var info in batch)
			{
				var run = info.Run;
				var runoffs = new List<XElement>();

				// update current cdata with first line
				info.Cdata.Value = colorized[info.First];

				// collect subsequent lines from soft-breaks
				for (var i = 1; i < info.Count; i++)
				{
					runoffs.Add(new XElement(ns + "OE",
						run.Parent.Attributes(),
						new XElement(ns + "T", new XCData(colorized[info.First + i])
						)));
				}

				if (runoffs.Count > 0)
				{
					run.Parent.AddAfterSelf(runoffs);
				}

				// restore children, appending to last runoff
				if (info.Children is not null && info.Children.Count > 0)
				{
					if (runoffs.Count > 0)
					{
						runoffs[runoffs.Count - 1].Add(info.Children);
					}
					else
					{
						run.Parent.Add(info.Children);
					}
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

			batch.Clear();
			lines.Clear();
			return true;
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
