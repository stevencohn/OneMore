//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Performs a deep copy of a SectionGroup (folder) into another section group or notebook
	/// </summary>
	internal class CopyFolderCommand : Command
	{
		private const string SectionName = "Section";
		private const string SectionGroupName = "SectionGroup";

		private List<string> failures;
		private int totalPages;
		private string infoMessage;
		private string sourcePageId;
		private string sourceNotebookId;

		public CopyFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			// capture the source page and notebook now, before the QuickFiling picker opens
			// and while OneNote's UI is still guaranteed to reflect what the user was looking
			// at when they invoked this command; once the picker is up and the copy is running
			// on a background thread, OneNote's UI is no longer blocked so CurrentPageId/
			// CurrentNotebookId could otherwise drift out from under a later read
			sourcePageId = one.CurrentPageId;
			sourceNotebookId = one.CurrentNotebookId;

			one.SelectLocation(
				Resx.SearchQF_Title, Resx.SearchQF_DescriptionCopy,
				OneNote.Scope.SectionGroups, Callback);

			await Task.Yield();
		}


		private async Task Callback(string targetId)
		{
			if (string.IsNullOrEmpty(targetId))
			{
				// cancelled
				return;
			}

			using var indent = logger.Indent($"..target folder {targetId}");

			infoMessage = null;
			failures = new List<string>();
			totalPages = 0;

			// this can take a minute or more for a large folder; run modeless so the copy
			// happens on a background thread and doesn't block OneNote's own UI thread while
			// OneNote is waiting for this QuickFiling OnDialogClosed callback to return
			var progress = new UI.ProgressDialog(async (dialog, token) =>
				await CopyFolder(targetId, dialog, token));

			progress.SetMessage(Resx.CopyFolderCommand_Preparing);
			progress.RunModeless(ReportResult);

			await Task.Yield();
		}


		private async Task CopyFolder(string targetId, UI.ProgressDialog dialog, CancellationToken token)
		{
			try
			{
				await using var one = new OneNote();

				// user might choose a sectiongroup or a notebook; GetSection will get either
				var target = await one.GetSection(targetId);
				if (target is null)
				{
					logger.WriteLine("invalid target section");
					return;
				}

				// source folder will be in the notebook that was current when invoked
				var notebook = await one.GetNotebook(sourceNotebookId, OneNote.Scope.Pages);
				var ns = one.GetNamespace(notebook);

				// use the page that was current when the command was invoked (captured in
				// Execute) to ascend back to closest folder to handle nesting...
				var element = notebook.Descendants(ns + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == sourcePageId);

				if (element is null)
				{
					logger.WriteLine("could not locate source page in notebook; cannot determine source folder");
					infoMessage = Resx.CopyFolderCommand_NoSourceFolder;
					return;
				}

				var folder = element.FirstAncestor(ns + SectionGroupName);
				if (folder is null)
				{
					logger.WriteLine("error finding ancestor folder");
					infoMessage = Resx.CopyFolderCommand_NoSourceFolder;
					return;
				}

				if (folder.DescendantsAndSelf().Any(e => e.Attribute("ID")?.Value == targetId))
				{
					logger.WriteLine("cannot copy a folder into itself or one of its children");
					infoMessage = Resx.CopyFolderCommand_InvalidTarget;
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

				// re-fetch target to find the newly copied folder and its assigned ID values;
				// match by name rather than diffing IDs before/after UpdateHierarchy since
				// OneNote may reassign IDs of more than just the new element on update, which
				// can make an ID-diff pick the wrong element (or none at all)
				var upTarget = await one.GetSection(targetId);
				var folderName = folder.Attribute("name").Value;

				clone = upTarget.Elements()
					.FirstOrDefault(e => e.Attribute("name")?.Value == folderName);

				if (clone is null)
				{
					logger.WriteLine($"could not locate newly copied folder '{folderName}' in target");
					return;
				}

				totalPages = folder.Descendants(ns + "Page").Count();
				dialog.SetMaximum(totalPages);

				// now with a new SectionGroup with a valid ID, copy all pages into it
				await CopyPages(folder, clone, one, ns, dialog, token);
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				dialog.Close();
			}
		}


		// runs on the UI thread after the modeless progress dialog closes
		private void ReportResult(object sender, EventArgs e)
		{
			if (sender is UI.ProgressDialog dialog)
			{
				// otherwise MoreMessageBox window could appear behind the progress dialog
				dialog.Visible = false;
			}

			if (!string.IsNullOrEmpty(infoMessage))
			{
				UI.MoreMessageBox.Show(owner,
					infoMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);

				return;
			}

			if (failures.Count > 0)
			{
				const int maxListed = 20;
				var listed = failures.Take(maxListed).ToList();
				if (failures.Count > maxListed)
				{
					listed.Add(string.Format(Resx.CopyFolderCommand_AndMore, failures.Count - maxListed));
				}

				UI.MoreMessageBox.Show(owner,
					string.Format(Resx.CopyFolderCommand_PartialFailure, failures.Count, totalPages) +
					Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, listed),
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}


		private XElement CloneFolder(XElement folder, XNamespace ns, XElement clone = null)
		{
			// deep copy without pages...

			clone ??= new XElement(
				ns + folder.Name.LocalName,
				folder.Attributes().Where(a => a.Name.LocalName != "ID"));

			foreach (var group in folder.Elements(ns + SectionName))
			{
				var s = new XElement(ns + group.Name.LocalName,
					group.Attributes().Where(a => a.Name.LocalName != "ID"));

				clone.Add(s);
				CloneFolder(group, ns, s);
			}

			foreach (var group in folder.Elements(ns + SectionGroupName))
			{
				var s = new XElement(ns + group.Name.LocalName,
					group.Attributes().Where(a => a.Name.LocalName != "ID"));

				clone.Add(s);
				CloneFolder(group, ns, s);
			}

			return clone;
		}


		private async Task CopyPages(
			XElement root, XElement clone, OneNote one, XNamespace ns,
			UI.ProgressDialog dialog, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			var cloneID = clone.Attribute("ID").Value;

			foreach (var element in root.Elements(ns + "Page"))
			{
				if (token.IsCancellationRequested)
				{
					logger.WriteLine("..copy cancelled by user");
					return;
				}

				// get the page to copy
				var page = await one.GetPage(element.Attribute("ID").Value);
				dialog.SetMessage(page.Title);

				// create a new page to get a new ID
				one.CreatePage(cloneID, out var newPageId);

				// set the page ID to the new page's ID
				page.Root.Attribute("ID").Value = newPageId;

				// remove all objectID values and let OneNote generate new IDs
				page.Root.Descendants().Attributes("objectID").Remove();

				var ok = await one.Update(page);
				if (!ok)
				{
					var hinfo = one.GetPageHierarchyInfo(element.Attribute("ID").Value);
					var path = $"{hinfo.Path}/{page.Title}";

					logger.WriteLine($"..failed to copy page content for '{path}'");
					failures.Add(page.Title);
				}

				dialog.Increment();
			}

			if (token.IsCancellationRequested)
			{
				return;
			}

			// recurse...

			// NOTE that OneNote does not allow duplicate section names at the same level in the
			// hierarchy. We take advantage of that, otherwise this will copy all pages into the
			// first occurance with a matching name!

			foreach (var section in root.Elements(ns + SectionName))
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var cloneSection = clone.Elements(ns + SectionName)
					.FirstOrDefault(e => e.Attribute("name").Value == section.Attribute("name").Value);

				await CopyPages(section, cloneSection, one, ns, dialog, token);
			}

			foreach (var group in root.Elements(ns + SectionGroupName))
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var cloneGroup = clone.Elements(ns + SectionGroupName)
					.FirstOrDefault(e => e.Attribute("name").Value == group.Attribute("name").Value);

				await CopyPages(group, cloneGroup, one, ns, dialog, token);
			}
		}
	}
}
