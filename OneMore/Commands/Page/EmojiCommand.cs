//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Prefix the page title with one or more chosen symbols from the Segoe UI Emoji char set
	/// </summary>
	internal class EmojiCommand : Command
	{
		private IEnumerable<IEmoji> emojis;


		public EmojiCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// check for replay
			var element = args?.FirstOrDefault(a => 
				a is XElement e && e.Name.LocalName == "symbols") as XElement;

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

				emojis = dialog.GetEmojis().Reverse();
			}

			await InsertSymbols();
		}


		private async Task InsertSymbols()
		{
			using var one = new OneNote(out var page, out var ns);

			var cdata = page.Root
				.Elements(page.Namespace + "Title")
				.Elements(page.Namespace + "OE")    // should have exactly one OE
				.Elements(page.Namespace + "T")     // but may have one or more Ts
				.DescendantNodes().OfType<XCData>()
				.FirstOrDefault();

			if (cdata == null)
			{
				// no title on page - this shouldn't happen!
				return;
			}

			// blindly prepend title with new span, letting OneNote normalize consecutive spans...

			foreach (var emoji in emojis)
			{
				var color = emoji.Color == null ? "" : $"color:{emoji.Color}";

				cdata.Value = cdata.Value.Insert(0,
					$"<span style=\"font-family:'Segoe UI Emoji';font-size:16pt;{color}\">{emoji.Symbol}</span>");
			}

			await one.Update(page);
		}


		public override XElement GetReplayArguments()
		{
			if (emojis.Any())
			{
				return new XElement("symbols", string.Join(",", emojis.Select(e => e.Symbol)));
			}

			return null;
		}
	}
}
