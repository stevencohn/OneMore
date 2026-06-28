//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Open a folder as a new notebook.
	/// </summary>
	internal class OpenFolderCommand : Command
	{
		private static bool commandIsActive = false;

		public OpenFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				using var dialog = new OpenFolderDialog();
				var result = dialog.ShowDialog(owner);

				if (result != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				var path = dialog.FolderPath;
				if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
				{
					return;
				}

				// no need to check if the folder is already open as a notebook; OneNote will
				// handle that by navigating directly to the notebook for the user. This is
				// also true for subfolders within that notebook.

				using var one = new OneNote();
				var notebookID = await one.OpenHierarchy(path);

				if (notebookID is null)
				{
					logger.WriteLine("could not load notebookID");
					ShowError(Resx.OpenFolderCommand_error);
					return;
				}

				if (dialog.RemoveTimestamps)
				{
					var regex = new Regex(
						@"^(.+?)(\.one)?\s+\(\w+\s+\d{1,4}([-.\/])\d{1,2}\3\d{2,4}\)$",
						RegexOptions.Compiled | RegexOptions.CultureInvariant);

					var hierarchy = await one.GetNotebook(notebookID, OneNote.Scope.Sections);

					for (int attempt = 1; 
						attempt < 4 && (hierarchy is null || !hierarchy.HasElements); 
						attempt++)
					{
						await Task.Delay(500 * attempt);
						hierarchy = await one.GetNotebook(notebookID, OneNote.Scope.Sections);
					}

					if (hierarchy is null || !hierarchy.HasElements)
					{
						logger.WriteLine("could not load hierarchy to clean section names");
						ShowError(Resx.OpenFolderCommand_error);
						return;
					}

					var ns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

					var count = 0;
					var skipped = false;
					CleanSectionNames(hierarchy, ns, regex, ref count, ref skipped);

					if (skipped)
					{
						ShowInfo(Resx.OpenFolderCommand_duplicates);
					}

					if (count > 0)
					{
						logger.WriteLine($"edited {count} section names, updating hierarchy");
						one.UpdateHierarchy(hierarchy);
					}
				}
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private void CleanSectionNames(
			XElement parent, XNamespace ns, Regex regex, ref int count, ref bool skipped)
		{
			// Pre-compute candidate final name for each direct child section
			var sections = parent.Elements(ns + "Section").ToList();
			var candidates = new Dictionary<XElement, (string Name, bool Changed)>();

			foreach (var section in sections)
			{
				var attribute = section.Attribute("name");
				if (attribute is not null)
				{
					var match = regex.Match(attribute.Value);
					candidates[section] = match.Success
						? (match.Groups[1].Value, true)
						: (attribute.Value, false);
				}
			}

			// Find candidate names that would collide among these siblings
			var seen = new HashSet<string>();
			var dupes = new HashSet<string>();
			foreach (var (name, _) in candidates.Values)
			{
				if (!seen.Add(name))
				{
					dupes.Add(name);
				}
			}

			// Rename only sections whose stripped name is unique within this parent
			foreach (var entry in candidates)
			{
				var (name, changed) = entry.Value;
				if (changed)
				{
					if (entry.Key.Attribute("locked") is not null || dupes.Contains(name))
					{
						skipped = true;
						continue;
					}

					entry.Key.Attribute("name").Value = name;
					count++;
				}
			}

			// Recurse into child section groups, skipping the recycle bin
			foreach (var group in parent.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null))
			{
				CleanSectionNames(group, ns, regex, ref count, ref skipped);
			}
		}
	}
}
