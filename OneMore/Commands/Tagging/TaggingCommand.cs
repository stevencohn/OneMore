//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class TaggingCommand : Command
	{

		public TaggingCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				using (var dialog = new TaggingDialog())
				{
					var content = page.GetMetaContent(Page.TaggingMetaName);
					if (!string.IsNullOrEmpty(content))
					{
						var parts = content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
							.ToList()
							.ConvertAll(s => s.Trim());

						dialog.Tags = parts;
					}

					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					var tags = dialog.Tags;
				}
			}
		}
	}
}
