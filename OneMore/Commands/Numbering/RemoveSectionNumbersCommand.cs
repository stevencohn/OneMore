//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class RemoveSectionNumbersCommand : Command
	{
		private Regex pattern;


		public RemoveSectionNumbersCommand()
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

					if (RemoveNumbering(notebook, one.GetNamespace(notebook)) > 0)
					{
						one.UpdateHierarchy(notebook);
					}
				}
			}

			await Task.Yield();
		}


		private int RemoveNumbering(XElement root, XNamespace ns)
		{
			var sections = root.Elements(ns + "Section")
				.Where(e => !e.Attributes().Any(a => a.Name.LocalName.Contains("RecycleBin")))
				.ToList();

			foreach (var section in sections)
			{
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
						var stripped = name.Value.Substring(match.Groups[1].Length);

						// only rename if not duplicate
						if (!sections.Any(s => s.Attribute("name").Value == stripped))
						{
							name.Value = stripped;
						}
					}
				}
			}

			int count = sections.Count;

			var groups = root.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null)
				.ToList();

			foreach (var group in groups)
			{
				count += RemoveNumbering(group, ns);
			}

			return count;
		}
	}
}
