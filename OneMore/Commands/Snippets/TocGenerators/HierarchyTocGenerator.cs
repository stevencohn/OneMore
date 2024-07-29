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


	/// <summary>
	/// Intermediate base class for hierarchy TOC generators, section and notebook
	/// </summary>
	internal abstract class HierarchyTocGenerator : TocGenerator
	{
		protected const string LongDash = "\u2015";
		protected const int MinProgress = 25;

		protected readonly bool withPages;
		protected readonly bool withPreviews;

		protected OneNote one;
		protected string primaryTitle;
		protected Style cite;
		protected UI.ProgressDialog progress;


		protected HierarchyTocGenerator(TocParameters parameters)
			: base(parameters)
		{
			withPages = parameters.Contains("pages");
			withPreviews = parameters.Contains("preview");
		}


		protected override string PrimaryTitle => primaryTitle;


		public override async Task<bool> Build()
		{
			one = new OneNote();

			try
			{
				var section = await one.GetSection();
				var sectionId = section.Attribute("ID").Value;

				logger.WriteLine($"build hierarchy toc in section {section.Attribute("name").Value}");

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


		protected abstract Task BuildContents(Page page, XElement container, XElement section);


		protected async Task<int> BuildSection(
			OneNote one, XElement container, XElement[] elements,
			int index, int level)
		{
			string css = null;
			if (parameters.Contains("preview"))
			{
				cite.IsItalic = true;
				css = cite.ToCss();
			}

			while (index < elements.Length)
			{
				var element = elements[index];
				var pageID = element.Attribute("ID").Value;

				var pageLevel = int.Parse(element.Attribute("pageLevel").Value);
				if (pageLevel > level)
				{
					var children = new XElement(PageNamespace.Value + "OEChildren");
					index = await BuildSection(one, children, elements, index, pageLevel);
					container.Elements().Last().Add(children);
				}
				else if (pageLevel == level)
				{
					var link = one.GetHyperlink(pageID, string.Empty);
					var name = element.Attribute("name").Value;

					if (progress is not null)
					{
						progress.SetMessage(name);
						progress.Increment();
					}

					var text = withPreviews
						? $"<a href=\"{link}\">{name}</a> {await MakePagePreview(one, pageID, css)}"
						: $"<a href=\"{link}\">{name}</a>";

					container.Add(new Paragraph(text));

					index++;
				}
				else
				{
					break;
				}
			}

			return index;
		}


		private async Task<string> MakePagePreview(OneNote one, string pageID, string css)
		{
			var page = await one.GetPage(pageID, OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			var outline = page.BodyOutlines.FirstOrDefault();

			if (outline is null)
			{
				return string.Empty;
			}

			// sanitize the content, extracting only raw text and aggregating lines
			var preview = outline.Descendants(ns + "T")?.Nodes().OfType<XCData>()
				.Select(c => c.GetWrapper().Value).Aggregate(string.Empty, (a, b) => $"{a} {b}");

			if (preview is null || preview.Length == 0)
			{
				return string.Empty;
			}

			if (preview.Length > 80)
			{
				preview = preview.Substring(0, 80);

				// cheap HTML encoding...
				if (preview.IndexOf('<') >= 0)
				{
					preview = preview.Replace("<", "&lt;");
				}

				if (preview.IndexOf('>') >= 0)
				{
					preview = preview.Replace(">", "&gt;");
				}

				preview = $"{preview}...";
			}

			return $"<span style=\"{css}\">{LongDash} {preview}</span>";
		}


		public override async Task<bool> Refresh()
		{
			one = new OneNote();

			try
			{
				var page = await one.GetPage();
				var ns = page.Namespace;

				// remove old contents...

				var container = page.Root.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == Toc.MetaName)
					.Select(e => e.Parent.Parent)
					.FirstOrDefault();

				container?.Elements().Remove();

				// rebuild...

				var section = await one.GetSection();
				logger.WriteLine($"refresh hierarchy toc in section {section.Attribute("name").Value}");

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
