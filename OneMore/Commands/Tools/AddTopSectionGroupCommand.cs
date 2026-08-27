//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
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
			var defaultName = baseName;
			if (existingNames.Contains(defaultName))
			{
				var n = 2;
				while (existingNames.Contains($"{baseName} {n}"))
				{
					n++;
				}

				defaultName = $"{baseName} {n}";
			}

			using var dialog = new AddTopSectionGroupDialog(existingNames, defaultName);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			// OneNote always displays top-level section groups alphabetically regardless of
			// where they're added to the hierarchy XML, so there's no benefit in trying to
			// control the insertion point here
			notebook.Add(new XElement(ns + "SectionGroup", new XAttribute("name", dialog.GroupName)));

			using var progress = new UI.ProgressDialog(60);
			var result = progress.ShowTimedDialog(async (dlg, token) =>
			{
				try
				{
					dlg.SetMessage(Resx.AddTopSectionGroupCommand_Saving);

					await using var one2 = new OneNote();
					one2.UpdateHierarchy(notebook);
					return true;
				}
				catch (Exception exc)
				{
					logger.WriteLine("error creating top-level section group", exc);
					return false;
				}
			}, cancelable: false);

			if (result != DialogResult.OK)
			{
				ShowError(Resx.AddTopSectionGroupCommand_Error);
			}
		}
	}
}
