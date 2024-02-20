//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Compress images on page by resizing down to view width and height.
	/// Quality will be compromised.
	/// </summary>
	internal class CompressImagesCommand : Command
	{
		public CompressImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);
			var elements = page.Root.Descendants(ns + "Image")?
				.Where(e => e.Attribute("selected")?.Value == "all");

			if ((elements == null) || !elements.Any())
			{
				// include background images
				elements = page.Root.Descendants(ns + "Image");
			}

			if (!elements.Any())
			{
				UI.MoreBubbleWindow.Show(Resx.ConvertImagesCommand_NoImages);
				return;
			}

			var count = 0;
			var delta = 0;

			foreach (var element in elements)
			{
				var wrapper = new OneImage(element);
				using var image = wrapper.ReadImage();

				if (image.Width > wrapper.Width || image.Height > wrapper.Height)
				{
					var editor = new ImageEditor
					{
						PreserveQualityOnResize = false,
						Size = new Size(wrapper.Width, wrapper.Height)
					};

					var datalen = wrapper.Data.Length;

					// work against image rather than wrapper so we can control if we
					// want to accept changes based on size
					var compressed = editor.Apply(image);

					var data = compressed.ToBase64String();
					if (data.Length < datalen)
					{
						logger.WriteLine($"compressed {datalen} to {data.Length}");
						wrapper.Data = data;

						delta += datalen - data.Length;
						count++;
					}
				}
			}

			if (count > 0)
			{
				await one.Update(page);

				UI.MoreBubbleWindow.Show(string.Format(
					Resx.ConvertImagesCommand_Converted, count, delta));
			}
			else
			{
				UI.MoreBubbleWindow.Show(Resx.ConvertImagesCommand_NoImages);
			}
		}
	}
}
