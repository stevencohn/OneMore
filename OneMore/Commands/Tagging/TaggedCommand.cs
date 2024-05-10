//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class TaggedCommand : Command
	{
		private TaggedDialog.Commands command;
		private List<string> pageIds;


		public TaggedCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var converter = new LegacyTaggingConverter();
			var upgraded = await converter.UpgradeLegacyTags(owner);

			if (upgraded)
			{
				using var box = new MoreMessageBox();
				box.SetIcon(MessageBoxIcon.Information);
				box.SetMessage(string.Format(Resx.TagsUpgraded,
					converter.TagsConverted, converter.PagesConverted
					));
	
				box.SetButtons(MessageBoxButtons.OKCancel);
				if (box.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}
			}

			if (converter.Converted)
			{
				var cmd = new HashtagCommand();
				cmd.SetLogger(logger);
				cmd.SetRibbon(ribbon);
				cmd.SetTrash(trash);
				await cmd.Execute();
				return;
			}

			var dialog = new TaggedDialog();

			dialog.RunModeless(async (sender, e) =>
			{
				var d = sender as TaggedDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = d.Command;
					pageIds = d.SelectedPages;

					var msg = command switch
					{
						TaggedDialog.Commands.Copy => Resx.SearchQF_DescriptionCopy,
						TaggedDialog.Commands.Move => Resx.SearchQF_DescriptionMove,
						_ => Resx.SearchQF_DescriptionIndex
					};

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, msg, OneNote.Scope.Sections, Callback);
				}
			},
			20);
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..{command} {pageIds.Count} pages");

			try
			{
				await using var one = new OneNote();
				var service = new SearchServices(one, sectionId);

				switch (command)
				{
					case TaggedDialog.Commands.Index:
						await service.IndexPages(pageIds);
						break;

					case TaggedDialog.Commands.Copy:
						await service.CopyPages(pageIds);
						break;

					case TaggedDialog.Commands.Move:
						await service.MovePages(pageIds);
						break;
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
	}
}
