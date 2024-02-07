//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
    using River.OneMoreAddIn.Commands.Review;
    using River.OneMoreAddIn.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class SummarizeReviewInfoCommand : Command
    {
        private const string HeaderShading = "#DEEBF6";
        private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";

        private OneNote one;
        private XNamespace ns;

        private UI.ProgressDialog progress;

        public SummarizeReviewInfoCommand()
        {
        }

        public override async Task Execute(params object[] args)
        {
            using (one = new OneNote(out var _, out var ns))
            {
                var hierarchy = await one.GetNotebook(OneNote.Scope.Pages);
                var order_info_list = GetOrderedReviewInfos(hierarchy, ns);

                one.CreatePage(one.CurrentSectionId, out var review_page_id);
                var review_page = await ReviewPageInit(review_page_id);

                using (progress = new UI.ProgressDialog())
                {
                    progress.SetMaximum(5);
                    progress.Show(owner);

                    var container = review_page.EnsureContentContainer();

                    ReportPagesInfo(container, order_info_list);

                    progress.SetMessage("Updating report...");
                }

                await one.NavigateTo(review_page_id);
                await one.Update(review_page);
            }
        }

        private List<ReviewInfo> GetOrderedReviewInfos(XElement hierarchy, XNamespace ns)
        {
            hierarchy.Descendants(ns + "SectionGroup")
                        .Where(e => e.Attribute("isRecycleBin") != null)
                        .ToList()
                        .ForEach(e => e.Remove());

            var page_list = hierarchy.Descendants(ns + "Page");

            var review_info_list = new List<ReviewInfo>();

            foreach (var cur_page in page_list)
            {
                string cur_page_id = cur_page.Attribute("ID").Value;
                var meta = cur_page.Elements(ns + "Meta")
                                   .Where(x => x.Attribute("name").Value == MetaNames.ReviewInfo)
                                   .FirstOrDefault();
                var content = (meta == null) ? "" : meta.Attribute("content").Value;
                review_info_list.Add(new ReviewInfo(cur_page_id, content));
            }

            var order_info_list = review_info_list.OrderBy(x => x.remain).ThenBy(x => x.last_date).ToList();
            return order_info_list;
        }

        private async Task<Page> ReviewPageInit(string pageID)
        {
            var review_page = await one.GetPage(pageID);

            review_page.Title = "Pages Reivew Info";
            review_page.SetMeta(MetaNames.AnalysisReport, "true");
            review_page.Root.SetAttributeValue("lang", "yo");

            ns = review_page.Namespace;
            PageNamespace.Set(ns);

            return review_page;
        }

        private void ReportPagesInfo(XElement container, List<ReviewInfo> page_list)
        {
            var table = CreateReviewTable();

            progress.SetMaximum(page_list.Count());

            foreach (var page in page_list)
            {
                AddPageInfo(table, page);
            }

            container.Add(
                new Paragraph(table.Root),
                new Paragraph(string.Empty)
                );
        }

        private Table CreateReviewTable()
        {
            var table = new Table(ns, 1, 5)
            {
                HasHeaderRow = true,
                BordersVisible = true
            };

            table.SetColumnWidth(0, 100);

            var row = table[0];
            row.SetShading(HeaderShading);
            row[0].SetContent(new Paragraph("Page").SetStyle(HeaderCss).SetAlignment("center"));
            row[1].SetContent(new Paragraph("Review interval").SetStyle(HeaderCss).SetAlignment("center"));
            row[2].SetContent(new Paragraph("Last").SetStyle(HeaderCss).SetAlignment("center"));
            row[3].SetContent(new Paragraph("Next").SetStyle(HeaderCss).SetAlignment("center"));
            row[4].SetContent(new Paragraph("Remain").SetStyle(HeaderCss).SetAlignment("center"));

            return table;
        }

        private async void AddPageInfo(Table table, ReviewInfo page_info)
        {
            var page = await one.GetPage(page_info.page_id);

            if (page.GetMetaContent(MetaNames.AnalysisReport) == "true")
            {
                // skip a previously generated analysis report
                return;
            }

            progress.SetMessage(page.Title);
            progress.Increment();

            var link = one.GetHyperlink(page_info.page_id, string.Empty);

            var row = table.AddRow();

            row[0].SetContent(new Paragraph(
                $"<a href='{link}'>{page.Title}</a>").SetStyle(HeaderCss));
            if (page_info.interval > 0)
            {
                row[1].SetContent(new Paragraph(page_info.interval.ToString()));
                row[2].SetContent(new Paragraph(page_info.last_date));
                row[3].SetContent(new Paragraph(page_info.next_date));
                row[4].SetContent(new Paragraph(page_info.remain.ToString()));
            }
        }
    }
}
