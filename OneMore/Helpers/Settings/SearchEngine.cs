//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	internal class SearchEngine
	{
		public SearchEngine()
		{
		}


		public SearchEngine(string name, string uri)
		{
			Name = name;
			Uri = uri;
		}


		public string Name { get; set; }


		public string Uri { get; set; }
	}
}
