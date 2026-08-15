//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Adds a new section group to the top level of the current notebook
	/// </summary>
	internal class AddTopSectionGroupCommand : Command
	{
		public AddTopSectionGroupCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			// full notebook tree, needed so that UpdateHierarchy below round-trips every
			// existing top-level Section/SectionGroup (including any that are themselves
			// empty, or nested several section groups deep); omitting an existing sibling
			// from that call would cause OneNote to delete it
			var notebook = await one.GetNotebook();
			if (notebook is null)
			{
				return;
			}

			var ns = one.GetNamespace(notebook);

			// top-level Section/SectionGroup names only (Elements, not Descendants) so an
			// empty notebook, or one containing only top-level section groups regardless of
			// what they themselves contain, is handled the same as any other notebook
			var existingNames = notebook.Elements(ns + "Section")
				.Concat(notebook.Elements(ns + "SectionGroup"))
				.Where(e => e.Attribute("isRecycleBin") is null && e.Attribute("isInRecycleBin") is null)
				.Attributes("name")
				.Select(a => a.Value)
				.ToHashSet(StringComparer.CurrentCultureIgnoreCase);

			var baseName = Resx.AddTopSectionGroupCommand_DefaultName;
			var name = baseName;
			if (existingNames.Contains(name))
			{
				var n = 2;
				while (existingNames.Contains($"{baseName} {n}"))
				{
					n++;
				}

				name = $"{baseName} {n}";
			}

			notebook.Add(new XElement(ns + "SectionGroup", new XAttribute("name", name)));

			try
			{
				one.UpdateHierarchy(notebook);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error creating top-level section group", exc);
				ShowError(Resx.AddTopSectionGroupCommand_Error);
			}
		}
	}
}
