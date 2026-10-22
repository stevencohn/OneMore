//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.IO;
	using System.Threading.Tasks;


	/// <summary>
	/// Opens a saved page-XML snippet on a new page of the current section so it can be
	/// edited like it was when created. Macros are left unexpanded. The new page is marked
	/// with the Snippet meta so SaveSnippetCommand can seed the name of the snippet.
	/// </summary>
	internal class SnippetEditor
	{
		/// <summary>
		/// Creates a new page from the snippet and navigates to it.
		/// </summary>
		/// <returns>True if the page was created</returns>
		public async Task<bool> Open(string path)
		{
			var xml = await new SnippetsProvider().Load(path);
			if (string.IsNullOrWhiteSpace(xml))
			{
				return false;
			}

			var name = Path.GetFileNameWithoutExtension(path);

			await using var one = new OneNote();
			one.CreatePage(one.CurrentSectionId, out var pageId);
			var page = await one.GetPage(pageId);

			var content = SnippetPackage.Parse(xml, page.Namespace, out var kind, out var root);
			if (content is null || !content.HasElements)
			{
				return false;
			}

			page.Title = name;
			page.SetMeta(MetaNames.Snippet, name);

			// macros are intentionally not expanded
			SnippetPackage.MergeDefinitions(page, root, content);
			SnippetPackage.Insert(page, content, kind);

			await one.Update(page);
			await one.NavigateTo(pageId, string.Empty);
			return true;
		}
	}
}
