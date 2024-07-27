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
			if (parameters.Contains(SectionTocGenerator.RefreshSectionCmd))
			{
				//await new SectionTocGenerator().Refresh(parameters);
			}
			else if (parameters.Contains(NotebookTocGenerator.RefreshNotebookCmd))
			{
				//await new NotebookTocGenerator().Refresh(parameters);
			}
			else // page is default
			{
				await new PageTocGenerator(parameters).Refresh();
			}
		}


		private async Task<TocParameters> CollectParameterDefaults()
		{
			var parameters = new TocParameters();

			await using var one = new OneNote();
			var page = await one.GetPage(OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			var meta = page.Root.Elements(ns + "Outline")
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
				await generator.Build();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error executing {nameof(InsertTocCommand)}", exc);
			}
		}
	}
}
