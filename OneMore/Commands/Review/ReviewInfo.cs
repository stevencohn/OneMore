using River.OneMoreAddIn.Models;
using River.OneMoreAddIn.Styles;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace River.OneMoreAddIn.Commands.Review
{
    internal class ReviewInfo
    {
        private static readonly string interval_default = "Unset";

        public string last_date;
        public int interval;
        public string next_date;
        public int remain;
        public string page_id;

        public ReviewInfo(string last_date, int interval)
        {
            this.last_date = last_date;
            this.interval = interval;
            next_date = DateTime.Parse(last_date).AddDays(interval).ToString("yyyy-MM-dd");
            remain = (DateTime.Parse(next_date) - DateTime.Now).Days; ;
        }

        public ReviewInfo(string content)
        {
            GetReviewMeta(content);
        }

        public ReviewInfo(string id, string content)
        {
            page_id = id;
            GetReviewMeta(content);
        }

        public ReviewInfo()
        {
        }

        public ReviewInfo(Page page)
        {
            GetReviewMeta(page);
        }

        private int MakeQuickStyle(Page page)
        {
            var styles = page.GetQuickStyles();
            var style = styles.FirstOrDefault(s => s.Name == MetaNames.ReviewInfo);

            if (style != null)
            {
                return style.Index;
            }

            var quick = StandardStyles.Citation.GetDefaults();
            quick.Index = styles.Max(s => s.Index) + 1;
            quick.Name = MetaNames.ReviewInfo;
            quick.IsBold = true;

            page.AddQuickStyleDef(quick.ToElement(page.Namespace));

            return quick.Index;
        }

        public void MakeReviewBank(Page page, XNamespace ns, string content)
        {
            var quickIndex = MakeQuickStyle(page);

            var outline = page.Root.Elements(ns + "Outline")
                .FirstOrDefault(e => e.Elements().Any(x =>
                    x.Name.LocalName == "Meta" &&
                    x.Attribute("name").Value == MetaNames.ReviewInfo));

            XCData cdata;

            if (outline == null)
            {
                cdata = new XCData(content);

                outline = new XElement(ns + "Outline",
                    new XElement(ns + "Position",
                        new XAttribute("x", "170"),
                        new XAttribute("y", "55"),
                        new XAttribute("z", "0")),
                    new XElement(ns + "Size",
                        new XAttribute("width", "400"),
                        new XAttribute("height", "11"),
                        new XAttribute("isSetByUser", "true")),
                    new XElement(ns + "Meta",
                        new XAttribute("name", MetaNames.ReviewInfo),
                        new XAttribute("content", "1")),
                    new XElement(ns + "OEChildren",
                        new XElement(ns + "OE",
                            new XAttribute("quickStyleIndex", quickIndex),
                            new XElement(ns + "T", cdata)
                        ))
                    );

                page.Root.Elements(ns + "Title").First().AddAfterSelf(outline);
            }
            else
            {
                var one_T = outline.Descendants(ns + "T");
                if (one_T.Count() > 1)
                {
                    one_T.Where(x => x.Attribute("selected") == null).ToList().ForEach(x => x.Remove());
                }
                cdata = one_T.First().GetCData();
                if (cdata != null)
                {
                    cdata.Value = content;
                }
            }
        }

        public void MakeReviewBank(Page page, XNamespace ns)
        {
            RefreshInfo();
            string content = "omReview:";
            content += " Last: " + last_date;
            if (interval == 0)
            {
                content += " Interval: " + interval_default;
            }
            else
            {
                content += " Interval: " + interval.ToString();
                content += " Next: " + next_date;
                content += " Remain: " + remain.ToString();
            }

            MakeReviewBank(page, ns, content);
        }

        public void ParseReviewBank(Page page, XNamespace ns)
        {
            var outline = page.Root.Elements(ns + "Outline")
                               .FirstOrDefault(e => e.Elements().Any(x =>
                                   x.Name.LocalName == "Meta" &&
                                   x.Attribute("name").Value == MetaNames.ReviewInfo));

            if (outline != null)
            {
                var one_T = outline.Descendants(ns + "T");
                string text = "";
                foreach (var t in one_T)
                {
                    text += t.GetCData().Value;
                }
                if (text != null)
                {
                    string[] review_info = GetInfoFromBank(text);
                    if (review_info.Length < 5)
                    {
                        UIHelper.ShowError("Invalid review setting!");
                    }
                    else
                    {
                        last_date = review_info[2];
                        interval = (review_info[4] == interval_default) ? 0 : int.Parse(review_info[4]);
                        RefreshInfo();
                    }
                }
            }
        }

        public void SetReviewMeta(Page page, string content)
        {
            page.SetMeta(MetaNames.ReviewInfo, content);
        }

        public void SetReviewMeta(Page page)
        {
            string content = last_date;
            if (interval != 0)
            {
                content += " " + interval.ToString();
            }
            SetReviewMeta(page, content);
        }

        public void GetReviewMeta(Page page)
        {
            string content = page.GetMetaContent(MetaNames.ReviewInfo);
            GetReviewMeta(content);
        }

        public void GetReviewMeta(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                string[] review_info = content.Split(' ');

                if (review_info.Length > 0)
                {
                    last_date = review_info[0];
                    if (review_info.Length > 1)
                    {
                        interval = int.Parse(review_info[1]);
                    }
                }

                RefreshInfo();
            }
        }

        private void RefreshInfo()
        {
            if (interval != 0)
            {
                next_date = DateTime.Parse(last_date).AddDays(interval).ToString("yyyy-MM-dd");
                remain = (DateTime.Parse(next_date) - DateTime.Now).Days;
            }
        }

        private string[] GetInfoFromBank(string content)
        {
            string pattern = "<[^>]+>";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            string review_bank = rgx.Replace(content, replacement);
            string[] review_info = review_bank.Split(' ');

            return review_info;
        }
    }
}
