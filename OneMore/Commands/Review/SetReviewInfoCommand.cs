//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
    using River.OneMoreAddIn.Commands.Review;
    using System.Threading.Tasks;

    internal class SetReviewInfoCommand : Command
    {

        public SetReviewInfoCommand()
        {
        }

        public override async Task Execute(params object[] args)
        {
            using (var one = new OneNote(out var page, out var ns))
            {
                ReviewInfo review_info = new ReviewInfo();

                review_info.ParseReviewBank(page, ns);
                review_info.SetReviewMeta(page);
                review_info.MakeReviewBank(page, ns);

                await one.Update(page);
            }
        }
    }
}
