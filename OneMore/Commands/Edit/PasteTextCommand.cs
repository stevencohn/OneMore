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
			var text = await new ClipboardProvider().GetText();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			await using var one = new OneNote(out var page, out var ns);
			PageNamespace.Set(ns);

			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			var editor = new PageEditor(page)
			{
				KeepSelected = false
			};

			if (elements.Any())
			{
				editor.ExtractSelectedContent();
			}

			// OneNote transforms \r\n into soft-break <br> but we want hard-breaks,
			// so split text into lines...

			var lines = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);

			XElement first = null;
			for (var i = lines.Length - 1; i >= 0; i--)
			{
				var run = new XElement(ns + "T", new XCData(lines[i]));
				first ??= run;

				editor.InsertAtAnchor(run);
			}

			// position insertion cursor after last line...

			page.Root.DescendantNodes().OfType<XAttribute>()
				.Where(a => a.Name == "selected")
				.Remove();

			first?.SetAttributeValue("selected", "all");

			await one.Update(page);
		}
	}
}
