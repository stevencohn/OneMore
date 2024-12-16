//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;


	#region Wrappers
	internal class OpenNotebookInBrowserCommand : OpenInBrowserCommand
	{
		public OpenNotebookInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Notebook);
		}
	}
	internal class OpenSectionInBrowserCommand : OpenInBrowserCommand
	{
		public OpenSectionInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Section);
		}
	}
	internal class OpenPageInBrowserCommand : OpenInBrowserCommand
	{
		public OpenPageInBrowserCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(OneNote.NodeType.Page);
		}
	}
	#endregion Wrappers


	internal class OpenInBrowserCommand : Command
	{
		public OpenInBrowserCommand()
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
