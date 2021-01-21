//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class NumberSectionsCommand : Command
	{
		private Regex pattern;


		public NumberSectionsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var notebook = one.GetNotebook();
				if (notebook != null)
				{
					pattern = new Regex(@"^(\(\d+\)\s).+");

					if (ApplyNumbering(notebook, one.GetNamespace(notebook)) > 0)
					{
						one.UpdateHierarchy(notebook);
					}
				}
			}

			await Task.Yield();
		}


		private int ApplyNumbering(XElement root, XNamespace ns)
		{
			var sections = root.Elements(ns + "Section").ToList();

			for (int i = 0; i < sections.Count; i++)
			{
				var section = sections[i];

				section.ReadAttributeValue("locked", out bool locked, false);
				if (locked)
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
				count += ApplyNumbering(group, ns);
			}

			return count;
		}
	}
}
