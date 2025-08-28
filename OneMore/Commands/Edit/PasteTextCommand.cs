//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Pastes the contents of the clipboard as plain text.
	/// </summary>
	internal class PasteTextCommand : Command
	{

		public PasteTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var text = await ClipboardProvider.GetText();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			await using var one = new OneNote(out var page, out var ns);
			PageNamespace.Set(ns);

			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			var editor = new PageEditor(page);

			var multiline = text.Contains("\n") || text.Contains("\r");
			if (multiline)
			{
				if (elements.Any())
				{
					editor.ExtractSelectedContent(breakParagraph: multiline);
				}

				// OneNote transforms \r\n into soft-break <br> but we want hard-breaks,
				// so split text into lines...

				var lines = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
				if (lines[lines.Length - 1].Length == 0)
				{
					lines = lines.Take(lines.Length - 1).ToArray();
				}

				XElement first = null;
				for (var i = lines.Length - 1; i >= 0; i--)
				{
					var run = new XElement(ns + "T", new XCData(lines[i]));
					first ??= run;

					editor.InsertAtAnchor(run);
				}

				// position insertion cursor after last line...
				editor.Deselect();
				first?.AddAfterSelf(
					new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(string.Empty)));
			}
			else
			{
				editor.ReplaceSelectedWith(new XElement(ns + "T", new XCData(text)));
			}

			await one.Update(page);
		}
	}
}
