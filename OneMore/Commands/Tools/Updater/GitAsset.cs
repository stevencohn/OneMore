//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable IDE1006 // Naming Styles

namespace River.OneMoreAddIn.Commands.Tools.Updater
{
    internal class GitAsset
    {
        public string id { get; set; }

        public string name { get; set; }

        public string content_type { get; set; }

        public string browser_download_url { get; set; }
    }
}
