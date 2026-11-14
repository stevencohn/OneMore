//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using Snippets.Toc;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	[CommandService]
	internal class InsertTocCommand : Command, ICliPageCommand
	{
		private string pageId;


		public InsertTocCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "InsertToc";


		public string Description => "Insert a table of contents into the current page";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook to process", required: true)
			.AddString("section", "Path of section to process", required: true)
			.AddString("page", "Name of page to process", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is string refresh && refresh.StartsWith("refresh"))
			{
				await Refresh(new TocParameters(args.Cast<string>()));
				return;
			}

			if (runningFromCli)
			{
				var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
				pageId = cliParams?.Get<string>("pageId");

				var tocParams = await CollectParameterDefaults(pageId);
				if (tocParams is null)
				{
					return;
				}

				await Build(tocParams);
				return;
			}

			using var guard = EnterOnce();
			if (guard is null) { return; }

			var parameters = await CollectParameterDefaults();
			if (parameters is null)
			{
				return;
			}

			var scope = args.Length > 0 ? args[0] as string : null;

			using MoreForm dialog = scope switch
			{
				"section" => new InsertSectionTocDialog(parameters),
				"notebook" => new InsertNotebookTocDialog(parameters),
				_ => new InsertPageTocDialog(parameters)
			};

			if (dialog.ShowDialog(owner) == DialogResult.Cancel)
			{
				return;
			}

			await Build(parameters);
		}


		private async Task Refresh(TocParameters parameters)
		{
			TocGenerator generator;
			if (parameters.Contains("refreshs"))
			{
				generator = new SectionTocGenerator(parameters);
			}
			else if (parameters.Contains("refreshn"))
			{
				generator = new NotebookTocGenerator(parameters);
			}
			else
			{
				generator = new PageTocGenerator(parameters, pageId);
			}

			// the up-to-date warning applies only to the interactive Refresh link on a page TOC
			if (!runningFromCli && generator is PageTocGenerator pageGenerator &&
				!await ConfirmRefresh(pageGenerator))
			{
				return;
			}

			if (!await generator.Refresh() && !runningFromCli)
			{
				ShowInfo("Could not refresh the table of contents. Please check the OneMore.log file");
			}
		}


		/// <summary>
		/// If enabled in Settings > Page, warns the user when the TOC appears to be up to date
		/// and the page is large enough that refreshing it would be slow.
		/// </summary>
		/// <returns>True to continue with the refresh</returns>
		private async Task<bool> ConfirmRefresh(PageTocGenerator generator)
		{
			var settings = new SettingsProvider().GetCollection(nameof(PageSheet));
			if (!settings.Get<bool>("tocWarnUpToDate"))
			{
				return true;
			}

			var minKb = settings.Get("tocWarnSize", PageSheet.DefaultTocWarnSize);

			var (upToDate, sizeKb) = await generator.CheckUpToDate();
			if (!upToDate || sizeKb < minKb)
			{
				return true;
			}

			var result = MoreMessageBox.ShowQuestion(owner, Resx.InsertTocCommand_UpToDate);
			if (result == DialogResult.Yes)
			{
				return true;
			}

			logger.WriteLine("table of contents is up to date; refresh cancelled by user");
			return false;
		}


		internal static async Task<TocParameters> CollectParameterDefaults(string pageId = null)
		{
			var parameters = new TocParameters();

			using var one = new OneNote();

			var page = pageId is null
				? await one.GetPage(OneNote.PageDetail.Basic)
				: await one.GetPage(pageId, OneNote.PageDetail.Basic);

			if (page is null)
			{
				Logger.Current.WriteLine(
					$"InsertTocCommand could not load page {pageId ?? "(current)"}");

				return null;
			}

			var ns = page.Namespace;

			var meta = page.BodyOutlines
				.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name") is XAttribute attr && attr.Value == Toc.MetaName);

			if (meta is null)
			{
				// no toc found
				return parameters;
			}

			// TOC2.0 - check tocMeta value itself first...

			if (meta.Value.Length > 0)
			{
				parameters.AddRange(
					meta.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));

				return parameters;
			}

			// TOC1.0 - look for URI and parse its query params...

			// all refresh URI queries start with "/refresh"
			var refreshCmd = $"{Toc.RefreshUri}refresh";

			var cdata = meta.Parent.DescendantNodes().OfType<XCData>()
				.FirstOrDefault(c => c.Value.Contains(refreshCmd));

			if (cdata is null)
			{
				return parameters;
			}

			var wrapper = cdata.GetWrapper();
			var href = wrapper.Elements("a").Attributes("href").FirstOrDefault()?.Value;
			if (href is not null)
			{
				var uri = new Uri(href);

				parameters.AddRange(uri.Segments
					.Where(s => s is not null && s.Length > 0 && s != "/")
					.Select(s => s.Replace("/", string.Empty)));
			}

			return parameters;
		}


		private async Task Build(TocParameters parameters)
		{
			TocGenerator generator;
			if (parameters.Contains("section"))
			{
				generator = new SectionTocGenerator(parameters);
			}
			else if (parameters.Contains("notebook"))
			{
				generator = new NotebookTocGenerator(parameters);
			}
			else
			{
				generator = new PageTocGenerator(parameters);
			}

			var option = await generator.RefreshExistingPage();
			if (option == RefreshOption.Refresh)
			{
				logger.WriteLine("refreshing instead of building");
				await generator.Refresh();
			}
			else if (option == RefreshOption.Build)
			{
				await generator.Build();
			}
		}
	}
}
