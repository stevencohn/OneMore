//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Chinese;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Displays the number of words on the current page or in the selected region. 
	/// </summary>
	/// <remarks>
	/// The word count may differ - be slightly lower - than the word count reported by Microsoft
	/// Word because Word counts things like URLs as a single word but OneMore separates the
	/// individual words in the URL. For example, Word reports one word in
	/// "https://github.com/OneMore" whereas OneMore counts it as four words.
	/// </remarks>
	internal class WordCountCommand : Command
	{
		private const string CJKPattern = @"\p{IsCJKUnifiedIdeographs}+";

		private const string HeaderShading = "#DEEBF6";
		private const string Header2Shading = "#F2F2F2";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

		private OneNote one;
		private XNamespace ns;
		private OneNote.Scope scope;
		private int grandTotal;
		private int grandTotalPages;
		private int heading2Index;
		private Regex regex;
		private UI.ProgressDialog progress;


		public WordCountCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			regex = new Regex(CJKPattern);

			await using (one = new OneNote())
			{
				scope = args.Length > 0 && args[0] is OneNote.Scope ascope
					? ascope
					: OneNote.Scope.Self;

				if (scope == OneNote.Scope.Self)
				{
					// words on the current page...

					var (count, wholePage) = await WordsOnPage(one.CurrentPageId);

					if (wholePage)
					{
						ShowInfo(string.Format(Resx.WordCountCommand_Count, count));
					}
					else
					{
						ShowInfo(string.Format(Resx.WordCountCommand_Selected, count));
					}
				}
				else
				{
					// words on pages within scope...

					await ReportWordCounts(scope);
				}
			}
		}


		private async Task<(int, bool)> WordsOnPage(string pageID)
		{
			var page = await one.GetPage(pageID,
				// only use Selection for current page,
				// otherwise counts might be off for pages other than the current page
				scope == OneNote.Scope.Self
					? OneNote.PageDetail.Selection
					: OneNote.PageDetail.Basic);

			var runs = scope == OneNote.Scope.Self
				? new SelectionRange(page).GetSelections(defaulToAnytIfNoRange: true)
				: page.Root.Descendants(ns + "T");

			var count = 0;

			foreach (var run in runs)
			{
				var cdatas = run.DescendantNodes().OfType<XCData>();
				foreach (var cdata in cdatas)
				{
					var text = cdata.GetWrapper().Value.Trim();
					if (text.Length > 0)
					{
						//logger.WriteLine($"counting '{text}'");

						if (regex.IsMatch(text))
						{
							// works well for Chinese but is questionable for JP and KO
							count += ChineseTokenizer.SplitWords(text).Count();
						}
						else
						{
							count += Regex.Matches(text, @"[\w]+").Count;
						}
					}
				}
			}

			// presume whole page if any non-selected runs were included
			var wholePage = runs.Any(e => e.Attribute("selected") is null);

			return (count, wholePage);
		}


		private async Task ReportWordCounts(OneNote.Scope scope)
		{
			grandTotal = grandTotalPages = 0;

			one.CreatePage(one.CurrentSectionId, out var pageId);
			var page = await one.GetPage(pageId);
			page.Title = Resx.WordCountsCommand_Title;
			page.SetMeta(MetaNames.WordCount, "true");
			page.Root.SetAttributeValue("lang", "yo");

			ns = page.Namespace;
			PageNamespace.Set(ns);

			var heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
			heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

			using (progress = new UI.ProgressDialog())
			{
				progress.Show();

				var container = page.EnsureContentContainer();

				if (scope == OneNote.Scope.Pages)
				{
					var section = await one.GetSection();
					progress.SetMaximum(section.Elements().Count());

					var notebook = await one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Self);
					var name = notebook.Attribute("name").Value;

					container.Add(
						new Paragraph(string.Format(
							Resx.WordCounts_Notebook, name)).SetQuickStyle(heading1Index),
						new Paragraph(string.Empty)
						);

					ReportSection(page, container, section);
				}
				else
				{
					var notebook = await one.GetNotebook(OneNote.Scope.Pages);

					var count = notebook.Descendants(ns + "Page")
						.Count(e =>
							e.Attribute("ID").Value != pageId &&
							e.Attribute("isInRecycleBin") == null);

					progress.SetMaximum(count);

					var name = notebook.Attribute("name").Value;

					container.Add(
						new Paragraph(string.Format(
							Resx.WordCounts_Notebook, name)).SetQuickStyle(heading1Index),
						new Paragraph(string.Empty)
						);

					notebook.Elements(ns + "Section").ForEach(section =>
					{
						ReportSection(page, container, section);
					});

					ReportGrandTotal(container);
				}

				progress.SetMessage("Updating report...");
				await one.Update(page);
			}

			await one.NavigateTo(page.PageId);
		}


		private void ReportSection(Page page, XElement container, XElement section)
		{
			var name = section.Attribute("name").Value;
			container.Add(new Paragraph(string.Format(
				Resx.WordCounts_Section, name)).SetQuickStyle(heading2Index));

			var table = new Table(ns, 1, 2)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 400);
			table.SetColumnWidth(1, 80);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(Resx.word_Page).SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(Resx.word_Words).SetStyle(HeaderCss).SetAlignment("center"));

			var total = 0;
			var pages = 0;

			section.Elements(ns + "Page")
				.Where(e =>
					e.Attribute("ID").Value != page.PageId &&
					e.Attribute("isInRecycleBin") == null)
				.ForEach(async child =>
			{
				var row = table.AddRow();

				name = child.Attribute("name").Value;
				progress.SetMessage(name);
				progress.Increment();

				row[0].SetContent(new Paragraph(name));

				var (count, _) = await WordsOnPage(child.Attribute("ID").Value);

				pages++;
				total += count;
				grandTotal += count;

				row[1].SetContent(new Paragraph(count.ToString("n0")).SetAlignment("center"));
			});

			grandTotalPages += pages;

			row = table.AddRow();
			var text = string.Format(Resx.WordCounts_SectionTotal, pages);

			row[0].SetContent(new Paragraph(text).SetStyle(HeaderCss).SetAlignment("right"));
			row[1].SetContent(new Paragraph(total.ToString("n0")).SetAlignment("center"));

			container.Add(
				new Paragraph(table.Root),
				new Paragraph(string.Empty)
				);
		}


		private void ReportGrandTotal(XElement container)
		{
			var table = new Table(ns, 1, 2)
			{
				BordersVisible = true
			};

			table.SetColumnWidth(0, 400);
			table.SetColumnWidth(1, 80);

			var row = table[0];
			var text = string.Format(Resx.WordCounts_NotebookTotal, grandTotalPages);

			row[0].ShadingColor = Header2Shading;
			row[0].SetContent(new Paragraph(text).SetStyle(HeaderCss).SetAlignment("right"));
			row[1].SetContent(new Paragraph(grandTotal.ToString("n0")).SetAlignment("center"));

			container.Add(new Paragraph(table.Root));
		}
	}
}
