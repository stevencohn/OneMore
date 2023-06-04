

namespace River.OneMoreAddIn.Commands.Navigator
{
    using River.OneMoreAddIn.UI;
    using River.OneMoreAddIn;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using Resx = River.OneMoreAddIn.Properties.Resources;
    using System.Xml;
    using System.Drawing;
    using Chinese;
    using System.Text;
    using NPinyin;
    using System.Diagnostics;
    using Microsoft.Office.Interop.Word;
    using System.Security.Policy;

    internal partial class NaviPages : LocalizableForm
    {
        //private readonly OneNote one;
        public NaviPages()
        {
            InitializeComponent();

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using var one = new OneNote();
            var AllPageXML = one.GetAllPages();
            //hierarchyView11.CheckBoxes = false;
            hierarchyView11.Nodes.Clear();
            hierarchyView11.Populate(AllPageXML, one.GetNamespace(AllPageXML));
            logger.WriteLine("pages loaded");
        }
        public bool CopySelections { get; private set; }


        public string CurPageID { get; private set; }
        public string SelectedNodeID; 

        /// <summary>
        /// Click the node in TreeView to nvaigate to the clicked page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // async event handlers should be be declared 'async void'
        private async void ClickNode(object sender, TreeNodeMouseClickEventArgs e)
        {
            // thanksfully, Bounds specifies bounds of label
            var node = e.Node as HierarchyNode;
            using var one = new OneNote();
            if (node.Hyperlinked && e.Node.Bounds.Contains(e.Location))
            {
                var pageId = node.Root.Attribute("ID").Value;
                if (!pageId.Equals(one.CurrentPageId))
                {
                    await one.NavigateTo(pageId);
                    Close();
                }
            }
        }

        /// <summary>
        /// allow to check only one node in a TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOneNode(object sender, TreeViewEventArgs e)
        {
            // only do it if the node became checked:
            var node = e.Node as HierarchyNode;
            //SelectedNodeID = node.Root.Attribute("ID").Value;
            if (e.Node.Checked)
            {
                UncheckNodes(hierarchyView11.Nodes, e.Node);
            }

            SelectedNodeID = node.Root.Attribute("ID").Value;

        }
        private void UncheckNodes(TreeNodeCollection nodes, TreeNode except)
        {
            foreach (TreeNode inode in nodes)
            {
                if (inode != except) inode.Checked = false;
                UncheckNodes(inode.Nodes, except);
            }
        }
        private async void ClickMoveTo(object sender, EventArgs e)

        {
            using var one = new OneNote();

            var DestPageID = SelectedNodeID;
            CurPageID = one.CurrentPageId;
            if(DestPageID!= CurPageID && !String.IsNullOrEmpty(DestPageID))
            {
                await one.MovedAfterPage(CurPageID, DestPageID);
            }

        }
        
        /// <summary>
        /// Conver Chinese sting into PinYin first letter, English Letters is kept
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        private string PinYinFirstLetter(string strInput)
        {
            Encoding gb2312 = Encoding.GetEncoding("GB2312");
            string s = NPinyin.Pinyin.ConvertEncoding(strInput, Encoding.UTF8, gb2312);
            return NPinyin.Pinyin.GetInitials(s, gb2312);
        }

        /// <summary>
        /// Indexing the pages by PinYin first letter
        /// </summary>
        /// <param name="inXML"></param>
        /// <param name="strKeyWords"></param>
        /// <returns></returns>
        public XElement FilterXml(XElement inXML, string strKeyWords)
        {
            string strSecKw, strPageKw;
            XElement root = inXML;
            if (strKeyWords.Contains(" "))
            {
                strSecKw = strKeyWords.Substring(0, strKeyWords.IndexOf(" "));  //sub-string before SPACE
                strPageKw = strKeyWords.Substring(strKeyWords.IndexOf(" ") + 1); //sub-string after SPACE
            }
            else
            {
                strSecKw = strKeyWords;
                strPageKw = null;
            }

            string RecycleBinSec = "onenote recyclebin";

            foreach (var NBSec in root.Elements().Elements())
            {
                string NBSecName = PinYinFirstLetter(NBSec.Attribute("name").Value).ToLower();
                if (!NBSecName.Contains(strSecKw) || NBSecName.Contains(RecycleBinSec))
                    NBSec.RemoveAll();

            }
            if (strPageKw != null)
            {
                foreach (var NBPage in root.Elements().Elements().Elements())
                {
                    string NBPageName = PinYinFirstLetter(NBPage.Attribute("name").Value).ToLower();
                    if (!NBPageName.Contains(strPageKw))
                        NBPage.RemoveAll();
                }
            }

            // delete empty page
            foreach (XElement child in root.Descendants().Reverse())
            {
                if (!child.HasElements && string.IsNullOrEmpty(child.Value) && !child.HasAttributes) child.Remove();

            }

            // delete empty section
            foreach (XElement child in root.Elements().Elements().Reverse())
            {
                if (!child.HasElements) child.Remove();

            }

            //delete empty notebook
            foreach (var child1 in root.Elements().Reverse())
            {
                if (!child1.HasElements) child1.Remove();
            }
            return root;
        }

             
        private void tBoxKw_TextChanged(object sender, EventArgs e)
        {
            using var one = new OneNote();
            XElement FilteredPageXml;
            var AllPageXML = one.GetAllPages();
            try
            {
                string strKeyWords = tBoxKw.Text.ToLower();
                if(strKeyWords=="")
                {
                    FilteredPageXml = AllPageXML;
                }
                else
                {
                    FilteredPageXml = FilterXml(AllPageXML, strKeyWords);
                }

                hierarchyView11.Nodes.Clear();
                hierarchyView11.Populate(FilteredPageXml, one.GetNamespace(FilteredPageXml));
            }
            catch
            {
                MessageBox.Show("Failed to filter!");
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }

}
