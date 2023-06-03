

namespace River.OneMoreAddIn.Commands.Navigator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Linq;



    internal class NaviPagesService
    {
        private readonly IWin32Window owner;
        private readonly OneNote one;
        private readonly string sectionId;

        public NaviPagesService(IWin32Window owner, OneNote one, string sectionId)
        {
            this.owner = owner;
            this.one = one;
            this.sectionId = sectionId;
        }


        private async Task MovePageAfter1(string CurPageID, string DestPageID)
        {
            //string RecycleBinSec = "onenote recyclebin";

            var root = one.GetAllPages();
            var ns = root.Name.Namespace;
            var CurSectionId = one.CurrentSectionId;
            //var sections = new Dictionary<string, XElement>();
            var CurSection = one.GetSection(CurSectionId);
            var DestSection = one.GetSection(DestPageID);
            var DestSectionId = one.GetParent(DestPageID);

            var CurPageNode = CurSection.Descendants(ns + "Page").Where(n => n.Attribute("ID").Value == CurPageID).FirstOrDefault();
            var ClonePageNode = CurPageNode;
            CurPageNode.Remove();

            var DestPageNode = DestSection.Descendants(ns + "Page").Where(n => n.Attribute("ID").Value == DestPageID).FirstOrDefault();

            DestPageNode.AddAfterSelf(ClonePageNode);


            // update each source section
            one.UpdateHierarchy(CurSection);

            // update target section
            one.UpdateHierarchy(DestSection);

            // navigate after progress dialog is closed otherwise it will hang!
            await one.NavigateTo(DestSectionId);

        }

    }
}
