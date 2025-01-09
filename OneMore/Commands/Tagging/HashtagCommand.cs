//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class HashtagCommand : Command
	{
		private HashtagDialog.Commands command;
		private IEnumerable<HashtagContext> selectedPages;
		private string query;

		private static HashtagDialog dialog;

		public HashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (!await ConfirmReady())
			{
				return;
			}

			if (dialog != null)
			{
				// single instance
				dialog.Elevate();
				return;
			}

			var converter = new LegacyTaggingConverter();
			await converter.UpgradeLegacyTags();

			// get page moreID...

			await using var one = new OneNote(out var page, out var ns);

			var moreID = page.Root.Elements(ns + "Meta")
				.Where(e => e.Attribute("name").Value == MetaNames.PageID)
				.Select(e => e.Attribute("content").Value)
				.FirstOrDefault();

			// dialog...

			dialog = new HashtagDialog(moreID);
			dialog.FormClosed += Dialog_FormClosed;

			dialog.RunModeless(async (sender, e) =>
			{
				var d = sender as HashtagDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = d.Command;
					selectedPages = d.SelectedPages;
					query = d.Query;

					var msg = command switch
					{
						HashtagDialog.Commands.Copy => Resx.SearchQF_DescriptionCopy,
						HashtagDialog.Commands.Move => Resx.SearchQF_DescriptionMove,
						_ => Resx.SearchQF_DescriptionIndex
					};

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, msg, OneNote.Scope.Sections, Callback);
				}
			},
			20);
		}


		private async Task<bool> ConfirmReady()
		{
			var scheduler = new HashtagScheduler();

			if (!HashtagProvider.CatalogExists())
			{
				using var sdialog = scheduler.State == ScanningState.None
					? new ScheduleScanDialog(false)
					: new ScheduleScanDialog(false, scheduler.StartTime);

				var result = sdialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					scheduler.Notebooks = sdialog.GetSelectedNotebooks();
					scheduler.StartTime = sdialog.StartTime;
					scheduler.State = ScanningState.PendingRebuild;
					await scheduler.Activate();
				}

				return false;
			}

			//if (scheduler.State != ScanningState.Ready)
			//{
			//	var msg = scheduler.State == ScanningState.Scanning
			//		? Resx.HashtagCommand_scanning
			//		: string.Format(Resx.HashtagCommand_waiting, scheduler.StartTime.ToFriendlyString());

			//	MoreMessageBox.Show(owner, msg);
			//	return false;
			//}

			return true;
		}


		private void Dialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (dialog != null)
			{
				dialog.FormClosed -= Dialog_FormClosed;
				dialog.Dispose();
				dialog = null;
			}
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..{command} {selectedPages.Count()} pages");

			try
			{
				if (command == HashtagDialog.Commands.Index)
				{
					await IndexTaggedPages(sectionId);
				}
				else
				{
					await using var one = new OneNote();
					var service = new SearchServices(one, sectionId);
					var pageIds = selectedPages.Select(p => p.PageID).ToList();

					if (command == HashtagDialog.Commands.Copy)
					{
						await service.CopyPages(pageIds);
					}
					else
					{
						await service.MovePages(pageIds);
					}
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


		private async Task IndexTaggedPages(string sectionId)
		{
			await using var one = new OneNote();
			string parentId = null;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(selectedPages.Count());
				progress.Show();

				// create a new page to get a new ID
				one.CreatePage(sectionId, out parentId);
				var parent = await one.GetPage(parentId);

				var ns = parent.Namespace;
				PageNamespace.Set(ns);

				parent.Title = Resx.HashtagCommand_indexTitle;
				parent.SetMeta(MetaNames.TagIndex, "true");

				var container = parent.EnsureContentContainer();

				var h1Index = parent.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				var todoIndex = parent.AddTagDef("3", "To Do", 4);

				container.Add(new Paragraph(query).SetQuickStyle(h1Index));

				var content = new XElement(ns + "OEChildren");
				container.Add(new Paragraph(content));

				foreach (var context in selectedPages
					.OrderBy(c => c.HierarchyPath)
					.ThenBy(c => c.PageTitle))
				{
					progress.SetMessage(context.PageTitle);
					progress.Increment();

					var link = one.GetHyperlink(context.PageID,
						string.IsNullOrWhiteSpace(context.TitleID) ? string.Empty : context.TitleID);

					var text = $"{context.HierarchyPath}/{context.PageTitle}";

					content.Add(new Paragraph(
						new Tag(todoIndex, false),
						new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{text}</a>"))
						));

					var bullets = new ContentList(ns);
					foreach (var snippet in context.Snippets)
					{
						link = one.GetHyperlink(context.PageID, snippet.ObjectID);
						bullets.Add(new Bullet($"<a href=\"{link}\">{snippet.Snippet}</a>"));
					}

					content.Add(new Paragraph(bullets));
					content.Add(new Paragraph(string.Empty));
				}

				await one.Update(parent);
				parentId = parent.PageId;
			}

			// navigate after progress dialog is closed otherwise it will hang!
			await one.NavigateTo(parentId);
		}
	}
}
