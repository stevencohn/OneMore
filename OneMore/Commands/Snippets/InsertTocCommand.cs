//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Snippets.TocGenerators;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	[CommandService]
	internal class InsertTocCommand : Command
	{

		public InsertTocCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length > 0 && args[0] is string refresh && refresh.StartsWith("refresh"))
			{
				await Refresh(new TocParameters(args.Cast<string>()));
				return;
			}

			var parameters = await CollectParameterDefaults();

			using var dialog = new InsertTocDialog(parameters);
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
				generator = new PageTocGenerator(parameters);
			}

			try
			{
				await generator.Refresh();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}


		private static async Task<TocParameters> CollectParameterDefaults()
		{
			var parameters = new TocParameters();

			await using var one = new OneNote();
			var page = await one.GetPage(OneNote.PageDetail.Basic);
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

			try
			{
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
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}
	}
}
