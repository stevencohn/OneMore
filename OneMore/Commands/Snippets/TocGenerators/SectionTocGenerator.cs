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


	/// <summary>
	/// Builds or refreshes a hierarchy TOC for the current section
	/// </summary>
	internal class SectionTocGenerator : HierarchyTocGenerator
	{
		public const string RefreshSectionCmd = "refreshs";


		public SectionTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => RefreshSectionCmd;


		public override async Task<RefreshOption> RefreshExistingPage()
		{
			await using var one = new OneNote();
			var section = await one.GetSection();
			var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

			var pageID = section.Elements(ns + "Page").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value == MetaNames.TableOfContents &&
					e.Attribute("content").Value == "section")
				.Select(e => e.Parent.Attribute("ID").Value)
				.FirstOrDefault();

			if (pageID is null)
			{
				return RefreshOption.Build;
			}

			var result = UI.MoreMessageBox.ShowQuestion(
				one.OwnerWindow, Resx.InsertTocForSection_RefreshQuestion, true);

			if (result == System.Windows.Forms.DialogResult.Cancel)
			{
				return RefreshOption.Cancel;
			}

			if (result == System.Windows.Forms.DialogResult.No)
			{
				return RefreshOption.Build;
			}

			await one.NavigateTo(pageID);

			return RefreshOption.Refresh;
		}


		protected override async Task BuildContents(
			Page page, XElement container, XElement section)
		{
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			// seeds the PrimaryTitle property
			primaryTitle = section.Attribute("name").Value;

			page.Title = string.Format(Resx.InsertTocCommand_TOCSections, section.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			page.SetMeta(MetaNames.TableOfContents, "section");

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
				if (progress is not null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

			await one.Update(page);
		}
	}
}
