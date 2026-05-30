//************************************************************************************************
// Copyright © 2019 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Trims trailing whitespace from selected text or all text on the page
	/// </summary>
	internal class TrimCommand : Command, ICliPageCommand
	{
		public TrimCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "Trim";

		public string Description => "Trim leading or trailing whitespace from page text";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false)
			.AddBoolean("leading", "Trim leading whitespace; default trims trailing whitespace",
				required: false, defaultValue: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }
				cliParams.TryGet("leading", out bool leading);

				await using var one = new OneNote();
				var page = await one.GetPage(pageId, OneNote.PageDetail.All);
				await Run(one, page, leading);
				return;
			}

			var ribbonLeading = (bool)args[0];
			await using var ribbon = new OneNote(out var rpage, out _);
			await Run(ribbon, rpage, ribbonLeading);
		}


		private async Task Run(OneNote one, Models.Page page, bool leading)
		{
			int count = 0;

			var regex = leading
				? new Regex(@"^([\s]|&#160;|&nbsp;)+", RegexOptions.Multiline)
				: new Regex(@"([\s]|&#160;|&nbsp;)+$", RegexOptions.Multiline);

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
