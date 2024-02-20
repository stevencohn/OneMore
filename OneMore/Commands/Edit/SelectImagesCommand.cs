//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Select all images on the page
	/// </summary>
	internal class SelectImagesCommand : Command
	{

		public SelectImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			// find foreground images
			var images = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "Image")?
				.ToList();

			if (!images.Any())
			{
				// else find background images
				images = page.Root
					.Elements(ns + "Image")
					.ToList();
			}

			if (images != null && images.Any())
			{
				// deselect selected images incase collision between back and fore
				var selections = page.Root.Descendants()
					.Where(e => e.Attribute("selected")?.Value == "all")
					.ToList();

				selections.ForEach((e) =>
				{
					e.Attributes("selected").Remove();
				});

				foreach (var image in images)
				{
					image.SetAttributeValue("selected", "all");
				}

				await one.Update(page);
			}
		}
	}
}
