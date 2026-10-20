//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SaveSnippetCommand : Command
	{
		public SaveSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			// All detail so inline image data is available to be saved with the snippet
			await using var one = new OneNote(out var page, out _, OneNote.PageDetail.All);

			var range = new Models.SelectionRange(page);
			range.GetSelection(allowNonEmpty: true);

			if (range.Scope != SelectionScope.Range &&
				range.Scope != SelectionScope.Run)
			{
				ShowError(Resx.SaveSnippet_NeedSelection);
				return;
			}

			// save the selection as page XML rather than clipboard HTML to preserve
			// quick styles, headings, table borders and widths, colors, etc.
			var package = SnippetPackage.Build(page, range.Scope);
			if (package is null)
			{
				logger.WriteLine($"{nameof(SaveSnippetCommand)} empty selection");
				ShowError(Resx.SaveSnippet_NoContext);
				return;
			}

			using var dialog = new SaveSnippetDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			await new SnippetsProvider().Save(
				package.ToString(System.Xml.Linq.SaveOptions.DisableFormatting),
				dialog.SnippetName);

			ribbon.SafeInvalidateControl(SnippetsProvider.MenuID);
		}
	}
}
