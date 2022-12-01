//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
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
	/// Performs a deep copy of a SectionGroup (folder) into another section group or notebook
	/// </summary>
	internal class CopyFolderCommand : Command
	{
		private UI.ProgressDialog progress;

		public CopyFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				one.SelectLocation(
					Resx.SearchQF_Title, Resx.SearchQF_DescriptionCopy,
					OneNote.Scope.SectionGroups, Callback);
			}

			await Task.Yield();
		}


		private async Task Callback(string targetId)
		{
			if (string.IsNullOrEmpty(targetId))
			{
				// cancelled
				return;
			}

			logger.Start($"..target folder {targetId}");

			try
			{
				using var one = new OneNote();
				// user might choose a sectiongroup or a notebook; GetSection will get either
				var target = one.GetSection(targetId);
				if (target == null)
				{
					logger.WriteLine("invalid target section");
					return;
				}

				// source folder will be in current notebook
				var notebook = await one.GetNotebook(OneNote.Scope.Pages);
				var ns = one.GetNamespace(notebook);

				// use current page to ascend back to closest folder to handle nesting...
				var element = notebook.Descendants(ns + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == one.CurrentPageId);

				var folder = element.FirstAncestor(ns + "SectionGroup");
				if (folder == null)
				{
					logger.WriteLine("error finding ancestor folder");
					return;
				}

				if (folder.DescendantsAndSelf().Any(e => e.Attribute("ID").Value == targetId))
				{
					logger.WriteLine("cannot copy a folder into itself or one of its children");

					MessageBox.Show(
						Resx.CopyFolderCommand_InvalidTarget,
						Resx.OneMoreTab_Label,
						MessageBoxButtons.OK, MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						MessageBoxOptions.DefaultDesktopOnly);

					return;
				}

				logger.WriteLine(
					$"copying folder {folder.Attribute("name").Value} " +
					$"to {target.Attribute("name").Value}");

				// clone structure of folder; this does not assign ID values
				var clone = CloneFolder(folder, ns);

				// update target so OneNote will apply new ID values
				target.Add(clone);
				one.UpdateHierarchy(target);

				// re-fetch target to find newly assigned ID values
				var upTarget = one.GetSection(targetId);

				var cloneID = upTarget.Elements()
					.Where(e => !e.Attributes().Any(a => a.Name == "isRecycleBin"))
					.Select(e => e.Attribute("ID").Value)
					.Except(
						target.Elements()
							.Where(e => e.Attributes().Any(a => a.Name == "ID")
								&& !e.Attributes().Any(a => a.Name == "isRecycleBin"))
							.Select(e => e.Attribute("ID").Value)
					).FirstOrDefault();

				clone = upTarget.Elements().FirstOrDefault(e => e.Attribute("ID").Value == cloneID);

				using (progress = new UI.ProgressDialog())
				{
					progress.SetMaximum(folder.Descendants(ns + "Page").Count());
					progress.Show();

					// now with a new SectionGroup with a valid ID, copy all pages into it
					await CopyPages(folder, clone, one, ns);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				logger.End();
			}
		}


		private XElement CloneFolder(XElement folder, XNamespace ns, XElement clone = null)
		{
			// deep copy without pages...

			clone ??= new XElement(
				ns + folder.Name.LocalName,
				folder.Attributes().Where(a => a.Name.LocalName != "ID"));

			foreach (var group in folder.Elements(ns + "SectionGroup"))
			{
				var s = new XElement(ns + group.Name.LocalName,
					group.Attributes().Where(a => a.Name.LocalName != "ID"));

				clone.Add(s);
				CloneFolder(group, ns, s);
			}

			foreach (var group in folder.Elements(ns + "Section"))
			{
				var s = new XElement(ns + group.Name.LocalName,
					group.Attributes().Where(a => a.Name.LocalName != "ID"));

				clone.Add(s);
				CloneFolder(group, ns, s);
			}

			return clone;
		}


		private async Task CopyPages(XElement root, XElement clone, OneNote one, XNamespace ns)
		{
			var cloneID = clone.Attribute("ID").Value;

			foreach (var element in root.Elements(ns + "Page"))
			{
				// get the page to copy
				var page = one.GetPage(element.Attribute("ID").Value);
				progress.SetMessage(page.Title);

				// create a new page to get a new ID
				one.CreatePage(cloneID, out var newPageId);

				// set the page ID to the new page's ID
				page.Root.Attribute("ID").Value = newPageId;

				// remove all objectID values and let OneNote generate new IDs
				page.Root.Descendants().Attributes("objectID").Remove();

				await one.Update(page);
				progress.Increment();
			}

			// recurse...

			// NOTE that these find target sections by name, so the names must be unique otherwise
			// this will copy all pages into the first occurance with a matching name!

			foreach (var section in root.Elements(ns + "SectionGroup").Elements(ns + "Section"))
			{
				var cloneSection = clone.Elements(ns + "SectionGroup").Elements(ns + "Section")
					.FirstOrDefault(e => e.Attribute("name").Value == section.Attribute("name").Value);

				await CopyPages(section, cloneSection, one, ns);
			}

			foreach (var section in root.Elements(ns + "Section"))
			{
				var cloneSection = clone.Elements(ns + "Section")
					.FirstOrDefault(e => e.Attribute("name").Value == section.Attribute("name").Value);

				await CopyPages(section, cloneSection, one, ns);
			}
		}
	}
}
