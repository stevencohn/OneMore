//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System.Linq;


	internal class RemoveAuthorsCommand : Command
	{
		public RemoveAuthorsCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var count = 0;
				var page = new Page(manager.CurrentPage());

				var elements = page.Root.Descendants().Where(d => 
					d.Name.LocalName == "Table" ||
					d.Name.LocalName == "Row" ||
					d.Name.LocalName == "Cell" ||
					d.Name.LocalName == "Outline" ||
					d.Name.LocalName == "OE")
					.ToList();

				foreach (var element in elements)
				{
					var atts = element.Attributes().Where(a =>
						a.Name == "author" ||
						a.Name == "authorInitials" ||
						a.Name == "authorResolutionID" ||
						a.Name == "lastModifiedBy" ||
						a.Name == "lastModifiedInitials" ||
						a.Name == "lastModifiedByResolutionID"
						)
						.ToList();

					foreach (var att in atts)
					{
						att.Remove();
						count++;
					}
				}

				if (count > 0)
				{
					logger.WriteLine($"clean {count} author attributes");
					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
/*
 * Table, Row, Cell, Outline, OE
 * 
	<xsd:attributeGroup name="EditedByAttributes">
		<xsd:annotation>
			<xsd:documentation xml:lang="en">
				Set of properties to define who edited what when.
				If not set on a child element, it has the same setting as the parent.
			</xsd:documentation>
		</xsd:annotation>
		<xsd:attribute name="author" type="xsd:string"/>
		<xsd:attribute name="authorInitials" type="xsd:string"/>
		<xsd:attribute name="authorResolutionID" type="xsd:string"/>
		<xsd:attribute name="lastModifiedBy" type="xsd:string"/>
		<xsd:attribute name="lastModifiedByInitials" type="xsd:string"/>
		<xsd:attribute name="lastModifiedByResolutionID" type="xsd:string"/>
		<xsd:attribute name="creationTime" type="xsd:dateTime"/>
	</xsd:attributeGroup>
*/
