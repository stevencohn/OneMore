//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	using System.Drawing;


	internal class SearchEngine
	{
		public SearchEngine()
		{
		}


		public SearchEngine(Image image, string name, string uri)
		{
			Image = image;
			Name = name;
			Uri = uri;
		}


		public Image Image { get; set; }


		public string Name { get; set; }


		public string Uri { get; set; }
	}
}
