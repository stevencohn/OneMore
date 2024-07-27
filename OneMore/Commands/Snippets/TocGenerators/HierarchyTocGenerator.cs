
namespace River.OneMoreAddIn.Commands.Snippets.TocGenerators
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal abstract class HierarchyTocGenerator : TocGenerator
	{
		protected const string LongDash = "\u2015";
		protected const int MinProgress = 25;

		protected readonly bool withPages;
		protected readonly bool withPreviews;

		protected Style cite;
		protected UI.ProgressDialog progress;


		protected HierarchyTocGenerator(TocParameters parameters)
			: base(parameters)
		{
			withPages = parameters.Contains("pages");
			withPreviews = parameters.Contains("preview");
		}


		protected async Task<int> BuildSectionToc(
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
					index = await BuildSectionToc(one, children, elements, index, pageLevel);
					container.Elements().Last().Add(children);
				}
				else if (pageLevel == level)
				{
					var link = one.GetHyperlink(pageID, string.Empty);
					var name = element.Attribute("name").Value;

					if (progress != null)
					{
						progress.SetMessage(name);
						progress.Increment();
					}

					var text = withPreviews
						? $"<a href=\"{link}\">{name}</a> {await GetPagePreview(one, pageID, css)}"
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


		private async Task<string> GetPagePreview(OneNote one, string pageID, string css)
		{
			var page = await one.GetPage(pageID, OneNote.PageDetail.Basic);
			var ns = page.Namespace;

			var outline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => !e.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank));

			if (outline == null)
			{
				return string.Empty;
			}

			logger.WriteLine($"page {page.Title}");

			// sanitize the content, extracting only raw text and aggregating lines
			var preview = outline.Descendants(ns + "T")?.Nodes().OfType<XCData>()
				.Select(c => c.GetWrapper().Value).Aggregate(string.Empty, (a, b) => $"{a} {b}");

			if (preview == null || preview.Length == 0)
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
	}
}
