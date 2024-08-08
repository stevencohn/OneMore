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


	/// <summary>
	/// Builds or refreshes a hierarchy TOC for the current notebook
	/// </summary>
	internal class NotebookTocGenerator : HierarchyTocGenerator
	{
		public const string RefreshNotebookCmd = "refreshn";


		public NotebookTocGenerator(TocParameters parameters)
			: base(parameters)
		{
		}


		protected override string RefreshCmd => RefreshNotebookCmd;


		public override async Task<RefreshOption> RefreshExistingPage()
		{
			await using var one = new OneNote();
			var notebook = await one.GetNotebook(OneNote.Scope.Pages);
			var ns = notebook.GetNamespaceOfPrefix(OneNote.Prefix);

			var pageID = notebook.Descendants(ns + "Page").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name").Value == MetaNames.TableOfContents &&
					e.Attribute("content").Value == "notebook")
				.Select(e => e.Parent.Attribute("ID").Value)
				.FirstOrDefault();

			if (pageID is null)
			{
				return RefreshOption.Build;
			}

			var result = UI.MoreMessageBox.ShowQuestion(
				one.OwnerWindow, Resx.InsertTocForNotebook_RefreshQuestion, true);

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

			var scope = withPages ? OneNote.Scope.Pages : OneNote.Scope.Sections;
			var notebook = await one.GetNotebook(scope);

			// seeds the PrimaryTitle property
			primaryTitle = notebook.Attribute("name").Value.Trim();

			page.Title = string.Format(Resx.InsertTocCommand_TOCNotebook, primaryTitle);
			cite = page.GetQuickStyle(StandardStyles.Citation);

			page.SetMeta(MetaNames.TableOfContents, "notebook");

			// TOC Title...

			var segments = string.Empty;
			if (parameters.Contains("pages")) segments = $"{segments}/pages";
			if (parameters.Contains("preview")) segments = $"{segments}/preview";

			var titleElement = MakeTitle(page, segments);

			// add meta to title OE
			if (!parameters.Contains("notebook")) parameters.Insert(0, "notebook");
			var segs = parameters.Aggregate((a, b) => $"{a}/{b}");
			titleElement.AddFirst(new Meta(Toc.MetaName, segs));

			container.Add(titleElement);
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
				if (progress is not null)
				{
					progress.Close();
					progress.Dispose();
				}
			}

			await one.Update(page);
		}


		private async Task BuildSectionTree(
		OneNote one, XNamespace ns, XElement container,
		IEnumerable<XElement> elements, int level)
		{
			foreach (var element in elements)
			{
				var notBin = element.Attribute("isRecycleBin") is null;

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

					container.Add(new Paragraph(indent));
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

						container.Add(new Paragraph(
							new XElement(ns + "T", new XCData($"§ <a href=\"{link}\">{name}</a>")),
							indent));
					}
					else
					{
						container.Add(new Paragraph($"§ <a href=\"{link}\">{name}</a>"));
					}
				}
			}
		}
	}
}
