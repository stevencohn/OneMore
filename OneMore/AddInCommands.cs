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


	public partial class AddIn
	{

		public void AboutCmd(IRibbonControl control)
		{
			factory.Run<AboutCommand>();
		}

		public void AddCaptionCmd(IRibbonControl control)
		{
			factory.Run<AddCaptionCommand>();
		}

		public void AddFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<AddFootnoteCommand>().Execute();
		}

		public void AddFormulaCmd(IRibbonControl control)
		{
			factory.Run<AddFormulaCommand>();
		}

		public void AddTitleIconCmd(IRibbonControl control)
		{
			factory.Run<AddTitleIconCommand>();
		}

		public void ApplyStyleCmd(IRibbonControl control, string selectedId, int selectedIndex)
		{
			factory.GetCommand<ApplyStyleCommand>().Execute(selectedIndex);
		}

		public void ApplyStylesCmd(IRibbonControl control)
		{
			factory.GetCommand<ApplyStylesCommand>().Execute();
		}

		public void ChangePageColorCmd(IRibbonControl control)
		{
			factory.GetCommand<ChangePageColorCommand>().Execute();
		}

		public void CollapseCmd(IRibbonControl control)
		{
			factory.Run<CollapseCommand>();
		}

		public void CropImageCmd(IRibbonControl control)
		{
			factory.Run<CropImageCommand>();
		}

		public void DecreaseFontSizeCmd(IRibbonControl control)
		{
			factory.Run<AlterSizeCommand>(-1);
		}

		public void DeleteFormulaCmd(IRibbonControl control)
		{
			factory.Run<DeleteFormulaCommand>();
		}

		public void EditSearchEnginesCmd(IRibbonControl control)
		{
			factory.Run<EditSearchEnginesCommand>();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void EditStylesCmd(IRibbonControl control)
		{
			factory.GetCommand<EditStylesCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void IncreaseFontSizeCmd(IRibbonControl control)
		{
			factory.Run<AlterSizeCommand>(1);
		}

		public void HighlightFormulaCmd(IRibbonControl control)
		{
			factory.Run<HighlightFormulaCommand>();
		}

		public void InsertBlueStatusCmd(IRibbonControl control)
		{
			factory.Run<InsertStatusCommand>(StatusColor.Blue);
		}

		public void InsertCalendarCmd(IRibbonControl control)
		{
			factory.Run<InsertCalendarCommand>();
		}

		public void InsertCodeBlockCmd(IRibbonControl control)
		{
			factory.Run<InsertCodeBlockCommand>();
		}

		public void InsertDoubleHorizontalLineCmd(IRibbonControl control)
		{
			factory.Run<InsertLineCommand>('═');
		}

		public void InsertExpandCmd(IRibbonControl control)
		{
			factory.Run<InsertExpandCommand>();
		}

		public void InsertGrayStatusCmd(IRibbonControl control)
		{
			factory.Run<InsertStatusCommand>(StatusColor.Gray);
		}

		public void InsertGreenStatusCmd(IRibbonControl control)
		{
			factory.Run<InsertStatusCommand>(StatusColor.Green);
		}

		public void InsertHorizontalLineCmd(IRibbonControl control)
		{
			factory.Run<InsertLineCommand>('─');
		}

		public void InsertInfoBlockCmd(IRibbonControl control)
		{
			factory.Run<InsertInfoBlockCommand>(false);
		}

		public void InsertRedStatusCmd(IRibbonControl control)
		{
			factory.Run<InsertStatusCommand>(StatusColor.Red);
		}

		public void InsertTocCmd(IRibbonControl control)
		{
			factory.Run<InsertTocCommand>();
		}

		public void InsertWarningBlockCmd(IRibbonControl control)
		{
			factory.Run<InsertInfoBlockCommand>(true);
		}

		public void InsertYellowStatusCmd(IRibbonControl control)
		{
			factory.Run<InsertStatusCommand>(StatusColor.Yellow);
		}

		public void MergeCmd(IRibbonControl control)
		{
			factory.Run<MergeCommand>();
		}

		public void NameUrlsCmd(IRibbonControl control)
		{
			factory.Run<NameUrlsCommand>();
		}

		public void NewStyleCmd(IRibbonControl control)
		{
			factory.GetCommand<NewStyleCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void NoSpellCheckCmd(IRibbonControl control)
		{
			factory.Run<NoSpellCheckCommand>();
		}

		public void NumberPagesCmd(IRibbonControl control)
		{
			factory.Run<NumberPagesCommand>();
		}

		public void NumberSectionsCmd(IRibbonControl control)
		{
			factory.Run<NumberSectionsCommand>();
		}

		public void OutlineCmd(IRibbonControl control)
		{
			factory.Run<OutlineCommand>();
		}

		public void PasteRtfCmd(IRibbonControl control)
		{
			factory.GetCommand<PasteRtfCommand>().Execute();
		}

		public void PronunciateCmd(IRibbonControl control)
		{
			factory.Run<PronunciateCommand>();
		}

		public void RecalculateFormulaCmd(IRibbonControl control)
		{
			factory.Run<RecalculateFormulaCommand>();
		}

		public void RemoveAuthorsCmd(IRibbonControl control)
		{
			factory.Run<RemoveAuthorsCommand>();
		}

		public void RemoveCitationsCmd(IRibbonControl control)
		{
			factory.Run<RemoveCitationsCommand>();
		}

		public void RemoveEmptyCmd(IRibbonControl control)
		{
			factory.Run<RemoveEmptyCommand>();
		}

		public void RemoveFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveFootnoteCommand>().Execute();
		}

		public void RemovePageNumbersCmd(IRibbonControl control)
		{
			factory.Run<RemovePageNumbersCommand>();
		}

		public void RemoveSectionNumbersCmd(IRibbonControl control)
		{
			factory.Run<RemoveSectionNumbersCommand>();
		}

		public void RemoveSpacingCmd(IRibbonControl control)
		{
			factory.Run<RemoveSpacingCommand>();
		}

		public void ResizeImagesCmd(IRibbonControl control)
		{
			factory.Run<ResizeImagesCommand>();
		}

		public void RunPluginCmd(IRibbonControl control)
		{
			factory.Run<RunPluginCommand>();
		}

		public void ExportCmd(IRibbonControl control)
		{
			factory.Run<ExportCommand>();
		}

		public void SearchAndReplaceCmd(IRibbonControl control)
		{
			factory.Run<SearchAndReplaceCommand>();
		}

		public void SearchEngineCmd(IRibbonControl control)
		{
			factory.Run<SearchEngineCommand>(control.Tag);
		}

		public void ShowXmlCmd(IRibbonControl control)
		{
			factory.Run<ShowXmlCommand>();
		}

		public void SortCmd(IRibbonControl control)
		{
			factory.Run<SortCommand>();
		}

		public void StrikeoutCmd(IRibbonControl control)
		{
			factory.Run<StrikeoutCommand>();
		}

		public void TextToTableCmd(IRibbonControl control)
		{
			factory.Run<TextToTableCommand>();
		}

		public void ToLowercaseCmd(IRibbonControl control)
		{
			factory.Run<ToCaseCommand>(false);
		}

		public void ToggleDttmCmd(IRibbonControl control)
		{
			factory.Run<ToggleDttmCommand>();
		}

		public void ToUppercaseCmd(IRibbonControl control)
		{
			factory.Run<ToCaseCommand>(true);
		}

		public void TrimCmd(IRibbonControl control)
		{
			factory.Run<TrimCommand>();
		}

		public void WordCountCmd(IRibbonControl control)
		{
			factory.Run<WordCountCommand>();
		}
	}
}
