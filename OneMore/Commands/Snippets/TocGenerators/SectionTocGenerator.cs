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
				var container = page.EnsureContentContainer();

				await BuildContents(page, container, section);

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


		private async Task BuildContents(Page page, XElement container, XElement section)
		{
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			// seeds the PrimaryTitle property
			primaryTitle = section.Attribute("name").Value;

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			// TOC Title...

			var segments = parameters.Contains("preview") ? "/preview" : string.Empty;

			var titleElement = MakeTitle(page, segments);

			// add meta to title OE
			if (!parameters.Contains("section")) parameters.Insert(0, "section");
			var segs = parameters.Aggregate((a, b) => $"{a}/{b}");
			titleElement.AddFirst(new Meta(Toc.MetaName, segs));

			container.Add(titleElement);
			container.Add(new Paragraph(string.Empty));

			// TOC contents...

			// don't include current page in TOC
			var elements = section.Elements(ns + "Page")
				.Where(e => e.Name.LocalName == "Page" && e.Attribute("ID").Value != page.PageId)
				.ToArray();

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
				_ = await BuildSection(one, container, elements, index, 1);
			}
			finally
			{
				if (progress != null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

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

				// remove old contents...

				var container = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == Toc.MetaName)
					.Select(e => e.Parent.Parent)
					.FirstOrDefault();

				if (container is not null)
				{
					container.Elements().Remove();
				}

				// rebuild...

				await BuildContents(page, container, section);
			}
			finally
			{
				await one.DisposeAsync();
			}

			return true;
		}
	}
}
