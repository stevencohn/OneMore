//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{

	/// <summary>
	/// Provides an on-demand instantiation of schema arrays.
	/// </summary>
	internal sealed class PageSchema
	{
		private string[] outHeaders;  // Outline info elements
		private string[] outContent;  // Outline content elements
		private string[] oecHeaders;  // OEChildren info elements
		private string[] oecContent;  // OEChildren content elements
		private string[] oeHeaders;   // OE info elements
		private string[] oeContent;   // OE content elements


		// Outline Schema...

		public string[] OutHeaders => outHeaders ??= new string[]
		{
			"Postion", "Size", "Meta", "Indents"
		};

		public string[] OutContent => outContent ??= new string[]
		{
			"OEChildren"
		};


		// OEChildren Schema...

		public string[] OecHeaders => oecHeaders ??= new string[]
		{
			"ChildOELayout"
		};

		public string[] OecContent => oecContent ??= new string[]
		{
			"OE", "HTMLBlock"
		};


		// OE Schema...

		public string[] OeHeaders => oeHeaders ??= new string[]
		{
			"MediaIndex", "Tag", "OutlookTask"/*+Tag?*/, "Meta", "List"
		};

		public string[] OeContent => oeContent ??= new string[]
		{
			"Image", "Table", "InkDrawing", "InsertedFile",
			"MediaFile", "InkParagraph", "FutureObject",
			"T", "InkWord",
			"OEChildren",
			"LinkedNote"
		};


		public string[] GetContentNames(string containerName)
		{
			if (containerName == "OE") return OeContent;
			if (containerName == "OEChildren") return OecContent;
			if (containerName == "Outline") return OutContent;
			return new string[0];
		}
	}
}
