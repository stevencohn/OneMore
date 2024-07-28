//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class NotebookTocGenerator : HierarchyTocGenerator
	{
		public const string RefreshNotebookCmd = "refreshn";


		public NotebookTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => RefreshNotebookCmd;


		public override async Task<bool> Build()
		{
			await using var one = new OneNote();
			var section = await one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			var page = await one.GetPage(pageId);
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			var scope = withPages ? OneNote.Scope.Pages : OneNote.Scope.Sections;
			var notebook = await one.GetNotebook(scope);

			// seeds the PrimaryTitle property
			primaryTitle = notebook.Attribute("name").Value;

			page.Title = string.Format(Resx.InsertTocCommand_TOCNotebook, notebook.Attribute("name").Value);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			var container = new XElement(ns + "OEChildren");

			// TOC Title...

			var segments = string.Empty;
			if (parameters.Contains("pages")) segments = $"{segments}/pages";
			if (parameters.Contains("preview")) segments = $"{segments}/preview";

			container.Add(MakeTitle(page, segments));
			container.Add(new Paragraph(string.Empty));

			// TOC contents...

			var pageCount = notebook.Descendants(ns + "Page").Count();
			if (pageCount > MinProgress)
			{
				progress = new UI.ProgressDialog();
				progress.SetMaximum(pageCount);
				progress.Show();
			}

			try
			{
				await BuildSectionTree(one, ns, container, notebook.Elements(), 1);
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

			await one.NavigateTo(pageId);
			return true;
		}


		private async Task BuildSectionTree(
			OneNote one, XNamespace ns, XElement container,
			IEnumerable<XElement> elements, int level)
		{
			foreach (var element in elements)
			{
				var notBin = element.Attribute("isRecycleBin") == null;

				if (element.Name.LocalName == "SectionGroup" && notBin)
				{
					// SectionGroup

					var name = element.Attribute("name").Value;

					var indent = new XElement(ns + "OEChildren");

					indent.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							// this is a Folder icon... but doesn't look great
							// <span style='font-family:Segoe UI Emoji'>&#128194; </span>
							new XCData($"<span style='font-weight:bold'>{name}</span>"))
						));

					await BuildSectionTree(one, ns, indent, element.Elements(), level + 1);

					container.Add(
						new XElement(ns + "OE", indent)
						);
				}
				else if (element.Name.LocalName == "Section" && notBin)
				{
					// Section

					var link = one.GetHyperlink(element.Attribute("ID").Value, string.Empty);
					var name = element.Attribute("name").Value;
					var pages = element.Elements(ns + "Page");

					if (withPages && pages.Any())
					{
						var indent = new XElement(ns + "OEChildren");
						var index = 0;

						_ = await BuildSection(one, indent, pages.ToArray(), index, 1);

						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"§ <a href=\"{link}\">{name}</a>")),
							indent
							));
					}
					else
					{
						container.Add(new XElement(ns + "OE",
							new XElement(ns + "T", new XCData($"§ <a href=\"{link}\">{name}</a>")
							)));
					}
				}
			}
		}


		public override Task<bool> Refresh()
		{
			throw new System.NotImplementedException();
		}
	}
}
