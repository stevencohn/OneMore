//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
    using River.OneMoreAddIn.Commands.Navigator;
    using River.OneMoreAddIn.Commands.Search;
    using System;
    using System.Collections.Generic;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Resx = River.OneMoreAddIn.Properties.Resources;
    using System.Xml.Linq;
    using System.Linq;




    internal class NaviPagesCommand : Command
    {
        private bool copying;
        private List<string> pageIds;

        public NaviPagesCommand()
        {
            

        }


        public override async Task Execute(params object[] args)
        {
            logger.WriteLine("command starts");
            copying = false;

            var dialog = new NaviPages();
            await dialog.RunModeless((sender, e) =>
            {
                var d = sender as NaviPages;
                if (d.DialogResult == DialogResult.OK)
                {
                    //copying = dialog.CopySelections;
                    //pageIds = dialog.SelectedPages;
                    ////DestPgID = dialog.DestPageID;

                    //var action = copying ? "copying" : "moving";

                    //var desc = copying
                    //    ? Resx.SearchQF_DescriptionCopy
                    //    : Resx.SearchQF_DescriptionMove;

                    //using var one = new OneNote();
                    //one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);

                    try
                    {
                        using var one = new OneNote();
                        //var service = new SearchServices(owner, one, sectionId);

                        if (copying)
                        {
                            //await service.CopyPages(pageIds);
                        }
                        else
                        {
                            //await MovePageAfter(CurPgID, DestPgID);
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.WriteLine(exc);
                    }
                    finally
                    {
                        logger.End();
                    }
                
                
                }
                logger.WriteLine("Navi pages window pop-up");

            },
            20);
            logger.WriteLine("command ends");
            await Task.Yield();
        }


        private async Task Callback(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                // cancelled
                return;
            }

            var action = copying ? "copying" : "moving";
            logger.Start($"..{action} {pageIds.Count} pages");

            try
            {
                using var one = new OneNote();
                var service = new SearchServices(owner, one, sectionId);

                if (copying)
                {
                    await service.CopyPages(pageIds);
                }
                else
                {
                    return;

                }
            }
            catch (Exception exc)
            {
                logger.WriteLine(exc);
            }
            finally
            {
                logger.End();
            }
        }



    }
}
