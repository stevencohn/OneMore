//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Removes all ink drawings and annotations from the current page.
	/// </summary>
	internal class RemoveInkCommand : Command
	{
		public RemoveInkCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var ink = page.Root.Descendants(ns + "InkDrawing");

				if (ink.Any())
				{
					ink.ForEach(e => 
						one.DeleteContent(page.PageId, e.Attribute("objectID").Value));
				}
			}

			await Task.Yield();
		}
	}
}
