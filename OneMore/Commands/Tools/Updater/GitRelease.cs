//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable IDE1006 // Naming Styles

namespace River.OneMoreAddIn.Commands.Tools.Updater
{
    using System.Collections.Generic;


    internal class GitRelease
	{
        public string url { get; set; }

        public string html_url { get; set; }
        
        public string id { get; set; }

		public string tag_name { get; set; }

        public string name { get; set; }

        public string published_at { get; set; }

        public List<GitAsset> assets { get; set; }
	}
}
/*
{
  "url": "https://api.github.com/repos/stevencohn/OneMore/releases/29812581",
  "assets_url": "https://api.github.com/repos/stevencohn/OneMore/releases/29812581/assets",
  "upload_url": "https://uploads.github.com/repos/stevencohn/OneMore/releases/29812581/assets{?name,label}",
  "html_url": "https://github.com/stevencohn/OneMore/releases/tag/2.8",
  "id": 29812581,
  "node_id": "MDc6UmVsZWFzZTI5ODEyNTgx",
  "tag_name": "2.8",
  "target_commitish": "master",
  "name": "Add Table Formula Commands",
  "draft": false,
  "author": {},
  "prerelease": false,
  "created_at": "2020-08-18T11:53:31Z",
  "published_at": "2020-08-18T12:00:46Z",
  "assets": [
    {
      "url": "https://api.github.com/repos/stevencohn/OneMore/releases/assets/24043328",
      "id": 24043328,
      "node_id": "MDEyOlJlbGVhc2VBc3NldDI0MDQzMzI4",
      "name": "OneMore_2.8_Setupx86.msi",
      "label": null,
      "uploader": {
        "login": "stevencohn",
        "id": 9964757,
        "node_id": "MDQ6VXNlcjk5NjQ3NTc=",
        "avatar_url": "https://avatars1.githubusercontent.com/u/9964757?v=4",
        "gravatar_id": "",
        "url": "https://api.github.com/users/stevencohn",
        "html_url": "https://github.com/stevencohn",
        "followers_url": "https://api.github.com/users/stevencohn/followers",
        "following_url": "https://api.github.com/users/stevencohn/following{/other_user}",
        "gists_url": "https://api.github.com/users/stevencohn/gists{/gist_id}",
        "starred_url": "https://api.github.com/users/stevencohn/starred{/owner}{/repo}",
        "subscriptions_url": "https://api.github.com/users/stevencohn/subscriptions",
        "organizations_url": "https://api.github.com/users/stevencohn/orgs",
        "repos_url": "https://api.github.com/users/stevencohn/repos",
        "events_url": "https://api.github.com/users/stevencohn/events{/privacy}",
        "received_events_url": "https://api.github.com/users/stevencohn/received_events",
        "type": "User",
        "site_admin": false
      },
      "content_type": "application/octet-stream",
      "state": "uploaded",
      "size": 919552,
      "download_count": 12,
      "created_at": "2020-08-18T12:00:26Z",
      "updated_at": "2020-08-18T12:00:26Z",
      "browser_download_url": "https://github.com/stevencohn/OneMore/releases/download/2.8/OneMore_2.8_Setupx86.msi"
    }
  ],
  "tarball_url": "https://api.github.com/repos/stevencohn/OneMore/tarball/2.8",
  "zipball_url": "https://api.github.com/repos/stevencohn/OneMore/zipball/2.8",
  "body": ""
} */
