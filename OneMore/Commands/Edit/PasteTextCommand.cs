//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Win = System.Windows;


	/// <summary>
	/// Join multiple lines of text into a single running paragraph, removing line breaks.
	/// </summary>
	internal class PasteTextCommand : Command
	{

		public PasteTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var elements = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all");

				var text = await SingleThreaded.Invoke(() =>
				{
					if (Win.Clipboard.ContainsText(Win.TextDataFormat.Text))
						return Win.Clipboard.GetText(Win.TextDataFormat.Text);
					else
						return null;
				});

				if (string.IsNullOrEmpty(text))
				{
					return;
				}

				var content = new XElement(ns + "T", new XCData(text));

				if (elements == null)
				{
					// empty page so add new content
					page.AddNextParagraph(content);
				}
				else if (elements.Count() > 1)
				{
					// selected multiple runs so replace them all
					page.ReplaceSelectedWithContent(content);
				}
				else
				{
					var line = elements.First();
					if (line.Value.Length == 0)
					{
						// empty cdata, unselected cursor so just insert
						line.GetCData().Value = text;
					}
					else
					{
						// something is selected so replace it
						page.ReplaceSelectedWithContent(content);
					}
				}

				await one.Update(page);
			}
		}
	}
}
