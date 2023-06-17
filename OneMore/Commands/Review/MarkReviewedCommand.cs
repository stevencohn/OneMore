//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
    using River.OneMoreAddIn.Commands.Review;
    using River.OneMoreAddIn.Models;
    using System;
    using System.Threading.Tasks;


    internal class MarkReviewedCommand : Command
    {

        public MarkReviewedCommand()
        {
        }


        public override async Task Execute(params object[] args)
        {
            using (var one = new OneNote(out var page, out var ns))
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string content = page.GetMetaContent(MetaNames.ReviewInfo);

                ReviewInfo review_info = new ReviewInfo(page);
                review_info.last_date = date;
                review_info.SetReviewMeta(page);
                review_info.MakeReviewBank(page, ns);

                await one.Update(page);
            }
        }
    }
}
