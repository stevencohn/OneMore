//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Select all Ink on the page
	/// </summary>
	/// <remarks>
	/// When ink is selected using the OneNote UI, a bounding box is displayed that lets the
	/// user rotate and access a context menu. But when selected programmatically, no bounding
	/// box is presented. Otherwise, the result is the same, e.g. you can convert Ink to Text.
	/// </remarks>
	internal class SelectInkCommand : Command
	{

		public SelectInkCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var drawings = page.Root.Elements(ns + "InkDrawing").ToList();

			if (drawings.Any())
			{
				// deselect everything first...

				var selections = page.Root.Descendants()
					.Where(e => e.Attribute("selected")?.Value == "all")
					.ToList();

				selections.ForEach((e) =>
				{
					e.Attributes("selected").Remove();
				});

				// select all InkDrawing elements...

				foreach (var drawing in drawings)
				{
					drawing.SetAttributeValue("selected", "all");
				}

				await one.Update(page);
			}
		}
	}
}
