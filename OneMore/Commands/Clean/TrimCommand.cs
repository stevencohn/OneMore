//************************************************************************************************
// Copyright © 2019 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Trims trailing whitespace from selected text or all text on the page
	/// </summary>
	internal class TrimCommand : Command
	{
		public TrimCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var leading = (bool)args[0];
			int count = 0;

			var regex = leading
				? new Regex(@"^([\s]|&#160;|&nbsp;)+", RegexOptions.Multiline)
				: new Regex(@"([\s]|&#160;|&nbsp;)+$", RegexOptions.Multiline);

			await using var one = new OneNote(out var page, out _);

			var range = new Models.SelectionRange(page);
			var runs = range.GetSelections(defaulToAnytIfNoRange: true);

			if (runs.Any())
			{
				foreach (var selection in runs)
				{
					// only include last T in an OE
					// only include Ts that have a CDATA
					if ((selection != selection.Parent.LastNode) ||
						(selection.LastNode?.NodeType != XmlNodeType.CDATA))
					{
						continue;
					}

					var cdata = selection.GetCData();
					if (cdata.Value.Length == 0)
					{
						continue;
					}

					var wrapper = cdata.GetWrapper();

					if (leading)
					{
						// T may start with an XText or have multiple spans, only need the first
						var text = wrapper.DescendantNodes().OfType<XText>().FirstOrDefault();
						if (text?.Value.Length > 0)
						{
							var edited = regex.Replace(text.Value, string.Empty);
							if (edited.Length < text.Value.Length)
							{
								text.ReplaceWith(edited);
								selection.FirstNode.ReplaceWith(new XCData(wrapper.GetInnerXml()));
								count++;
							}
						}
					}
					else
					{
						// T may end with an XText or have multiple spans, only need the last
						var text = wrapper.DescendantNodes().OfType<XText>().LastOrDefault();
						if (text?.Value.Length > 0)
						{
							var edited = regex.Replace(text.Value, string.Empty);
							if (edited.Length < text.Value.Length)
							{
								text.ReplaceWith(edited);
								selection.FirstNode.ReplaceWith(new XCData(wrapper.GetInnerXml()));
								count++;
							}
						}
					}
				}

				if (count > 0)
				{
					await one.Update(page);
				}
			}

			logger.WriteLine($"trimmed {count} lines");
		}
	}
}
