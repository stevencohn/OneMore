//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;


	#region Wrappers
	internal class ViewNotebookInBrowserCommand : ViewInBrowserCommand
	{
		public ViewNotebookInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Notebook);
		}
	}
	internal class ViewSectionInBrowserCommand : ViewInBrowserCommand
	{
		public ViewSectionInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Section);
		}
	}
	internal class ViewPageInBrowserCommand : ViewInBrowserCommand
	{
		public ViewPageInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Page);
		}
	}
	#endregion Wrappers


	internal class ViewInBrowserCommand : Command
	{
		public ViewInBrowserCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var node = (OneNote.NodeType)args[0];

			await using var one = new OneNote();

			var id = node switch
			{
				OneNote.NodeType.Notebook => one.CurrentNotebookId,
				OneNote.NodeType.Section => one.CurrentSectionId,
				_ => one.CurrentPageId
			};

			var url = one.GetWebHyperlink(id, string.Empty);

			try
			{
				System.Diagnostics.Process.Start(url);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot browse {url}", exc);
			}

			await Task.Yield();
		}
	}
}
