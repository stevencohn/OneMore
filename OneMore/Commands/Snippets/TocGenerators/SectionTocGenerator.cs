//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class SectionTocGenerator : HierarchyTocGenerator
	{
		public const string RefreshSectionCmd = "refreshs";


		public SectionTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => RefreshSectionCmd;


		public override async Task<bool> Build()
		{
			await using var one = new OneNote();
			var section = await one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = await one.GetPage(pageId);
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			var container = new XElement(ns + "OEChildren");

			var elements = section.Elements(ns + "Page");
			var index = 0;

			var pageCount = elements.Count();
			if (pageCount > MinProgress)
			{
				progress = new UI.ProgressDialog();
				progress.SetMaximum(pageCount);
				progress.Show();
			}

			try
			{
				_ = await BuildSectionToc(one, container, elements.ToArray(), index, 1);
			}
			finally
			{
				if (progress != null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

			var title = page.Root.Elements(ns + "Title").First();
			title.AddAfterSelf(new XElement(ns + "Outline", container));
			await one.Update(page);

			// move TOC page to top of section...

			// get current section again after new page is created
			section = await one.GetSection();

			var entry = section.Elements(ns + "Page")
				.First(e => e.Attribute("ID").Value == pageId);

			entry.Remove();
			section.AddFirst(entry);
			one.UpdateHierarchy(section);

			await one.NavigateTo(pageId); return true;
		}


		public override Task<bool> Refresh()
		{
			throw new System.NotImplementedException();
		}
	}
}
