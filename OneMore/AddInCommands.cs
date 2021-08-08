//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S1135       // Track uses of "TODO" tags

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System.Threading.Tasks;


	public partial class AddIn
	{
		public async Task AboutCmd(IRibbonControl control)
			=> await factory.Run<AboutCommand>();

		public async Task AddCaptionCmd(IRibbonControl control)
			=> await factory.Run<AddCaptionCommand>();

		public void AddFavoritePageCmd(IRibbonControl control)
			=> new FavoritesProvider(ribbon).AddFavorite();

		public void AddFavoriteSectionCmd(IRibbonControl control)
			=> new FavoritesProvider(ribbon).AddFavorite(true);

		public async Task AddFootnoteCmd(IRibbonControl control)
			=> await factory.Run<AddFootnoteCommand>();

		public async Task AddFormulaCmd(IRibbonControl control)
			=> await factory.Run<AddFormulaCommand>();

		public async Task AddTitleIconCmd(IRibbonControl control)
			=> await factory.Run<AddTitleIconCommand>();

		public async Task ApplyStyleCmd(IRibbonControl control, string selectedId, int selectedIndex)
			=> await factory.Run<ApplyStyleCommand>(selectedIndex);

		public async Task ApplyStylesCmd(IRibbonControl control)
			=> await factory.Run<ApplyStylesCommand>();

		public async Task ArchiveCmd(IRibbonControl control)
			=> await factory.Run<ArchiveCommand>(control.Tag);

		public async Task BreakingCmd(IRibbonControl control)
			=> await factory.Run<BreakingCommand>();

		public async Task ChangePageColorCmd(IRibbonControl control)
			=> await factory.Run<ChangePageColorCommand>();

		public async Task ClearBackgroundCmd(IRibbonControl control)
			=> await factory.Run<ClearBackgroundCommand>();

		public async Task CollapsePagesCmd(IRibbonControl control)
			=> await factory.Run<CollapsePagesCommand>();

		public async Task CollapseContentCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Collapse);

		public async Task ColorizeCmd(IRibbonControl control)
			=> await factory.Run<ColorizeCommand>(control.Tag);

		public async Task CropImageCmd(IRibbonControl control)
			=> await factory.Run<CropImageCommand>();

		public async Task CopyFolderCmd(IRibbonControl control)
			=> await factory.Run<CopyFolderCommand>();

		public async Task DateStampCmd(IRibbonControl control)
			=> await factory.Run<DateStampCommand>();

		public async Task DecreaseFontSizeCmd(IRibbonControl control)
			=> await factory.Run<AlterSizeCommand>(-1);

		public async Task DeleteFormulaCmd(IRibbonControl control)
			=> await factory.Run<DeleteFormulaCommand>();

		public async Task DisableSpellCheckCmd(IRibbonControl control)
			=> await factory.Run<SpellCheckCommand>(false);

		public async Task EditStylesCmd(IRibbonControl control)
			=> await factory.Run<EditStylesCommand>();

		public async Task EnableSpellCheckCmd(IRibbonControl control)
			=> await factory.Run<SpellCheckCommand>(true);

		public async Task ExpandContentCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Expand);

		public async Task FinishBiLinkCmd(IRibbonControl control)
			=> await factory.Run<BiLinkCommand>("link");

		public async Task GetImagesCmd(IRibbonControl control)
			=> await factory.Run<GetImagesCommand>(true);

		public async Task GotoFavoriteCmd(IRibbonControl control)
			=> await factory.Run<Commands.GotoFavoriteCommand>(control.Tag);

		public async Task IncreaseFontSizeCmd(IRibbonControl control)
			=> await factory.Run<AlterSizeCommand>(1);

		public async Task HighlightCmd(IRibbonControl control)
			=> await factory.Run<HighlightCommand>();

		public async Task HighlightFormulaCmd(IRibbonControl control)
			=> await factory.Run<HighlightFormulaCommand>();

		public async Task InsertBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertCodeBlockCommand>(false);

		public async Task ImportCmd(IRibbonControl control)
			=> await factory.Run<ImportCommand>();

		public async Task ImportWebCmd(IRibbonControl control)
			=> await factory.Run<ImportWebCommand>();

		public async Task InsertBlueStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertStatusCommand>(StatusColor.Blue);

		public async Task InsertCalendarCmd(IRibbonControl control)
			=> await factory.Run<InsertCalendarCommand>();

		public async Task InsertCellsCmd(IRibbonControl control)
			=> await factory.Run<InsertCellsCommand>();

		public async Task InsertCodeBlockCmd(IRibbonControl control)
			=> await factory.Run<InsertCodeBlockCommand>(true);

		public async Task InsertDateCmd(IRibbonControl control)
			=> await factory.Run<InsertDateCommand>();

		public async Task InsertDoubleHorizontalLineCmd(IRibbonControl control)
			=> await factory.Run<InsertLineCommand>('═');

		public async Task InsertExpandCmd(IRibbonControl control)
			=> await factory.Run<InsertExpandCommand>();

		public async Task InsertGrayStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertStatusCommand>(StatusColor.Gray);

		public async Task InsertGreenStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertStatusCommand>(StatusColor.Green);

		public async Task InsertHorizontalLineCmd(IRibbonControl control)
			=> await factory.Run<InsertLineCommand>('─');

		public async Task InsertInfoBlockCmd(IRibbonControl control)
			=> await factory.Run<InsertInfoBlockCommand>(false);

		public async Task InsertQRCmd(IRibbonControl control)
			=> await factory.Run<InsertQRCommand>(false);

		public async Task InsertRedStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertStatusCommand>(StatusColor.Red);

		public async Task InsertSnippetCmd(IRibbonControl control)
			=> await factory.Run<InsertSnippetCommand>(control.Tag);

		public async Task InsertTocCmd(IRibbonControl control)
			=> await factory.Run<InsertTocCommand>();

		public async Task InsertWarningBlockCmd(IRibbonControl control)
			=> await factory.Run<InsertInfoBlockCommand>(true);

		public async Task InsertYellowStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertStatusCommand>(StatusColor.Yellow);

		public async Task InvertSelectionCmd(IRibbonControl control)
			=> await factory.Run<InvertSelectionCommand>();

		public async Task LinkReferencesCmd(IRibbonControl control)
			=> await factory.Run<LinkReferencesCommand>();

		public async Task JoinParagraphCmd(IRibbonControl control)
			=> await factory.Run<JoinParagraphCommand>();

		public async Task ManageFavoritesCmd(IRibbonControl control)
			=> await factory.Run<ManageFavoritesCommand>(ribbon);

		public async Task ManagePluginsCmd(IRibbonControl control)
			=> await factory.Run<ManagePluginsCommand>(ribbon);

		public async Task ManageSnippetsCmd(IRibbonControl control)
			=> await factory.Run<ManageSnippetsCommand>(ribbon);

		public async Task MapCmd(IRibbonControl control)
			=> await factory.Run<MapCommand>();

		public async Task MergeCmd(IRibbonControl control)
			=> await factory.Run<MergeCommand>();

		public async Task NameUrlsCmd(IRibbonControl control)
			=> await factory.Run<NameUrlsCommand>();

		public async Task NewStyleCmd(IRibbonControl control)
			=> await factory.Run<NewStyleCommand>();

		public async Task NumberPagesCmd(IRibbonControl control)
			=> await factory.Run<NumberPagesCommand>();

		public async Task NumberSectionsCmd(IRibbonControl control)
			=> await factory.Run<NumberSectionsCommand>();

		public async Task OutlineCmd(IRibbonControl control)
			=> await factory.Run<OutlineCommand>();

		public async Task PasteCellsCmd(IRibbonControl control)
			=> await factory.Run<PasteCellsCommand>();

		public async Task PasteRtfCmd(IRibbonControl control)
			=> await factory.Run<PasteRtfCommand>();

		public async Task PronunciateCmd(IRibbonControl control)
			=> await factory.Run<PronunciateCommand>();

		public async Task RecalculateFormulaCmd(IRibbonControl control)
			=> await factory.Run<RecalculateFormulaCommand>();

		public async Task RefreshFootnotesCmd(IRibbonControl control)
			=> await factory.Run<RefreshFootnotesCommand>();

		public async Task RemoveAuthorsCmd(IRibbonControl control)
			=> await factory.Run<RemoveAuthorsCommand>();

		public async Task RemoveCitationsCmd(IRibbonControl control)
			=> await factory.Run<RemoveCitationsCommand>();

		public async Task RemoveEmptyCmd(IRibbonControl control)
			=> await factory.Run<RemoveEmptyCommand>();

		public async Task RemoveFootnoteCmd(IRibbonControl control)
			=> await factory.Run<RemoveFootnoteCommand>();

		public async Task RemovePageNumbersCmd(IRibbonControl control)
			=> await factory.Run<RemovePageNumbersCommand>();

		public async Task RemoveSectionNumbersCmd(IRibbonControl control)
			=> await factory.Run<RemoveSectionNumbersCommand>();

		public async Task RemoveSpacingCmd(IRibbonControl control)
			=> await factory.Run<RemoveSpacingCommand>();

		public async Task ReplayCmd(IRibbonControl control)
			=> await factory.ReplayLastAction();

		public async Task ResizeImagesCmd(IRibbonControl control)
			=> await factory.Run<ResizeImagesCommand>();

		public async Task RestoreCollapsedCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Restore);

		public async Task RunPluginCmd(IRibbonControl control)
			=> await factory.Run<RunPluginCommand>(control.Tag);

		public async Task ExportCmd(IRibbonControl control)
			=> await factory.Run<ExportCommand>();

		public async Task SaveCollapsedCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Save);

		public async Task SaveSnippetCmd(IRibbonControl control)
			=> await factory.Run<SaveSnippetCommand>();

		public async Task SearchCmd(IRibbonControl control)
			=> await factory.Run<SearchCommand>();

		public async Task SearchAndReplaceCmd(IRibbonControl control)
			=> await factory.Run<SearchAndReplaceCommand>();

		public async Task SearchWebCmd(IRibbonControl control)
			=> await factory.Run<SearchWebCommand>(control.Tag);

		public async Task SettingsCmd(IRibbonControl control)
			=> await factory.Run<SettingsCommand>(ribbon);

		public async Task ShowKeyboardShortcutsCmd(IRibbonControl control)
			=> await factory.Run<ShowKeyboardShortcutsCommand>();

		public async Task ShowXmlCmd(IRibbonControl control)
			=> await factory.Run<ShowXmlCommand>();

		public async Task SortCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>();

		public async Task SplitCmd(IRibbonControl control)
			=> await factory.Run<SplitCommand>();

		public async Task SplitTableCmd(IRibbonControl control)
			=> await factory.Run<SplitTableCommand>();

		public async Task StartBiLinkCmd(IRibbonControl control)
			=> await factory.Run<BiLinkCommand>("mark");

		public async Task StrikeoutCmd(IRibbonControl control)
			=> await factory.Run<StrikeoutCommand>();

		public async Task TaggedCmd(IRibbonControl control)
			=> await factory.Run<TaggedCommand>();

		public async Task TaggingCmd(IRibbonControl control)
			=> await factory.Run<TaggingCommand>();

		public async Task TextToTableCmd(IRibbonControl control)
			=> await factory.Run<TextToTableCommand>();

		public async Task ToLowercaseCmd(IRibbonControl control)
			=> await factory.Run<ToCaseCommand>(ToCaseCommand.Lowercase);

		public async Task ToggleDttmCmd(IRibbonControl control)
			=> await factory.Run<ToggleDttmCommand>();

		public async Task ToTitlecaseCmd(IRibbonControl control)
			=> await factory.Run<ToCaseCommand>(ToCaseCommand.Titlecase);

		public async Task ToUppercaseCmd(IRibbonControl control)
			=> await factory.Run<ToCaseCommand>(ToCaseCommand.Uppercase);

		public async Task TrimCmd(IRibbonControl control)
			=> await factory.Run<TrimCommand>(false);

		public async Task TrimLeadingCmd(IRibbonControl control)
			=> await factory.Run<TrimCommand>(true);

		public async Task WordCountCmd(IRibbonControl control)
			=> await factory.Run<WordCountCommand>();
	}
}
