//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Insert one or more selected symbols from the Segoe UI Emoji char set
	/// </summary>
	internal class InsertEmojiCommand : Command
	{
		private const string ReplayElementName = "symbols";
		private IEnumerable<IEmoji> emojis;


		public InsertEmojiCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// check for replay
			XElement element = null;
			if (args != null)
			{
				element = System.Array.Find(args,
					a => a is XElement e && e.Name.LocalName == ReplayElementName) as XElement;
			}

			if (!string.IsNullOrEmpty(element?.Value))
			{
				using var map = new Emojis();
				emojis = map.ParseSymbols(element.Value);
			}

			if (emojis == null)
			{
				using var dialog = new EmojiDialog();
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					IsCancelled = true;
					return;
				}

				emojis = dialog.GetEmojis();
			}

			await InsertSymbols();
		}


		private async Task InsertSymbols()
		{
			await using var one = new OneNote(out var page, out var ns);
			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (!elements.Any())
			{
				// empty page so add new content
				ShowInfo("dummy");
				return;
			}

			// collect selected symbols
			var builder = new StringBuilder();
			foreach (var emoji in emojis)
			{
				var color = emoji.Color == null ? string.Empty : $"color:{emoji.Color}";

				builder.Append(
					$"<span style=\"font-family:'Segoe UI Emoji';{color}\">{emoji.Glyph}</span>");
			}

			var text = builder.ToString();
			var content = new XElement(ns + "T", text);
			var editor = new PageEditor(page);

			if (elements.Count() > 1)
			{
				// selected multiple runs so replace them all
				editor.ReplaceSelectedWith(content);
			}
			else
			{
				var line = elements.First();
				if (line.Parent.Parent.Name.LocalName == "Title" &&
					line.Value == line.Parent.Value)
				{
					// special case for page titles; if entire title is selected then only
					// prepend title, do not delete the title
					var cdata = line.GetCData();
					cdata.Value = $"{text} {cdata.Value}";
				}
				else
				{
					// something is selected so replace it
					editor.ReplaceSelectedWith(content);
				}
			}

			await one.Update(page);
		}


		public override XElement GetReplayArguments()
		{
			if (emojis.Any())
			{
				return new XElement(
					ReplayElementName,
					string.Join(",", emojis.Select(e => e.Glyph))
					);
			}

			return null;
		}
	}
}
