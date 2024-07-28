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

		private OneNote one;


		public SectionTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => RefreshSectionCmd;


		public override async Task<bool> Build()
		{
			one = new OneNote();

			try
			{
				var section = await one.GetSection();
				var sectionId = section.Attribute("ID").Value;

				one.CreatePage(sectionId, out var pageId);

				var page = await one.GetPage(pageId);

				await BuildContents(page, section);

				// move TOC page to top of section...

				// get current section again after new page is created
				section = await one.GetSection();

				var entry = section.Elements(page.Namespace + "Page")
					.First(e => e.Attribute("ID").Value == pageId);

				entry.Remove();
				section.AddFirst(entry);
				one.UpdateHierarchy(section);

				await one.NavigateTo(pageId);
			}
			finally
			{
				await one.DisposeAsync();
			}
			return true;
		}


		private async Task BuildContents(Page page, XElement section)
		{
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			// seeds the PrimaryTitle property
			primaryTitle = section.Attribute("name").Value;

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			var container = new XElement(ns + "OEChildren");

			// TOC Title...

			var segments = parameters.Contains("preview") ? "/preview" : string.Empty;
			container.Add(MakeTitle(page, segments));
			container.Add(new Paragraph(string.Empty));

			// TOC contents...

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
				_ = await BuildSection(one, container, elements.ToArray(), index, 1);
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
		}


		public override async Task<bool> Refresh()
		{
			one = new OneNote();

			try
			{
				var section = await one.GetSection();

				var page = await one.GetPage();
				var ns = page.Namespace;

				//
				// TODO: this should be replaced with Locate omToc and delete siblings
				//

				foreach (var outline in page.Root.Elements(ns + "Outline"))
				{
					one.DeleteContent(page.PageId, outline.Attribute("objectID").Value);
					outline.Remove();
				}



				await BuildContents(page, section);
			}
			finally
			{
				await one.DisposeAsync();
			}

			return true;
		}
	}
}
