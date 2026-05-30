//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Removes citations that OneNote auto-generates when you paste screen clipping and parts
	/// of Web pages into OneNote, for example From <https://www....
	/// </summary>
	internal class RemoveCitationsCommand : Command, ICliPageCommand
	{
		public RemoveCitationsCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RemoveCitations";

		public string Description => "Remove auto-generated web clipping citations from a page";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }
				await using var one = new OneNote();
				var page = await one.GetPage(pageId, OneNote.PageDetail.All);
				await Run(one, page, page.Namespace);
				return;
			}

			await using var ribbon = new OneNote(out var rpage, out var rns);
			logger.StartClock();
			await Run(ribbon, rpage, rns);
		}


		private async Task Run(OneNote one, Page page, XNamespace ns)
		{
			var style = page.GetQuickStyles()
				.Find(s => s.Name == "cite");

			if (style == null)
			{
				return;
			}

			var index = style.Index.ToString(CultureInfo.InvariantCulture);

			// TODO: not sure if this entire string needs localizing to accomodate RTL
			var regex = new Regex($@"{Resx.RemoveCitations_From} &lt;\s*<a\shref=.+?</a>\s*&gt;");

			var elements =
				(from e in page.Root.Descendants(ns + "OE")
				 where e.Attributes("quickStyleIndex").Any(a => a.Value == index)
				 let text = e.Element(ns + "T").GetCData().Value
				 where text.Contains(Resx.RemoveCitations_Clippings) || regex.Match(text).Success
				 select e)
				.ToList();

			if (elements.Any())
			{
				foreach (var element in elements)
				{
					element.Remove();
				}

				logger.WriteTime("removed citations, now saving...");

				await one.Update(page);
			}

			logger.StopClock();
		}
	}
}
