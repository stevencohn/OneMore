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


		protected override async Task BuildContents(
			Page page, XElement container, XElement section)
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
	}
}
