//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Builders
{
	using System.Collections.Generic;
	using System.Xml.Linq;


	/// <summary>
	/// Builds OneNote 2013 hierarchy XML (notebooks/sections/pages) for use in unit tests.
	/// </summary>
	internal class HierarchyBuilder
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private readonly List<XElement> notebooks = new();


		public HierarchyBuilder WithNotebook(string id, string name)
		{
			notebooks.Add(new XElement(Ns + "Notebook",
				new XAttribute("ID", id),
				new XAttribute("name", name),
				new XAttribute("nickname", name)));
			return this;
		}

		public HierarchyBuilder WithSection(string id, string name, string notebookId)
		{
			var nb = FindById(notebookId);
			if (nb != null)
			{
				nb.Add(new XElement(Ns + "Section",
					new XAttribute("ID", id),
					new XAttribute("name", name)));
			}

			return this;
		}

		public HierarchyBuilder WithPage(string id, string name, string sectionId)
		{
			var section = FindById(sectionId);
			if (section != null)
			{
				section.Add(new XElement(Ns + "Page",
					new XAttribute("ID", id),
					new XAttribute("name", name)));
			}

			return this;
		}

		public string Build()
		{
			var root = new XElement(Ns + "Notebooks",
				new XAttribute(XNamespace.Xmlns + "one",
					"http://schemas.microsoft.com/office/onenote/2013/onenote"));

			foreach (var nb in notebooks)
			{
				root.Add(nb);
			}

			return root.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		private XElement FindById(string id)
		{
			foreach (var nb in notebooks)
			{
				if ((string)nb.Attribute("ID") == id)
				{
					return nb;
				}

				foreach (var section in nb.Descendants())
				{
					if ((string)section.Attribute("ID") == id)
					{
						return section;
					}
				}
			}

			return null;
		}
	}
}
