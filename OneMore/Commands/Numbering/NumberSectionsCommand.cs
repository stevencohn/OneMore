//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	internal class NumberSectionsCommand : Command
	{
		private Regex pattern;
		private XNamespace ns;


		public NumberSectionsCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var notebooks = manager.GetHierarchy(HierarchyScope.hsSections);
				ns = notebooks.GetNamespaceOfPrefix("one");

				var notebook = notebooks.Elements(ns + "Notebook")
					.FirstOrDefault(e => e.Attribute("isCurrentlyViewed") != null);

				if (notebook != null)
				{
					pattern = new Regex(@"^(\(\d+\)\s).+");

					if (ApplyNumbering(notebook) > 0)
					{
						manager.UpdateHierarchy(notebook);
					}
				}
			}
		}


		private int ApplyNumbering(XElement root)
		{
			var sections = root.Elements(ns + "Section").ToList();

			for (int i = 0; i < sections.Count; i++)
			{
				var section = sections[i];

				if (section.ReadAttributeValue("locked", out bool locked, false))
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

					name.Value = $"({i + 1}) {name.Value}";
				}
			}

			var groups = root.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null)
				.ToList();

			int count = sections.Count;

			foreach (var group in groups)
			{
				count += ApplyNumbering(group);
			}

			return count;
		}
	}
}
