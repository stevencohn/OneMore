//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class RemoveSectionNumbersCommand : Command
	{
		private Regex pattern;
		private XNamespace ns;


		public RemoveSectionNumbersCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var notebooks = manager.GetHierarchy(HierarchyScope.hsSections);
				ns = notebooks.GetNamespaceOfPrefix("one");

				var notebook = notebooks.Elements(ns + "Notebook")
					.Where(e => e.Attribute("isCurrentlyViewed") != null)
					.FirstOrDefault();

				if (notebook != null)
				{
					pattern = new Regex(@"^(\(\d+\)\s).+");

					if (RemoveNumbering(notebook) > 0)
					{
						manager.UpdateHierarchy(notebook);
					}
				}
			}
		}


		private int RemoveNumbering(XElement root)
		{
			var sections = root.Elements(ns + "Section").ToList();

			foreach (var section in sections)
			{
				if (section.ReadAttributeValue("locked", out bool locked, false) == true)
				{
					continue;
				}

				var name = section.Attributes("name").FirstOrDefault();
				if (!string.IsNullOrEmpty(name?.Value))
				{
					// numeric 1.
					var match = pattern.Match(name.Value);
					if (match.Success)
					{
						name.Value = name.Value.Substring(match.Groups[1].Length);
					}
				}
			}

			int count = sections.Count;

			var groups = root.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null)
				.ToList();

			foreach (var group in groups)
			{
				count += RemoveNumbering(group);
			}

			return count;
		}
	}
}
