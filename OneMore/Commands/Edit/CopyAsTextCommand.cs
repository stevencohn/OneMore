//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using River.OneMoreAddIn.Models;


	/// <summary>
	/// Copy the page or selected content as plan text onto the system clipboard
	/// </summary>
	internal class CopyAsTextCommand : Command
	{
		public CopyAsTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var _);
			var cursor = page.GetTextCursor();

			if (// cursor is not null if selection range is empty
				cursor != null &&
				// selection range is a single line containing a hyperlink
				!(page.SelectionSpecial && page.SelectionScope == SelectionScope.Empty))
			{
				await CopyPageAsText(one, page);
			}
			else
			{
				// if only images are selected and no text content then copy entire page...

				var other = page.Root.Descendants().Where(e =>
					e.Attribute("selected")?.Value == "all" &&
					e.Name.LocalName != "Image");

				if (other.Any())
				{
					// some text was found, maybe with one or more images
					var clipboard = new ClipboardProvider();
					await clipboard.Copy();
					await clipboard.SetText(await clipboard.GetText());
				}
				else
				{
					// no range selection or only an image was selected
					await CopyPageAsText(one, page);
				}
			}
		}


		private async Task CopyPageAsText(OneNote one, Page page)
		{
			var updated = one.GetPage(OneNote.PageDetail.Basic);
			var ns = updated.Root.GetNamespaceOfPrefix(OneNote.Prefix);

			updated.Root.Elements(ns + "Outline").ForEach(e =>
			{
				e.SetAttributeValue("selected", "all");
			});

			await one.Update(updated);

			var clipboard = new ClipboardProvider();
			await clipboard.Copy();
			await clipboard.SetText(await clipboard.GetText());

			await one.Update(page);
		}
	}
}
