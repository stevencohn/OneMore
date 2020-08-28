//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
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
		public XElement Element;


		/// <summary>
		/// The extracted text of the heading, or null
		/// </summary>
		public string Text;


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