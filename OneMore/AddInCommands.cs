//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;


    public partial class AddIn
	{

		public void AboutCmd(IRibbonControl control)
		{
			factory.GetCommand<AboutCommand>().Execute();
		}

		public void AddCaptionCmd(IRibbonControl control)
		{
			factory.GetCommand<AddCaptionCommand>().Execute();
		}

		public void AddFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<AddFootnoteCommand>().Execute();
		}

		public void AddFormulaCmd(IRibbonControl control)
		{
			factory.GetCommand<AddFormulaCommand>().Execute();
		}

		public void AddTitleIconCmd(IRibbonControl control)
		{
			factory.GetCommand<AddTitleIconCommand>().Execute();
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
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void CollapseCmd(IRibbonControl control)
		{
			factory.GetCommand<CollapseCommand>().Execute();
		}

		public void CropImageCmd(IRibbonControl control)
		{
			factory.GetCommand<CropImageCommand>().Execute();
		}

		public void DecreaseFontSizeCmd(IRibbonControl control)
		{
			factory.GetCommand<AlterSizeCommand>().Execute(-1);
		}

		public void DeleteFormulaCmd(IRibbonControl control)
		{
			factory.GetCommand<DeleteFormulaCommand>().Execute();
		}

		public void EditStylesCmd(IRibbonControl control)
		{
			factory.GetCommand<EditStylesCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void IncreaseFontSizeCmd(IRibbonControl control)
		{
			factory.GetCommand<AlterSizeCommand>().Execute(1);
		}

		public void HighlightFormulaCmd(IRibbonControl control)
		{
			factory.GetCommand<HighlightFormulaCommand>().Execute();
		}

		public void InsertBlueStatusCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertStatusCommand>().Execute(StatusColor.Blue);
		}

		public void InsertCodeBlockCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertCodeBlockCommand>().Execute();
		}

		public void InsertDoubleHorizontalLineCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertLineCommand>().Execute('═');
		}

		public void InsertExpandCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertExpandCommand>().Execute();
		}

		public void InsertGrayStatusCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertStatusCommand>().Execute(StatusColor.Gray);
		}

		public void InsertGreenStatusCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertStatusCommand>().Execute(StatusColor.Green);
		}

		public void InsertHorizontalLineCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertLineCommand>().Execute('─');
		}

		public void InsertInfoBlockCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertInfoBlockCommand>().Execute(false);
		}

		public void InsertRedStatusCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertStatusCommand>().Execute(StatusColor.Red);
		}

		public void InsertTocCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertTocCommand>().Execute();
		}

		public void InsertWarningBlockCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertInfoBlockCommand>().Execute(true);
		}

		public void InsertYellowStatusCmd(IRibbonControl control)
		{
			factory.GetCommand<InsertStatusCommand>().Execute(StatusColor.Yellow);
		}

		public void MergeCmd(IRibbonControl control)
		{
			factory.GetCommand<MergeCommand>().Execute();
		}

		public void NameUrlsCmd(IRibbonControl control)
		{
			factory.GetCommand<NameUrlsCommand>().Execute();
		}

		public void NewStyleCmd(IRibbonControl control)
		{
			factory.GetCommand<NewStyleCommand>().Execute();
			ribbon.Invalidate(); // TODO: only if changes?
		}

		public void NoSpellCheckCmd(IRibbonControl control)
		{
			factory.GetCommand<NoSpellCheckCommand>().Execute();
		}

		public void NumberPagesCmd(IRibbonControl control)
		{
			factory.GetCommand<NumberPagesCommand>().Execute();
		}

		public void NumberSectionsCmd(IRibbonControl control)
		{
			factory.GetCommand<NumberSectionsCommand>().Execute();
		}

		public void OutlineCmd(IRibbonControl control)
		{
			factory.GetCommand<OutlineCommand>().Execute();
		}

		public void PasteRtfCmd(IRibbonControl control)
		{
			factory.GetCommand<PasteRtfCommand>().Execute();
		}

		public void PronunciateCmd(IRibbonControl control)
		{
			factory.GetCommand<PronunciateCommand>().Execute();
		}

		public void RecalculateFormulaCmd(IRibbonControl control)
		{
			factory.GetCommand<RecalculateFormulaCommand>().Execute();
		}

		public void RemoveAuthorsCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveAuthorsCommand>().Execute();
		}

		public void RemoveCitationsCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveCitationsCommand>().Execute();
		}

		public void RemoveEmptyCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveEmptyCommand>().Execute();
		}

		public void RemoveFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveFootnoteCommand>().Execute();
		}

		public void RemovePageNumbersCmd(IRibbonControl control)
		{
			factory.GetCommand<RemovePageNumbersCommand>().Execute();
		}

		public void RemoveSectionNumbersCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveSectionNumbersCommand>().Execute();
		}

		public void RemoveSpacingCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveSpacingCommand>().Execute();
		}

		public void ResizeImagesCmd(IRibbonControl control)
		{
			factory.GetCommand<ResizeImagesCommand>().Execute();
		}

		public void RunPluginCmd(IRibbonControl control)
		{
			factory.GetCommand<RunPluginCommand>().Execute();
		}

		public void SaveAsCmd(IRibbonControl control)
		{
			factory.GetCommand<SaveAsCommand>().Execute();
		}

		public void SearchAndReplaceCmd(IRibbonControl control)
		{
			factory.GetCommand<SearchAndReplaceCommand>().Execute();
		}

		public void SearchEngineCmd(IRibbonControl control)
		{
			factory.GetCommand<SearchEngineCommand>().Execute(control.Tag);
		}

		public void ShowXmlCmd(IRibbonControl control)
		{
			factory.GetCommand<ShowXmlCommand>().Execute();
		}

		public void SortCmd(IRibbonControl control)
		{
			factory.GetCommand<SortCommand>().Execute();
		}

		public void StrikeoutCmd(IRibbonControl control)
		{
			factory.GetCommand<StrikeoutCommand>().Execute();
		}

		public void ToLowercaseCmd(IRibbonControl control)
		{
			factory.GetCommand<ToCaseCommand>().Execute(false);
		}

		public void ToggleDttmCmd(IRibbonControl control)
		{
			factory.GetCommand<ToggleDttmCommand>().Execute();
		}

		public void ToUppercaseCmd(IRibbonControl control)
		{
			factory.GetCommand<ToCaseCommand>().Execute(true);
		}

		public void TrimCmd(IRibbonControl control)
		{
			factory.GetCommand<TrimCommand>().Execute();
		}
	}
}
