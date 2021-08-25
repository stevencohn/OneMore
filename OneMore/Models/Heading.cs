//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using River.OneMoreAddIn.Styles;
	using System.Xml.Linq;


	/// <summary>
	/// Used by InsertTOC and OutlineFormatting commands to reference 
	/// predefined and user-defined headings on the page
	/// </summary>
	internal class Heading
	{
		/// <summary>
		/// Reference to the heading OE on the page
		/// </summary>
		public XElement Root;


		/// <summary>
		/// Reference to the containing Outline on the page;
		/// used by Split and Merge
		/// </summary>
		public XElement Outline;


		/// <summary>
		/// The extracted text of the heading, or null
		/// </summary>
		public string Text;


		/// <summary>
		/// True if the header is an internal OneNote hyperlink to another page;
		/// used by Split and Merge
		/// </summary>
		public bool IsHyper;


		/// <summary>
		/// The logical indent level of the header (1, 2, 3, ...)
		/// </summary>
		public int Level;


		/// <summary>
		/// A hyperlink to the heading, or null
		/// </summary>
		public string Link;


		/// <summary>
		/// The associated style describing the heading
		/// </summary>
		public StyleBase Style;
	}
}