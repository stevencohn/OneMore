//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001      // Type is not CLS-compliant
#pragma warning disable IDE0060     // remove unused parameter

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;


    public partial class AddIn
	{

		public void AddFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<AddFootnoteCommand>().Execute();
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

		public void DecreaseFontSizeCmd(IRibbonControl control)
		{
			factory.GetCommand<AlterSizeCommand>().Execute(-1);
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

		public void PasteRtfCmd(IRibbonControl control)
		{
			factory.GetCommand<PasteRtfCommand>().Execute();
		}

		public void RemoveFootnoteCmd(IRibbonControl control)
		{
			factory.GetCommand<RemoveFootnoteCommand>().Execute();
		}

		public void SearchAndReplaceCmd(IRibbonControl control)
		{
			factory.GetCommand<SearchAndReplaceCommand>().Execute();
		}

		public void ShowAboutCmd(IRibbonControl control)
		{
			factory.GetCommand<ShowAboutCommand>().Execute();
		}

		public void ShowXmlCmd(IRibbonControl control)
		{
			factory.GetCommand<ShowXmlCommand>().Execute();
		}

		public void SortCmd(IRibbonControl control)
		{
			factory.GetCommand<SortCommand>().Execute();
		}

		public void ToLowercaseCmd(IRibbonControl control)
		{
			factory.GetCommand<ToCaseCommand>().Execute(false);
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
