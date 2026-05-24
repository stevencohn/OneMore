//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class SearchCommand : Command
	{
		private static bool commandIsActive = false;

		private SearchDialog.Commands command;
		private List<string> pageIds;
		private IEnumerable<CardModel> selectedCards;
		private string query;
		private SearchDialog dialog;


		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (dialog is not null)
			{
				dialog.Elevate();
				return;
			}

			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				dialog = new SearchDialog();
				dialog.RunModeless(async (sender, e) =>
				{
					if (sender is SearchDialog d && d.DialogResult == DialogResult.OK)
					{
						command = d.Command;
						query = d.Query;

						if (command == SearchDialog.Commands.Index)
						{
							selectedCards = d.SelectedCards;
						}
						else
						{
							pageIds = d.SelectedPages;
						}

						var desc = command switch
						{
							SearchDialog.Commands.Copy => Resx.SearchQF_DescriptionCopy,
							SearchDialog.Commands.Move => Resx.SearchQF_DescriptionMove,
							_ => Resx.SearchQF_DescriptionIndex
						};

						await using var one = new OneNote();
						one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
					}
				},
				20);

				dialog.Elevate(true);

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
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

			logger.Start($"..{command} pages");

			try
			{
				if (command == SearchDialog.Commands.Index)
				{
					await IndexSearchResults(sectionId);
				}
				else
				{
					await using var one = new OneNote();
					var service = new SearchServices(one, sectionId);

					if (command == SearchDialog.Commands.Copy)
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


		private async Task IndexSearchResults(string sectionId)
		{
			await using var one = new OneNote();
			string parentId = null;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(selectedCards.Count());
				progress.Show();

				one.CreatePage(sectionId, out parentId);
				var parent = await one.GetPage(parentId);

				var ns = parent.Namespace;
				PageNamespace.Set(ns);

				parent.Title = Resx.SearchCommand_indexTitle;
				parent.SetMeta(MetaNames.SearchIndex, "true");

				var container = parent.EnsureContentContainer();

				var h1Index = parent.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				var todoIndex = parent.AddTagDef("3", "To Do", 4);

				container.Add(new Paragraph(query).SetQuickStyle(h1Index));

				var content = new XElement(ns + "OEChildren");
				container.Add(new Paragraph(content));

				foreach (var card in selectedCards.OrderBy(c => c.Title))
				{
					progress.SetMessage(card.Title);
					progress.Increment();

					var link = one.GetHyperlink(card.PageId, string.Empty);

					content.Add(new Paragraph(
						new Tag(todoIndex, false),
						new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{WebUtility.HtmlEncode(card.Title)}</a>"))
						));

					var bullets = new ContentList(ns);
					foreach (var hit in card.Hits)
					{
						link = one.GetHyperlink(card.PageId, hit.ObjectId);
						bullets.Add(new Bullet($"<a href=\"{link}\">{WebUtility.HtmlEncode(hit.PlainText)}</a>"));
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
