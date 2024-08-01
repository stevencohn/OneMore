﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Insert a custom snippet onto the page at the current insert point. 
	/// This can run as a direct command from the My Custom Snippets menu or can be invoked
	/// as to expand the name of a custom snippet to its contents using the Alt+F3 shortcut
	/// </summary>
	internal class InsertSnippetCommand : Command
	{

		public InsertSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var path = args[0] as string;
			string snippet = null;

			var provider = new SnippetsProvider();

			await using var one = new OneNote(out var page, out _);

			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			// Either invoked via Alt+F3 to expand the selected name to a snippet
			// or invoked from the My Custom snippets menu...

			if (string.IsNullOrWhiteSpace(path))
			{
				// assume Expand command and infer name from current word...

				path = new Models.PageEditor(page).GetSelectedText();
				if (!string.IsNullOrWhiteSpace(path))
				{
					snippet = await provider.LoadByName(path);
					if (!string.IsNullOrEmpty(snippet))
					{
						// remove placeholder
						var updated = new Models.PageEditor(page).EditSelected((s) =>
						{
							if (s is XText text)
							{
								text.Value = string.Empty;
								return text;
							}

							var element = (XElement)s;
							element.Value = string.Empty;
							return element;
						});

						if (updated)
						{
							await one.Update(page);
						}
					}
				}
			}
			else
			{
				snippet = await provider.Load(path);
			}

			if (string.IsNullOrWhiteSpace(snippet))
			{
				ShowError(string.Format(Resx.InsertSnippets_CouldNotLoad, path));
				return;
			}

			var clippy = new ClipboardProvider();
			await clippy.StashState();

			var success = await clippy.SetHtml(snippet);
			if (success)
			{
				await clippy.Paste(true);
			}
			else
			{
				ShowInfo(Resx.Clipboard_locked);
			}

			success = await clippy.RestoreState();
			if (!success)
			{
				ShowInfo(Resx.Clipboard_norestore);
			}
		}
	}
}
