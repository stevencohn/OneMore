//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class TaggedCommand : Command
	{
		private TaggedDialog.Commands command;
		private List<string> pageIds;


		public TaggedCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var dialog = new TaggedDialog();

			dialog.RunModeless((sender, e) =>
			{
				var d = sender as TaggedDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = dialog.Command;
					pageIds = dialog.SelectedPages;

					var desc = command == TaggedDialog.Commands.Copy
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					using (var one = new OneNote())
					{
						one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
					}
				}
			},
			20);
		}


		private void Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..{command} {pageIds.Count} pages");

			try
			{
				using (var one = new OneNote())
				{
					var service = new SearchServices(owner, one, sectionId);

					switch (command)
					{
						case TaggedDialog.Commands.Index:
							service.IndexPages(pageIds);
							break;

						case TaggedDialog.Commands.Copy:
							service.CopyPages(pageIds);
							break;

						case TaggedDialog.Commands.Move:
							service.MovePages(pageIds);
							break;
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
	}
}
