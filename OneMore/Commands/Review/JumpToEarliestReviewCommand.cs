//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
    using River.OneMoreAddIn.Commands.Review;
    using River.OneMoreAddIn.Models;
    using System.Linq;
    using System.Threading.Tasks;


    internal class JumpToEarliestReviewCommand : Command
    {
        public JumpToEarliestReviewCommand()
        {
        }


        public override async Task Execute(params object[] args)
        {
            using (var one = new OneNote(out var page, out var ns))
            {
                int min_days = int.MaxValue;
                string ret_page = page.PageId;
                bool has_review_info = true;

                var hierarchy = await one.GetNotebook(OneNote.Scope.Pages);
                hierarchy.Descendants(ns + "SectionGroup")
                        .Where(e => e.Attribute("isRecycleBin") != null)
                        .ToList()
                        .ForEach(e => e.Remove());
                var page_list = hierarchy.Descendants(ns + "Page");

                foreach (var cur_page in page_list)
                {
                    string cur_page_id = cur_page.Attribute("ID").Value;
                    if (cur_page_id == page.PageId)
                    {
                        continue;
                    }

                    var meta = cur_page.Elements(ns + "Meta")
                                       .Where(x => x.Attribute("name").Value == MetaNames.ReviewInfo)
                                       .FirstOrDefault();
                    if (meta == null)
                    {
                        ret_page = cur_page_id;
                        has_review_info = false;
                        break;
                    }

                    ReviewInfo review_info = new ReviewInfo(meta.Attribute("content").Value);
                    if (review_info.interval == 0)
                    {
                        ret_page = cur_page_id;
                        break;
                    }

                    int days = review_info.remain;
                    if (days < min_days)
                    {
                        min_days = days;
                        ret_page = cur_page_id;
                    }
                }

                Page one_page = await one.GetPage(ret_page);
                if (has_review_info)
                {
                    (new ReviewInfo(one_page)).MakeReviewBank(one_page, ns);
                }

                await one.NavigateTo(ret_page);
                await one.Update(one_page);
            }
        }
    }
}
