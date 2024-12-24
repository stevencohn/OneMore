//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter
#pragma warning disable S1135       // Track uses of "TODO" tags

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	public partial class AddIn
	{
		public async Task AboutCmd(IRibbonControl control)
			=> await factory.Run<AboutCommand>();


		[Command("ribAddCaptionButton_Label", Keys.None, "ribImagesMenu")]
		public async Task AddCaptionCmd(IRibbonControl control)
			=> await factory.Run<AddCaptionCommand>();


		[IgnorePalette]
		public async Task AddFavoritePageCmd(IRibbonControl control)
		{
			await using var provider = new FavoritesProvider(ribbon);
			await provider.AddFavorite();
		}


		[IgnorePalette]
		public async Task AddFavoriteSectionCmd(IRibbonControl control)
		{
			await using var provider = new FavoritesProvider(ribbon);
			await provider.AddFavorite(true);
		}


		[Command("ribAddFootnoteButton_Label", Keys.Control | Keys.Alt | Keys.F, "ribReferencesMenu")]
		public async Task AddFootnoteCmd(IRibbonControl control)
			=> await factory.Run<AddFootnoteCommand>();


		[Command("ribAddFormulaButton_Label", Keys.F5, "ribTableMenu")]
		public async Task AddFormulaCmd(IRibbonControl control)
			=> await factory.Run<AddFormulaCommand>();


		[Command("ribAddTagBankButton_Label", Keys.None, "ribSearchMenu")]
		public async Task AddTagBankCmd(IRibbonControl control)
			=> await factory.Run<TagBankCommand>(true);


		[Command("ribAdjustImagesButton_Label", Keys.None, "ribImagesMenu")]
		public async Task AdjustImagesCmd(IRibbonControl control)
			=> await factory.Run<AdjustImagesCommand>();

		public async Task AnalyzeCmd(IRibbonControl control)
			=> await factory.Run<AnalyzeCommand>();


		[Command("ribApplyStyle0Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D1)]
		public async Task ApplyStyle0Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(0);


		[Command("ribApplyStyle1Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D2)]
		public async Task ApplyStyle1Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(1);


		[Command("ribApplyStyle2Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D3)]
		public async Task ApplyStyle2Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(2);


		[Command("ribApplyStyle3Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D4)]
		public async Task ApplyStyle3Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(3);


		[Command("ribApplyStyle4Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D5)]
		public async Task ApplyStyle4Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(4);


		[Command("ribApplyStyle5Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D6)]
		public async Task ApplyStyle5Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(5);


		[Command("ribApplyStyle6Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D7)]
		public async Task ApplyStyle6Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(6);


		[Command("ribApplyStyle7Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D8)]
		public async Task ApplyStyle7Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(7);


		[Command("ribApplyStyle8Button_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.D9)]
		public async Task ApplyStyle8Cmd(IRibbonControl control)
			=> await factory.Run<ApplyStyleCommand>(8);


		public async Task ApplyStyleCmd(IRibbonControl control, string selectedId, int selectedIndex)
			=> await factory.Run<ApplyStyleCommand>(selectedIndex);

		public async Task ApplyStylesCmd(IRibbonControl control)
			=> await factory.Run<ApplyStylesCommand>();

		public async Task ApplyTableThemeCmd(IRibbonControl control, string selectedId, int selectedIndex)
			=> await factory.Run<ApplyTableThemeCommand>(selectedIndex);

		public async Task ArchiveCmd(IRibbonControl control)
			=> await factory.Run<ArchiveCommand>(control.Tag); // tag=scope

		public async Task ArrangeContainersCmd(IRibbonControl control)
			=> await factory.Run<ArrangeContainersCommand>();

		public async Task BreakingCmd(IRibbonControl control)
			=> await factory.Run<BreakingCommand>();


		[Command("ribCalendarButton_Label", Keys.None)]
		public async Task CalendarCmd(IRibbonControl control)
			=> await factory.Run<CalendarCommand>();


		[Command("ribCaptionAttachmentsButton_Label", Keys.None, "ribPageMenu")]
		public async Task CaptionAttachmentsCmd(IRibbonControl control)
			=> await factory.Run<CaptionAttachmentsCommand>();


		[Command("ribCheckForUpdatesButton_Label", Keys.None)]
		public async Task CheckForUpdatesCmd(IRibbonControl control)
			=> await factory.Run<UpdateCommand>(true);


		[Command("ribCheckUrlsButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task CheckUrlsCmd(IRibbonControl control)
			=> await factory.Run<CheckUrlsCommand>(true);


		[Command("ribChooseFavoriteButton_Label", Keys.Alt | Keys.F)]
		public async Task ChooseFavoriteCmd(IRibbonControl control)
			=> await factory.Run<GotoFavoriteCommand>(null);


		[Command("ribCleanRemindersButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task CleanRemindersCmd(IRibbonControl control)
			=> await factory.Run<CleanRemindersCommand>();


		[Command("ribClearBackgroundButton_Label", Keys.None, "ribCleanMenu")]
		public async Task ClearBackgroundCmd(IRibbonControl control)
			=> await factory.Run<ClearBackgroundCommand>();


		[Command("ribClearLogButton_Label", Keys.Control | Keys.F8)]
		public async Task ClearLogCmd(IRibbonControl control)
			=> await factory.Run<ClearLogCommand>();


		[Command("ribClearTableShadingButton_Label", Keys.None, "ribTableMenu")]
		public async Task ClearTableShadingCmd(IRibbonControl control)
			=> await factory.Run<ApplyTableThemeCommand>(int.MaxValue);


		[Command("ribCollapsePagesButton_Label", Keys.None)]
		public async Task CollapsePagesCmd(IRibbonControl control)
			=> await factory.Run<CollapsePagesCommand>();


		[Command("ribCollapseContentButton_Label", Keys.None, "ribPageMenu")]
		public async Task CollapseContentCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Collapse);


		[Command("ribChooseColorizerButton_Label", Keys.Alt | Keys.C)]
		public async Task ChooseColorizerCmd(IRibbonControl control)
			=> await factory.Run<ChooseColorizerCommand>();


		[IgnorePalette]
		[Command("ribColorizerButton_Label", Keys.None, "ribColorizeMenu")]
		public async Task ColorizeCmd(IRibbonControl control)
			=> await factory.Run<ColorizeCommand>(control.Tag); // tag=language


		[Command("ribCommandPaletteButton_Label", Keys.Control | Keys.Shift | Keys.P)]
		public async Task CommandPaletteCmd(IRibbonControl control)
			=> await factory.Run<CommandPaletteCommand>();


		[Command("ribCompleteReminderButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task CompleteReminderCmd(IRibbonControl control)
			=> await factory.Run<CompleteReminderCommand>();


		[Command("ribCompressImagesButton_Label", Keys.None, "ribImagesMenu")]
		public async Task CompressImagesCmd(IRibbonControl control)
			=> await factory.Run<CompressImagesCommand>();


		[Command("ribCopyAcrossButton_Label", Keys.None, "ribTableMenu")]
		public async Task CopyAcrossCmd(IRibbonControl control)
			=> await factory.Run<CopyAcrossCommand>();


		[Command("ribConvertMarkdownButton_Label", Keys.Shift | Keys.Alt | Keys.M, "ribEditMenu")]
		public async Task ConvertMarkdownCmd(IRibbonControl control)
			=> await factory.Run<ConvertMarkdownCommand>();


		[Command("ribCopyAsTextButton_Label", Keys.None, "ribEditMenu")]
		public async Task CopyAsTextCmd(IRibbonControl control)
			=> await factory.Run<CopyAsTextCommand>();


		[Command("ribCopyDownButton_Label", Keys.None, "ribTableMenu")]
		public async Task CopyDownCmd(IRibbonControl control)
			=> await factory.Run<CopyDownCommand>();


		[Command("ribCopyFolderButton_Label", Keys.None)]
		public async Task CopyFolderCmd(IRibbonControl control)
			=> await factory.Run<CopyFolderCommand>();


		[Command("ribCopyLinkToPageButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task CopyLinkToPageCmd(IRibbonControl control)
			=> await factory.Run<CopyLinkToPageCommand>();


		[Command("ribCopyLinkToParagraphButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task CopyLinkToParagraphCmd(IRibbonControl control)
			=> await factory.Run<CopyLinkToParagraphCommand>();


		[Command("ribCopyAsMarkdownButton_Label", Keys.None, "ribEditMenu")]
		public async Task CopyAsMarkdownCmd(IRibbonControl control)
			=> await factory.Run<CopyAsMarkdownCommand>(true);


		[Command("ribCrawlWebPageButton_Label", Keys.None)]
		public async Task CrawlWebPageCmd(IRibbonControl control)
			=> await factory.Run<CrawlWebPageCommand>();


		[Command("ribCopyPageContentButton_Label", Keys.None, "ribPageMenu")]
		public async Task CopyPageContentCmd(IRibbonControl control)
			=> await factory.Run<CopyPageContentCommand>();


		[Command("ribCreatePagesButton_Label", Keys.None, "ribPageMenu")]
		public async Task CreatePagesCmd(IRibbonControl control)
			=> await factory.Run<CreatePagesCommand>();


		[Command("ribCropImageButton_Label", Keys.None)]
		public async Task CropImageCmd(IRibbonControl control)
			=> await factory.Run<CropImageCommand>();


		[Command("ribDateStampButton_Label", Keys.None)]
		public async Task DateStampCmd(IRibbonControl control)
			=> await factory.Run<DateStampCommand>();


		[Command("ribDecreaseFontSizeButton_Label", Keys.Control | Keys.Alt | Keys.OemMinus, "ribEditMenu")]
		public async Task DecreaseFontSizeCmd(IRibbonControl control)
			=> await factory.Run<DecreaseFontSizeCommand>();


		[Command("ribDeleteFormulaButton_Label", Keys.None, "ribTableMenu")]
		public async Task DeleteFormulaCmd(IRibbonControl control)
			=> await factory.Run<DeleteFormulaCommand>();


		[Command("ribDeleteReminderButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task DeleteReminderCmd(IRibbonControl control)
			=> await factory.Run<DeleteReminderCommand>();


		[Command("ribDiagnosticsButton_Label", Keys.Shift | Keys.F8)]
		public async Task DiagnosticsCmd(IRibbonControl control)
			=> await factory.Run<DiagnosticsCommand>();


		[Command("ribDisableSpellCheckButton_Label", Keys.F4, "ribEditMenu")]
		public async Task DisableSpellCheckCmd(IRibbonControl control)
			=> await factory.Run<DisableSpellCheckCommand>();


		[Command("ribDuplicateLineButton_Label", Keys.Alt | Keys.Shift | Keys.C, "ribEditMenu")]
		public async Task DuplicateLineCmd(IRibbonControl control)
			=> await factory.Run<DuplicateLineCommand>(false);


		[Command("ribDuplicateLineAboveButton_Label", Keys.None, "ribEditMenu")]
		public async Task DuplicateLineAboveCmd(IRibbonControl control)
			=> await factory.Run<DuplicateLineCommand>(true);


		[Command("ribDuplicatePageButton_Label", Keys.None, "ribPageMenu")]
		public async Task DuplicatePageCmd(IRibbonControl control)
			=> await factory.Run<DuplicatePageCommand>(true);


		[Command("ribEditStylesButton_Label", Keys.None)]
		public async Task EditStylesCmd(IRibbonControl control)
			=> await factory.Run<EditStylesCommand>();


		[Command("ribEditTableThemesButton_Label", Keys.None)]
		public async Task EditTableThemesCmd(IRibbonControl control)
			=> await factory.Run<EditTableThemesCommand>();


		[Command("ribEmbedSubpageButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task EmbedSubpageCmd(IRibbonControl control)
			=> await factory.Run<EmbedSubpageCommand>(false);


		[Command("ribEnableSpellCheckButton_Label", Keys.None, "ribEditMenu")]
		public async Task EnableSpellCheckCmd(IRibbonControl control)
			=> await factory.Run<EnableSpellCheckCommand>();


		[Command("ribExpandContentButton_Label", Keys.None, "ribPageMenu")]
		public async Task ExpandContentCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Expand);


		[Command("ribExpandSnippetButton_Label", Keys.Alt | Keys.F3, "ribSnippetsMenu")]
		public async Task ExpandSnippetCmd(IRibbonControl control)
			=> await factory.Run<InsertSnippetCommand>(string.Empty);


		[Command("ribExportButton_Label", Keys.None)]
		public async Task ExportCmd(IRibbonControl control)
			=> await factory.Run<ExportCommand>();


		[Command("ribFileQuickNotesButton_Label", Keys.None)]
		public async Task FileQuickNotesCmd(IRibbonControl control)
			=> await factory.Run<FileQuickNotesCommand>();


		[Command("ribFillAcrossButton_Label", Keys.None, "ribTableMenu")]
		public async Task FillAcrossCmd(IRibbonControl control)
			=> await factory.Run<FillAcrossCommand>();


		[Command("ribFillDownButton_Label", Keys.Control | Keys.D, "ribTableMenu")]
		public async Task FillDownCmd(IRibbonControl control)
			=> await factory.Run<FillDownCommand>();


		[Command("ribFinishBiLinkButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task FinishBiLinkCmd(IRibbonControl control)
			=> await factory.Run<BiLinkCommand>("link");


		[Command("ribFitGridToTextButton_Label", Keys.None, "ribPageMenu")]
		public async Task FitGridToTextCmd(IRibbonControl control)
			=> await factory.Run<FitGridToTextCommand>();


		[Command("ribGetImagesButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task GetImagesCmd(IRibbonControl control)
			=> await factory.Run<GetImagesCommand>(true);


		public async Task GotoFavoriteCmd(IRibbonControl control)
			=> await factory.Run<GotoFavoriteCommand>(control.Tag); //tag=pageid


		[Command("ribHashtaggerButton_Label", Keys.Alt | Keys.T, "ribSearchMenu")]
		public async Task HashtaggerCmd(IRibbonControl control)
			=> await factory.Run<HashtaggerCommand>(1);



		[Command("ribHighlightButton_Label", Keys.Control | Keys.Shift | Keys.H, "ribEditMenu")]
		public async Task HighlightCmd(IRibbonControl control)
			=> await factory.Run<HighlightCommand>(1);


		[Command("ribHighlightLastButton_Label", Keys.Control | Keys.Shift | Keys.J, "ribEditMenu")]
		public async Task HighlightLastCmd(IRibbonControl control)
			=> await factory.Run<HighlightCommand>(0);


		[Command("ribHighlightNoneButton_Label", Keys.Control | Keys.Shift | Keys.D0, "ribEditMenu")]
		public async Task HighlightNoneCmd(IRibbonControl control)
			=> await factory.Run<HighlightCommand>(-1);


		[Command("ribHighlightFormulaButton_Label", Keys.None, "ribTableMenu")]
		public async Task HighlightFormulaCmd(IRibbonControl control)
			=> await factory.Run<HighlightFormulaCommand>();


		[Command("ribImportButton_Label", Keys.None)]
		public async Task ImportCmd(IRibbonControl control)
			=> await factory.Run<ImportCommand>();


		[Command("ribImportWebButton_Label", Keys.None)]
		public async Task ImportWebCmd(IRibbonControl control)
			=> await factory.Run<ImportWebCommand>();


		[Command("ribImportOutlookTasksButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task ImportOutlookTasksCmd(IRibbonControl control)
			=> await factory.Run<ImportOutlookTasksCommand>();


		[Command("ribIncreaseFontSizeButton_Label", Keys.Control | Keys.Alt | Keys.Oemplus, "ribEditMenu")]
		public async Task IncreaseFontSizeCmd(IRibbonControl control)
			=> await factory.Run<IncreaseFontSizeCommand>();


		[Command("ribInsertBlueStatusButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertBlueStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertBlueStatusCommand>();


		[Command("ribInsertTextBoxButton_Label", Keys.Alt | Keys.F6, "ribSnippetsMenu")]
		public async Task InsertTextBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertTextBoxCommand>();


		[Command("ribInsertBreadcrumbButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertBreadcrumbCmd(IRibbonControl control)
			=> await factory.Run<InsertBreadcrumbCommand>();


		[Command("ribInsertCalendarButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertCalendarCmd(IRibbonControl control)
			=> await factory.Run<InsertCalendarCommand>();


		[Command("ribInsertCellsButton_Label", Keys.None, "ribTableMenu")]
		public async Task InsertCellsCmd(IRibbonControl control)
			=> await factory.Run<InsertCellsCommand>();


		[Command("ribInsertCodeBoxButton_Label", Keys.F6, "ribSnippetsMenu")]
		public async Task InsertCodeBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertCodeBoxCommand>();


		[Command("ribInsertDateButton_Label", Keys.Control | Keys.Shift | Keys.D, "ribSnippetsMenu")]
		public async Task InsertDateCmd(IRibbonControl control)
			=> await factory.Run<InsertDateCommand>(false);


		[Command("ribInsertDateTimeButton_Label", Keys.Control | Keys.Shift | Keys.Alt | Keys.D, "ribSnippetsMenu")]
		public async Task InsertDateTimeCmd(IRibbonControl control)
			=> await factory.Run<InsertDateCommand>(true);


		[Command("ribInsertDoubleLineButton_Label", Keys.Alt | Keys.Shift | Keys.F12, "ribSnippetsMenu")]
		public async Task InsertDoubleLineCmd(IRibbonControl control)
			=> await factory.Run<InsertDoubleLineCommand>();


		[Command("ribInsertEmojiButton_Label", Keys.Alt | Keys.F12, "ribSnippetsMenu")]
		public async Task InsertEmojiCmd(IRibbonControl control)
			=> await factory.Run<InsertEmojiCommand>();


		[Command("ribInsertExpandButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertExpandCmd(IRibbonControl control)
			=> await factory.Run<InsertExpandCommand>();


		[Command("ribInsertGrayStatusButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertGrayStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertGrayStatusCommand>();


		[Command("ribInsertGreenStatusButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertGreenStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertGreenStatusCommand>();


		[Command("ribInsertInfoBoxButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertInfoBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertInfoBoxCommand>("info");


		[Command("ribInsertNoteBoxButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertNoteBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertInfoBoxCommand>("note");


		[Command("ribInsertQRButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task InsertQRCmd(IRibbonControl control)
			=> await factory.Run<InsertQRCommand>(false);


		[Command("ribInsertRedStatusButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertRedStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertRedStatusCommand>();


		[Command("ribInsertSingleLineButton_Label", Keys.Alt | Keys.Shift | Keys.F11, "ribSnippetsMenu")]
		public async Task InsertSingleLineCmd(IRibbonControl control)
			=> await factory.Run<InsertSingleLineCommand>();


		public async Task InsertSnippetCmd(IRibbonControl control)
			=> await factory.Run<InsertSnippetCommand>(control.Tag); // tag=filepath


		[Command("ribInsertTimerButton_Label", Keys.F2)]
		public async Task InsertTimerCmd(IRibbonControl control)
			=> await factory.Run<TimerWindowCommand>(TimerWindow.CopyCmd);


		[Command("ribInsertTocButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertTocCmd(IRibbonControl control)
			=> await factory.Run<InsertTocCommand>();


		[Command("ribInsertWarnBoxButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertWarnBoxCmd(IRibbonControl control)
			=> await factory.Run<InsertInfoBoxCommand>("warn");


		[Command("ribInsertYellowStatusButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task InsertYellowStatusCmd(IRibbonControl control)
			=> await factory.Run<InsertYellowStatusCommand>();


		[Command("ribInvertSelectionButton_Label", Keys.None, "ribEditMenu")]
		public async Task InvertSelectionCmd(IRibbonControl control)
			=> await factory.Run<InvertSelectionCommand>();


		[Command("ribJoinParagraphButton_Label", Keys.None, "ribEditMenu")]
		public async Task JoinParagraphCmd(IRibbonControl control)
			=> await factory.Run<JoinParagraphCommand>();


		[Command("ribLinkReferencesButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task LinkReferencesCmd(IRibbonControl control)
			=> await factory.Run<LinkReferencesCommand>();


		[Command("ribLoadStylesButton_Label", Keys.None)]
		public async Task LoadStylesCmd(IRibbonControl control)
			=> await factory.Run<LoadStylesCommand>();


		public async Task ManageFavoritesCmd(IRibbonControl control)
			=> await factory.Run<ManageFavoritesCommand>(ribbon);

		public async Task ManagePluginsCmd(IRibbonControl control)
			=> await factory.Run<ManagePluginsCommand>(ribbon);

		public async Task ManageSnippetsCmd(IRibbonControl control)
			=> await factory.Run<ManageSnippetsCommand>(ribbon);


		[Command("ribMapButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task MapCmd(IRibbonControl control)
			=> await factory.Run<MapCommand>();


		[Command("ribMergeButton_Label", Keys.None, "ribPageMenu")]
		public async Task MergeCmd(IRibbonControl control)
			=> await factory.Run<MergeCommand>();


		[Command("ribMermaidButton_Label", Keys.None, "ribImagesMenu")]
		public async Task MermaidCmd(IRibbonControl control)
			=> await factory.Run<MermaidCommand>();


		[Command("ribMovePageBottomButton_Label", Keys.None, "ribPageMenu")]
		public async Task MovePageBottomCmd(IRibbonControl control)
			=> await factory.Run<MovePageCommand>(false);


		[Command("ribMovePageContentButton_Label", Keys.None, "ribPageMenu")]
		public async Task MovePageContentCmd(IRibbonControl control)
			=> await factory.Run<MovePageContentCommand>();


		[Command("ribMovePageTopButton_Label", Keys.None, "ribPageMenu")]
		public async Task MovePageTopCmd(IRibbonControl control)
			=> await factory.Run<MovePageCommand>(true);


		[Command("ribNameUrlsButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task NameUrlsCmd(IRibbonControl control)
			=> await factory.Run<NameUrlsCommand>();


		[Command("ribNavigatorButton_Label", Keys.Shift | Keys.Alt | Keys.N)]
		public async Task NavigatorCmd(IRibbonControl control)
			=> await factory.Run<NavigatorCommand>();


		[Command("ribNextUnreadPageButton_Label", Keys.None, "ribSearchMenu")]
		public async Task NextUnreadPageCmd(IRibbonControl control)
			=> await factory.Run<NextUnreadPageCommand>();


		[Command("ribNewStyleButton_Label", Keys.None)]
		public async Task NewStyleCmd(IRibbonControl control)
			=> await factory.Run<NewStyleCommand>();


		[Command("ribNotebookWordCountButton_Label", Keys.None)]
		public async Task NotebookWordCountCmd(IRibbonControl control)
			=> await factory.Run<WordCountCommand>(OneNote.Scope.Sections);


		[Command("ribNumberPagesButton_Label", Keys.None, "ribNumberingMenu")]
		public async Task NumberPagesCmd(IRibbonControl control)
			=> await factory.Run<NumberPagesCommand>();


		[Command("ribNumberSectionsButton_Label", Keys.None)]
		public async Task NumberSectionsCmd(IRibbonControl control)
			=> await factory.Run<NumberSectionsCommand>();


		[Command("ribOpenImageWithButton_Label", Keys.None, "ribImagesMenu")]
		public async Task OpenImageWithCmd(IRibbonControl control)
			=> await factory.Run<OpenImageWithCommand>();


		[Command("ribOpenLogButton_Label", Keys.None)]
		public async Task OpenLogCmd(IRibbonControl control)
			=> await factory.Run<OpenLogCommand>();


		[Command("ribOutlineButton_Label", Keys.None, "ribNumberingMenu")]
		public async Task OutlineCmd(IRibbonControl control)
			=> await factory.Run<OutlineCommand>();


		[Command("ribPageColorButton_Label", Keys.None, "ribPageMenu")]
		public async Task PageColorCmd(IRibbonControl control)
			=> await factory.Run<PageColorCommand>();


		[Command("ribPasteCellsButton_Label", Keys.None, "ribTableMenu")]
		public async Task PasteCellsCmd(IRibbonControl control)
			=> await factory.Run<PasteCellsCommand>();


		[Command("ribPasteRtfButton_Label", Keys.Control | Keys.Alt | Keys.V, "ribEditMenu")]
		public async Task PasteRtfCmd(IRibbonControl control)
			=> await factory.Run<PasteRtfCommand>();


		[Command("ribPasteTextButton_Label", Keys.Control | Keys.Shift | Keys.V, "ribEditMenu")]
		public async Task PasteTextCmd(IRibbonControl control)
			=> await factory.Run<PasteTextCommand>();


		[IgnorePalette]
		[Command("ribPinPageButton_Label", Keys.Control | Keys.Shift | Keys.B)]
		public async Task PinpageCmd(IRibbonControl control)
			=> await factory.Run<PinPageCommand>();


		[Command("ribPlantUmlButton_Label", Keys.None, "ribImagesMenu")]
		public async Task PlantUmlCmd(IRibbonControl control)
			=> await factory.Run<PlantUmlCommand>();


		[Command("ribPreviewMarkdownButton_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.M, "ribEditMenu")]
		public async Task PreviewMarkdownCmd(IRibbonControl control)
			=> await factory.Run<PreviewMarkdownCommand>();


		[Command("ribPreviousUnreadPageButton_Label", Keys.None, "ribSearchMenu")]
		public async Task PreviousUnreadPageCmd(IRibbonControl control)
			=> await factory.Run<PreviousUnreadPageCommand>();


		[Command("ribPronunciateButton_Label", Keys.None, "ribEditMenu")]
		public async Task PronunciateCmd(IRibbonControl control)
			=> await factory.Run<PronunciateCommand>();


		[IgnorePalette]
		[Command("ribQuickPaletteButton_Label", Keys.Control | Keys.Oemcomma)]
		public async Task QuickPaletteCmd(IRibbonControl control)
			=> await factory.Run<QuickPaletteCommand>();


		[Command("ribRecalculateFormulaButton_Label", Keys.Shift | Keys.F5, "ribTableMenu")]
		public async Task RecalculateFormulaCmd(IRibbonControl control)
			=> await factory.Run<RecalculateFormulaCommand>();


		[Command("ribRefreshFootnotesButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task RefreshFootnotesCmd(IRibbonControl control)
			=> await factory.Run<RefreshFootnotesCommand>();


		[Command("ribRefreshPageLinksButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task RefreshPageLinksCmd(IRibbonControl control)
			=> await factory.Run<RefreshPageLinksCommand>();


		[Command("ribRemindButton_Label", Keys.F8, "ribRemindersMenu")]
		public async Task RemindCmd(IRibbonControl control)
			=> await factory.Run<RemindCommand>();


		[Command("ribRemoveAuthorsButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveAuthorsCmd(IRibbonControl control)
			=> await factory.Run<RemoveAuthorsCommand>();


		[Command("ribRemoveCitationsButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveCitationsCmd(IRibbonControl control)
			=> await factory.Run<RemoveCitationsCommand>();


		[Command("ribRemoveDateStampButton_Label", Keys.None)]
		public async Task RemoveDateStampCmd(IRibbonControl control)
			=> await factory.Run<DateStampCommand>(false);


		[Command("ribRemoveDuplicatesButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveDuplicatesCmd(IRibbonControl control)
			=> await factory.Run<RemoveDuplicatesCommand>();


		[Command("ribRemoveEmptyButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveEmptyCmd(IRibbonControl control)
			=> await factory.Run<RemoveEmptyCommand>();


		[Command("ribRemoveFootnoteButton_Label", Keys.Control | Keys.Shift | Keys.F, "ribReferencesMenu")]
		public async Task RemoveFootnoteCmd(IRibbonControl control)
			=> await factory.Run<RemoveFootnoteCommand>();


		[Command("ribRemoveInkButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveInkCmd(IRibbonControl control)
			=> await factory.Run<RemoveInkCommand>();


		[Command("ribRemovePageNumbersButton_Label", Keys.None, "ribNumberingMenu")]
		public async Task RemovePageNumbersCmd(IRibbonControl control)
			=> await factory.Run<RemovePageNumbersCommand>();


		[Command("ribRemoveSectionNumbersButton_Label", Keys.None)]
		public async Task RemoveSectionNumbersCmd(IRibbonControl control)
			=> await factory.Run<RemoveSectionNumbersCommand>();


		[Command("ribRemoveSpacingButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveSpacingCmd(IRibbonControl control)
			=> await factory.Run<RemoveSpacingCommand>();


		[Command("ribRemoveTagBankButton_Label", Keys.None, "ribSearchMenu")]
		public async Task RemoveTagBankCmd(IRibbonControl control)
			=> await factory.Run<TagBankCommand>(false);


		[Command("ribRemoveTagsButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RemoveTagsCmd(IRibbonControl control)
			=> await factory.Run<RemoveTagsCommand>();


		[Command("ribReplayButton_Label", Keys.Alt | Keys.Shift | Keys.R)]
		public async Task ReplayCmd(IRibbonControl control)
			=> await factory.ReplayLastAction();


		[Command("ribReportRemindersButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task ReportRemindersCmd(IRibbonControl control)
			=> await factory.Run<ReportRemindersCommand>();


		[Command("ribResetTasksButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task ResetTasksCmd(IRibbonControl control)
			=> await factory.Run<ResetTasksCommand>();


		[Command("ribRestartTimerButton_Label", Keys.Shift | Keys.F2)]
		public async Task RestartTimerCmd(IRibbonControl control)
			=> await factory.Run<TimerWindowCommand>(TimerWindow.RestartCmd);


		[Command("ribRestoreAutosizeButton_Label", Keys.None, "ribCleanMenu")]
		public async Task RestoreAutosizeCmd(IRibbonControl control)
			=> await factory.Run<RestoreAutosizeCommand>();


		[Command("ribRestoreCollapsedButton_Label", Keys.None, "ribPageMenu")]
		public async Task RestoreCollapsedCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Restore);

		public async Task RunPluginCmd(IRibbonControl control)
			=> await factory.Run<RunPluginCommand>(control?.Tag); // tag=plugin


		[Command("ribSaveCollapsedButton_Label", Keys.None, "ribPageMenu")]
		public async Task SaveCollapsedCmd(IRibbonControl control)
			=> await factory.Run<ExpandoCommand>(Expando.Save);


		[Command("ribSaveSnippetButton_Label", Keys.None, "ribSnippetsMenu")]
		public async Task SaveSnippetCmd(IRibbonControl control)
			=> await factory.Run<SaveSnippetCommand>();


		[Command("ribScanHashtagsButton_Label", Keys.Control | Keys.Alt | Keys.F9, "ribSearchMenu")]
		public async Task ScanHashtagsCmd(IRibbonControl control)
			=> await factory.Run<ScanHashtagsCommand>();


		[Command("ribScanHashtagsOnPageButton_Label", Keys.Control | Keys.Alt | Keys.F10, "ribSearchMenu")]
		public async Task ScanHashtagsOnPageCmd(IRibbonControl control)
			=> await factory.Run<ScanHashtagsOnPageCommand>();


		[Command("ribScheduleHashtagScanButton_Label", Keys.None, "ribSearchMenu")]
		public async Task ScheduleHashtagScanCmd(IRibbonControl control)
			=> await factory.Run<HashtagScanCommand>();


		[Command("ribSearchButton_Label", Keys.None, "ribSearchMenu")]
		public async Task SearchCmd(IRibbonControl control)
			=> await factory.Run<SearchCommand>();


		[Command("ribSearchAndReplaceButton_Label", Keys.Control | Keys.H, "ribSearchMenu")]
		public async Task SearchAndReplaceCmd(IRibbonControl control)
			=> await factory.Run<SearchAndReplaceCommand>();


		[Command("ribSearchHashtagsButton_Label", Keys.Alt | Keys.F9, "ribSearchMenu")]
		public async Task SearchHashtagsCmd(IRibbonControl control)
			=> await factory.Run<HashtagCommand>();

		// added to page context menu via Search Engine settings
		public async Task SearchWebCmd(IRibbonControl control)
			=> await factory.Run<SearchWebCommand>(control.Tag); // tag=engine


		public async Task SectionColorCmd(IRibbonControl control)
			=> await factory.Run<SectionColorCommand>();


		[Command("ribSectionWordCountButton_Label", Keys.None)]
		public async Task SectionWordCountCmd(IRibbonControl control)
			=> await factory.Run<WordCountCommand>(OneNote.Scope.Pages);


		[Command("ribSelectImagesButton_Label", Keys.None, "ribEditMenu")]
		public async Task SelectImagesCmd(IRibbonControl control)
			=> await factory.Run<SelectImagesCommand>();


		[Command("ribSelectInkButton_Label", Keys.None, "ribEditMenu")]
		public async Task SelectInkCmd(IRibbonControl control)
			=> await factory.Run<SelectInkCommand>();


		[Command("ribSelectStyleButton_Label", Keys.None, "ribEditMenu")]
		public async Task SelectStyleCmd(IRibbonControl control)
			=> await factory.Run<SelectStyleCommand>();


		[Command("ribSelectTablesButton_Label", Keys.None, "ribTableMenu")]
		public async Task SelectTablesCmd(IRibbonControl control)
			=> await factory.Run<SelectTablesCommand>();


		public async Task ProofingCmd(IRibbonControl control)
			=> await factory.Run<ProofingCommand>(control.Tag); // tag=language


		[Command("ribSettingsButton_Label", Keys.None)]
		public async Task SettingsCmd(IRibbonControl control)
			=> await factory.Run<SettingsCommand>(ribbon);


		public async Task ShowKeyboardShortcutsCmd(IRibbonControl control)
			=> await factory.Run<ShowKeyboardShortcutsCommand>();


		[Command("ribShowXmlButton_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.X)]
		public async Task ShowXmlCmd(IRibbonControl control)
			=> await factory.Run<ShowXmlCommand>();


		[Command("ribShutdownTimerButton_Label", Keys.Control | Keys.Shift | Keys.F2)]
		public async Task ShutdownTimerCmd(IRibbonControl control)
			=> await factory.Run<TimerWindowCommand>(TimerWindow.ShutdownCmd);


		[Command("ribSortButton_Label", Keys.None)]
		public async Task SortCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>();


		[Command("ribSortListButton_Label", Keys.None, "ribEditMenu")]
		public async Task SortListCmd(IRibbonControl control)
			=> await factory.Run<SortListCommand>();


		public async Task SortPageCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>(OneNote.Scope.Children);

		public async Task SortPagesCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>(OneNote.Scope.Pages);

		public async Task SortNotebooksCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>(OneNote.Scope.Notebooks);


		public async Task SortSectionsCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>(OneNote.Scope.Sections);

		public async Task SortSectionsInGroupCmd(IRibbonControl control)
			=> await factory.Run<SortCommand>(OneNote.Scope.SectionGroups);


		[Command("ribSplitButton_Label", Keys.None, "ribPageMenu")]
		public async Task SplitCmd(IRibbonControl control)
			=> await factory.Run<SplitCommand>();


		[Command("ribSplitTableButton_Label", Keys.None, "ribTableMenu")]
		public async Task SplitTableCmd(IRibbonControl control)
			=> await factory.Run<SplitTableCommand>();


		[Command("ribStackBackgroundImagesButton_Label", Keys.None, "ribImagesMenu")]
		public async Task StackBackgroundImagesCmd(IRibbonControl control)
			=> await factory.Run<StackBackgroundImagesCommand>();


		[Command("ribStartBiLinkButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task StartBiLinkCmd(IRibbonControl control)
			=> await factory.Run<BiLinkCommand>("mark");


		[Command("ribStartTimerButton_Label", Keys.Alt | Keys.F2)]
		public async Task StartTimerCmd(IRibbonControl control)
			=> await factory.Run<TimerWindowCommand>();


		[Command("ribStrikeoutTasksButton_Label", Keys.None, "ribRemindersMenu")]
		public async Task StrikeoutTasksCmd(IRibbonControl control)
			=> await factory.Run<StrikeoutTasksCommand>();


		[Command("ribStylizeImagesButton_Label", Keys.None, "ribImagesMenu")]
		public async Task StylizeImagesCmd(IRibbonControl control)
			=> await factory.Run<StylizeImagesCommand>();


		[Command("ribTextToTableButton_Label", Keys.None, "ribTableMenu")]
		public async Task TextToTableCmd(IRibbonControl control)
			=> await factory.Run<TextToTableCommand>();


		[Command("ribLowercaseButton_Label", Keys.Control | Keys.Shift | Keys.U, "ribEditMenu")]
		public async Task LowercaseCmd(IRibbonControl control)
			=> await factory.Run<LowercaseCommand>();


		[Command("ribToggleDttmButton_Label", Keys.None, "ribCleanMenu")]
		public async Task ToggleDttmCmd(IRibbonControl control)
			=> await factory.Run<ToggleDttmCommand>();


		[Command("ribTitlecaseButton_Label", Keys.None, "ribEditMenu")]
		public async Task TitlecaseCmd(IRibbonControl control)
			=> await factory.Run<TitlecaseCommand>();


		[Command("ribTrimButton_Label", Keys.None, "ribCleanMenu")]
		public async Task TrimCmd(IRibbonControl control)
			=> await factory.Run<TrimCommand>(false);


		[Command("ribTrimLeadingButton_Label", Keys.None, "ribCleanMenu")]
		public async Task TrimLeadingCmd(IRibbonControl control)
			=> await factory.Run<TrimCommand>(true);


		[Command("ribUnnameUrlsButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task UnnameUrlsCmd(IRibbonControl control)
			=> await factory.Run<UnnameUrlsCommand>();


		[Command("ribUpdatePageTimeButton_Label", Keys.None, "ribPageMenu")]
		public async Task UpdatePageTimeCmd(IRibbonControl control)
			=> await factory.Run<UpdatePageTimeCommand>(true);


		[Command("ribUpdateSubpageButton_Label", Keys.None, "ribReferencesMenu")]
		public async Task UpdateSubpageCmd(IRibbonControl control)
			=> await factory.Run<EmbedSubpageCommand>(true);


		[Command("ribUppercaseButton_Label", Keys.Control | Keys.Alt | Keys.Shift | Keys.U, "ribEditMenu")]
		public async Task UppercaseCmd(IRibbonControl control)
			=> await factory.Run<UppercaseCommand>();


		[Command("ribViewNotebookInBrowserButton_Label", Keys.None)]
		public async Task ViewNotebookInBrowserCmd(IRibbonControl control)
			=> await factory.Run<ViewNotebookInBrowserCommand>();


		[Command("ribViewPageInBrowserButton_Label", Keys.None, "ribPageMenu")]
		public async Task ViewPageInBrowserCmd(IRibbonControl control)
			=> await factory.Run<ViewPageInBrowserCommand>();


		[Command("ribViewSectionInBrowserButton_Label", Keys.None)]
		public async Task ViewSectionInBrowserCmd(IRibbonControl control)
			=> await factory.Run<ViewSectionInBrowserCommand>();


		[Command("ribWordCountButton_Label", Keys.None, "ribPageMenu")]
		public async Task WordCountCmd(IRibbonControl control)
			=> await factory.Run<WordCountCommand>();
	}
}
